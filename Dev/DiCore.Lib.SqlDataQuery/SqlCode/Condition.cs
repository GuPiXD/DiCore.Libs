namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Условие фильтрации БД
    /// </summary>
    public class Condition: BaseCondition
    {
        public string ColumnAlias { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }

        public override string ToString(bool single)
        {
            return $"{ExternalOperator} {ColumnAlias} {Operator} {Value}";
        }
    }
}