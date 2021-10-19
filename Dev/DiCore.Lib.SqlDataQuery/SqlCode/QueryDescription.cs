using System.Collections.Generic;
using System.Linq;
using DiCore.Lib.SqlDataQuery.Utils;

namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Запрос к БД (создание конфигурации запроса)
    /// </summary>
    public class QueryDescription
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public QueryDescription()
        {
            Tables = new Dictionary<string, Table>();
            Joins = new List<Join>();
            FilterColumnMappings = new Dictionary<string, FilterColumnMapping>();
            Conditions = new List<BaseCondition>();
        }

        internal IDictionary<string, Table> Tables { get; set; }
        internal Table MainTable { get; set; }
        internal IList<Join> Joins { get; set; }
        internal IDictionary<string, FilterColumnMapping> FilterColumnMappings { get; set; }
        internal IList<BaseCondition> Conditions { get; set; }

        internal Tree<string> TableTree { get; set; }

        /// <summary>
        /// Получение базового объекта столбца по псевдониму таблицы и псевдониму столбца.
        /// </summary>
        /// <param name="tableAlias">Псевдоним таблицы</param>
        /// <param name="columnAlias">Псевдоним столбца</param>
        /// <returns>Базовый объект столбца</returns>
        public BaseColumn GetColumn(string tableAlias, string columnAlias)
        {
            var table = Tables[tableAlias];
            return table?.GetColumn(columnAlias);
        }

        /// <summary>
        /// Получение базового объекта столбца по псевдониму.
        /// Поиск произодится по всем таблицам
        /// </summary>
        /// <param name="columnAlias">Псевдоним столбца</param>
        /// <returns>Базовый объект столбца</returns>
        public BaseColumn GetColumn(string columnAlias)
        {
            foreach (var table in Tables.Values)
            {
                var col = GetColumn(table.Alias, columnAlias);
                if (col != null)
                    return col;
            }
            return null;
        }

        /// <summary>
        /// Получение объекта таблицы по псевдониму
        /// </summary>
        /// <param name="alias">Псевдоним таблицы</param>
        /// <returns>Объект таблицы</returns>
        public Table GetTable(string alias)
        {
            Tables.TryGetValue(alias, out var table);

            return table;
        }

        /// <summary>
        /// Получение объекта таблицы по имени
        /// </summary>
        /// <param name="name">Наименование таблицы</param>
        /// <returns>Объект таблицы</returns>
        public Table GetTableByName(string name)
        {
            return Tables.Values.FirstOrDefault(table => table.Name == name);
        }

        /// <summary>
        /// Добавление таблицы
        /// </summary>
        /// <param name="schema">Имя схемы</param>
        /// <param name="name">Имя таблицы</param>
        /// <param name="alias">Псевдоним таблицы</param>
        /// <param name="isMain">Признак основной таблицы запроса (по умолчанию false)
        /// Для основной таблицы запроса должен быть true</param>
        /// <returns>Объект таблицы</returns>
        public Table AddTable(string schema, string name, string alias, bool isMain = false)
        {
            var table = new Table(schema, name, alias);
            Tables.Add(alias, table);
            if (isMain)
                MainTable = table;
            return table;
        }

        /// <summary>
        /// Добавление таблицы
        /// (псевдониму таблицы присваивается значение имени таблицы в нижнем регистре)
        /// </summary>
        /// <param name="schema">Имя схемы</param>
        /// <param name="name">Имя таблицы</param>
        /// <param name="isMain">Признак основной таблицы запроса (по умолчанию false)
        /// Для основной таблицы запроса должен быть true</param>
        /// <returns>Объект таблицы</returns>
        public Table AddTable(string schema, string name, bool isMain = false)
        {
            return AddTable(schema, name, name.ToLower(), isMain);
        }

        /// <summary>
        /// Добавление объединения таблиц
        /// </summary>
        /// <param name="table1Alias">Псевдоним первой таблицы</param>
        /// <param name="column1Alias">Псевдоним столбца первой таблицы</param>
        /// <param name="table2Alias">Псевдоним второй таблицы</param>
        /// <param name="column2Alias">Псевдоним столбца второй таблицы</param>
        /// <param name="joinType">Тип объединения (по умолчанию Inner)</param>
        /// <returns>Объект объединения таблиц</returns>
        public Join AddJoin(string table1Alias, string column1Alias, string table2Alias, string column2Alias, EnJoinType joinType = EnJoinType.Inner)
        {
            var join = new Join(table1Alias, column1Alias, table2Alias, column2Alias, joinType);
            Joins.Add(join);
            return join;
        }

        /// <summary>
        /// Добавление сопоставления столбцов для фильтрации
        /// </summary>
        /// <param name="tableAlias">Псевдоним таблицы</param>
        /// <param name="columnAlias">Псевдоним отображаемого столбца</param>
        /// <param name="filterTableAlias">Псевдоним таблицы</param>
        /// <param name="filterColumnAlias">Псевдоним столбца, по которому производится фильтрация</param>
        /// <returns>Объект сопоставления столбцов</returns>
        public FilterColumnMapping AddFilterColumnMapping(string tableAlias, string columnAlias, string filterTableAlias, string filterColumnAlias)
        {
            var filterColumnMapping = new FilterColumnMapping(tableAlias, columnAlias, filterTableAlias, filterColumnAlias);
            FilterColumnMappings.Add(columnAlias, filterColumnMapping);
                return filterColumnMapping;
        }

        /// <summary>
        /// Добавление сопоставления столбцов для фильтрации
        /// (при сопоставлении в пределах одной таблицы)
        /// </summary>
        /// <param name="tableAlias">Псевдоним таблицы</param>
        /// <param name="columnAlias">Псевдоним отображаемого столбца</param>
        /// <param name="filterColumnAlias">Псевдоним столбца, по которому производится фильтрация</param>
        /// <returns>Объект сопоставления столбцов</returns>
        public FilterColumnMapping AddFilterColumnMapping(string tableAlias, string columnAlias, string filterColumnAlias)
        {
            return AddFilterColumnMapping(tableAlias, columnAlias, tableAlias, filterColumnAlias);
        }

        /// <summary>
        /// Добавление пользовательского условия
        /// </summary>
        /// <param name="externalOperator">Оператор условия, с котором оно присоединяется к общему условию</param>
        /// <param name="sqlCode">Код условия</param>
        /// <returns>Объект пользовательского условия</returns>
        public CustomCondition AddCustomCondition(EnConditionType externalOperator, string sqlCode)
        {
            var condition = new CustomCondition(externalOperator, sqlCode);
            Conditions.Add(condition);
            return condition;
        }

        public void Prepare()
        {
            TableTree = new Tree<string>();

            TableTree.SetRoot(new TreeNode<string>(MainTable.Alias, null));
            GetChilds(TableTree, TableTree.Root, null);
        }

        private void GetChilds(Tree<string> tree, TreeNode<string> treeNode, TreeNode<string> parent)
        {
            /*
            var childs = (
                from @join in Joins
                where (@join.Table1Alias == treeNode.Node || @join.Table2Alias == treeNode.Node) && ( 
                        @join.Table1Alias != parent.Node && @join.Table2Alias != parent.Node))
                select new TreeNode<string>(@join.Table1Alias == treeNode.Node ? @join.Table2Alias : @join.Column2Alias, parent))
                .ToList();

            tree.AddChilds(treeNode, childs);
            if (childs.Count == 0)
                return;

            foreach (var child in childs)
            {
                GetChilds(tree, child, treeNode);
            }
            */
        }

        internal List<string> GetTablePath(string tableAlias)
        {
            return TableTree.GetPath(tableAlias);
        }
    }
}