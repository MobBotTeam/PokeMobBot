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
    public class AuthSettingsRepository
    {
        private readonly ILogger _logger;
        private readonly AuthSettings _defaultAuthSettings;
        private string _filePath;

        public AuthSettingsRepository(ILogger logger, AuthSettings defaultAuthSettings)
        {
            _logger = logger;
            _defaultAuthSettings = defaultAuthSettings;
        }

        public AuthSettings Get(string filePath)
        {
            _filePath = filePath;

            try
            {
                if (File.Exists(filePath))
                {
                    //if the file exists, load the settings
                    var input = File.ReadAllText(filePath);

                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

                    return JsonConvert.DeserializeObject<AuthSettings>(input, settings);
                }
                else
                {
                    var output = JsonConvert.SerializeObject(_defaultAuthSettings, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });

                    var folder = Path.GetDirectoryName(filePath);
                    if (folder != null && !Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    File.WriteAllText(filePath, output);
                }
            }
            catch (JsonReaderException exception)
            {
                if (exception.Message.Contains("Unexpected character") && exception.Message.Contains("PtcUsername"))
                    _logger.Write("JSON Exception: You need to properly configure your PtcUsername using quotations.",
                        LogLevel.Error);
                else if (exception.Message.Contains("Unexpected character") && exception.Message.Contains("PtcPassword"))
                    _logger.Write(
                        "JSON Exception: You need to properly configure your PtcPassword using quotations.",
                        LogLevel.Error);
                else if (exception.Message.Contains("Unexpected character") &&
                         exception.Message.Contains("GoogleUsername"))
                    _logger.Write(
                        "JSON Exception: You need to properly configure your GoogleUsername using quotations.",
                        LogLevel.Error);
                else if (exception.Message.Contains("Unexpected character") &&
                         exception.Message.Contains("GooglePassword"))
                    _logger.Write(
                        "JSON Exception: You need to properly configure your GooglePassword using quotations.",
                        LogLevel.Error);
                else
                    _logger.Write("JSON Exception: " + exception.Message, LogLevel.Error);
            }

            return null;
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(_filePath))
            {
                var output = JsonConvert.SerializeObject(this, Formatting.Indented,
                    new StringEnumConverter { CamelCaseText = true });

                var folder = Path.GetDirectoryName(_filePath);
                if (folder != null && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                File.WriteAllText(_filePath, output);
            }
        }
    }
}
