using System.IO;
using Newtonsoft.Json;

namespace WeatherIs.Web.Configuration
{
    public static class ConfigContext
    {
        static ConfigContext()
        {
            Config = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText("config.json"));
        }
        
        public static ConfigModel Config { get; }
    }
}