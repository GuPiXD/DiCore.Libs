using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using DiCore.Lib.SqlDataQuery.QueryParameter;
using DiCore.Lib.SqlDataQuery.Utils;

namespace DiCore.Lib.SqlDataQuery.Builders
{
    internal class FiltersTranslator<T> : ExpressionVisitor
    {
        private readonly List<ColumnFilters> filtersGroups = new List<ColumnFilters>();
        private CustomFilter currentFilter;
        private ColumnFilters currentFiltersGroup;

        public ColumnFilters[] Translate(Expression<Func<T, bool>> predicate)
        {
            Visit(predicate);

            return filtersGroups.ToArray();
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;

                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }

            return u;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return node.Method.DeclaringType == typeof(string) ? BuildStringQuery(node) : base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            CustomFilter filter;
            var finishFilter = false;

            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    var filterLogic = b.ToFilterLogic();
                    if (currentFiltersGroup != null)
                    {
                        if (String.IsNullOrEmpty(currentFiltersGroup.Logic))
                        {
                            currentFiltersGroup.Logic = filterLogic;
                        }
                        else if (currentFiltersGroup.Logic != filterLogic)
                        {
                            CreateCurrentFiltersGroup(filterLogic);
                        }
                    }
                    else
                    {
                        CreateCurrentFiltersGroup(filterLogic);
                    }

                    break;

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                    filter = GetOrCreateCurrentFilter();
                    filter.Clause = String.Join(" ", filter.Clause, b.ToArithmeticOperation());
                    break;

                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                    filter = GetOrCreateCurrentFilter();
                    filter.Clause = String.Join(" ",
                        $"{(b.IsNeedBrackets() ? filter.Clause.WrapUp('(', ')') : filter.Clause)}",
                        b.ToArithmeticOperation());
                    break;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    filter = GetOrCreateCurrentFilter();
                    filter.Clause = String.Join(" ", filter.Clause, b.ToFilterOperation().ToSql());
                    finishFilter = true;
                    break;

                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }

            Visit(b.Right);

            if (!finishFilter) return b;

            currentFilter = null;

            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
            {
                return c;
            }

            var filter = GetOrCreateCurrentFilter();
            var type = c.Value.GetType();
            var value = String.Empty;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DBNull:
                    value = "NULL";
                    break;

                case TypeCode.Boolean:
                    value = (bool) c.Value ? "true" : "false";
                    break;

                case TypeCode.String:
                    value = ((string) c.Value).WrapUp('\'');
                    break;

                case TypeCode.DateTime:
                    value = ((DateTime) c.Value).ToString("yyyy-MM-dd").WrapUp('\'');
                    break;

                case TypeCode.Object:
                    if (type == typeof(Guid))
                    {
                        value = c.Value.ToString().WrapUp('\'');
                    }
                    //else if (type == typeof(TimeSpan))
                    //{
                    //    value = ((TimeSpan)c.Value).ToString("yyyy-MM-dd HH:mm:ss").WrapUp('\'');
                    //}
                    else
                    {
                        throw new NotSupportedException($"The constant for '{c.Value}' is not supported");
                    }

                    break;

                default:
                    value = Convert.ToString(c.Value, CultureInfo.InvariantCulture);
                    break;
            }

            filter.Clause = String.Join(" ", filter.Clause, value);
            filter.Value = value;

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            var expression = m.Expression;
            if (expression == null)
                throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");

            var memberExpression = m;
            while (expression is MemberExpression me)
            {
                expression = me.Expression;
                memberExpression = me;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Parameter:
                    var filter = GetOrCreateCurrentFilter();
                    var tableName = m.GetTableName();
                    if (String.IsNullOrEmpty(tableName))
                    {
                        tableName = typeof(T).GetTableName();
                    }
                    var tableAlias = QueryBuilderHelper.GetTableAlias(tableName);
                    var columnName = m.Member.GetColumnName();
                    filter.Field = $"{tableAlias.Quotes()}.{columnName.Quotes()}";
                    filter.Clause = String.Join(" ", filter.Clause, filter.Field);
                    break;

                case ExpressionType.Constant:                    
                    var container = ((ConstantExpression) expression).Value;
                    var value = memberExpression.GetValue(container);

                    if (!value.GetType().IsSystemType())
                    {
                        value = m.GetValue(value);
                    }

                    Visit(Expression.Constant(value));
                    break;

                default:
                    throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
            }

            return m;
        }        

        private CustomFilter GetOrCreateCurrentFilter()
        {
            if (currentFilter != null) return currentFilter;

            currentFilter = new CustomFilter();
            var filtersGroup = currentFiltersGroup ?? CreateCurrentFiltersGroup();
            filtersGroup.AddFilter(currentFilter);

            return currentFilter;
        }

        private ColumnFilters CreateCurrentFiltersGroup(string logic = null)
        {
            currentFiltersGroup = new ColumnFilters() {Logic = logic};
            filtersGroups.Add(currentFiltersGroup);

            return currentFiltersGroup;
        }

        private Expression BuildStringQuery(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType != typeof(string))
            {
                return methodCallExpression;
            }

            var arguments = methodCallExpression.Arguments.ToArray();
            if (arguments.Length != 1)
            {
                return methodCallExpression;
            }
            
            var caseInsensitive = false;
            MethodCallExpression stringMethodCallExpression;
            var stringExpression = methodCallExpression.Object;
            while ((stringMethodCallExpression = stringExpression as MethodCallExpression) != null)
            {
                switch (stringMethodCallExpression.Method.Name)
                {
                    case "ToLower":
                    case "ToLowerInvariant":
                    case "ToUpper":
                    case "ToUpperInvariant":
                        caseInsensitive = true;
                        break;
                }

                stringExpression = stringMethodCallExpression.Object;
            }
            
            if (!(stringExpression is MemberExpression))
                throw new NotSupportedException();

            Visit(stringExpression);

            var filter = GetOrCreateCurrentFilter();
            var clause = String.Join(" ", filter.Clause, $"{(caseInsensitive ? "ILIKE" : "LIKE")}");

            Visit(arguments[0]);

            var pattern = filter.Value?.UnWrapUp('\'');
            
            switch (methodCallExpression.Method.Name)
            {
                case "Contains":
                    pattern = $"%{pattern}%";
                    break;
                case "EndsWith":
                    pattern = $"%{pattern}";
                    break;
                case "StartsWith":
                    pattern = $"{pattern}%";
                    break;

                default:
                    throw new NotSupportedException($"The string method '{methodCallExpression.Method.Name}' is not supported");
            }

            filter.Clause = $"{clause} {pattern.WrapUp('\'')}";

            var filterGroup = CreateCurrentFiltersGroup();
            filterGroup.AddFilter(filter);

            return methodCallExpression;
        }
    }
}