using System;

namespace DiCore.Lib.SmallLogger
{
    public interface ISmallLogger
    {
        string Name { get; }
        void Trace(string message);
        void Trace<T>(T obj);
        void Debug(string message);
        void Debug<T>(T obj);
        void Info(string message);
        void Info<T>(T obj);
        void Warn(string message);
        void Warn<T>(T obj);
        void Error(string message);
        void Error<T>(T obj);
        void Fatal(string message);
        void Fatal<T>(T obj);

    }
}