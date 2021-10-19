using System;
using NLog;

namespace DiCore.Lib.SmallLogger.NLog
{
    public class SmallLoggerNlog:ISmallLogger
    {
        static SmallLoggerNlog()
        {
            StaticLoggerFactory.CreateLogger = () => new SmallLoggerNlog();
            StaticLoggerFactory.CreateLoggerByName = name => new SmallLoggerNlog(name);
            StaticLoggerFactory.CreateLoggerByType = t => new SmallLoggerNlog(t);
        }
        private readonly Logger logger;

        public string Name => logger.Name;

        public SmallLoggerNlog()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        public SmallLoggerNlog(string name)
        {
            logger = LogManager.GetLogger(name);
        }

        public SmallLoggerNlog(Type t)
        {
            logger = LogManager.GetLogger(t.FullName);
        }

        public void Trace(string message)
        {
            logger.Trace(message);
        }

        public void Trace<T>(T obj)
        {
            logger.Trace(obj);
        }

        public void Debug(string message)
        {
            logger.Debug(message);
        }

        public void Debug<T>(T obj)
        {
            logger.Debug(obj);
        }

        public void Info(string message)
        {
            logger.Info(message);
        }

        public void Info<T>(T obj)
        {
            logger.Info(obj);
        }

        public void Warn(string message)
        {
            throw new NotImplementedException();
        }

        public void Warn<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public void Error(string message)
        {
            throw new NotImplementedException();
        }

        public void Error<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string message)
        {
            throw new NotImplementedException();
        }

        public void Fatal<T>(T obj)
        {
            throw new NotImplementedException();
        }
    }
}
