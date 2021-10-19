using System;

namespace DiCore.Lib.SmallLogger
{
    public static class StaticLoggerFactory
    {
        private static Func<ISmallLogger> createLogger;
        private static Func<Type, ISmallLogger> createLoggerByType;
        private static Func<string, ISmallLogger> createLoggerByName;

        public static Func<ISmallLogger> CreateLogger
        {
            set { createLogger = value; }
        }

        public static Func<Type, ISmallLogger> CreateLoggerByType
        {
            set { createLoggerByType = value; }
        }

        public static Func<string, ISmallLogger> CreateLoggerByName
        {
            set { createLoggerByName = value; }
        }

        public static ISmallLogger Create()
        {
            return createLogger?.Invoke() ?? new NullLogger();
        }

        public static ISmallLogger Create(string name)
        {
            return createLoggerByName?.Invoke(name) ?? new NullLogger();
        }

        public static ISmallLogger Create(Type t)
        {
            return createLoggerByType?.Invoke(t) ?? new NullLogger();
        }
    }
}
