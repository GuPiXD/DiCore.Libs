namespace DiCore.Lib.RestClient.TestCore
{
    public struct Configs
    {
        public const int DefaultTestApiPort = 7001;
        public const int WindowsAuthTestApiPort = 7002;
        public const int WindowsImpersonateTestMiddleApiPort = 7003;
        public const int WindowsImpersonateTestRemoteApiPort = 7004;
        public const string ImpersonateUserDomain = null;// null - локальный пользователь
        public const string ImpersonateUserLogin = "testUser";// Необходимо создать локального пользователя с таким именем
        public const string ImpersonateUserPassword = "123qweQWE";// Необходимо создать локального пользователя с таким паролем
    }
}