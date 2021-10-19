namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    public unsafe struct CDPASensorItem
    {
        public ushort RayCount;
        public CDPASensorData* Data;
    }
}