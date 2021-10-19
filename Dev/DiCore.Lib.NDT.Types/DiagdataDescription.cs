using Diascan.NDT.Enums;

namespace DiCore.Lib.NDT.Types
{
    public class DiagdataDescription
    {
        public string[] PointerFileExt { get; private set; }
        public string IndexFileExt { get; private set; }
        public string DataFileExt { get; private set; }
        public ushort TypeCode { get; private set; }
        public ushort ReverseTypeCode { get; private set; }
        public string DataDirSuffix { get; set; }
        public DataType DataType { get; set; }

        public DiagdataDescription(DataType dataType, string pointerFileExt, string indexFileExt, string dataFileExt, ushort typeCode, ushort reverseTypeCode, string dataDirSuffix)
        {
            PointerFileExt = new[] { pointerFileExt };
            IndexFileExt = indexFileExt;
            DataFileExt = dataFileExt;
            TypeCode = typeCode;
            ReverseTypeCode = reverseTypeCode;
            DataDirSuffix = dataDirSuffix;
            DataType = dataType;
        }

        public DiagdataDescription(DataType dataType, string[] pointerFileExt, string indexFileExt, string dataFileExt, ushort typeCode, ushort reverseTypeCode, string dataDirSuffix)
        {
            PointerFileExt =  pointerFileExt;
            IndexFileExt = indexFileExt;
            DataFileExt = dataFileExt;
            TypeCode = typeCode;
            ReverseTypeCode = reverseTypeCode;
            DataDirSuffix = dataDirSuffix;
            DataType = dataType;
        }
    }
}
