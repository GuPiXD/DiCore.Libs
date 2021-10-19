using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DiCore.Lib.Web;

namespace Dicore.Lib.ApplicationSettingsLoader
{
    /// <summary>
    /// Получение списка настроек раздела appSettings конфигурационного файла 
    /// с загрузкой внешних настроек из ТИС УИС
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public AppSettings()
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="codeName">Наименование настройки, в которой находится код приложения (ТИС УИС)</param>
        /// <param name="webApiAddressName">Наименование настройки, в которой хранится адрес веб-апи ТИС УИС</param>
        public AppSettings(string codeName, string webApiAddressName)
        {
            CodeName = codeName;
            WebApiAddressName = webApiAddressName;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="codeName">Наименование настройки, в которой находится код приложения (ТИС УИС)</param>
        /// <param name="webApiAddressName">Наименование настройки, в которой хранится адрес веб-апи ТИС УИС</param>
        /// <param name="delimiter">Разделитель частей значения настройки</param>
        /// <param name="prefix">Префикс внешней настройки</param>
        public AppSettings(string codeName, string webApiAddressName, char delimiter, string prefix)
            : this(codeName, webApiAddressName)
        {
            Delimiter = delimiter;
            Prefix = prefix;
        }

        /// <summary>
        /// Префикс внешней настройки
        /// </summary>
        public string Prefix { get; private set; } = "External";

        /// <summary>
        /// Наименование настройки, в которой находится код приложения (ТИС УИС)
        /// </summary>
        public string CodeName { get; private set; } = "auth:Application";

        /// <summary>
        /// Наименование настройки, в которой хранится адрес веб-апи ТИС УИС
        /// </summary>
        public string WebApiAddressName { get; private set; } = "auth:ApiUrl";

        /// <summary>
        /// Разделитель частей значения настройки
        /// </summary>
        public char Delimiter { get; private set; } = '#';

        /// <summary>
        /// Код приложения
        /// </summary>
        public string AppCode { get; set; }

        /// <summary>
        /// Адрес веб-апи приложения ТИС УИС
        /// </summary>
        public string WebApiAddress { get; set; }

        private Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Загрузка параметров из класса XDocument
        /// </summary>
        /// <param name="xDoc">XDocument</param>
        public void FromXDocument(XDocument xDoc)
        {
            var appSettings = xDoc.Descendants("appSettings").Elements("add");

            foreach (var appSetting in appSettings)
            {
                var keyAttr = appSetting.Attribute("key");
                var valueAttr = appSetting.Attribute("value");

                if (keyAttr == null)
                    throw new FormatException("Отсутствует атрибут key тэга add");

                if (valueAttr == null)
                    throw new FormatException("Отсутствует атрибут value тэга add");

                var key = keyAttr.Value;
                var val = valueAttr.Value;

                if (key == $"{WebApiAddressName}")
                    WebApiAddress = val;
                else if (key == CodeName)
                    AppCode = val;
                else
                    Settings.Add(key, val);
            }
        }

        /// <summary>
        /// Загрузка параметров из экземпляра класса NameValueCollection 
        /// (например из конфигурационного файла: ConfigurationManager.AppSettings)
        /// </summary>
        /// <param name="collection">XDocument</param>
        public void FromCollection(NameValueCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException();

            foreach (var key in collection.AllKeys)
            {
                var val = collection[key];
                if (key == $"{WebApiAddressName}")
                    WebApiAddress = val;
                else if (key == CodeName)
                    AppCode = val;
                else
                    Settings.Add(key, val);
            }
        }

        /// <summary>
        /// Получение настроек со значениями, загруженными из ТИС УИС
        /// </summary>
        /// <returns>Словарь с настройками</returns>
        public Dictionary<string, string> Resolve()
        {
            var settings = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(WebApiAddress) && !string.IsNullOrEmpty(AppCode))
            {
                var webApi = new SimpleWebClient(WebApiAddress);
                foreach (var setting in Settings)
                {
                    if (string.IsNullOrEmpty(setting.Key))
                        throw new ArgumentException($"Наименование параметра отсутствует");

                    var splitValue = setting.Value.Split(Delimiter);
                    if (string.Compare(splitValue[0], Prefix, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (splitValue.Length == 2)
                        {
                            var settingName = splitValue[1];
                            var value = GetSettingValue(webApi, AppCode, settingName);
                            settings.Add(setting.Key, value);
                        }
                        else if (splitValue.Length == 3)
                        {
                            var appCode = splitValue[1];
                            var settingName = splitValue[2];
                            var value = GetSettingValue(webApi, appCode, settingName);
                            settings.Add(setting.Key, value);
                        }
                    }
                    else
                    {
                        settings.Add(setting.Key, setting.Value);
                    }
                }
            }
            return settings;
        }

        private string GetSettingValue(SimpleWebClient webClient, string appCode, string settingName)
        {
            return webClient.Get<string>($"Settings/{appCode}/{settingName}");
        }
    }
}
