namespace DiCore.Lib.WebDiagnostic.Interfaces
{
    public interface IDiagnostic
    {
        /// <summary>
        /// Проверка строки соединения с PostgreSQL
        /// </summary>
        /// <param name="connectionString">Строка соединения</param>
        /// <param name="message">Сообщение об ошибке (при положительном результате "Ok")</param>
        /// <returns>Результат (true - положительный результат проверки)</returns>
        bool CheckNpgsqlConnectionString(string connectionString, out string message);

        /// <summary>
        /// Проверка Web-API приложения (в проверяемом приложении должен быть реализован контроллер 
        /// самодиагностики (метод selfdiag/test))
        /// </summary>
        /// <param name="webApiUrl">Url-адрес Web-API</param>
        /// <param name="message">Сообщение об ошибке (при положительном результате "Ok")</param>
        /// <returns>Результат (true - положительный результат проверки)</returns>
        bool CheckWebApi(string webApiUrl, out string message);

        /// <summary>
        /// Проверка существования папки
        /// </summary>
        /// <param name="path">Путь к папке</param>
        /// <param name="message">Сообщение об ошибке (при положительном результате "Ok")</param>
        /// <returns>Результат (true - положительный результат проверки)</returns>
        bool CheckDirectoryExists(string path, out string message);
    }
}