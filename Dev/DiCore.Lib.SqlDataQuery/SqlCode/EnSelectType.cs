namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// 
    /// </summary>
    public enum EnSelectType
    {
        /// <summary>
        /// Никогда не включается в select часть запроса
        /// </summary>
        Inner,
        /// <summary>
        /// Включается в select часть запроса в зависимости от видимости
        /// </summary>
        InSelect,
        /// <summary>
        /// Включается в select часть запроса всегда
        /// </summary>
        InSelectRequired
    }
}