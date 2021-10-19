using System.Collections.Generic;
using System.Linq;

namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Таблица БД
    /// </summary>
    public abstract class BaseTable
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="alias">Псевдоним</param>
        protected BaseTable(string alias)
        {
            Columns = new Dictionary<string, BaseColumn>();
            Alias = alias;
        }

        public string Alias { get; protected set; }
        internal IDictionary<string, BaseColumn> Columns { get; set; }

        /// <summary>
        /// Получение базового объекта столбца по псевдониму
        /// </summary>
        /// <param name="alias">Псевдоним столбца</param>
        /// <returns>Базовый объект столбца</returns>
        public BaseColumn GetColumn(string alias)
        {
            Columns.TryGetValue(alias, out var column);
            
            return column;
        }

        /// <summary>
        /// Получение базового объекта столбца по имени
        /// </summary>
        /// <param name="name">Наименование столбца</param>
        /// <returns>Базовый объект столбца</returns>
        public BaseColumn GetColumnByName(string name)
        {
            return Columns.Values.OfType<Column>().FirstOrDefault(column => column.Name == name);
        }

        /// <summary>
        /// Добавление столбца
        /// </summary>
        /// <param name="name">Имя столбца</param>
        /// <param name="alias">Псевдоним столбца</param>
        /// <param name="type">Тип данных столбца (по умолчанию EnDbDataType.Text)</param>
        /// <param name="selectType">Включение столбца в select части запроса</param>
        /// <returns>Объект столбца</returns>
        public Column AddColumn(string name, string alias, EnDbDataType type = EnDbDataType.Text, EnSelectType selectType = EnSelectType.InSelect)
        {
            var column = new Column(Alias, name, alias, type, selectType);
            Columns.Add(alias, column);
            return column;
        }

        /// <summary>
        /// Добавление столбца
        /// </summary>
        /// <param name="name">Имя столбца</param>
        /// <param name="type">Тип данных столбца (по умолчанию EnDbDataType.Text)</param>
        /// <param name="selectType">Включение столбца в select части запроса</param>
        /// <returns>Объект столбца</returns>
        public Column AddColumn(string name, EnDbDataType type = EnDbDataType.Text, EnSelectType selectType = EnSelectType.InSelect)
        {
            return AddColumn(name, name, type, selectType);
        }

        /// <summary>
        /// Добавление пользовательского столбца
        /// </summary>
        /// <param name="alias">Псевдоним столбца</param>
        /// <param name="code">Sql код</param>
        /// <param name="tableAlias">Псевдоним таблицы, которая должна присутствовать в разделе FROM для этого столбца</param>
        /// <returns>Объект пользовательского столбца</returns>
        public CustomColumn AddCustomColumn(string alias, string code, string tableAlias = null)
        {
            var column = new CustomColumn(alias, code, tableAlias);
            Columns.Add(alias, column);
            return column;
        }
    }
}