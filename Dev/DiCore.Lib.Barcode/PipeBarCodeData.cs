namespace DiCore.Lib.Barcode
{
    public struct PipeBarCodeData
    {
        public PipeBarCodeData(string factoryCode, string partNumber, string pipeNumber)
        {
            FactoryCode = factoryCode;
            PartNumber = partNumber;
            PipeNumber = pipeNumber;
        }

        public string FactoryCode { get; }
        public string PartNumber { get; }
        public string PipeNumber { get; }

        public override string ToString()
        {
            return FactoryCode + PartNumber + PipeNumber;
        }
    }
}
