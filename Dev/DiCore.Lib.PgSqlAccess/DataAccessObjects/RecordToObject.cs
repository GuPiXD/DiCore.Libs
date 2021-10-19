using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DiCore.Lib.FastPropertyAccess;
using DiCore.Lib.PgSqlAccess.Types;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    public class RecordToObject<T>
    {
        private readonly Tuple<int, int>[] fieldPropNamesMatching;
        private readonly Dictionary<string, (PropertyDescription propertyDescriptor, ITypeMapper valueMapper)> propertiesWithMappers;

        public RecordToObject(): this(Helpers.Helper.GetProperties<T>(out var usingOrder, true), usingOrder, null, null)
        {}

        public RecordToObject(List<PropertyDescription> props, bool usingOrder, List<string> excludeProps,
            Tuple<int, int>[] fieldPropNamesMatching)
        {
            this.fieldPropNamesMatching = fieldPropNamesMatching;
            propertiesWithMappers = props.Where(p => SqlMapper.TypeMappers.ContainsKey(p.PropertyType))
                .ToDictionary(o => o.Name, p => (p, SqlMapper.TypeMappers[p.PropertyType]));

            FastPropertyAccess = new FastPropertyAccess<T>(EnPropertyUsingMode.Create, excludeProps,
                propsWithTypeMapper: propertiesWithMappers.Keys.ToArray());

            var propertyInfos = props;
            PropertyValues = new Dictionary<string, object>();
            PropertyNames = new string[propertyInfos.Count];
            int index = 0;
            if (usingOrder)
                propertyInfos = propertyInfos.OrderBy(e => e.Order).ToList();
            foreach (var propertyInfo in propertyInfos)
            {
                PropertyNames[index] = propertyInfo.Name;
                PropertyValues.Add(propertyInfo.Name, null);
                index++;
            }
        }

        private IFastPropertyAccess<T> FastPropertyAccess { get; set; }
        private Dictionary<string, object> PropertyValues { get; set; }
        public string[] PropertyNames { get; private set; }

        public T GetResult(object[] record)
        {
            return CreateObject(record.Length, i => record[i]);
        }

        public T DataReaderGetResult(IDataReader reader)
        {
            return CreateObject(reader.FieldCount, i => reader[i]);
        }

        private T CreateObject(int valuesCount, Func<int, object> getValue)
        {
            if (fieldPropNamesMatching == null)
            {
                var count = valuesCount;
                if (PropertyValues.Count != count)
                    throw new Exception(
                        $"Не совпадает количество параметров типа {typeof(T)} и количество параметров, " +
                        $"возвращаемых функцией (ожидаемое количество параметров: {PropertyValues.Count}, " +
                        $"количество параметров, возвращаемое функцией: {count})");

                for (var i = 0; i < count; i++)
                {
                    var propName = PropertyNames[i];
                    PropertyValues[propName] = PrepareValue(getValue(i), propName);
                }
            }
            else
            {
                foreach (var name in fieldPropNamesMatching)
                {
                    var propName = PropertyNames[name.Item2];
                    PropertyValues[propName] = PrepareValue(getValue(name.Item1), propName);
                }
            }

            return FastPropertyAccess.CreateObject(PropertyValues);
        }

        private object PrepareValue(object source, string propName)
        {
            return propertiesWithMappers.TryGetValue(propName, out var mapper)
                ? mapper.valueMapper.Parse(mapper.propertyDescriptor.PropertyType, source)
                : source;
        }
    }
}
