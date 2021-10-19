namespace DiCore.Lib.SqlDataQuery
{
    public struct ColumnConfig
    {
        public ColumnConfig(bool present, bool visible)
        {
            Present = present;
            Visible = visible;
        }
        public bool Present { get; set; }
        public bool Visible { get; set; }
    }
}
