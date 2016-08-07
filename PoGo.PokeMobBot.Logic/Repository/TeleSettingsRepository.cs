using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoGo.PokeMobBot.Logic.Logging;

namespace PoGo.PokeMobBot.Logic.Repository
{
    public class TeleSettingsRepository
    {
        private readonly ILogger _logger;
        private readonly TeleSettings _defaultTeleSettings;

        public TeleSettingsRepository(ILogger logger, TeleSettings defaultTeleSettings)
        {
            _logger = logger;
            _defaultTeleSettings = defaultTeleSettings;
        }

        public TeleSettings Load(string path)
        {
            TeleSettings settings2;
            var profilePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "TeleAI.json");

            if (File.Exists(configFile))
            {
                try
                {
                    //if the file exists, load the settings
                    var input = File.ReadAllText(configFile);

                    var jsonSettings = new JsonSerializerSettings();
                    jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                    jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                    jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;

                    settings2 = JsonConvert.DeserializeObject<TeleSettings>(input, jsonSettings);
                }
                catch (JsonReaderException exception)
                {
                    _logger.Write("JSON Exception: " + exception.Message, LogLevel.Error);
                    return null;
                }
            }
            else
            {
                settings2 = new TeleSettings();
            }



            settings2.ProfilePath = profilePath;
            settings2.ProfileConfigPath = profileConfigPath;
            settings2.GeneralConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "config");

            var firstRun = !File.Exists(configFile);

            Save(configFile);

            if (firstRun)
            {
                return null;
            }

            return settings2;
        }



        public void Save(string path)
        {
            var output = JsonConvert.SerializeObject(_defaultTeleSettings, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });

            var folder = Path.GetDirectoryName(path);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, output);
        }
    }
}
