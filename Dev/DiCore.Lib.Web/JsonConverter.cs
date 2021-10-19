using Newtonsoft.Json;

namespace DiCore.Lib.Web
{
    public static class JsonConverter
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
            DateParseHandling = DateParseHandling.DateTimeOffset
        };

        public static string SerializeObject(object data)
        {
            return JsonConvert.SerializeObject(data, JsonSerializerSettings);
        }

        public static T DeserializeObject<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str, JsonSerializerSettings);
        }
    }
}