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
    public class AuthSettings
    {
        [JsonIgnore]
        private string _filePath;
        public AuthType AuthType;
        public string GoogleRefreshToken = "";
        public string GoogleUsername;
        public string GooglePassword;
        public string PtcUsername;
        public string PtcPassword;
    }

    public class DelaySettings
    {//delays
        public int DelayBetweenPlayerActions = 5;
        public int DelayPositionCheckState = 200;
        public int DelayPokestop = 1000;
        public int DelayCatchPokemon = 5;
        public int DelayBetweenPokemonCatch = 5;
        public int DelayCatchNearbyPokemon = 5;
        public int DelayCatchLurePokemon = 5;
        public int DelayCatchIncensePokemon = 5;
        public int DelayEvolvePokemon = 5;
        public double DelayEvolveVariation = 0.3;
        public int DelayTransferPokemon = 5;
        public int DelayDisplayPokemon = 5;
        public int DelayUseLuckyEgg = 5;
        public int DelaySoftbanRetry = 5;
        public int DelayRecyleItem = 5;
        public int DelaySnipePokemon = 250;
        public int MinDelayBetweenSnipes = 10000;
        public double SnipingScanOffset = 0.003;
    }

    public class StartUpSettings
    {
        //bot start
        public bool AutoUpdate = true;
        public bool TransferConfigAndAuthOnUpdate = true;
        public bool DumpPokemonStats = false;
        public int AmountOfPokemonToDisplayOnStart = 10;
        public bool StartupWelcomeDelay = false;
        public string TranslationLanguageCode = "en";
        public int WebSocketPort = 14251;
        //display
        public bool DisplayPokemonMaxPoweredCp = true;
        public bool DisplayPokemonMovesetRank = true;
    }

    public class PokemonConfig
    {
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
        public float KeepMinIvPercentage = 95;
        public int KeepMinDuplicatePokemon = 1;
        public bool KeepPokemonsThatCanEvolve = false;

        //evolve
        public bool EvolveAllPokemonWithEnoughCandy = true;
        public bool EvolveAllPokemonAboveIv = false;
        public float EvolveAboveIvValue = 95;
        public bool UseLuckyEggsWhileEvolving = false;
        public int UseLuckyEggsMinPokemonAmount = 50;

        //levelup
        public bool AutomaticallyLevelUpPokemon = false;
        public string LevelUpByCPorIv = "iv";
        public float UpgradePokemonCpMinimum = 1000;
        public float UpgradePokemonIvMinimum = 95;

        //favorite
        public bool AutoFavoritePokemon = false;
        public float FavoriteMinIvPercentage = 95;
    }

    public class LocationSettings
    {
        //coords and movement
        public bool Teleport = false;
        //TeleAI will override Teleport if both enabled
        public bool TeleAI = false;
        public double DefaultLatitude = 40.785091;
        public double DefaultLongitude = -73.968285;
        public double DefaultAltitude = 10;
        public double WalkingSpeedInKilometerPerHour = 50.0;
        public int MaxSpawnLocationOffset = 10;
        public int MaxTravelDistanceInMeters = 1000;
        public bool UseGpxPathing = false;
        public string GpxFile = "GPXPath.GPX";
    }

    public class CatchSettings
    {
        public bool CatchWildPokemon = true;

        //catch
        public bool HumanizeThrows = false;
        public double ThrowAccuracyMin = 0.80;
        public double ThrowAccuracyMax = 1.00;
        public double ThrowSpinFrequency = 0.80;
        public int MaxPokeballsPerPokemon = 6;
        public int UseGreatBallAboveIv = 80;
        public int UseUltraBallAboveIv = 90;
        public double UseGreatBallBelowCatchProbability = 0.35;
        public double UseUltraBallBelowCatchProbability = 0.2;
        public bool UsePokemonToNotCatchFilter = false;

        //berries
        public int UseBerryMinCp = 450;
        public float UseBerryMinIv = 95;
        public double UseBerryBelowCatchProbability = 0.25;
    }

    public class RecycleSettings
    {
        //recycle
        public bool AutomaticInventoryManagement = false;
        public int AutomaticMaxAllPokeballs = 100;
        public int AutomaticMaxAllPotions = 60;
        public int AutomaticMaxAllRevives = 80;
        public int AutomaticMaxAllBerries = 50;
        public int TotalAmountOfPokeballsToKeep = 0;
        public int TotalAmountOfGreatballsToKeep = 40;
        public int TotalAmountOfUltraballsToKeep = 60;
        public int TotalAmountOfMasterballsToKeep = 100;
        public int TotalAmountOfPotionsToKeep = 0;
        public int TotalAmountOfSuperPotionsToKeep = 0;
        public int TotalAmountOfHyperPotionsToKeep = 20;
        public int TotalAmountOfMaxPotionsToKeep = 40;
        public int TotalAmountOfRevivesToKeep = 20;
        public int TotalAmountOfMaxRevivesToKeep = 60;
        public int TotalAmountOfRazzToKeep = 50;
        //public int TotalAmountOfBlukToKeep = 50;
        //public int TotalAmountOfNanabToKeep = 50;
        //public int TotalAmountOfPinapToKeep = 50;
        //public int TotalAmountOfWeparToKeep = 50;
        public double RecycleInventoryAtUsagePercentage = 0.90;
    }

    public class SnipeConfig
    {
        //snipe
        public bool SnipeAtPokestops = false;
        public bool SnipeIgnoreUnknownIv = false;
        public bool UseTransferIvForSnipe = false;
        public int MinPokeballsToSnipe = 20;
        public int MinPokeballsWhileSnipe = 5;
        public bool UseSnipeLocationServer = false;
        public bool UsePokeSnipersLocationServer = false;
        public string SnipeLocationServer = "localhost";
        public int SnipeLocationServerPort = 16969;
        public int SnipeRequestTimeoutSeconds = 5;
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

        public StartUpSettings StartUpSettings = new StartUpSettings();

        public LocationSettings LocationSettings = new LocationSettings();

        public DelaySettings DelaySettings = new DelaySettings();

        public PokemonConfig PokemonSettings = new PokemonConfig();

        public CatchSettings CatchSettings = new CatchSettings();

        public RecycleSettings RecycleSettings = new RecycleSettings();

        public SnipeConfig SnipeSettings = new SnipeConfig();





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
            new KeyValuePair<ItemId, int>(ItemId.ItemSpecialCamera, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasicUnlimited, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasic, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemPokemonStorageUpgrade, 100),
            new KeyValuePair<ItemId, int>(ItemId.ItemItemStorageUpgrade, 100)
        };

        public List<PokemonId> PokemonsNotToTransfer = new List<PokemonId>
        {
            //criteria: from SS Tier to A Tier + Regional Exclusive
            //PokemonId.Venusaur,
            //PokemonId.Charizard,
            //PokemonId.Blastoise,
            //PokemonId.Nidoqueen,
            //PokemonId.Nidoking,
            //PokemonId.Clefable,
            //PokemonId.Vileplume,
            //PokemonId.Golduck,
            //PokemonId.Arcanine,
            //PokemonId.Poliwrath,
            //PokemonId.Machamp,
            //PokemonId.Victreebel,
            //PokemonId.Golem,
            //PokemonId.Slowbro,
            //PokemonId.Farfetchd,
            //PokemonId.Muk,
            //PokemonId.Exeggutor,
            //PokemonId.Lickitung,
            //PokemonId.Chansey,
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
            PokemonId.Rattata,
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
            PokemonId.Zubat,
            //PokemonId.Paras,
            //PokemonId.Venonat,
            //PokemonId.Psyduck,
            //PokemonId.Slowpoke,
            PokemonId.Doduo
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
            //criteria: based on NY Central Park and Tokyo variety + sniping optimization v4.1
            {PokemonId.Venusaur, new TransferFilter(1750, 80, 1)},
            {PokemonId.Charizard, new TransferFilter(1750, 20, 1)},
            {PokemonId.Blastoise, new TransferFilter(1750, 50, 1)},
            {PokemonId.Nidoqueen, new TransferFilter(1750, 80, 1)},
            {PokemonId.Nidoking, new TransferFilter(1750, 80, 1)},
            {PokemonId.Clefable, new TransferFilter(1500, 60, 1)},
            {PokemonId.Vileplume, new TransferFilter(1750, 80, 1)},
            {PokemonId.Golduck, new TransferFilter(1750, 80, 1)},
            {PokemonId.Arcanine, new TransferFilter(2250, 90, 1)},
            {PokemonId.Poliwrath, new TransferFilter(1750, 80, 1)},
            {PokemonId.Machamp, new TransferFilter(1250, 80, 1)},
            {PokemonId.Victreebel, new TransferFilter(1250, 60, 1)},
            {PokemonId.Golem, new TransferFilter(1500, 80, 1)},
            {PokemonId.Slowbro, new TransferFilter(1750, 90, 1)},
            {PokemonId.Farfetchd, new TransferFilter(1250, 90, 1)},
            {PokemonId.Muk, new TransferFilter(2000, 80, 1)},
            {PokemonId.Exeggutor, new TransferFilter(2250, 80, 1)},
            {PokemonId.Lickitung, new TransferFilter(1500, 80, 1)},
            {PokemonId.Chansey, new TransferFilter(1500, 95, 1)},
            {PokemonId.Kangaskhan, new TransferFilter(1500, 60, 1)},
            {PokemonId.MrMime, new TransferFilter(1250, 80, 1)},
            {PokemonId.Scyther, new TransferFilter(1750, 90, 1)},
            {PokemonId.Jynx, new TransferFilter(1250, 90, 1)},
            {PokemonId.Electabuzz, new TransferFilter(1500, 80, 1)},
            {PokemonId.Magmar, new TransferFilter(1750, 90, 1)},
            {PokemonId.Pinsir, new TransferFilter(2000, 98, 1)},
            {PokemonId.Tauros, new TransferFilter(500, 90, 1)},
            {PokemonId.Gyarados, new TransferFilter(2000, 90, 1)},
            {PokemonId.Lapras, new TransferFilter(2250, 90, 1)},
            {PokemonId.Eevee, new TransferFilter(1500, 98, 1)},
            {PokemonId.Vaporeon, new TransferFilter(2250, 98, 1)},
            {PokemonId.Jolteon, new TransferFilter(2250, 95, 1)},
            {PokemonId.Flareon, new TransferFilter(2250, 95, 1)},
            {PokemonId.Porygon, new TransferFilter(1500, 95, 1)},
            {PokemonId.Aerodactyl, new TransferFilter(2000, 95, 1)},
            {PokemonId.Snorlax, new TransferFilter(2750, 96, 1)},
            {PokemonId.Dragonite, new TransferFilter(2750, 90, 1)}
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
                PokemonId.Bulbasaur,
                PokemonId.Ivysaur,
                PokemonId.Venusaur,
                PokemonId.Charmander,
                PokemonId.Charmeleon,
                PokemonId.Charizard,
                PokemonId.Squirtle,
                PokemonId.Wartortle,
                PokemonId.Blastoise,
                PokemonId.Butterfree,
                PokemonId.Beedrill,
                PokemonId.Pidgeot,
                PokemonId.Raticate,
                PokemonId.Fearow,
                PokemonId.Arbok,
                PokemonId.Pikachu,
                PokemonId.Raichu,
                PokemonId.Sandslash,
                PokemonId.Nidoqueen,
                PokemonId.Nidoking,
                PokemonId.Clefable,
                PokemonId.Ninetales,
                PokemonId.Wigglytuff,
                PokemonId.Golbat,
                PokemonId.Vileplume,
                PokemonId.Parasect,
                PokemonId.Venomoth,
                PokemonId.Dugtrio,
                PokemonId.Persian,
                PokemonId.Golduck,
                PokemonId.Primeape,
                PokemonId.Growlithe,
                PokemonId.Arcanine,
                PokemonId.Poliwag,
                PokemonId.Poliwhirl,
                PokemonId.Poliwrath,
                PokemonId.Abra,
                PokemonId.Kadabra,
                PokemonId.Alakazam,
                PokemonId.Machop,
                PokemonId.Machoke,
                PokemonId.Machamp,
                PokemonId.Victreebel,
                PokemonId.Tentacruel,
                PokemonId.Golem,
                PokemonId.Rapidash,
                PokemonId.Slowbro,
                PokemonId.Magneton,
                PokemonId.Farfetchd,
                PokemonId.Dodrio,
                PokemonId.Dewgong,
                PokemonId.Grimer,
                PokemonId.Muk,
                PokemonId.Cloyster,
                PokemonId.Gastly,
                PokemonId.Haunter,
                PokemonId.Gengar,
                PokemonId.Onix,
                PokemonId.Hypno,
                PokemonId.Kingler,
                PokemonId.Electrode,
                PokemonId.Exeggutor,
                PokemonId.Marowak,
                PokemonId.Hitmonlee,
                PokemonId.Hitmonchan,
                PokemonId.Lickitung,
                PokemonId.Koffing,
                PokemonId.Weezing,
                PokemonId.Rhyhorn,
                PokemonId.Rhydon,
                PokemonId.Chansey,
                PokemonId.Tangela,
                PokemonId.Kangaskhan,
                PokemonId.Seadra,
                PokemonId.Seaking,
                PokemonId.Starmie,
                PokemonId.MrMime,
                PokemonId.Scyther,
                PokemonId.Jynx,
                PokemonId.Electabuzz,
                PokemonId.Magmar,
                PokemonId.Pinsir,
                PokemonId.Tauros,
                PokemonId.Magikarp,
                PokemonId.Gyarados,
                PokemonId.Lapras,
                PokemonId.Ditto,
                PokemonId.Eevee,
                PokemonId.Vaporeon,
                PokemonId.Jolteon,
                PokemonId.Flareon,
                PokemonId.Porygon,
                PokemonId.Omanyte,
                PokemonId.Omastar,
                PokemonId.Kabuto,
                PokemonId.Kabutops,
                PokemonId.Aerodactyl,
                PokemonId.Snorlax,
                PokemonId.Articuno,
                PokemonId.Zapdos,
                PokemonId.Moltres,
                PokemonId.Dratini,
                PokemonId.Dragonair,
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

        public bool TeleAI { get; internal set; }
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
            get { return null; }
            set { GoogleRefreshToken = null; }
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
                return _settings.LocationSettings.DefaultLatitude + _rand.NextDouble() * ((double)_settings.LocationSettings.MaxSpawnLocationOffset / 111111);
            }

            set { _settings.LocationSettings.DefaultLatitude = value; }
        }

        double ISettings.DefaultLongitude
        {
            get
            {
                return _settings.LocationSettings.DefaultLongitude +
                       _rand.NextDouble() *
                       ((double)_settings.LocationSettings.MaxSpawnLocationOffset / 111111 / Math.Cos(_settings.LocationSettings.DefaultLatitude));
            }

            set { _settings.LocationSettings.DefaultLongitude = value; }
        }

        double ISettings.DefaultAltitude
        {
            get { return _settings.LocationSettings.DefaultAltitude; }

            set { _settings.LocationSettings.DefaultAltitude = value; }
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
        public int SnipeRequestTimeoutSeconds => _settings.SnipeSettings.SnipeRequestTimeoutSeconds * 1000;
        public string GeneralConfigPath => _settings.GeneralConfigPath;
        public bool AutoUpdate => _settings.StartUpSettings.AutoUpdate;
        public bool TransferConfigAndAuthOnUpdate => _settings.StartUpSettings.TransferConfigAndAuthOnUpdate;
        public float KeepMinIvPercentage => _settings.PokemonSettings.KeepMinIvPercentage;
        public int KeepMinCp => _settings.PokemonSettings.KeepMinCp;
        public bool AutomaticallyLevelUpPokemon => _settings.PokemonSettings.AutomaticallyLevelUpPokemon;
        public string LevelUpByCPorIv => _settings.PokemonSettings.LevelUpByCPorIv;
        public float UpgradePokemonIvMinimum => _settings.PokemonSettings.UpgradePokemonIvMinimum;
        public float UpgradePokemonCpMinimum => _settings.PokemonSettings.UpgradePokemonCpMinimum;
        public double WalkingSpeedInKilometerPerHour => _settings.LocationSettings.WalkingSpeedInKilometerPerHour;
        public bool EvolveAllPokemonWithEnoughCandy => _settings.PokemonSettings.EvolveAllPokemonWithEnoughCandy;
        public bool KeepPokemonsThatCanEvolve => _settings.PokemonSettings.KeepPokemonsThatCanEvolve;
        public bool TransferDuplicatePokemon => _settings.PokemonSettings.TransferDuplicatePokemon;
        public bool UseEggIncubators => _settings.PokemonSettings.UseEggIncubators;
        public int UseGreatBallAboveIv => _settings.CatchSettings.UseGreatBallAboveIv;
        public int UseUltraBallAboveIv => _settings.CatchSettings.UseUltraBallAboveIv;
        public double UseUltraBallBelowCatchProbability => _settings.CatchSettings.UseUltraBallBelowCatchProbability;
        public double UseGreatBallBelowCatchProbability => _settings.CatchSettings.UseGreatBallBelowCatchProbability;
        public int DelayBetweenPokemonCatch => _settings.DelaySettings.DelayBetweenPokemonCatch;
        public int DelayBetweenPlayerActions => _settings.DelaySettings.DelayBetweenPlayerActions;
        public bool UsePokemonToNotCatchFilter => _settings.CatchSettings.UsePokemonToNotCatchFilter;
        public int KeepMinDuplicatePokemon => _settings.PokemonSettings.KeepMinDuplicatePokemon;
        public bool PrioritizeIvOverCp => _settings.PokemonSettings.PrioritizeIvOverCp;
        public int MaxTravelDistanceInMeters => _settings.LocationSettings.MaxTravelDistanceInMeters;
        public string GpxFile => _settings.LocationSettings.GpxFile;
        public bool UseGpxPathing => _settings.LocationSettings.UseGpxPathing;
        public bool UseLuckyEggsWhileEvolving => _settings.PokemonSettings.UseLuckyEggsWhileEvolving;
        public int UseLuckyEggsMinPokemonAmount => _settings.PokemonSettings.UseLuckyEggsMinPokemonAmount;
        public bool EvolveAllPokemonAboveIv => _settings.PokemonSettings.EvolveAllPokemonAboveIv;
        public float EvolveAboveIvValue => _settings.PokemonSettings.EvolveAboveIvValue;
        public bool RenamePokemon => _settings.PokemonSettings.RenamePokemon;
        public bool RenameOnlyAboveIv => _settings.PokemonSettings.RenameOnlyAboveIv;
        public float FavoriteMinIvPercentage => _settings.PokemonSettings.FavoriteMinIvPercentage;
        public bool AutoFavoritePokemon => _settings.PokemonSettings.AutoFavoritePokemon;
        public string RenameTemplate => _settings.PokemonSettings.RenameTemplate;
        public int AmountOfPokemonToDisplayOnStart => _settings.StartUpSettings.AmountOfPokemonToDisplayOnStart;
        public bool DisplayPokemonMaxPoweredCp => _settings.StartUpSettings.DisplayPokemonMaxPoweredCp;
        public bool DisplayPokemonMovesetRank => _settings.StartUpSettings.DisplayPokemonMovesetRank;
        public bool DumpPokemonStats => _settings.StartUpSettings.DumpPokemonStats;
        public string TranslationLanguageCode => _settings.StartUpSettings.TranslationLanguageCode;
        public ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter => _settings.ItemRecycleFilter;
        public ICollection<PokemonId> PokemonsToEvolve => _settings.PokemonsToEvolve;
        public ICollection<PokemonId> PokemonsNotToTransfer => _settings.PokemonsNotToTransfer;
        public ICollection<PokemonId> PokemonsNotToCatch => _settings.PokemonsToIgnore;
        public ICollection<PokemonId> PokemonToUseMasterball => _settings.PokemonToUseMasterball;
        public Dictionary<PokemonId, TransferFilter> PokemonsTransferFilter => _settings.PokemonsTransferFilter;
        public bool StartupWelcomeDelay => _settings.StartUpSettings.StartupWelcomeDelay;
        public bool SnipeAtPokestops => _settings.SnipeSettings.SnipeAtPokestops;
        public int MinPokeballsToSnipe => _settings.SnipeSettings.MinPokeballsToSnipe;
        public int MinPokeballsWhileSnipe => _settings.SnipeSettings.MinPokeballsWhileSnipe;
        public int MaxPokeballsPerPokemon => _settings.CatchSettings.MaxPokeballsPerPokemon;
        public SnipeSettings PokemonToSnipe => _settings.PokemonToSnipe;
        public string SnipeLocationServer => _settings.SnipeSettings.SnipeLocationServer;
        public int SnipeLocationServerPort => _settings.SnipeSettings.SnipeLocationServerPort;
        public bool UseSnipeLocationServer => _settings.SnipeSettings.UseSnipeLocationServer;
        public bool UsePokeSnipersLocationServer => _settings.SnipeSettings.UsePokeSnipersLocationServer;
        public bool UseTransferIvForSnipe => _settings.SnipeSettings.UseTransferIvForSnipe;
        public bool SnipeIgnoreUnknownIv => _settings.SnipeSettings.SnipeIgnoreUnknownIv;
        public int MinDelayBetweenSnipes => _settings.DelaySettings.MinDelayBetweenSnipes;
        public double SnipingScanOffset => _settings.DelaySettings.SnipingScanOffset;
        public bool AutomaticInventoryManagement => _settings.RecycleSettings.AutomaticInventoryManagement;
        public int AutomaticMaxAllPokeballs => _settings.RecycleSettings.AutomaticMaxAllPokeballs;
        public int AutomaticMaxAllPotions => _settings.RecycleSettings.AutomaticMaxAllPotions;
        public int AutomaticMaxAllRevives => _settings.RecycleSettings.AutomaticMaxAllRevives;
        public int AutomaticMaxAllBerries => _settings.RecycleSettings.AutomaticMaxAllBerries;
        public int TotalAmountOfPokeballsToKeep => _settings.RecycleSettings.TotalAmountOfPokeballsToKeep;
        public int TotalAmountOfGreatballsToKeep => _settings.RecycleSettings.TotalAmountOfGreatballsToKeep;
        public int TotalAmountOfUltraballsToKeep => _settings.RecycleSettings.TotalAmountOfUltraballsToKeep;
        public int TotalAmountOfMasterballsToKeep => _settings.RecycleSettings.TotalAmountOfMasterballsToKeep;
        public int TotalAmountOfRazzToKeep => _settings.RecycleSettings.TotalAmountOfRazzToKeep;
        //public int TotalAmountOfBlukToKeep => _settings.RecycleSettings.TotalAmountOfBlukToKeep;
        //public int TotalAmountOfNanabToKeep => _settings.RecycleSettings.TotalAmountOfNanabToKeep;
        //public int TotalAmountOfPinapToKeep => _settings.RecycleSettings.TotalAmountOfPinapToKeep;
        //public int TotalAmountOfWeparToKeep => _settings.RecycleSettings.TotalAmountOfWeparToKeep;
        public int TotalAmountOfPotionsToKeep => _settings.RecycleSettings.TotalAmountOfPotionsToKeep;
        public int TotalAmountOfSuperPotionsToKeep => _settings.RecycleSettings.TotalAmountOfSuperPotionsToKeep;
        public int TotalAmountOfHyperPotionsToKeep => _settings.RecycleSettings.TotalAmountOfHyperPotionsToKeep;
        public int TotalAmountOfMaxPotionsToKeep => _settings.RecycleSettings.TotalAmountOfMaxPotionsToKeep;
        public int TotalAmountOfRevivesToKeep => _settings.RecycleSettings.TotalAmountOfRevivesToKeep;
        public int TotalAmountOfMaxRevivesToKeep => _settings.RecycleSettings.TotalAmountOfRevivesToKeep;
        public bool Teleport => _settings.LocationSettings.Teleport;
        public bool TeleAI => _settings.TeleAI;
        public int DelayCatchIncensePokemon => _settings.DelaySettings.DelayCatchIncensePokemon;
        public int DelayCatchNearbyPokemon => _settings.DelaySettings.DelayCatchNearbyPokemon;
        public int DelayPositionCheckState => _settings.DelaySettings.DelayPositionCheckState;
        public int DelayCatchLurePokemon => _settings.DelaySettings.DelayCatchLurePokemon;
        public int DelayCatchPokemon => _settings.DelaySettings.DelayCatchPokemon;
        public int DelayDisplayPokemon => _settings.DelaySettings.DelayDisplayPokemon;
        public int DelayUseLuckyEgg => _settings.DelaySettings.DelayUseLuckyEgg;
        public int DelaySoftbanRetry => _settings.DelaySettings.DelaySoftbanRetry;
        public int DelayPokestop => _settings.DelaySettings.DelayPokestop;
        public int DelayRecyleItem => _settings.DelaySettings.DelayRecyleItem;
        public int DelaySnipePokemon => _settings.DelaySettings.DelaySnipePokemon;
        public int DelayTransferPokemon => _settings.DelaySettings.DelayTransferPokemon;
        public int DelayEvolvePokemon => _settings.DelaySettings.DelayEvolvePokemon;
        public double DelayEvolveVariation => _settings.DelaySettings.DelayEvolveVariation;
        public double RecycleInventoryAtUsagePercentage => _settings.RecycleSettings.RecycleInventoryAtUsagePercentage;
        public bool HumanizeThrows => _settings.CatchSettings.HumanizeThrows;
        public double ThrowAccuracyMin => _settings.CatchSettings.ThrowAccuracyMin;
        public double ThrowAccuracyMax => _settings.CatchSettings.ThrowAccuracyMax;
        public double ThrowSpinFrequency => _settings.CatchSettings.ThrowSpinFrequency;
        public int UseBerryMinCp => _settings.CatchSettings.UseBerryMinCp;
        public float UseBerryMinIv => _settings.CatchSettings.UseBerryMinIv;
        public double UseBerryBelowCatchProbability => _settings.CatchSettings.UseBerryBelowCatchProbability;

        public bool CatchWildPokemon => _settings.CatchSettings.CatchWildPokemon;

    }
    public class TeleSettings
    {
        [JsonIgnore]
        public string GeneralConfigPath;
        [JsonIgnore]
        public string ProfilePath;
        [JsonIgnore]
        public string ProfileConfigPath;

        //bot start
        public int waitTime50 = 0;
        public int waitTime100 = 0;
        public int waitTime200 = 0;
        public int waitTime300 = 0;
        public int waitTime400 = 0;
        public int waitTime500 = 0;
        public int waitTime600 = 0;
        public int waitTime700 = 0;
        public int waitTime800 = 0;
        public int waitTime900 = 0;
        public int waitTime1000 = 0;
        public int waitTime1250 = 0;
        public int waitTime1500 = 0;
        public int waitTime2000 = 0;
    }
    public class TeleLogicSettings
    {
        public TeleSettings _settings;


        public TeleLogicSettings(TeleSettings settings)
        {
            _settings = settings;
        }

        public int waitTime50 => _settings.waitTime50;
        public int waitTime100 => _settings.waitTime100;
        public int waitTime200 => _settings.waitTime200;
        public int waitTime300 => _settings.waitTime300;
        public int waitTime400 => _settings.waitTime400;
        public int waitTime500 => _settings.waitTime500;
        public int waitTime600 => _settings.waitTime600;
        public int waitTime700 => _settings.waitTime700;
        public int waitTime800 => _settings.waitTime800;
        public int waitTime900 => _settings.waitTime900;
        public int waitTime1000 => _settings.waitTime1000;
        public int waitTime1250 => _settings.waitTime1250;
        public int waitTime1500 => _settings.waitTime1500;
        public int waitTime2000 => _settings.waitTime2000;
    }
}

