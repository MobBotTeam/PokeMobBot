#region using directives

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Repository;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class VersionCheckState : IState
    {
        public const string VersionUri =
            "https://raw.githubusercontent.com/PocketMobsters/PokeMobBot/master/PoGo.PokeMobBot.Logic/Properties/AssemblyInfo.cs";

        public const string LatestReleaseApi =
            "https://api.github.com/repos/PocketMobsters/PokeMobBot/releases/latest";

        private const string LatestRelease =
            "https://github.com/PocketMobsters/PokeMobBot/releases";

        private readonly LoginState _loginState;
        private readonly ILogicSettings _logicSettings;
        private readonly GlobalSettingsRepository _globalSettingsRepository;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;

        public static Version RemoteVersion;

        public VersionCheckState(LoginState loginState, GlobalSettingsRepository globalSettingsRepository, ILogicSettings logicSettings, IEventDispatcher eventDispatcher, ITranslation translation)
        {
            _loginState = loginState;
            _globalSettingsRepository = globalSettingsRepository;
            _logicSettings = logicSettings;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
        }

        public async Task<IState> Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await CleanupOldFiles();
            var autoUpdate = _logicSettings.AutoUpdate;
            var needupdate = IsLatest();
            if (!needupdate || !autoUpdate)
            {
                if (!needupdate)
                {
                    _eventDispatcher.Send(new UpdateEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.GotUpToDateVersion, RemoteVersion)
                    });
                    return _loginState;
                }
                _eventDispatcher.Send(new UpdateEvent
                {
                    Message = _translation.GetTranslation(TranslationString.AutoUpdaterDisabled, LatestRelease)
                });

                return _loginState;
            }
            _eventDispatcher.Send(new UpdateEvent
            {
                Message = _translation.GetTranslation(TranslationString.DownloadingUpdate)
            });
            var remoteReleaseUrl =
                $"https://github.com/PocketMobsters/PokeMobBot/releases/download/v{RemoteVersion}/";
            const string zipName = "Release.zip";
            var downloadLink = remoteReleaseUrl + zipName;
            var baseDir = Directory.GetCurrentDirectory();
            var downloadFilePath = Path.Combine(baseDir, zipName);
            var tempPath = Path.Combine(baseDir, "tmp");
            var extractedDir = Path.Combine(tempPath, "Release");
            var destinationDir = baseDir + Path.DirectorySeparatorChar;

            _eventDispatcher.Send(new NoticeEvent() { Message = downloadLink });

            if (!DownloadFile(downloadLink, downloadFilePath))
                return _loginState;

            _eventDispatcher.Send(new UpdateEvent
            {
                Message = _translation.GetTranslation(TranslationString.FinishedDownloadingRelease)
            });

            if (!UnpackFile(downloadFilePath, tempPath))
                return _loginState;

            _eventDispatcher.Send(new UpdateEvent
            {
                Message = _translation.GetTranslation(TranslationString.FinishedUnpackingFiles)
            });

            if (!MoveAllFiles(extractedDir, destinationDir))
                return _loginState;

            _eventDispatcher.Send(new UpdateEvent
            {
                Message = _translation.GetTranslation(TranslationString.UpdateFinished)
            });

            if (TransferConfig(baseDir))
                _eventDispatcher.Send(new UpdateEvent
                {
                    Message = _translation.GetTranslation(TranslationString.FinishedTransferringConfig)
                });

            await Task.Delay(2000, cancellationToken);

            Process.Start(Assembly.GetEntryAssembly().Location);
            Environment.Exit(-1);
            return null;
        }

        public async Task CleanupOldFiles()
        {
            var tmpDir = Path.Combine(Directory.GetCurrentDirectory(), "tmp");

            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, true);
            }

            var di = new DirectoryInfo(Directory.GetCurrentDirectory());
            var files = di.GetFiles("*.old", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    if (file.Name.Contains("vshost"))
                        continue;
                    File.Delete(file.FullName);
                }
                catch (Exception e)
                {
                    _eventDispatcher.Send(new ErrorEvent()
                    {
                        Message = e.ToString()
                    });
                }
            }
            await Task.Delay(200);
        }

        public bool DownloadFile(string url, string dest)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, dest);
                    _eventDispatcher.Send(new NoticeEvent()
                    {
                        Message = dest
                    });
                }
                catch
                {
                    // ignored
                }
                return true;
            }
        }

        private string DownloadServerVersion()
        {
            using (var wC = new WebClient())
            {
                return wC.DownloadString(VersionUri);
            }
        }

        private JObject GetJObject(string filePath)
        {
            return JObject.Parse(File.ReadAllText(filePath));
        }


        public bool IsLatest()
        {
            try
            {
                var regex = new Regex(@"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]");
                var match = regex.Match(DownloadServerVersion());

                if (!match.Success)
                    return false;

                var gitVersion = new Version($"{match.Groups[1]}.{match.Groups[2]}.{match.Groups[3]}");
                RemoteVersion = gitVersion;
                var localVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (gitVersion >= localVersion)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return true; //better than just doing nothing when git server down
            }

            return false;
        }

        public bool MoveAllFiles(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            var oldfiles = Directory.GetFiles(destFolder);
            foreach (var old in oldfiles)
            {
                if (old.Contains("vshost")) continue;
                File.Move(old, old + ".old");
            }

            try
            {
                var files = Directory.GetFiles(sourceFolder);
                foreach (var file in files)
                {
                    if (file.Contains("vshost")) continue;
                    var name = Path.GetFileName(file);
                    var dest = Path.Combine(destFolder, name);
                    File.Copy(file, dest, true);
                }

                var folders = Directory.GetDirectories(sourceFolder);

                foreach (var folder in folders)
                {
                    var name = Path.GetFileName(folder);
                    if (name == null) continue;
                    var dest = Path.Combine(destFolder, name);
                    MoveAllFiles(folder, dest);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool TransferConfig(string baseDir)
        {
            if (!_logicSettings.TransferConfigAndAuthOnUpdate)
                return false;

            var configDir = Path.Combine(baseDir, "Config");
            if (!Directory.Exists(configDir))
                return false;

            var oldConf = GetJObject(Path.Combine(configDir, "config.json.old"));
            var oldAuth = GetJObject(Path.Combine(configDir, "auth.json.old"));

            _globalSettingsRepository.Load("");

            var newConf = GetJObject(Path.Combine(configDir, "config.json"));
            var newAuth = GetJObject(Path.Combine(configDir, "auth.json"));

            TransferJson(oldConf, newConf);
            TransferJson(oldAuth, newAuth);

            File.WriteAllText(Path.Combine(configDir, "config.json"), newConf.ToString());
            File.WriteAllText(Path.Combine(configDir, "auth.json"), newAuth.ToString());

            return true;
        }

        private void TransferJson(JObject oldFile, JObject newFile)
        {
            try
            {
                foreach (var newProperty in newFile.Properties())
                    foreach (var oldProperty in oldFile.Properties())
                        if (newProperty.Name.Equals(oldProperty.Name))
                        {
                            newFile[newProperty.Name] = oldProperty.Value;
                            break;
                        }
            }
            catch
            {
                // ignored
            }
        }

        public bool UnpackFile(string sourceTarget, string destPath)
        {
            var source = sourceTarget;
            var dest = destPath;
            try
            {
                ZipFile.ExtractToDirectory(source, dest);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}