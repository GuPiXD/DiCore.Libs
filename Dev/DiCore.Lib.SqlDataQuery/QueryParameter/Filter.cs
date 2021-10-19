namespace DiCore.Lib.SqlDataQuery.QueryParameter
{
    /// <summary>
    /// Фильтр
    /// </summary>
    public class Filter
    {
        public string Field { get; set; }
        public EnFilterOperation Operator { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Field} {Operator.ToSql()} {Value}";
        }
    }

    public class CustomFilter : Filter
    {
        public string Clause { get; set; }
        public override string ToString()
        {
            return Clause ?? base.ToString();
        }
    }
}