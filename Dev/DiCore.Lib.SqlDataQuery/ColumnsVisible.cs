using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiCore.Lib.SqlDataQuery
{
    public class ColumnsVisible
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="config"></param>
        public ColumnsVisible(JObject config)
        {
            if (config == null)
                return;
            ConstraintsPresent = true;

            if (config["Configuration"] != null)
                this.Configuration = JsonConvert.DeserializeObject<Dictionary<string, ColumnConfig>>(config["Configuration"].ToString());

            this.Visible = config["Visible"].Value<bool?>();
        }

        /// <summary>
        /// Признак наличия ограничений
        /// </summary>
        public bool ConstraintsPresent { get; }

        /// <summary>
        /// Конфигурация
        /// </summary>
        private Dictionary<string, ColumnConfig> Configuration { get;  }
        
        /// <summary>
        /// Видимость
        /// </summary>
        public bool? Visible { get; }

        /// <summary>
        /// Получение конфигурации по имени столбца
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ColumnConfig GetConfiguration(string name)
        {
            if (ConstraintsPresent == false || Configuration == null || !Configuration.ContainsKey(name))
                return new ColumnConfig(true, true);
            return Configuration[name];
        }
    }
}
