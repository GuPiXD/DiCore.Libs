using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DiCore.Lib.FastPropertyAccess
{
    /// <summary>
    /// Класс для создания объекта и инициализации его свойств, а также получения 
    /// значений и присвоения значений его свойствам
    /// </summary>
    /// <typeparam name="T">Тип объекта</typeparam>
    public class FastPropertyAccess<T> : IFastPropertyAccess<T>
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public FastPropertyAccess(EnPropertyUsingMode mode, List<string> excludeProps = null, bool ignoreCase = false, string[] propsWithTypeMapper = null)
        {
            type = typeof(T);
            newExpression = Expression.New(type);
            dictParam = Expression.Parameter(typeof(Dictionary<string, object>), "d");
            propertyInfos = type.GetBindableProperties();
            getFunctions = ignoreCase
                ? new Dictionary<string, Func<T, object>>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, Func<T, object>>();
            if (mode == EnPropertyUsingMode.Create || mode == EnPropertyUsingMode.CreateAndGet)
            {
                createFunction = CreateObjectCreator(excludeProps, propsWithTypeMapper);
            }

            if (mode == EnPropertyUsingMode.Get ||
                mode == EnPropertyUsingMode.CreateAndGet ||
                mode == EnPropertyUsingMode.SetAndGet)
                CreatePropertyGetters();

            if (mode == EnPropertyUsingMode.Set ||
                mode == EnPropertyUsingMode.SetAndGet)
                CreatePropertySetters();
        }

        private readonly Func<Dictionary<string, object>, T> createFunction;
        private readonly Dictionary<string, Func<T, object>> getFunctions;

        private readonly Dictionary<string, Action<T, object>> setFunctions =
            new Dictionary<string, Action<T, object>>();

        private readonly Type type;
        private readonly NewExpression newExpression;
        private readonly ParameterExpression dictParam;
        private readonly PropertyInfo[] propertyInfos;
        private static readonly Type helperType = typeof(Helper);

        /// <summary>
        /// Создание функции создания экземпляра класса и присвоения значений свойств
        /// </summary>
        private Func<Dictionary<string, object>, T> CreateObjectCreator(List<string> excludeProps,
            string[] propsWithTypeMapper)
        {
            var list = CreateMemberBindings(propertyInfos, excludeProps, propsWithTypeMapper);
            var ex = Expression.Lambda<Func<Dictionary<string, object>, T>>(Expression.MemberInit(newExpression, list),
                dictParam);

            return ex.Compile();
        }

        private List<MemberBinding> CreateMemberBindings(PropertyInfo[] properties, List<string> excludeProps,
            string[] propsWithTypeMapper,
            string baseName = "")
        {
            var list = new List<MemberBinding>();

            foreach (var propertyInfo in properties)
            {
                if (excludeProps != null && excludeProps.Contains(propertyInfo.Name))
                    continue;

                if (propertyInfo.DeclaringType == null)
                    continue;

                MethodInfo setMethod;

                if (type.FullName != propertyInfo.DeclaringType.FullName)
                {
                    var prop = propertyInfo.DeclaringType.GetProperty(propertyInfo.Name);
                    setMethod = prop.GetSetMethod();
                }
                else
                {
                    setMethod = propertyInfo.GetSetMethod();
                }

                if (setMethod == null)
                    continue;

                Expression call;

                var propertyType = propertyInfo.PropertyType;
                var propName = String.IsNullOrEmpty(baseName) ? propertyInfo.Name : $"{baseName}#{propertyInfo.Name}";

                if (propertyType.IsSystemType() || (propsWithTypeMapper != null && Array.IndexOf(propsWithTypeMapper, propName) != -1))
                {
                    call = Expression.Call(helperType,
                        "GetValue", new[] {propertyType}, dictParam,
                        Expression.Constant(propName));
                }
                else
                {
                    var pt = propertyType;
                    var bindings = CreateMemberBindings(pt.GetBindableProperties(), excludeProps, propsWithTypeMapper, propName);
                    call = Expression.MemberInit(Expression.New(pt), bindings);
                }

                var mb = Expression.Bind(setMethod, call);
                list.Add(mb);
            }

            return list;
        }

        /// <summary>
        /// Создание функций для получения значений свойств экземпляра класса
        /// </summary>
        private void CreatePropertyGetters()
        {
            var typeObj = typeof(object);
            foreach (var propertyInfo in propertyInfos)
            {
                var propertyType = propertyInfo.DeclaringType;
                var instance = Expression.Parameter(typeObj, "instance");
                UnaryExpression instanceCast = (!propertyType.IsValueType)
                    ? Expression.TypeAs(instance, propertyType)
                    : Expression.Convert(instance, propertyType);
                getFunctions.Add(propertyInfo.Name,
                    Expression.Lambda<Func<T, object>>(
                        Expression.TypeAs(Expression.Call(instanceCast, propertyInfo.GetGetMethod()), typeObj),
                        instance).Compile());
            }
        }

        /// <summary>
        /// Создание функций для присвоения значений свойствам экземпляра класса
        /// </summary>
        private void CreatePropertySetters()
        {
            foreach (var propertyInfo in propertyInfos)
            {
                var propertyType = propertyInfo.DeclaringType;
                var instance = Expression.Parameter(propertyType, "instance");
                var value = Expression.Parameter(typeof(object), "value");

                UnaryExpression instanceCast = (!propertyType.IsValueType)
                    ? Expression.TypeAs(instance, propertyType)
                    : Expression.Convert(instance, propertyType);
                UnaryExpression valueCast = (!propertyInfo.PropertyType.IsValueType)
                    ? Expression.TypeAs(value, propertyInfo.PropertyType)
                    : Expression.Convert(value, propertyInfo.PropertyType);
                var callExpression = Expression.Call(instanceCast, propertyInfo.GetSetMethod(), valueCast);
                setFunctions.Add(propertyInfo.Name,
                    Expression.Lambda<Action<T, object>>(callExpression, instance, value).Compile());
            }
        }

        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="props"></param>
        /// <returns>Экземпляр объекта</returns>
        public T CreateObject(Dictionary<string, object> props)
        {
            return createFunction(props);
        }

        /// <summary>
        /// Получение значения свойства объекта
        /// </summary>
        /// <param name="instance">Экземпляр объекта</param>
        /// <param name="propertyName">Наименование свойства</param>
        /// <returns>Значение свойства</returns>
        public object Get(T instance, string propertyName)
        {
            return getFunctions[propertyName](instance);
        }


        /// <summary>
        /// Присвоение значения свойству
        /// </summary>
        /// <param name="instance">Экземпляр объекта</param>
        /// <param name="propertyName">Наименование свойства</param>
        /// <param name="value">Значение</param>
        /// <returns>Результат выполнения операции</returns>
        public void Set(T instance, string propertyName, object value)
        {
            setFunctions[propertyName](instance, value);
        }

        /// <summary>
        /// Получение списка имен свойств
        /// </summary>
        public IEnumerable<string> PropertyNames => propertyInfos.Select(p => p.Name);
    }
}