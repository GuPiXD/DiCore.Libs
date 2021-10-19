namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Базовый объект столбца БД
    /// </summary>
    public abstract class BaseColumn
    {
        public string Alias { get; protected set; }
        public string TableAlias { get; protected set; }
        public EnDbDataType Type { get; protected set; } = EnDbDataType.Text;
        public EnSelectType SelectType { get; protected set; } = EnSelectType.InSelect;        
    }
}