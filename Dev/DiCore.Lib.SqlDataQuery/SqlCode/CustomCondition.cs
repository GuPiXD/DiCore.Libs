namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Пользовательское условие
    /// </summary>
    public class CustomCondition : BaseCondition
    {
        public CustomCondition(EnConditionType externalOperator, string sqlCode)
        {
            SqlCode = sqlCode;
            ExternalOperator = externalOperator;
        }

        /// <summary>
        /// Sql код условия
        /// </summary>
        public string SqlCode { get; set; }

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToString(bool single)
        {
            return single ? SqlCode : $"{ExternalOperator} {SqlCode}";
        }
    }
}
