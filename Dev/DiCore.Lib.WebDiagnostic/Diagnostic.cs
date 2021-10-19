using System;
using System.IO;
using DiCore.Lib.Web;
using DiCore.Lib.WebDiagnostic.Interfaces;
using Npgsql;

namespace DiCore.Lib.WebDiagnostic
{
    public class Diagnostic : IDiagnostic
    {
        /// <summary>
        /// Проверка строки соединения с PostgreSQL
        /// </summary>
        /// <param name="connectionString">Строка соединения</param>
        /// <param name="message">Сообщение об ошибке (при положительном результате "Ok")</param>
        /// <returns>Результат (true - положительный результат проверки)</returns>
        public bool CheckNpgsqlConnectionString(string connectionString, out string message)
        {
            message = "Ok";
            var diCoreConnectionString = new NpgsqlConnection(connectionString);
            try
            {
                diCoreConnectionString.Open();
                diCoreConnectionString.Close();
            }
            catch (Exception e)
            {
                message = $"Ошибка: {e.Message}";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Проверка Web-API приложения (в проверяемом приложении должен быть реализован контроллер 
        /// самодиагностики (метод selfdiag/test))
        /// </summary>
        /// <param name="webApiUrl">Url-адрес Web-API</param>
        /// <param name="message">Сообщение об ошибке (при положительном результате "Ok")</param>
        /// <returns>Результат (true - положительный результат проверки)</returns>
        public bool CheckWebApi(string webApiUrl, out string message)
        {
            var simpleWebClient = new SimpleWebClient(webApiUrl);

            message = "Ok";
            try
            {
                simpleWebClient.Get<object>($"selfdiag/test");
            }
            catch (Exception e)
            {
                message = $"Ошибка: {e.Message}";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Проверка существования папки
        /// </summary>
        /// <param name="path">Путь к папке</param>
        /// <param name="message">Сообщение об ошибке (при положительном результате "Ok")</param>
        /// <returns>Результат (true - положительный результат проверки)</returns>
        public bool CheckDirectoryExists(string path, out string message)
        {
            message = "Ok";
            try
            {
                if (!Directory.Exists(path))
                {
                    message = "Папка не существует";
                    return false;
                }
            }
            catch (Exception e)
            {
                message = $"Ошибка: {e.Message}";
                return false;
            }
            return true;
        }
    }
}
