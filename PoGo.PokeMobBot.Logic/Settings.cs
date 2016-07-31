#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoGo.PokeMobBot.Logic.Logging;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic
{
    internal class AuthSettings
    {
        [JsonIgnore]
        private string _filePath;
        public AuthType AuthType;
        public string GoogleRefreshToken;
        public string GoogleUsername;
        public string GooglePassword;
        public string PtcUsername;
        public string PtcPassword;

        public void Load(string path)
        {
            try
            {
                _filePath = path;

                if (File.Exists(_filePath))
                {
                    //if the file exists, load the settings
                    var input = File.ReadAllText(_filePath);

                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

                    JsonConvert.PopulateObject(input, this, settings);
                }
                else
                {
                    Save(_filePath);
                }
            }
            catch (JsonReaderException exception)
            {
                if (exception.Message.Contains("Unexpected character") && exception.Message.Contains("PtcUsername"))
                    Logger.Write("JSON Exception: You need to properly configure your PtcUsername using quotations.",
                        LogLevel.Error);
                else if (exception.Message.Contains("Unexpected character") && exception.Message.Contains("PtcPassword"))
                    Logger.Write(
                        "JSON Exception: You need to properly configure your PtcPassword using quotations.",
                        LogLevel.Error);
                else if (exception.Message.Contains("Unexpected character") &&
                         exception.Message.Contains("GoogleUsername"))
                    Logger.Write(
                        "JSON Exception: You need to properly configure your GoogleUsername using quotations.",
                        LogLevel.Error);
                else if (exception.Message.Contains("Unexpected character") &&
                         exception.Message.Contains("GooglePassword"))
                    Logger.Write(
                        "JSON Exception: You need to properly configure your GooglePassword using quotations.",
                        LogLevel.Error);
                else
                    Logger.Write("JSON Exception: " + exception.Message, LogLevel.Error);
            }
        }

        public void Save(string path)
        {
            var output = JsonConvert.SerializeObject(this, Formatting.Indented,
                new StringEnumConverter { CamelCaseText = true });

            var folder = Path.GetDirectoryName(path);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, output);
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(_filePath))
            {
                Save(_filePath);
            }
        }
    }

    public class GlobalSettings
    {
        [JsonIgnore]
        internal AuthSettings Auth = new AuthSettings();
        [JsonIgnore]
        public string GeneralConfigPath;
        [JsonIgnore]
        public string ProfilePath;
        [JsonIgnore]
        public string ProfileConfigPath;

        //bot start
        public string TranslationLanguageCode = "en";
        public bool AutoUpdate = true;
        public bool TransferConfigAndAuthOnUpdate = true;
        public bool StartupWelcomeDelay = true;
        public bool DumpPokemonStats = false;
        public int AmountOfPokemonToDisplayOnStart = 10;
        public int WebSocketPort = 14251;

        //coords and movement
        public bool Teleport = false;
        public double DefaultLatitude = 40.785091;
        public double DefaultLongitude = -73.968285;
        public double DefaultAltitude = 10;
        public double WalkingSpeedInKilometerPerHour = 15.0;
        public int MaxSpawnLocationOffset = 10;
        public int MaxTravelDistanceInMeters = 1000;
        public bool UseGpxPathing = false;
        public string GpxFile = "GPXPath.GPX";

        //delays
        public int DelayBetweenPlayerActions = 5000;
        public int DelayBetweenPokemonCatch = 2000;
        public int DelayCatchNearbyPokemon = 1000;
        public int DelayPositionCheckState = 1000;
        public int DelayCatchIncensePokemon = 1000;
        public int DelayCatchLurePokemon = 1000;
        public int DelayCatchPokemon = 1000;
        public int DelayDisplayPokemon = 1000;
        public int DelayUseLuckyEgg = 1000;
        public int DelaySoftbanRetry = 1000;
        public int DelayPokestop = 1000;
        public int DelayRecyleItem = 1000;
        public int DelaySnipePokemon = 1000;
        public int DelayTransferPokemon = 1000;

        //incubator
        public bool UseEggIncubators = true;

        //rename
        public bool RenamePokemon = false;
        public bool RenameOnlyAboveIv = true;
        public string RenameTemplate = "{1}_{0}";

        //transfer
        public bool TransferDuplicatePokemon = true;
        public bool PrioritizeIvOverCp = true;
        public int KeepMinCp = 1250;
        public float KeepMinIvPercentage = 90;
        public int KeepMinDuplicatePokemon = 1;
        public bool KeepPokemonsThatCanEvolve = false;

        //evolve
        public bool EvolveAllPokemonWithEnoughCandy = true;
        public bool EvolveAllPokemonAboveIv = false;
        public float EvolveAboveIvValue = 95;
        public bool UseLuckyEggsWhileEvolving = false;
        public int UseLuckyEggsMinPokemonAmount = 30;

        //levelup
        public bool AutomaticallyLevelUpPokemon = false;
        public string LevelUpByCPorIv = "iv";
        public float UpgradePokemonCpMinimum = 1000;
        public float UpgradePokemonIvMinimum = 95;

        //catch
        public int MaxPokeballsPerPokemon = 6;
        public int UseGreatBallAboveCp = 750;
        public int UseUltraBallAboveCp = 1000;
        public int UseMasterBallAboveCp = 1500;
        public bool UsePokemonToNotCatchFilter = false;
        public bool HumanizeThrows = false;
        public double ThrowAccuracyMin = 0.50;
        public double ThrowAccuracyMax = 1.00;
        public double ThrowSpinFrequency = 0.75;

        //recycle
        public int TotalAmountOfPokeballsToKeep = 100;
        public int TotalAmountOfPotionsToKeep = 80;
        public int TotalAmountOfRevivesToKeep = 60;

        //snipe
        public int MinDelayBetweenSnipes = 60000;
        public int MinPokeballsToSnipe = 20;
        public int MinPokeballsWhileSnipe = 0;
        public bool SnipeAtPokestops = false;
        public bool SnipeIgnoreUnknownIv = false;
        public bool UseTransferIvForSnipe = false;
        public bool UseSnipeLocationServer = false;
        public string SnipeLocationServer = "localhost";
        public int SnipeLocationServerPort = 16969;

        public List<KeyValuePair<ItemId, int>> ItemRecycleFilter = new List<KeyValuePair<ItemId, int>>
        {
            new KeyValuePair<ItemId, int>(ItemId.ItemUnknown, 0),
            new KeyValuePair<ItemId, int>(ItemId.ItemLuckyEgg, 200),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncenseOrdinary, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncenseSpicy, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncenseCool, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncenseFloral, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemTroyDisk, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemXAttack, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemXDefense, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemXMiracle, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemRazzBerry, 50),
            new KeyValuePair<ItemId, int>(ItemId.ItemBlukBerry, 10),
            new KeyValuePair<ItemId, int>(ItemId.ItemNanabBerry, 10),
            new KeyValuePair<ItemId, int>(ItemId.ItemWeparBerry, 30),
            new KeyValuePair<ItemId, int>(ItemId.ItemPinapBerry, 30),
            new KeyValuePair<ItemId, int>(ItemId.ItemSpecialCamera, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasicUnlimited, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasic, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemPokemonStorageUpgrade, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemItemStorageUpgrade, 100)
        };

        public List<PokemonId> PokemonsNotToTransfer = new List<PokemonId>
        {
            //criteria: from SS Tier to A Tier + Regional Exclusive
            PokemonId.Venusaur,
            PokemonId.Charizard,
            PokemonId.Blastoise,
            PokemonId.Nidoqueen,
            PokemonId.Nidoking,
            PokemonId.Clefable,
            PokemonId.Vileplume,
            //PokemonId.Golduck,
            PokemonId.Arcanine,
            PokemonId.Poliwrath,
            PokemonId.Machamp,
            PokemonId.Victreebel,
            PokemonId.Golem,
            PokemonId.Slowbro,
            //PokemonId.Farfetchd,
            PokemonId.Muk,
            PokemonId.Exeggutor,
            PokemonId.Lickitung,
            PokemonId.Chansey,
            //PokemonId.Kangaskhan,
            //PokemonId.MrMime,
            //PokemonId.Tauros,
            //PokemonId.Gyarados,
            //PokemonId.Lapras,
            PokemonId.Ditto,
            //PokemonId.Vaporeon,
            //PokemonId.Jolteon,
            //PokemonId.Flareon,
            //PokemonId.Porygon,
            //PokemonId.Snorlax,
            PokemonId.Articuno,
            PokemonId.Zapdos,
            PokemonId.Moltres,
            //PokemonId.Dragonite,
            PokemonId.Mewtwo,
            PokemonId.Mew
        };

        public List<PokemonId> PokemonsToEvolve = new List<PokemonId>
        {
            /*NOTE: keep all the end-of-line commas exept for the last one or an exception will be thrown!
            criteria: 12 candies*/
            PokemonId.Caterpie,
            PokemonId.Weedle,
            PokemonId.Pidgey,
            /*criteria: 25 candies*/
            //PokemonId.Bulbasaur,
            //PokemonId.Charmander,
            //PokemonId.Squirtle,
            PokemonId.Rattata
            //PokemonId.NidoranFemale,
            //PokemonId.NidoranMale,
            //PokemonId.Oddish,
            //PokemonId.Poliwag,
            //PokemonId.Abra,
            //PokemonId.Machop,
            //PokemonId.Bellsprout,
            //PokemonId.Geodude,
            //PokemonId.Gastly,
            //PokemonId.Eevee,
            //PokemonId.Dratini,
            /*criteria: 50 candies commons*/
            //PokemonId.Spearow,
            //PokemonId.Ekans,
            //PokemonId.Zubat,
            //PokemonId.Paras,
            //PokemonId.Venonat,
            //PokemonId.Psyduck,
            //PokemonId.Slowpoke,
            //PokemonId.Doduo,
            //PokemonId.Drowzee,
            //PokemonId.Krabby,
            //PokemonId.Horsea,
            //PokemonId.Goldeen,
            //PokemonId.Staryu
        };

        public List<PokemonId> PokemonsToIgnore = new List<PokemonId>
        {
            //criteria: most common
            PokemonId.Caterpie,
            PokemonId.Weedle,
            PokemonId.Pidgey,
            PokemonId.Rattata,
            PokemonId.Spearow,
            PokemonId.Zubat,
            PokemonId.Doduo
        };

        public Dictionary<PokemonId, TransferFilter> PokemonsTransferFilter = new Dictionary<PokemonId, TransferFilter>
        {
            //criteria: based on NY Central Park and Tokyo variety + sniping optimization
            {PokemonId.Golduck, new TransferFilter(1800, 95, 1)},
            {PokemonId.Farfetchd, new TransferFilter(1250, 80, 1)},
            {PokemonId.Krabby, new TransferFilter(1250, 95, 1)},
            {PokemonId.Kangaskhan, new TransferFilter(1500, 60, 1)},
            {PokemonId.Horsea, new TransferFilter(1250, 95, 1)},
            {PokemonId.Staryu, new TransferFilter(1250, 95, 1)},
            {PokemonId.MrMime, new TransferFilter(1250, 40, 1)},
            {PokemonId.Scyther, new TransferFilter(1800, 80, 1)},
            {PokemonId.Jynx, new TransferFilter(1250, 95, 1)},
            {PokemonId.Electabuzz, new TransferFilter(1250, 80, 1)},
            {PokemonId.Magmar, new TransferFilter(1500, 80, 1)},
            {PokemonId.Pinsir, new TransferFilter(1800, 95, 1)},
            {PokemonId.Tauros, new TransferFilter(1250, 90, 1)},
            {PokemonId.Magikarp, new TransferFilter(1250, 95, 1)},
            {PokemonId.Gyarados, new TransferFilter(1250, 90, 1)},
            {PokemonId.Lapras, new TransferFilter(1800, 80, 1)},
            {PokemonId.Eevee, new TransferFilter(1250, 95, 1)},
            {PokemonId.Vaporeon, new TransferFilter(1500, 90, 1)},
            {PokemonId.Jolteon, new TransferFilter(1500, 90, 1)},
            {PokemonId.Flareon, new TransferFilter(1500, 90, 1)},
            {PokemonId.Porygon, new TransferFilter(1250, 60, 1)},
            {PokemonId.Snorlax, new TransferFilter(2600, 90, 1)},
            {PokemonId.Dragonite, new TransferFilter(2600, 90, 1)}
        };

        public SnipeSettings PokemonToSnipe = new SnipeSettings
        {
            Locations = new List<Location>
            {
                new Location(38.55680748646112, -121.2383794784546), //Dratini Spot
                new Location(-33.85901900, 151.21309800), //Magikarp Spot
                new Location(47.5014969, -122.0959568), //Eevee Spot
                new Location(51.5025343, -0.2055027) //Charmender Spot
            },
            Pokemon = new List<PokemonId>
            {
                PokemonId.Venusaur,
                PokemonId.Charizard,
                PokemonId.Blastoise,
                PokemonId.Beedrill,
                PokemonId.Raichu,
                PokemonId.Sandslash,
                PokemonId.Nidoking,
                PokemonId.Nidoqueen,
                PokemonId.Clefable,
                PokemonId.Ninetales,
                PokemonId.Golbat,
                PokemonId.Vileplume,
                PokemonId.Golduck,
                PokemonId.Primeape,
                PokemonId.Arcanine,
                PokemonId.Poliwrath,
                PokemonId.Alakazam,
                PokemonId.Machamp,
                PokemonId.Golem,
                PokemonId.Rapidash,
                PokemonId.Slowbro,
                PokemonId.Farfetchd,
                PokemonId.Muk,
                PokemonId.Cloyster,
                PokemonId.Gengar,
                PokemonId.Exeggutor,
                PokemonId.Marowak,
                PokemonId.Hitmonchan,
                PokemonId.Lickitung,
                PokemonId.Rhydon,
                PokemonId.Chansey,
                PokemonId.Kangaskhan,
                PokemonId.Starmie,
                PokemonId.MrMime,
                PokemonId.Scyther,
                PokemonId.Magmar,
                PokemonId.Electabuzz,
                PokemonId.Magmar,
                PokemonId.Jynx,
                PokemonId.Gyarados,
                PokemonId.Lapras,
                PokemonId.Ditto,
                PokemonId.Vaporeon,
                PokemonId.Jolteon,
                PokemonId.Flareon,
                PokemonId.Porygon,
                PokemonId.Kabutops,
                PokemonId.Aerodactyl,
                PokemonId.Snorlax,
                PokemonId.Articuno,
                PokemonId.Zapdos,
                PokemonId.Moltres,
                PokemonId.Dragonite,
                PokemonId.Mewtwo,
                PokemonId.Mew
            }
        };

        public List<PokemonId> PokemonToUseMasterball = new List<PokemonId>
        {
            PokemonId.Articuno,
            PokemonId.Zapdos,
            PokemonId.Moltres,
            PokemonId.Mew,
            PokemonId.Mewtwo
        };

        public static GlobalSettings Default => new GlobalSettings();
        
        public static GlobalSettings Load(string path)
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
                    Logger.Write("JSON Exception: " + exception.Message, LogLevel.Error);
                    return null;
                }
            }
            else
            {
                settings = new GlobalSettings();
            }

            if (settings.WebSocketPort == 0)
            {
                settings.WebSocketPort = 14251;
            }

            if (settings.PokemonToSnipe == null)
            {
                settings.PokemonToSnipe = Default.PokemonToSnipe;
            }

            if (settings.RenameTemplate == null)
            {
                settings.RenameTemplate = Default.RenameTemplate;
            }

            if (settings.SnipeLocationServer == null)
            {
                settings.SnipeLocationServer = Default.SnipeLocationServer;
            }

            settings.ProfilePath = profilePath;
            settings.ProfileConfigPath = profileConfigPath;
            settings.GeneralConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "config");

            var firstRun = !File.Exists(configFile);

            settings.Save(configFile);
            settings.Auth.Load(Path.Combine(profileConfigPath, "auth.json"));

            return firstRun ? null : settings;
        }

        public void Save(string fullPath)
        {
            var output = JsonConvert.SerializeObject(this, Formatting.Indented,
                new StringEnumConverter { CamelCaseText = true });

            var folder = Path.GetDirectoryName(fullPath);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(fullPath, output);
        }
    }

    public class ClientSettings : ISettings
    {
        // Never spawn at the same position.
        private readonly Random _rand = new Random();
        private readonly GlobalSettings _settings;

        public ClientSettings(GlobalSettings settings)
        {
            _settings = settings;
        }


        public string GoogleUsername => _settings.Auth.GoogleUsername;
        public string GooglePassword => _settings.Auth.GooglePassword;

        public string GoogleRefreshToken
        {
            get { return _settings.Auth.GoogleRefreshToken; }
            set
            {
                _settings.Auth.GoogleRefreshToken = value;
                _settings.Auth.Save();
            }
        }

        AuthType ISettings.AuthType
        {
            get { return _settings.Auth.AuthType; }

            set { _settings.Auth.AuthType = value; }
        }

        double ISettings.DefaultLatitude
        {
            get
            {
                return _settings.DefaultLatitude + _rand.NextDouble() * ((double)_settings.MaxSpawnLocationOffset / 111111);
            }

            set { _settings.DefaultLatitude = value; }
        }

        double ISettings.DefaultLongitude
        {
            get
            {
                return _settings.DefaultLongitude +
                       _rand.NextDouble() *
                       ((double)_settings.MaxSpawnLocationOffset / 111111 / Math.Cos(_settings.DefaultLatitude));
            }

            set { _settings.DefaultLongitude = value; }
        }

        double ISettings.DefaultAltitude
        {
            get { return _settings.DefaultAltitude; }

            set { _settings.DefaultAltitude = value; }
        }

        string ISettings.PtcPassword
        {
            get { return _settings.Auth.PtcPassword; }

            set { _settings.Auth.PtcPassword = value; }
        }

        string ISettings.PtcUsername
        {
            get { return _settings.Auth.PtcUsername; }

            set { _settings.Auth.PtcUsername = value; }
        }

        string ISettings.GoogleUsername
        {
            get { return _settings.Auth.GoogleUsername; }

            set { _settings.Auth.GoogleUsername = value; }
        }

        string ISettings.GooglePassword
        {
            get { return _settings.Auth.GooglePassword; }

            set { _settings.Auth.GooglePassword = value; }
        }
    }

    public class LogicSettings : ILogicSettings
    {
        private readonly GlobalSettings _settings;

        public LogicSettings(GlobalSettings settings)
        {
            _settings = settings;
        }

        public string ProfilePath => _settings.ProfilePath;
        public string ProfileConfigPath => _settings.ProfileConfigPath;
        public string GeneralConfigPath => _settings.GeneralConfigPath;
        public bool AutoUpdate => _settings.AutoUpdate;
        public bool TransferConfigAndAuthOnUpdate => _settings.TransferConfigAndAuthOnUpdate;
        public float KeepMinIvPercentage => _settings.KeepMinIvPercentage;
        public int KeepMinCp => _settings.KeepMinCp;
        public bool AutomaticallyLevelUpPokemon => _settings.AutomaticallyLevelUpPokemon;
        public string LevelUpByCPorIv => _settings.LevelUpByCPorIv;
        public float UpgradePokemonIvMinimum => _settings.UpgradePokemonIvMinimum;
        public float UpgradePokemonCpMinimum => _settings.UpgradePokemonCpMinimum;
        public double WalkingSpeedInKilometerPerHour => _settings.WalkingSpeedInKilometerPerHour;
        public bool EvolveAllPokemonWithEnoughCandy => _settings.EvolveAllPokemonWithEnoughCandy;
        public bool KeepPokemonsThatCanEvolve => _settings.KeepPokemonsThatCanEvolve;
        public bool TransferDuplicatePokemon => _settings.TransferDuplicatePokemon;
        public bool UseEggIncubators => _settings.UseEggIncubators;
        public int UseGreatBallAboveCp => _settings.UseGreatBallAboveCp;
        public int UseUltraBallAboveCp => _settings.UseUltraBallAboveCp;
        public int UseMasterBallAboveCp => _settings.UseMasterBallAboveCp;
        public int DelayBetweenPokemonCatch => _settings.DelayBetweenPokemonCatch;
        public int DelayBetweenPlayerActions => _settings.DelayBetweenPlayerActions;
        public bool UsePokemonToNotCatchFilter => _settings.UsePokemonToNotCatchFilter;
        public int KeepMinDuplicatePokemon => _settings.KeepMinDuplicatePokemon;
        public bool PrioritizeIvOverCp => _settings.PrioritizeIvOverCp;
        public int MaxTravelDistanceInMeters => _settings.MaxTravelDistanceInMeters;
        public string GpxFile => _settings.GpxFile;
        public bool UseGpxPathing => _settings.UseGpxPathing;
        public bool UseLuckyEggsWhileEvolving => _settings.UseLuckyEggsWhileEvolving;
        public int UseLuckyEggsMinPokemonAmount => _settings.UseLuckyEggsMinPokemonAmount;
        public bool EvolveAllPokemonAboveIv => _settings.EvolveAllPokemonAboveIv;
        public float EvolveAboveIvValue => _settings.EvolveAboveIvValue;
        public bool RenamePokemon => _settings.RenamePokemon;
        public bool RenameOnlyAboveIv => _settings.RenameOnlyAboveIv;
        public string RenameTemplate => _settings.RenameTemplate;
        public int AmountOfPokemonToDisplayOnStart => _settings.AmountOfPokemonToDisplayOnStart;
        public bool DumpPokemonStats => _settings.DumpPokemonStats;
        public string TranslationLanguageCode => _settings.TranslationLanguageCode;
        public IEnumerable<KeyValuePair<ItemId, int>> ItemRecycleFilter => _settings.ItemRecycleFilter;
        public IEnumerable<PokemonId> PokemonsToEvolve => _settings.PokemonsToEvolve;
        public IEnumerable<PokemonId> PokemonsNotToTransfer => _settings.PokemonsNotToTransfer;
        public ICollection<PokemonId> PokemonsNotToCatch => _settings.PokemonsToIgnore;
        public ICollection<PokemonId> PokemonToUseMasterball => _settings.PokemonToUseMasterball;
        public Dictionary<PokemonId, TransferFilter> PokemonsTransferFilter => _settings.PokemonsTransferFilter;
        public bool StartupWelcomeDelay => _settings.StartupWelcomeDelay;
        public bool SnipeAtPokestops => _settings.SnipeAtPokestops;
        public int MinPokeballsToSnipe => _settings.MinPokeballsToSnipe;
        public int MinPokeballsWhileSnipe => _settings.MinPokeballsWhileSnipe;
        public int MaxPokeballsPerPokemon => _settings.MaxPokeballsPerPokemon;

        public SnipeSettings PokemonToSnipe => _settings.PokemonToSnipe;
        public string SnipeLocationServer => _settings.SnipeLocationServer;
        public int SnipeLocationServerPort => _settings.SnipeLocationServerPort;
        public bool UseSnipeLocationServer => _settings.UseSnipeLocationServer;
        public bool UseTransferIvForSnipe => _settings.UseTransferIvForSnipe;
        public bool SnipeIgnoreUnknownIv => _settings.SnipeIgnoreUnknownIv;
        public int MinDelayBetweenSnipes => _settings.MinDelayBetweenSnipes;
        public int TotalAmountOfPokeballsToKeep => _settings.TotalAmountOfPokeballsToKeep;
        public int TotalAmountOfPotionsToKeep => _settings.TotalAmountOfPotionsToKeep;
        public int TotalAmountOfRevivesToKeep => _settings.TotalAmountOfRevivesToKeep;

        public bool Teleport => _settings.Teleport;
        public int DelayCatchIncensePokemon => _settings.DelayCatchIncensePokemon;
        public int DelayCatchNearbyPokemon => _settings.DelayCatchNearbyPokemon;
        public int DelayPositionCheckState => _settings.DelayPositionCheckState;
        public int DelayCatchLurePokemon => _settings.DelayCatchLurePokemon;
        public int DelayCatchPokemon => _settings.DelayCatchPokemon;
        public int DelayDisplayPokemon => _settings.DelayDisplayPokemon;
        public int DelayUseLuckyEgg => _settings.DelayUseLuckyEgg;
        public int DelaySoftbanRetry => _settings.DelaySoftbanRetry;
        public int DelayPokestop => _settings.DelayPokestop;
        public int DelayRecyleItem => _settings.DelayRecyleItem;
        public int DelaySnipePokemon => _settings.DelaySnipePokemon;
        public int DelayTransferPokemon => _settings.DelayTransferPokemon;
        public bool HumanizeThrows => _settings.HumanizeThrows;
        public double ThrowAccuracyMin => _settings.ThrowAccuracyMin;
        public double ThrowAccuracyMax => _settings.ThrowAccuracyMax;
        public double ThrowSpinFrequency => _settings.ThrowSpinFrequency;
    }
}