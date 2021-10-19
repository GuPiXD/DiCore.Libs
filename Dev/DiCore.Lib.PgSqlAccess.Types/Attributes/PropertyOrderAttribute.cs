using System;

namespace DiCore.Lib.PgSqlAccess.Types.Attributes
{
    /// <summary>
    /// Аттрибут, устанавливающий порядковый номер
    /// </summary>
    public class PropertyOrderAttribute : Attribute
    {
        public PropertyOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; set; }
    }
}
