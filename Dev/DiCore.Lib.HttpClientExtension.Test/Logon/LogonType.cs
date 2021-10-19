namespace DiCore.Lib.RestClient.TestCore.Logon
{
    public enum LogonType
    {
        LOGON32_LOGON_INTERACTIVE = 2,
        LOGON32_LOGON_NETWORK = 3,
        LOGON32_LOGON_BATCH = 4,
        LOGON32_LOGON_SERVICE = 5,
        LOGON32_LOGON_UNLOCK = 7,
        LOGON32_LOGON_NETWORK_CLEARTEXT = 8, // Win2K or higher
        LOGON32_LOGON_NEW_CREDENTIALS = 9 // Win2K or higher
    }
}