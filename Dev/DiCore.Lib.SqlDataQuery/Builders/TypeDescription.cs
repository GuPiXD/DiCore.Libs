using System;

namespace DiCore.Lib.SqlDataQuery.Builders
{
    public class TypeDescription
    {
        public Type Type { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public bool Json { get; set; }

        public static readonly TypeDescription Empty = new TypeDescription();

        public static TypeDescription FromType(Type source)
        {
            return new TypeDescription {Type = source, TableName = source.GetTableName()};
        }
    }
}