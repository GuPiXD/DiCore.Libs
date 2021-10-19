using Diascan.Utils.DataBuffers;

namespace DiCore.Lib.NDT.Types
{
    public class ReadRawDataArgs<T> where T: struct
    {
        public BufferMemoryManager<T> MemoryManager { get; private set; }
        public IMatrixBuffer RawBuffer { get; private set; }
        public int EchoCount { get; set; }

        public ReadRawDataArgs(BufferMemoryManager<T> memoryManager, IMatrixBuffer rawBuffer)
        {
            MemoryManager = memoryManager;
            RawBuffer = rawBuffer;
        }
    }
}
