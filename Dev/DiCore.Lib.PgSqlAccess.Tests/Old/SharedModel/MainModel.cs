namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    public class MainModel
    {
        public int Id { get; set; }

        public MainJsonbValue JsonbValue { get; set; }

        public class MainJsonbValue
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}