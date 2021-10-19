using System;

namespace DiCore.Lib.NDT.Types
{
    public interface IDiagDataReader : IDisposable
    {
        bool Open(int fileNumber);
        bool ContainsScan(int scan);
        UIntPtr GetDataPointer(int scan);
    }
}