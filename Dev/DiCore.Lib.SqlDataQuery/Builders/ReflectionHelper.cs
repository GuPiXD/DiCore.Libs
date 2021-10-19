using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DiCore.Lib.SqlDataQuery.Builders
{
    internal static class ReflectionHelper
    {
        private static readonly Assembly SystemAssembly = typeof(object).Assembly;

        public static bool IsSystemType(this Type type) => type.Assembly == SystemAssembly;

        /// <summary>
        /// Returns the <see cref="T:System.Reflection.MemberInfo"/> for the specified lambda expression.
        /// </summary>
        /// <param name="lambda">A lamba expression containing a MemberExpression.</param>
        /// <returns>A <see cref="MemberInfo"/> object for the member in the specified lambda expression.</returns>
        public static Tuple<MemberInfo, MemberInfo> GetMemberInfo(this LambdaExpression lambda)
        {
            Expression expr = lambda;
            while (true)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression) expr).Body;
                        break;

                    case ExpressionType.Convert:
                        expr = ((UnaryExpression) expr).Operand;
                        break;

                    case ExpressionType.Parameter:
                        return new Tuple<MemberInfo, MemberInfo>(null, null);

                    case ExpressionType.MemberAccess:
                        var memberExpression = (MemberExpression) expr;
                        var member = memberExpression.Member;
                        Type paramType;

                        while (memberExpression != null)
                        {
                            paramType = memberExpression.Type;

                            // Find the member on the base type of the member type
                            // E.g. EmailAddress.Value
                            var baseMember = paramType.GetMembers().FirstOrDefault(m => m.Name == member.Name);
                            if (baseMember != null)
                            {
                                // Don't use the base type if it's just the nullable type of the derived type
                                // or when the same member exists on a different type
                                // E.g. Nullable<decimal> -> decimal
                                // or:  SomeType { string Length; } -> string.Length
                                if (baseMember is PropertyInfo baseProperty && member is PropertyInfo property)
                                {
                                    if (baseProperty.DeclaringType == property.DeclaringType &&
                                        baseProperty.PropertyType != Nullable.GetUnderlyingType(property.PropertyType))
                                    {
                                        return new Tuple<MemberInfo, MemberInfo>(baseMember, memberExpression.Member);
                                    }
                                }
                                else
                                {
                                    return new Tuple<MemberInfo, MemberInfo>(baseMember, memberExpression.Member);
                                }
                            }

                            memberExpression = memberExpression.Expression as MemberExpression;
                        }

                        // Make sure we get the property from the derived type.
                        paramType = lambda.Parameters[0].Type;
                        return new Tuple<MemberInfo, MemberInfo>(paramType.GetMember(member.Name)[0], member);

                    default:
                        throw new NotSupportedException($"The expression node type '{expr.NodeType}' is not supported");
                }
            }
        }

        public static string GetColumnName(this LambdaExpression lambda)
        {
            var expr = lambda.Body;
            
            if (expr.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException($"The expression '{lambda}' is not supported");
            }

            var columnName = String.Empty;
            var m = (MemberExpression) expr;

            switch (m.Expression.NodeType)
            {
                case ExpressionType.Parameter:
                    columnName = QueryBuilderHelper.GetColumnAlias(m.Member.Name);
                    break;
               
                case ExpressionType.MemberAccess:
                    var fieldName = m.Member.Name;
                    var tableName = m.GetTableName();
                    columnName = QueryBuilderHelper.GetJointColumnAlias(tableName, fieldName);
                    break;                    
            }

            return columnName;
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            Type type;
            switch (memberInfo)
            {
                case MethodInfo mInfo:
                    type = mInfo.ReturnType;
                    break;

                case PropertyInfo pInfo:
                    type = pInfo.PropertyType;
                    break;

                case FieldInfo fInfo:
                    type = fInfo.FieldType;
                    break;

                case null:
                    throw new ArgumentNullException(nameof(memberInfo));
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberInfo));
            }

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                var typeInfo = type.GetTypeInfo();
                var implementsGenericIEnumerableOrIsGenericIEnumerable =
                    typeInfo.ImplementedInterfaces.Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                    typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (implementsGenericIEnumerableOrIsGenericIEnumerable)
                {
                    type = type.GetGenericArguments()[0];
                }
            }

            return type;
        }   
        
        public static object GetValue(this MemberExpression memberExpression, object container)
        {
            object value = null;

            switch (memberExpression.Member)
            {
                case FieldInfo fieldInfo:
                    value = fieldInfo.GetValue(container);
                    break;

                case PropertyInfo propertyInfo:
                    value = propertyInfo.GetValue(container, null);
                    break;
            }

            return value;
        }
    }
}