using System;
using System.Runtime.InteropServices;

namespace DiCore.Lib.RuToken.RtAPIlt
{        
    [StructLayout(LayoutKind.Sequential)]
    public struct ReadersEnum
    {
        public IntPtr Value;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Token
    {
        public IntPtr Value;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct DirEnum
    {
        public IntPtr Value;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Dir
    {
        public IntPtr Value;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct File
    {
        public IntPtr Value;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct FilesEnum
    {
        public IntPtr Value;
    };


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ReaderInf_A
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string ReaderName;
        [MarshalAs(UnmanagedType.Bool)]
        public bool TokenPresent;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ReaderInf_W
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string ReaderName;
        [MarshalAs(UnmanagedType.Bool)]
        public bool TokenPresent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DirInfo
    {
        //Directory name
        public ushort DirName;
        // not used (ever FALSE)
        [MarshalAs(UnmanagedType.Bool)]
        public bool IsPrivate;
        // not used (ever 0)
        public ushort Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FileInfo
    {
        // File name: 
        public ushort FileName;
        // Is private:
        [MarshalAs(UnmanagedType.Bool)]
        public bool IsPrivate;
        // File size:
        public ushort Size;
    }


    public class RtAPIltImport
    {
        /// <summary>
        /// Открывает перечисление считывателей с подключенными токенами
        /// HRESULT RTAPILT rtlOpenReadersEnum(RTLReaderEnum* phEnum);
        /// Параметры:	[out]	HReaderEnum*	phEnum	- указатель на handle перечисления
        /// Возвращает:	
        ///     E_POINTER
        ///     HRESULT_FROM_WIN32(код ошибки WIN32)
        ///     HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlOpenReadersEnum",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlOpenReadersEnum(out ReadersEnum hEnum);

        /// <summary>
        /// Возвращает следующий элемент перечисления считывателей
        /// HRESULT RTAPILT rtlGetNextReader_W(RTLReaderEnum hEnum, PRTLReaderInf_W pReaderInfo);
        /// Параметры:	[in]	HReaderEnum		hEnum		- handle перечисления
        ///				[out]	PRTLReaderInf	pReaderInfo	- указатель на структуру RTLReaderInf
        /// Возвращает:  E_POINTER
        ///              E_HANDLE
        ///              HRESULT_FROM_WIN32(ERROR_NO_MORE_ITEMS) - перечисления закончено
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetNextReader_W", CharSet = CharSet.Unicode,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetNextReader_W(ReadersEnum hEnum, out ReaderInf_W pReaderInfo);

        /// <summary>
        /// Возвращает следующий элемент перечисления считывателей
        /// HRESULT RTAPILT rtlGetNextReader_A(RTLReaderEnum hEnum, PRTLReaderInf_A pReaderInfo);
        /// Параметры:	[in]	HReaderEnum		hEnum		- handle перечисления
        ///				[out]	PRTLReaderInf	pReaderInfo	- указатель на структуру RTLReaderInf
        /// Возвращает:  E_POINTER
        ///              E_HANDLE
        ///              HRESULT_FROM_WIN32(ERROR_NO_MORE_ITEMS) - перечисления закончено
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetNextReader_A", CharSet = CharSet.Ansi,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetNextReader_A(ReadersEnum hEnum, out ReaderInf_A pReaderInfo);

        /// <summary>
        /// Уничтожает идентификатор(handle) и закрывает перечисление считывателей
        /// HRESULT RTAPILT rtlCloseReadersEnum(RTLReaderEnum hEnum);
        /// Параметры:	[in]	HReaderEnum	hEnum	- handle перечисления
        /// Возвращает:	E_HANDLE
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlCloseReadersEnum",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlCloseReadersEnum(ReadersEnum hEnum);

        /// <summary>
        /// Создаёт идентификатор(handle) токена и связывает его с считывателем
        /// HRESULT RTAPILT rtlBindToken_A(RTLToken* phToken, LPCSTR lpszReader);
        /// Параметры:	[out] RTLToken* phToken	- указатель на переменную, в которую будет 
        ///                                         установлен хендл объекта
        ///             [in] LPCTSTR lpszReader	- имя ридера, к котрому подключен Rutoken
        /// Возвращает:	E_POINTER
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlBindToken_A", CharSet = CharSet.Ansi,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlBindToken_A(out Token phToken, 
            [MarshalAs(UnmanagedType.LPStr)]
            string lpszReader);

        /// <summary>
        /// Создаёт идентификатор(handle) токена и связывает его с считывателем
        /// HRESULT RTAPILT rtlBindToken_W(RTLToken* phToken, LPCWSTR lpszReader);
        /// Параметры:	[out] RTLToken* phToken	- указатель на переменную, в которую будет 
        ///                                         установлен хендл объекта
        ///             [in] LPCTSTR lpszReader	- имя ридера, к котрому подключен Rutoken
        /// Возвращает:	E_POINTER
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlBindToken_W", CharSet = CharSet.Unicode,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlBindToken_W(out Token phToken,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpszReader);

        /// <summary>
        /// Уничтожает идентификатор(handle) токена
        /// HRESULT RTAPILT rtlUnbindToken(RTLToken hToken);
        /// Параметры:	[in] RTLToken hToken - handle объекта Rutoken
        /// Возвращает:	E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32)
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlUnbindToken",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlUnbindToken(Token hToken);

        /// <summary>
        /// Начинает транзакцию, считыватель блокируется
        /// HRESULT RTAPILT rtlLockToken(RTLToken hToken);
        /// Параметры:	[in] RTLToken hToken - handle объекта Rutoken
        /// Возвращает:	E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32)
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlLockToken",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlLockToken(Token hToken);

        /// <summary>
        /// Заканчивает транзакцию, считыватель разблокируется
        /// HRESULT RTAPILT rtlUnlockToken(RTLToken hToken);
        /// Параметры:	[in] RTLToken hToken - handle объекта Rutoken
        ///
        /// Возвращает:	E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32)
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlUnlockToken",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlUnlockToken(Token hToken);

        /// <summary>
        /// Выполняет авторизацию пользователя с указанным PIN-кодом на токен
        /// HRESULT RTAPILT rtlLoginToken_A(RTLToken hToken, LPCSTR lpszPin);
        /// Параметры:	[in] RTLToken hToken - handle Rutoken'a
        ///             [in] LPCTSTR lpszPin - пин-код
        ///
        /// Возвращает:	E_HANDLE
        ///				E_RLT_WRONG_PIN - неправильный пин-код 
        ///				HRESULT_FROM_WIN32(код ошибки WIN32)
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlLoginToken_A", CharSet = CharSet.Ansi,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlLoginToken_A(Token hToken, 
            [MarshalAs(UnmanagedType.LPStr)]
            string lpszPin);

        /// <summary>
        /// Выполняет авторизацию пользователя с указанным PIN-кодом на токен
        /// HRESULT RTAPILT rtlLoginToken_W(RTLToken hToken, LPCWSTR lpszPin);
        /// Параметры:	[in] RTLToken hToken - handle Rutoken'a
        ///             [in] LPCTSTR lpszPin - пин-код
        ///
        /// Возвращает:	E_HANDLE
        ///				E_RLT_WRONG_PIN - неправильный пин-код 
        ///				HRESULT_FROM_WIN32(код ошибки WIN32)
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlLoginToken_W", CharSet = CharSet.Unicode,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlLoginToken_W(Token hToken,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpszPin);
        /// <summary>
        /// Сбрасывает текущие права доступа на токен
        /// HRESULT RTAPILT rtlLogoutToken(RTLToken hToken);
        /// Параметры:	[in] RTLToken hToken - handle Rutoken'a
        /// Возвращает:	E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32)
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlLogoutToken",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlLogoutToken(Token hToken);

        /// <summary>
        /// Сбрасывает текущие права доступа на токен, в зависимости от значения входных флагов очищает кэш PIN-кода
        /// HRESULT RTAPILT rtlLogoutTokenEx(RTLToken hToken, DWORD dwFlags);
        /// Параметры:	[in] RTLToken hToken - handle Rutoken'a
        ///             [in] DWORD dwFlags - флаги функции.
        ///                 RTL_CLEAR_PIN_CACHE - для очистки кэша ПИН-кода 
        /// Возвращает:	E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32)
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlLogoutTokenEx",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlLogoutTokenEx(Token hToken, ushort dwFlags);

        /// <summary>
        /// Проверяет, аутентифицирован ли пользователь
        /// HRESULT RTAPILT rtlIsAuthenticated(RTLToken hToken, BOOL* pbIsAuthenticated);
        /// Параметры:	[in] RTLToken hToken - handle Rutoken'a
        ///             [out] BOOL* pbIsAuthenticated - указатель на флаг, который выставляется 
        ///                 в TRUE, если пользователь уже аутентифицирован, иначе – FALSE.
        /// Возвращает:	E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlIsAuthenticated",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlIsAuthenticated(Token hToken, 
            [MarshalAs(UnmanagedType.Bool)]
            out bool pbIsAuthenticated);

        /// <summary>
        /// Изменяет PIN-код пользователя на токене
        /// HRESULT RTAPILT rtlChangeTokenPin_A(RTLToken hToken, LPCSTR lpszNewPin);
        /// HRESULT RTAPILT rtlChangeTokenPin_W(RTLToken hToken, LPCWSTR lpszNewPin);
        /// Назначение:	Устанавливает новое значение пин-кода. До этого 
        ///				необходимо вызвать rtlLoginToken с старым пин-кодом.
        /// Параметры:	[in] RTLToken hToken - handle Rutoken'a
        ///             [in] LPCTSTR lpszNewPin - новый пин-код
        /// Возвращает: E_HANDLE
        ///             E_POINTER
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlChangeTokenPin_W", CharSet = CharSet.Unicode,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlChangeTokenPin_W(Token hToken, 
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpszNewPin);

        /// <summary>
        /// Возвращает максимально возможную для токена длину PIN-кода
        /// HRESULT RTAPILT rtlGetTokenPinMaxLength(RTLToken hToken, DWORD* pdwLen);
        /// Параметры:	[in] RTLToken hToken - handle Rutoken'a
        ///				[out] DWORD* pdwLen - указатель на переменную
        /// Возвращает:	E_POINTER
        ///				E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetTokenPinMaxLength",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetTokenPinMaxLength(Token hToken, out ushort pdwLen);

        /// <summary>
        /// Возвращает минимально возможную для токена длину PIN-кода
        /// HRESULT RTAPILT rtlGetTokenPinMinLength(RTLToken hToken, DWORD* pdwLen);
        /// Параметры:	[in] RTLToken	hToken	- handle Rutoken'a
        ///             [out] DWORD*	pdwLen	- указатель на переменную
        /// Возвращает:	E_POINTER
        ///				E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetTokenPinMinLength",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetTokenPinMinLength(Token hToken, out ushort pdwLen);

        /// <summary>
        /// Возвращает имя(label) токена
        /// HRESULT RTAPILT rtlGetTokenLabel_A(RTLToken hToken, LPSTR lpszLabel, DWORD* pdwLen);
        /// HRESULT RTAPILT rtlGetTokenLabel_W(RTLToken hToken, LPWSTR lpszLabel, DWORD* pdwLen);
        /// Параметры:	[in] RTLToken hToken - handle Rutoken'a
        ///				[out] LPTSTR lpszLabel - указатель на буфер
        ///             [in,out] DWORD*	pdwLen - указатель на переменную, в которой на
        ///						входе ожидается размер буфера lpszLabel. Размер указывается в количестве 
        ///                     TCHAR (вместе с терминатором). Если lpszLabel = NULL, то в эту
        ///						переменную будет записан размер необходимого буфера
        /// Возвращает:	E_POINTER
        ///				E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32)
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetTokenLabel_W", CharSet = CharSet.Unicode,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetTokenLabel_W(Token hToken, 
            [MarshalAs(UnmanagedType.LPWStr)]
            out string lpszLabel, ref ushort pdwLen);

        /// <summary>
        /// Возвращает ID токена
        /// HRESULT RTAPILT rtlGetTokenID_A(RTLToken hToken, LPSTR lpszId, DWORD* pdwLen);
        /// HRESULT RTAPILT rtlGetTokenID_W(RTLToken hToken, LPWSTR lpszId, DWORD* pdwLen);
        // Параметры:	[in] RTLToken hToken - handle Rutoken'a
        ///             [out] LPTSTR lpszId	- указатель на буфер
        ///             [in,out] DWORD*	pdwLen - указатель на переменную, в которой на входе
        ///					ожидается размер буфера lpszLabel. Размер указывается в количестве 
        ///                 TCHAR (вместе с терминатором).  Если lpszLabel = NULL, то в эту
        ///					переменную будет записан размер
        /// Возвращает:	E_POINTER
        ///				E_HANDLE
        ///             HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER)
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetTokenID_W", CharSet = CharSet.Unicode,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetTokenID_W(Token hToken,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpszId, ref ushort pdwLen);

        /// <summary>
        /// Возвращает количество свободной памяти файловой системы в байтах
        /// HRESULT RTAPILT rtlGetTokenFreeMem(RTLToken hToken, DWORD* pdwFreeMem);
        /// Параметры:	[in] RTLToken hToken - handle Rutoken'a
        ///             [out] DWORD* pdwFreeMem - указатель на переменную содержащею 
        ///	                количество свободной памяти в байтах
        /// Возвращает:	E_HANDLE
        ///             E_POINTER
        ///             HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetTokenFreeMem",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetTokenFreeMem(Token hToken, out ushort pdwFreeMem);

        /// <summary>
        /// Открывает корневую директорию вендора в файловой системе токена
        /// HRESULT RTAPILT rtlOpenRootDir(RTLDir* phRoot, RTLToken hToken, WORD wVendorID, WORD wProductID);
        /// Параметры:	[out] RTLDir* phRoot - указатель на handle объекта директория
        ///             [in] RTLToken hToken - handle объекта Rutoken
        ///				[in] WORD wVendorID - идентификатор вендора из rtAPIlt_Vendor_IDs.h
        ///				[in] WORD wProductID - идентификатор продукта. Предполагается, что
        ///					каждый вендор производит несколько продуктов. Информация по каждому продукту может 
        ///                 записываться в отдельную директорию. Если передан 0, то
        ///					будет открыта корневая папка вендора. ProductID в праве выбирать вендор.
        /// Возвращает:	E_POINTER
        ///				E_INVALIDARG
        ///             E_HANDLE
        ///				E_RTL_NOTLOGGEDIN
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlOpenRootDir",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlOpenRootDir(out Dir phRoot, Token hToken, ushort wVendorID, ushort wProductID);

        /// <summary>
        /// Создаёт новую директорию в файловой системе токена
        /// HRESULT RTAPILT rtlCreateDir(RTLDir* phDir, RTLDir hParent, WORD wDirName, BOOL bPrivate);
        /// Параметры:	[out] RTLDir* phDir - уакзатель на хендл объекта директория
        ///             [in] RTLDir hParent - родительская директория
        ///             [in] WORD dwDirName	- имя директории
        ///             [in] BOOL bPrivate - данный параметр не используется!!!
        /// Возвращает:	E_POINTER
        ///             E_HANDLE
        ///				E_INVALIDARG
        ///				E_RTL_NOTLOGGEDIN
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlCreateDir",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlCreateDir(out Dir dir, Dir parent, ushort dirName, 
            [MarshalAs(UnmanagedType.Bool)]
            bool isPrivate);

        /// <summary>
        /// Открывает существующую в файловой системе токена директорию
        /// HRESULT RTAPILT rtlOpenDir(RTLDir* phDir, WORD wDirName, RTLDir hParent);
        /// Параметры:	[out] RTLDir* phDir - уакзатель на handle объекта директория
        ///				[in] WORD wDirName - имя директории
        ///				[in] RTLDir hParent - родительская дирреткория
        /// Возвращает:	E_POINTER
        ///             E_HANDLE
        ///				E_RTL_FILE_NOT_FOUND
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlOpenDir",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlOpenDir(out Dir phDir, ushort wDirName, Dir hParent);

        /// <summary>
        /// Получает информацию о директории
        /// HRESULT RTAPILT rtlGetDirInfo(RTLDir hDir, PRTLDirInfo pDirInfo);
        /// Параметры:	[in] RTLDir phDir - handle объекта директория
        ///             [out] PDirInfo pDirInfo - указатель на структуру RTLDirInfo
        /// Возвращает:	E_POINTER
        ///				E_HANDLE
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetDirInfo",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetDirInfo(Dir hDir, out DirInfo pDirInfo);

        /// <summary>
        /// Уничтожает идентификатор(handle) и закрывает директорию
        /// HRESULT RTAPILT rtlCloseDir(RTLDir hDir);
        /// Параметры:	[in] RTLDir phDir - handle объекта директория
        /// Возвращает:	E_HANDLE
        ///             E_RTL_NOTLOGGEDIN  
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlCloseDir",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlCloseDir(Dir hDir);

        /// <summary>
        /// Удаляет директорию из файловой системы токена
        /// HRESULT RTAPILT rtlDeleteDir(RTLDir hDir);
        /// Параметры:	[in] RTLDir phDir - handle объекта директория
        /// Возвращает:	E_HANDLE
        ///             E_RTL_NOTLOGGEDIN  
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlDeleteDir",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlDeleteDir(Dir hDir);

        /// <summary>
        /// Создаёт идентификатор(handle) и открывает перечисление директорий в данной директории
        /// HRESULT RTAPILT rtlOpenDirEnum(RTLDirEnum* phEnum, RTLDir hParentDir);
        /// Параметры:	[out]	RTLDirEnum*	phEnum - указатель на handle перечисления
        ///				[in]	RTLDir		hParentDir - handle родительской директории
        /// Возвращает:	E_POINTER
        ///				E_INVALIDARG
        ///				HRESULT_FROM_WIN32(код ошибки WIN32) 
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlOpenDirEnum",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlOpenDirEnum(out DirEnum phEnum, Dir hParentDir);

        /// <summary>
        /// Возвращает следующий элемент из перечисления директорий
        /// HRESULT RTAPILT rtlGetNextDir(RTLDirEnum hDirEnum, PRTLDirInfo pDirInfo);
        /// Параметры:	[in] RTLDirEnum	hEnum - handle перечисления
        ///             [out] PDirInfo	pDirInfo - указатель на структуру RTLDirInfo
        /// Возвращает: E_POINTER
        ///             E_HANDLE
        ///             HRESULT_FROM_WIN32(ERROR_NO_MORE_ITEMS) - перечисление закончено
        ///             HRESULT_FROM_WIN32(код ошибки WIN32)  
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetNextDir",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetNextDir(DirEnum hDirEnum, out DirInfo pDirInfo);

        /// <summary>
        /// Уничтожает идентификатор(handle) и закрывает перечисление директорий
        /// HRESULT RTAPILT rtlCloseDirEnum(RTLDirEnum hDirEnum);
        /// Параметры:	[in]	RTLDirEnum	hEnum	- handle перечисления
        /// Возвращает:	E_HANDLE        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlCloseDirEnum",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlCloseDirEnum(DirEnum hDirEnum);

        /// <summary>
        /// Создаёт бинарный файл в файловой системе токена
        /// HRESULT RTAPILT rtlCreateFile(RTLFile* phFile, RTLDir hDir, WORD wFileName, DWORD dwFileSize, BOOL bPrivate);
        /// Параметры:	[out]	RTLFile*	phFile		- указатель на handle файла
        ///				[in]	RTLDir	    hDir		- родительская директория
        ///				[in]	WORD	    wFileName	- имя файла
        ///				[in]	DWORD	    dwFileSize	- размер файла
        ///				[in]	BOOL	    bPrivate	- необходимо ли защищать доступ 
        ///									                к файлу с помошью пин-кода
        /// Возвращает:	E_POINTER
        ///				E_HANDLE
        ///				E_RTL_NOTLOGGEDIN
        ///             HRESULT_FROM_WIN32(код ошибки WIN32)  
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlCreateFile",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlCreateFile(out File file, Dir dir, ushort fileName, uint fileSize,
            [MarshalAs(UnmanagedType.Bool)]
            bool isPrivate);

        /// <summary>
        /// Открывает существующий бинарный в файловой системе токена файл
        /// HRESULT RTAPILT rtlOpenFile(RTLFile* phFile, RTLDir hDir, WORD wFileName);
        // Параметры:	[out]	RTLFile*	phFile		- указатель на handle файла
        ///				[in]	RTLDir	    hDir		- родительская директория
        ///				[in]	WORD	    wFileName	- имя файла
        /// Возвращает:	E_POINTER
        ///				E_HANDLE
        ///				E_RTL_FILE_NOT_FOUND
        ///             HRESULT_FROM_WIN32(код ошибки WIN32)  
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlOpenFile",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlOpenFile(out File file, Dir dir, ushort fileName);

        /// <summary>
        /// Выполняет чтение бинарных данных из файла
        /// HRESULT RTAPILT rtlReadFile(RTLFile hFile, DWORD dwOffset, DWORD dwLen, BYTE* pbData, DWORD* pdwDataLen);
        /// Параметры:	[in]		RTLFile	hFile		- handle файла
        ///				[in]		DWORD	dwOffset	- смещение от начала файла
        ///				[in]		DWORD	dwLen		- длина считываемого отрезка
        ///				[out]		BYTE*	pbData		- указатель на буфер
        ///				[in,out]	DWORD*	pdwDataLen	- длина буфера. 
        /// Возвращает:	E_HANDLE
        ///				E_POINTER
        ///				E_RTL_NOTLOGGEDIN
        ///             HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER) 
        ///             HRESULT_FROM_WIN32(код ошибки WIN32)  
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlReadFile",
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe uint rtlReadFile(File file, uint offset, uint length, byte* data, uint* dataLen);

        /// <summary>
        /// Записывает бинарные данные в файл
        /// HRESULT RTAPILT rtlWriteFile(RTLFile hFile, DWORD dwOffset, const BYTE* pbData, DWORD dwLen);
        /// Параметры:	[in]	RTLFile		hFile		- handle файла
        ///				[in]	DWORD		dwOffset	- смещение от начала файла
        ///				[in]	const BYTE*	pbData		- указатель на буфер
        ///				[in]	DWORD		dwDataLen	- длина данных в буфере
        /// Возвращает: E_HANDLE
        ///				E_INVALIDARG
        ///				E_RTL_NOTLOGGEDIN
        ///             HRESULT_FROM_WIN32(код ошибки WIN32)  
        ///             HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlWriteFile", 
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlWriteFile(File file, uint offset, byte[] data, uint length);

        /// <summary>
        /// Возвращает информацию о файле
        /// HRESULT RTAPILT rtlGetFileInfo(RTLFile hFile, PRTLFileInfo pFileInfo);
        /// Параметры:	[in]	RTLFile		hFile		- handle файла
        ///				[out]	PRTLFileInfo	pFileInfo	- указатель на структуру RTLFileInfo
        /// Возвращает:	E_POINTER
        ///				E_HANDLE
        ///              E_RTL_NOTLOGGEDIN
        ///              HRESULT_FROM_WIN32(код ошибки WIN32)  
        ///              HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetFileInfo", 
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetFileInfo(File file, out FileInfo fileInfo);

        /// <summary>
        /// Уничтожает идентификатор(handle) и закрывает файл
        /// HRESULT RTAPILT rtlCloseFile(RTLFile hFile);
        /// Параметры:	[in]	RTLFile	hFile	- handle файла
        /// Возвращает:	E_HANDLE
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlCloseFile", 
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlCloseFile(File file);

        /// <summary>
        /// Удаляет файл из файловой системы
        /// HRESULT RTAPILT rtlDeleteFile(RTLFile hFile);
        /// Параметры:	[in]	RTLFile	hFile	- handle файла
        /// Возвращает:	E_HANDLE
        ///				E_RTL_NOTLOGGEDIN
        ///              HRESULT_FROM_WIN32(код ошибки WIN32)  
        ///              HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlDeleteFile", 
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlDeleteFile(File file);

        /// <summary>
        /// Создаёт идентификатор(handle) и открывает перечисление файлов в указанной директории
        /// HRESULT RTAPILT rtlOpenFilesEnum(RTLFilesEnum* phFilesEnum, RTLDir hDir);
        /// Параметры:	[out]	RTLFilesEnum*	phFilesEnum	- указатель на handle перечисления 
        ///				[in]	RTLDir			hDir		- handle директории
        /// Возвращает:	E_HANDLE
        ///              E_POINTER
        ///              HRESULT_FROM_WIN32(ERROR_NO_MORE_ITEMS) - перечисление закончено
        ///              HRESULT_FROM_WIN32(код ошибки WIN32)  
        ///              HRESULT_FROM_TOKEN_ERR(код ошибки Rutoken)
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlOpenFilesEnum", 
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlOpenFilesEnum(out FilesEnum phFilesEnum, Dir dir);

        /// <summary>
        /// Возвращает следующий элемент из перечисления файлов
        /// HRESULT RTAPILT rtlGetNextFile(RTLFilesEnum hFilesEnum, PRTLFileInfo pInfo);
        /// Параметры:	[in]	RTLFilesEnum	hFilesEnum	- handle перечислениz
        ///				[out]	PRTLFileInfo	pInfo		- указатель на структуру RTLFileInfo
        ///
        /// Возвращает:	E_POINTER
        ///				E_HANDLE
        ///				HRESULT_FROM_WIN32(ERROR_NO_MORE_ITEMS) - перечисление завершено        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlGetNextFile", 
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlGetNextFile(FilesEnum filesEnum, out FileInfo info);

        /// <summary>
        /// Уничтожает идентификатор(handle) и закрывает перечисление файлов в директории
        /// HRESULT RTAPILT rtlCloseFilesEnum(RTLFilesEnum hFilesEnum);
        /// Параметры:	[in]	RTLFilesEnum	hFilesEnum	- handle перечисления
        /// Возвращает:	E_HANDLE        
        /// </summary>
        [DllImport("rtAPIlt.dll", EntryPoint = "rtlCloseFilesEnum", 
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint rtlCloseFilesEnum(FilesEnum filesEnum);
    }
}
