using DiCore.Lib.RuToken.RtAPIlt;
using System;
using System.ComponentModel;
using NLog;

namespace DiCore.Lib.RuToken.Test
{
    class Program
    {
        private static Logger logger;
        internal static ushort DirectoryName = 18;
        internal static ushort FileName = 104;

        internal static uint KeyLength = 0x10;

        internal static ushort VendorId = 0xFFFF;
        internal static ushort ProductId = 0x0001;

        static void Main(string[] args)
        {
            logger = LogManager.GetCurrentClassLogger();            
            var options = new Options();                        
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine(options.GetUsage());
                return;
            }

            if (options.UnIncutGetKey)
            {
                var key = BitConverter.ToString(GetKey("12345678"));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Rutoken key: {key}");
                Console.ResetColor();
            }

            if (options.EnumKeys)
            {
                EnumKeys();
            }
            else
            {
                if (options.KeyIndex == -1)
                {
                    WriteFailed("enter rutoken key index", 0);
                    Console.WriteLine(options.GetUsage());
                    return;
                }

                if (options.ReadKey)
                {
                    var key = ReadKey(options.KeyIndex);
                    logger.Info(key);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Rutoken key: {key}");
                    Console.ResetColor();
                }
            }
        }

        private static unsafe string ReadKey(int keyIndex)
        {
            ReadersEnum readersEnum;

            var result = RtAPIltImport.rtlOpenReadersEnum(out readersEnum);
            ReaderInf_A readerInfo = new ReaderInf_A();

            for (int i = 0; i < keyIndex + 1; i++)
            {
                result = RtAPIltImport.rtlGetNextReader_A(readersEnum, out readerInfo);
            }            

            result = RtAPIltImport.rtlCloseReadersEnum(readersEnum);

            Token token;
            result = RtAPIltImport.rtlBindToken_A(out token, readerInfo.ReaderName);
            string pinCode = "12345678";
            result = RtAPIltImport.rtlLoginToken_W(token, pinCode);

            var rootDir = new Dir();
            ushort ProductId = 1;
            ushort VendorId = 0xFFFF;
            result = RtAPIltImport.rtlOpenRootDir(out rootDir, token, VendorId, ProductId);

            ushort DirectoryName = 18;
            ushort FileName = 104;
            Dir dir;
            result = RtAPIltImport.rtlOpenDir(out dir, DirectoryName, rootDir);
            File file;
            result = RtAPIltImport.rtlOpenFile(out file, dir, FileName);

            uint KeyLength = 0x10;
            uint length = KeyLength;
            var key = new byte[length];
            fixed (byte* pBuff = &key[0])
            {
                byte* outBuff = pBuff;
                result = RtAPIltImport.rtlReadFile(file, 0, KeyLength, outBuff, &length);
            }            

            result = RtAPIltImport.rtlCloseFile(file);
            result = RtAPIltImport.rtlCloseDir(dir);

            result = RtAPIltImport.rtlLogoutToken(token);
            result = RtAPIltImport.rtlUnbindToken(token);

            return BitConverter.ToString(key);
        }       

        private static void EnumKeys()
        {
            ReadersEnum readersEnum;

            var result = RtAPIltImport.rtlOpenReadersEnum(out readersEnum);
            if (result != 0)
            {
                WriteFailed("rtlOpenReadersEnum failed.", result);

                return;
            }

            ReaderInf_A readerInfo;
            var i = 0;
            while ((result = RtAPIltImport.rtlGetNextReader_A(readersEnum, out readerInfo)) == 0)
            {
                WriteSuccess($"[{i++}] ReaderName: {readerInfo.ReaderName}; TokenPresent: {readerInfo.TokenPresent}");                
            }

            RtAPIltImport.rtlCloseReadersEnum(readersEnum);
        }

        private static void WriteFailed(string message, uint result)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{message} Error code: {result}");
            Console.ResetColor();
        }

        private static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static unsafe byte[] GetKey(string pinCode)
        {
            byte[] key;

            key = new byte[KeyLength];
            Error error = new Error();

            var TokenName = GetTokenName();

            Dir dir = new Dir() { Value = IntPtr.Zero };
            File file = new File() { Value = IntPtr.Zero };
            Token tokenHandle = new Token();
            try
            {
                error.Code = RtAPIltImport.rtlBindToken_A(out tokenHandle, TokenName);
                CheckError(error);
                error.Code = RtAPIltImport.rtlLoginToken_A(tokenHandle, pinCode);
                CheckError(error);


                var rootDir = new Dir();
                error.Code = RtAPIltImport.rtlOpenRootDir(out rootDir, tokenHandle, VendorId, ProductId);
                CheckError(error);

                error.Code = RtAPIltImport.rtlOpenDir(out dir, DirectoryName, rootDir);
                error.Code = RtAPIltImport.rtlOpenFile(out file, dir, FileName);

                uint length = KeyLength;
                fixed (byte* pBuff = &key[0])
                {
                    byte* outBuff = pBuff;
                    error.Code = RtAPIltImport.rtlReadFile(file, 0, KeyLength, outBuff, &length);
                }
                CheckError(error);                
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
            finally
            {
                CloseAll(file, dir, tokenHandle);
            }
            return key;
        }

        private static void CloseAll(File file, Dir dir, Token tokenHandle)
        {
            Error error = new Error();
            if (file.Value != IntPtr.Zero)
            {
                error.Code = RtAPIltImport.rtlCloseFile(file);
                CheckError(error);
            }
            if (file.Value != IntPtr.Zero)
            {
                error.Code = RtAPIltImport.rtlCloseDir(dir);
                CheckError(error);
            }
            error.Code = RtAPIltImport.rtlLogoutToken(tokenHandle);
            CheckError(error);
            error.Code = RtAPIltImport.rtlUnbindToken(tokenHandle);
            CheckError(error);
        }

        internal static void CheckError(Error error)
        {
            if (error.Severity == EnSeverity.Error)
            {
                throw new Win32Exception((int)error.Code);
            }
        }

        private static string GetTokenName()
        {
            Error error = new Error();
            ReadersEnum readers;
            error.Code = RtAPIltImport.rtlOpenReadersEnum(out readers);
            CheckError(error);
            ReaderInf_A readerInfo;
            error.Code = RtAPIltImport.rtlGetNextReader_A(readers, out readerInfo);
            CheckError(error);
            var tokenName = readerInfo.ReaderName;
            error.Code = RtAPIltImport.rtlCloseReadersEnum(readers);
            CheckError(error);
            return tokenName;
        }

        public struct Error
        {
            public uint Code { get; set; }

            public EnSeverity Severity
            {
                get
                {
                    var sev = (Code & 0xC0000000) >> 30;
                    switch (sev)
                    {
                        case 0: return EnSeverity.Success;
                        case 1: return EnSeverity.Informational;
                        case 2: return EnSeverity.Warning;
                        case 3: return EnSeverity.Error;
                    }
                    return EnSeverity.Error;
                }
            }

            public EnError ErrorCode
            {
                get
                {
                    return (EnError)(Code & 0x0000FFFF);
                }
            }

            public enum EnError
            {
                /// <summary>
                /// The command is finished successfully.
                /// </summary>
                RTSW_SUCCESS = 0x00009000,

                /// <summary>
                /// Unsuccessful authentication
                /// </summary>
                RTSW_UNSUCCESSFUL_AUTHENTICATION = 0x00006300,

                /// <summary>
                /// Rutoken memory state has not changed
                /// </summary>
                RTSW_STATE_UNCHANGED = 0x00006400,

                /// <summary>
                /// Rutoken memory state has changed
                /// </summary>
                RTSW_STATE_CHANGED = 0x00006500,

                /// <summary>
                /// Rutoken memory failure
                /// </summary>
                RTSW_MEMORY_FAILURE = 0x00006581,

                /// <summary>
                /// Wrong input/output buffer length
                /// </summary>
                RTSW_WRONG_LENGTH = 0x00006700,

                /// <summary>
                /// last command in the chain is expected 
                /// </summary>
                RTSW_NO_ENDING_COMMAND = 0x00006883,

                /// <summary>
                /// This command chain is not supported
                /// </summary>
                RTSW_COMMAND_CHAIN_NOT_SUPPORTED = 0x00006884,

                /// <summary>
                /// The command is not allowed with current access rights
                /// </summary>
                RTSW_NO_RIGHTS = 0x00006982,

                /// <summary>
                /// The RSF is blocked
                /// </summary>
                RTSW_DO_BLOCKED = 0x00006983,

                /// <summary>
                /// Reference data corrupted or invalid
                /// </summary>
                RTSW_REFERENCE_DATA_CORRUPTED = 0x00006984,

                /// <summary>
                /// Ineligible conditions for command execution
                /// </summary>
                RTSW_BAD_CONDITIONS = 0x00006985,

                /// <summary>
                /// No current EF 
                /// </summary>
                RTSW_NO_CURRENT_EF = 0x00006986,

                /// <summary>
                /// Command is not alloweded on this token/object lifecycle stage
                /// </summary>
                RTSW_WRONG_LIFECYCLE_STAGE = 0x00006989,

                /// <summary>
                /// Cryptographic key is not intended for this command
                /// </summary>
                RTSW_WRONG_CRYPTOGRAPHIC_KEY_USAGE = 0x00006994,

                /// <summary>
                /// Digital signature is invalid 
                /// </summary>
                RTSW_INVALID_SIGNATURE = 0x00006996,

                /// <summary>
                /// Wrong parameters (data) in the input buffer
                /// </summary>
                RTSW_WRONG_INPUT = 0x00006A80,

                /// <summary>
                /// Unsupported function 
                /// </summary>
                RTSW_UNSUPPORTED_FUNCTION = 0x00006A81,

                /// <summary>
                /// File/RSF can't be found
                /// </summary>
                RTSW_FILE_NOT_FOUND = 0x00006A82,

                /// <summary>
                /// Not enough space in EEPROM-memory of the token 
                /// </summary>
                RTSW_LOW_EEPROM = 0x00006A84,

                /// <summary>
                /// Wrong parameters (P1-P2) 
                /// </summary>
                RTSW_WRONG_PARAMETERS = 0x00006A86,

                /// <summary>
                /// Reference data not found 
                /// </summary>
                RTSW_REFERENCE_DATA_NOT_FOUND = 0x00006A88,

                /// <summary>
                /// File/RSF already exists 
                /// </summary>
                RTSW_FILE_ALREADY_EXISTS = 0x00006A89,

                /// <summary>
                /// Too big offset for EF 
                /// </summary>
                RTSW_WRONG_OFFSET = 0x00006B00,

                /// <summary>
                /// Wrong output buffer length
                /// </summary>
                RTSW_WRONG_OUTPUT_BUFFER_LENGTH = 0x00006C00,

                /// <summary>
                /// Incorrect or unsupported command
                /// </summary>
                RTSW_UNSUPPORTED_COMMAND = 0x00006D00,

                /// <summary>
                /// Rutoken exchange protocol is not supported by the USB driver
                /// </summary>
                RTSW_OLD_EXCHANGE_PROTOCOL = 0x00006F01,

                /// <summary>
                /// Error during cryptographic operation
                /// </summary>
                RTSW_CRYPTOGRAPHIC_ERROR = 0x00006F10,

                /// <summary>
                /// System memory is corrupted (invalid CRC)
                /// </summary>
                RTSW_SYSTEM_MEMORY_CORRUPTED = 0x00006F20,

                /// <summary>
                /// Rutoken exchange protocol error
                /// </summary>
                RTSW_PROTOCOL_ERROR = 0x00006F83,

                /// <summary>
                /// Rutoken is busy
                /// </summary>
                RTSW_RUTOKEN_BUSY = 0x00006F84,

                /// <summary>
                /// Too many files in current folder
                /// </summary>
                RTSW_TOO_MANY_FILES = 0x00006F85,

                /// <summary>
                /// Access level is not "Guest". RESET STATUS is required before authentication
                /// </summary>
                RTSW_NOT_GUEST_RIGHTS = 0x00006F86,

                /// <summary>
                /// Invalid object CRC
                /// </summary>
                RTSW_INVALID_OBJECT_CRC = 0x00006F87,

                /// <summary>
                /// Too many local logins
                /// </summary>
                RTSW_TOO_MANY_LOGINS = 0x00006F88,

                /// <summary>
                /// New PIN length is less than the minimum required for this CHV_RSF
                /// </summary>
                RTSW_PIN_TOO_SHORT = 0x00006F89,

                /// <summary>
                /// Transaction with other ID is expected (CrytptoPro FKC)
                /// </summary>
                RTSW_CRYPTO_PRO_FKC_TRANSACTION_WITH_ID_EXPECTED = 0x00006F90,

                /// <summary>
                /// EKE operation counter is up (CryptoPro FKC)
                /// </summary>
                RTSW_CRYPTO_PRO_FKC_EKE_OPERATION_COUNTER_IS_UP = 0x00006F91,

                /// <summary>
                /// Signature operation counter is up (CryptoPro FKC)
                /// </summary>
                RTSW_CRYPTO_PRO_FKC_SIGNATURE_OPERATION_COUNTER_IS_UP = 0x00006F92,

                /// <summary>
                /// DH operation counter is up (CryptoPro FKC)
                /// </summary>
                RTSW_CRYPTO_PRO_FKC_DH_OPERATION_COUNTER_IS_UP = 0x00006F93,

                /// <summary>
                /// Failed operation counter is up (CryptoPro FKC).
                /// </summary>
                RTSW_CRYPTO_PRO_FKC_FAILED_OPERATION_COUNTER_IS_UP = 0x00006F94,

                /// <summary>
                /// Failed consequent operation counter is up (EKE PIN entry)(CryptoPro FKC).
                /// </summary>
                RTSW_CRYPTO_PRO_FKC_FAILED_CONSEQUENT_OPERATION_COUNTER_IS_UP = 0x00006F95,

                /// <summary>
                /// Bad operations of cryptographic algorithm or RNG detected in work process (not while diagnosing) 
                /// </summary>
                RTSW_CRYPTO_ALGORITHM_OR_RNG_CORRUPTED = 0x00006FA0,

                /// <summary>
                /// rtAPIi Context is not initiallized
                /// </summary>
                RTSW_RTAPII_CONTEXT_NOT_INITILIZED = 0x0000F000,

                /// <summary>
                /// rtAPIi Context already initialized
                /// </summary>
                RTSW_RTAPII_CONTEXT_ALREADY_INITILIZED = 0x0000F001,

                /// <summary>
                /// Argument is invalid
                /// </summary>
                RTSW_INVALID_ARGUMENT = 0x0000F002,

                /// <summary>
                /// Invalid value
                /// </summary>
                RTSW_INVALID_VALUE = 0x0000F003,

                /// <summary>
                /// The data area passed to a system call is too small
                /// </summary>
                ERROR_INSUFFICIENT_BUFFER = 0x0000007a
            }
        }

        public enum EnSeverity
        {
            Success,
            Informational,
            Warning,
            Error
        }
    }
}
