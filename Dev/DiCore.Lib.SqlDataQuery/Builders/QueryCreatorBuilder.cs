using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DiCore.Lib.SqlDataQuery.QueryParameter;
using DiCore.Lib.SqlDataQuery.SqlCode;
using DiCore.Lib.SqlDataQuery.Utils;

namespace DiCore.Lib.SqlDataQuery.Builders
{
    public static class QueryCreatorBuilder
    {
        public static QueryCreatorConfig<T> Create<T>(string schema, string mainTableName = "")
        {
            var qcc = new QueryCreatorConfig<T>(schema);

            var typeDescription = TypeDescription.FromType(typeof(T));
            if (!String.IsNullOrEmpty(mainTableName))
            {
                typeDescription.TableName = mainTableName;
            }

            qcc.TypeDescriptions.Add(typeof(T), typeDescription);

            qcc.QueryDescription.AddTable(schema, mainTableName,
                QueryBuilderHelper.GetTableAlias(mainTableName), true);

            return qcc;
        }

        public static QueryCreatorConfig<T> MapJson<T, TValue>(this QueryCreatorConfig<T> config,
            Expression<Func<T, TValue>> mapField)
        {
            var type = mapField.GetMemberInfo().Item1.GetMemberType();
            if (!config.TypeDescriptions.TryGetValue(type, out var typeDescription))
            {
                typeDescription = TypeDescription.FromType(typeof(T));
            }
            typeDescription.Json = true;

            config.TypeDescriptions[type] = typeDescription;

            config.Map(mapField);

            return config;
        }

        public static QueryCreatorConfig<T> AddTypeDescription<T, TValue>(this QueryCreatorConfig<T> config,
            Expression<Func<T, TValue>> mapField, TypeDescription description)
        {
            var type = mapField.GetMemberInfo().Item1.GetMemberType();
            config.TypeDescriptions[type] = description;

            return config;
        }

        public static QueryCreatorConfig<T> MapEntity<T>(this QueryCreatorConfig<T> config)
        {            
            return config.MapEntity(null);
        }

        public static QueryCreatorConfig<T> Map<T>(this QueryCreatorConfig<T> config)
        {
            return config.Map(x => x);
        }
        
        public static QueryCreatorConfig<T> Map<T, TValue>(this QueryCreatorConfig<T> config, Expression<Func<T, TValue>> mapField, 
            string name = "", EnSelectType selectType = EnSelectType.InSelect, bool distinct = false)
        {
            var mapFieldInfo = mapField.GetMemberInfo();

            // Случай мапинга всего объекта x.Map(x => x)
            if (mapFieldInfo.Item1 == null)
            {
                return config.MapEntity(null, String.Empty, true);
            }

            var columnInfo = mapFieldInfo.Item1;
            var complexType = !columnInfo.GetMemberType().IsSystemType();
            var tableName = complexType ? name : String.Empty;
            var columnName = complexType ? String.Empty : name;

            return config.Map(columnInfo, tableName, columnName, selectType, distinct);
        }

        /// <summary>
        /// Маппинг полей соединения таблиц. SQL конструкция JOIN ON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="config"></param>
        /// <param name="joinField">Поле UID таблицы связи</param>
        /// <param name="joinType">Тип связи (по умолчанию INNER JOIN)</param>
        /// <param name="name">Наименование поля связи в главной таблице (по умолчанию JoinTableNameId)</param>
        /// <returns></returns>
        public static QueryCreatorConfig<T> MapJoin<T, TValue>(this QueryCreatorConfig<T> config,
            Expression<Func<T, TValue>> joinField, Expression<Func<T, TValue>> mainField = null, EnJoinType joinType = EnJoinType.Inner)
        {
            var joinColumnInfo = joinField.GetMemberInfo();
            var mainColumnInfo = mainField?.GetMemberInfo();

            return config.MapJoin(joinColumnInfo, mainColumnInfo?.Item1, joinType);
        }  

        public static QueryCreator Build<T>(this QueryCreatorConfig<T> config)
        {
            return new QueryCreator(config.QueryDescription);
        }

        public static string Build<T>(this QueryCreatorConfig<T> config, QueryParameters parameters)
        {
            return new QueryCreator(config.QueryDescription).Create(parameters);
        }

        public static string Build<T>(this QueryCreatorConfig<T> config, QueryParametersConfig<T> parametersConfig)
        {
            return new QueryCreator(config.QueryDescription).Create(parametersConfig.Build());
        }        

        private static QueryCreatorConfig<T> Map<T>(this QueryCreatorConfig<T> config, MemberInfo field,
            string tableAlias, string columnAlias, EnSelectType selectType, bool distinct)
        {
            var type = field.GetMemberType();

            if (!config.TypeDescriptions.TryGetValue(type, out var typeDescription))
            {
                typeDescription = TypeDescription.Empty;
            }

            if (type.IsSystemType() || typeDescription.Json)
            {
                var columnInfo = field;
                var tableName = config.GetTableName(columnInfo.DeclaringType);
                tableAlias = String.IsNullOrEmpty(tableAlias)
                    ? QueryBuilderHelper.GetTableAlias(tableName)
                    : tableAlias;
                var table = config.QueryDescription.GetTableByName(tableName);
                var nestedType = columnInfo.DeclaringType != typeof(T);

                if (table == null)
                {
                    table = config.QueryDescription.AddTable(config.Schema, tableName, tableAlias, !nestedType);
                }

                var columnName = columnInfo.GetColumnName();
                columnAlias = String.IsNullOrEmpty(columnAlias)
                    ? nestedType
                        ? QueryBuilderHelper.GetJointColumnAlias(tableAlias, columnName)
                        : QueryBuilderHelper.GetColumnAlias(columnName)
                    : columnAlias;

                if (table.GetColumn(columnAlias) != null)
                {
                    Debug.WriteLine($"WARNING!!! Detect duplicate column {columnName} mapping as alias {columnAlias}");
                    return config;
                }

                if (distinct)
                {
                    var code = $"DISTINCT {tableName.Quotes()}.{columnName.Quotes()}";
                    table.AddCustomColumn(columnAlias, code, tableAlias);
                }
                else
                {
                    table.AddColumn(columnName, columnAlias, config.ToEnDbDataType(columnInfo), selectType);
                }
            }
            else
            {
                var properties = type.GetBindableProperties();
                tableAlias = String.IsNullOrEmpty(tableAlias)
                    ? QueryBuilderHelper.GetTableAlias(field.Name)
                    : $"{tableAlias}#{QueryBuilderHelper.GetTableAlias(field.Name)}";

                foreach (var property in properties)
                {
                    config.Map(property, tableAlias, columnAlias, selectType, distinct);
                }
            }

            return config;
        }        
        private static QueryCreatorConfig<T> MapJoin<T>(this QueryCreatorConfig<T> config, Tuple<MemberInfo, MemberInfo> jointField, MemberInfo mainField, EnJoinType joinType)
        {
            var joinColumnInfo = jointField.Item1;
            var jointColumnOwnerInfo = jointField.Item2;
            var mainColumnInfo = mainField ?? jointColumnOwnerInfo;
            var mainColumnName = mainField?.Name;

            if (String.IsNullOrEmpty(mainColumnName))
            {
                var mcn = QueryBuilderHelper.GetJointIdColumnAlias(jointColumnOwnerInfo.Name);
                var props = jointColumnOwnerInfo.DeclaringType.GetBindableProperties();
                if (props.All(x => x.Name != mcn) && jointColumnOwnerInfo is PropertyInfo pi)
                {
                    var pt = pi.GetMemberType();
                    mcn = QueryBuilderHelper.GetJointIdColumnAlias(pt.GetTableName());
                }

                mainColumnName = mcn;
            }

            var mainTableName = config.GetTableName(mainColumnInfo.DeclaringType);
            var columnInfo = joinColumnInfo;
            var joinTableName = config.GetTableName(columnInfo.DeclaringType);
            var joinColumnName = columnInfo.GetColumnName();

            var mainTable = config.QueryDescription.GetTableByName(mainTableName);
            var joinTable = config.QueryDescription.GetTableByName(joinTableName);

            if (config.QueryDescription.Joins.Any(o =>
                o.Table1Alias == mainTable.Alias && o.Table2Alias == joinTable.Alias ||
                o.Table1Alias == joinTable.Alias && o.Table2Alias == mainTable.Alias)) return config;

            var mainColumn = mainTable.GetColumnByName(mainColumnName);
            if (mainColumn == null)
            {
                var mainColumnNameAlias = QueryBuilderHelper.GetColumnAlias(mainColumnName);
                mainColumn = mainTable.AddColumn(mainColumnName, mainColumnNameAlias, config.ToEnDbDataType(columnInfo), EnSelectType.Inner);
            }

            var joinColumn = joinTable.GetColumnByName(joinColumnName);
            if (joinColumn == null)
            {
                var joinColumnAlias = QueryBuilderHelper.GetJointColumnAlias(joinTableName, joinColumnName);
                joinColumn = joinTable.AddColumn(joinColumnName, joinColumnAlias, config.ToEnDbDataType(columnInfo));
            }

            config.QueryDescription.AddJoin(mainTable.Alias, mainColumn.Alias, joinTable.Alias, joinColumn.Alias,
                joinType);

            return config;
        }
        private static QueryCreatorConfig<T> MapEntity<T>(this QueryCreatorConfig<T> config, PropertyInfo ownerInfo, string tableAlias = "", bool withoutJoin = false)
        {
            var propertyType = typeof(T);
            if (ownerInfo != null)
            {
                var mt = ownerInfo.GetMemberType();
                //Исключение циклической ссылки для случая когда вложенный объект ссылается на головной
                if (typeof(T).IsSubclassOf(mt)) return config;

                propertyType = mt;
            }

            var properties = propertyType.GetBindableProperties();

            foreach (var info in properties)
            {
                var pt = info.GetMemberType();

                if (!config.TypeDescriptions.TryGetValue(pt, out var typeDescription))
                {
                    typeDescription = TypeDescription.Empty;
                }

                if (pt.IsSystemType() || typeDescription.Json)
                {
                    config.Map(info, tableAlias, String.Empty, EnSelectType.InSelect, false);
                }
                else
                {
                    config.MapEntity(info,
                        String.IsNullOrEmpty(tableAlias) || tableAlias == QueryBuilderHelper.GetTableAlias(config.QueryDescription.MainTable.Name)
                            ? QueryBuilderHelper.GetTableAlias(info.Name)
                            : String.Join("#", tableAlias, QueryBuilderHelper.GetTableAlias(info.Name)), withoutJoin);


                    if (withoutJoin) continue;

                    if (info.PropertyType.IsArray || typeof(IEnumerable).IsAssignableFrom(info.PropertyType))
                    {
                        //config.MapJoin(new Tuple<MemberInfo, MemberInfo>(pt.GetProperty("Id"), info), propertyType.GetProperty("Id"),
                        //    EnJoinType.Left);
                    }
                    else
                    {
                        config.MapJoin(new Tuple<MemberInfo, MemberInfo>(pt.GetProperty("Id"), info), null,
                            EnJoinType.Inner);
                    }
                }
            }            

            return config;
        }
    }
}
