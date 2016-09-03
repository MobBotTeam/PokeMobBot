using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoGo.PokeMobBot.Logic.Logging;

namespace PoGo.PokeMobBot.Logic.Repository
{
    public class GlobalSettingsRepository
    {
        private readonly ILogger _logger;
        private readonly GlobalSettings _defaultGlobalSettings;
        private readonly AuthSettingsRepository _authSettingsRepository;
        private readonly TeleSettingsRepository _teleSettingsRepository;

        public GlobalSettingsRepository(ILogger logger, GlobalSettings defaultGlobalSettings, AuthSettingsRepository authSettingsRepository, TeleSettingsRepository teleSettingsRepository)
        {
            _logger = logger;
            _defaultGlobalSettings = defaultGlobalSettings;
            _authSettingsRepository = authSettingsRepository;
            _teleSettingsRepository = teleSettingsRepository;
        }

        public GlobalSettings Load(string path)
        {
            GlobalSettings settings;
            var profilePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "config.json");

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

                    settings = JsonConvert.DeserializeObject<GlobalSettings>(input, jsonSettings);
                }
                catch (JsonReaderException exception)
                {
                    _logger.Write("JSON Exception: " + exception.Message, LogLevel.Error);
                    return null;
                }
            }
            else
            {
                settings = new GlobalSettings();
            }

            if (settings.StartUpSettings.WebSocketPort == 0)
            {
                settings.StartUpSettings.WebSocketPort = 14251;
            }

            if (settings.PokemonToSnipe == null)
            {
                settings.PokemonToSnipe = _defaultGlobalSettings.PokemonToSnipe;
            }

            if (settings.PokemonSettings.RenameTemplate == null)
            {
                settings.PokemonSettings.RenameTemplate = _defaultGlobalSettings.PokemonSettings.RenameTemplate;
            }

            if (settings.SnipeSettings.SnipeLocationServer == null)
            {
                settings.SnipeSettings.SnipeLocationServer = _defaultGlobalSettings.SnipeSettings.SnipeLocationServer;
            }

            settings.ProfilePath = profilePath;
            settings.ProfileConfigPath = profileConfigPath;
            settings.GeneralConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "config");

            var firstRun = !File.Exists(configFile);

            Save(configFile);
            settings.Auth = _authSettingsRepository.Load(Path.Combine(profileConfigPath, "auth.json"));

            if (firstRun)
            {
                return null;
            }

            return settings;
        }

        public void Save(string fullPath)
        {
            var output = JsonConvert.SerializeObject(_defaultGlobalSettings, Formatting.Indented,
                new StringEnumConverter { CamelCaseText = true });

            var folder = Path.GetDirectoryName(fullPath);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(fullPath, output);
        }
    }
}
