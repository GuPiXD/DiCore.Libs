namespace DiCore.Lib.SmallLogger
{
    public class NullLogger:ISmallLogger
    {
        public string Name => "";
        public void Trace(string message)
        {
        }

        public void Trace<T>(T obj)
        {
        }

        public void Debug(string message)
        {
        }

        public void Debug<T>(T obj)
        {
        }

        public void Info(string message)
        {
        }

        public void Info<T>(T obj)
        {
        }

        public void Warn(string message)
        {
        }

        public void Warn<T>(T obj)
        {
        }

        public void Error(string message)
        {
        }

        public void Error<T>(T obj)
        {
        }

        public void Fatal(string message)
        {
        }

        public void Fatal<T>(T obj)
        {
        }
    }
}