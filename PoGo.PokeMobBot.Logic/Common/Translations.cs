#region using directives

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace PoGo.PokeMobBot.Logic.Common
{
    using POGOProtos.Enums;

    public interface ITranslation
    {
        string GetTranslation(TranslationString translationString, params object[] data);

        string GetTranslation(TranslationString translationString);

        string GetPokemonName(PokemonId pkmnId);
    }

    public enum TranslationString
    {
        Pokeball,
        GreatPokeball,
        UltraPokeball,
        MasterPokeball,
        LogLevelDebug,
        LogLevelPokestop,
        WrongAuthType,
        FarmPokestopsOutsideRadius,
        FarmPokestopsNoUsableFound,
        EventFortUsed,
        EventFortFailed,
        EventFortTargeted,
        EventProfileLogin,
        EventUsedLuckyEgg,
        EventUseLuckyEggMinPokemonCheck,
        EventPokemonEvolvedSuccess,
        EventPokemonEvolvedFailed,
        EventPokemonTransferred,
        EventItemRecycled,
        EventPokemonCapture,
        EventNoPokeballs,
        CatchStatusAttempt,
        CatchStatus,
        Candies,
        UnhandledGpxData,
        DisplayHighestsHeader,
        CommonWordPerfect,
        CommonWordName,
        DisplayHighestsCpHeader,
        DisplayHighestsPerfectHeader,
        WelcomeWarning,
        IncubatorPuttingEgg,
        IncubatorStatusUpdate,
        DisplayHighestsLevelHeader,
        LogEntryError,
        LogEntryAttention,
        LogEntryInfo,
        LogEntryPokestop,
        LogEntryFarming,
        LogEntryRecycling,
        LogEntryPkmn,
        LogEntryTransfered,
        LogEntryEvolved,
        LogEntryBerry,
        LogEntryEgg,
        LogEntryDebug,
        LogEntryUpdate,
        LoggingIn,
        PtcOffline,
        TryingAgainIn,
        AccountNotVerified,
        CommonWordUnknown,
        OpeningGoogleDevicePage,
        CouldntCopyToClipboard,
        CouldntCopyToClipboard2,
        RealisticTravelDetected,
        NotRealisticTravel,
        CoordinatesAreInvalid,
        GotUpToDateVersion,
        AutoUpdaterDisabled,
        DownloadingUpdate,
        FinishedDownloadingRelease,
        FinishedUnpackingFiles,
        FinishedTransferringConfig,
        UpdateFinished,
        LookingForIncensePokemon,
        PokemonSkipped,
        ZeroPokeballInv,
        CurrentPokeballInv,
        CheckingForBallsToRecycle,
        CheckingForPotionsToRecycle,
        CheckingForRevivesToRecycle,
        PokeballsToKeepIncorrect,
        PotionsToKeepIncorrect,
        RevivesToKeepIncorrect,
        InvFullTransferring,
        InvFullTransferManually,
        InvFullPokestopLooting,
        IncubatorEggHatched,
        EncounterProblem,
        EncounterProblemLurePokemon,
        LookingForPokemon,
        LookingForLurePokemon,
        DesiredDestTooFar,
        PokemonRename,
        PokemonIgnoreFilter,
        CatchStatusError,
        CatchStatusEscape,
        CatchStatusFlee,
        CatchStatusMissed,
        CatchStatusSuccess,
        CatchTypeNormal,
        CatchTypeLure,
        CatchTypeIncense,
        WebSocketFailStart,
        StatsTemplateString,
        StatsXpTemplateString,
        RequireInputText,
        GoogleTwoFactorAuth,
        GoogleTwoFactorAuthExplanation,
        GoogleError,
        MissingCredentialsGoogle,
        MissingCredentialsPtc,
        SnipeScan,
        SnipeScanEx,
        NoPokemonToSnipe,
        NotEnoughPokeballsToSnipe,
        DisplayHighestMove1Header,
        DisplayHighestMove2Header,
        UseBerry
    }

    public class Translation : ITranslation
    {
        [JsonProperty("TranslationStrings",
            ItemTypeNameHandling = TypeNameHandling.Arrays,
            ItemConverterType = typeof(KeyValuePairConverter),
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Populate)]
        //Default Translations (ENGLISH)        
        private readonly List<KeyValuePair<TranslationString, string>> _translationStrings = new List
            <KeyValuePair<TranslationString, string>>
        {
            new KeyValuePair<TranslationString, string>(TranslationString.Pokeball, "PokeBall"),
            new KeyValuePair<TranslationString, string>(TranslationString.GreatPokeball, "GreatBall"),
            new KeyValuePair<TranslationString, string>(TranslationString.UltraPokeball, "UltraBall"),
            new KeyValuePair<TranslationString, string>(TranslationString.MasterPokeball, "MasterBall"),
            new KeyValuePair<TranslationString, string>(TranslationString.WrongAuthType,
                "Unknown AuthType in config.json"),
            new KeyValuePair<TranslationString, string>(TranslationString.FarmPokestopsOutsideRadius,
                "You're outside of your defined radius! Walking to start ({0}m away) in 5 seconds. Is your Coords.ini file correct?"),
            new KeyValuePair<TranslationString, string>(TranslationString.FarmPokestopsNoUsableFound,
                "No usable PokeStops found in your area. Is your maximum distance too small?"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventFortUsed,
                "Name: {0} XP: {1}, Gems: {2}, Items: {3}"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventFortFailed,
                "Name: {0} INFO: Looting failed, possible softban. Unban in: {1}/{2}"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventFortTargeted,
                "Arriving to Pokestop: {0} in ({1}m)"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventProfileLogin, "Playing as {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventUsedLuckyEgg,
                "Used Lucky Egg, remaining: {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventUseLuckyEggMinPokemonCheck,
                "Not enough Pokemon to trigger a lucky egg. Waiting for {0} more. ({1}/{2})"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventPokemonEvolvedSuccess,
                "{0} successfully for {1}xp"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventPokemonEvolvedFailed,
                "Failed {0}. Result was {1}, stopping evolving {2}"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventPokemonTransferred,
                "{0}\t- CP: {1}  IV: {2}%   [Best CP: {3}  IV: {4}%] (Candies: {5})"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventItemRecycled, "{0}x {1}"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventPokemonCapture,
                "({0}) | ({1}) {2} Lvl: {3} CP: ({4}/{5}) IV: {6}% | Chance: {7}% | {8}m dist | with a {9} ({10} left). | {11}"),
            new KeyValuePair<TranslationString, string>(TranslationString.EventNoPokeballs,
                "No Pokeballs - We missed a {0} with CP {1}"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchStatusAttempt, "{0} Attempt #{1}"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchStatus, "{0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.Candies, "Candies: {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.UnhandledGpxData,
                "Unhandled data in GPX file, attempting to skip."),
            new KeyValuePair<TranslationString, string>(TranslationString.DisplayHighestsHeader, "Pokemons"),
            new KeyValuePair<TranslationString, string>(TranslationString.CommonWordPerfect, "perfect"),
            new KeyValuePair<TranslationString, string>(TranslationString.CommonWordName, "name"),
            new KeyValuePair<TranslationString, string>(TranslationString.CommonWordUnknown, "Unknown"),
            new KeyValuePair<TranslationString, string>(TranslationString.DisplayHighestsCpHeader, "DisplayHighestsCP"),
            new KeyValuePair<TranslationString, string>(TranslationString.DisplayHighestsPerfectHeader,
                "DisplayHighestsPerfect"),
            new KeyValuePair<TranslationString, string>(TranslationString.DisplayHighestsLevelHeader,
                "DisplayHighestsLevel"),
            new KeyValuePair<TranslationString, string>(TranslationString.WelcomeWarning,
                "Make sure Lat & Lng are right. Exit Program if not! Lat: {0} Lng: {1}"),
            new KeyValuePair<TranslationString, string>(TranslationString.IncubatorPuttingEgg,
                "Putting egg in incubator: {0:0.00}km left"),
            new KeyValuePair<TranslationString, string>(TranslationString.IncubatorStatusUpdate,
                "Incubator status update: {0:0.00}km left"),
            new KeyValuePair<TranslationString, string>(TranslationString.IncubatorEggHatched,
                "Incubated egg has hatched: {0} | Lvl: {1} CP: ({2}/{3}) IV: {4}%"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryError, "ERROR"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryAttention, "ATTENTION"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryInfo, "INFO"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryPokestop, "POKESTOP"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryFarming, "FARMING"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryRecycling, "RECYCLING"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryPkmn, "PKMN"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryTransfered, "TRANSFERED"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryEvolved, "EVOLVED"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryBerry, "BERRY"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryEgg, "EGG"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryDebug, "DEBUG"),
            new KeyValuePair<TranslationString, string>(TranslationString.LogEntryUpdate, "UPDATE"),
            new KeyValuePair<TranslationString, string>(TranslationString.LoggingIn, "Logging in using {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.PtcOffline,
                "PTC Servers are probably down OR your credentials are wrong. Try google"),
            new KeyValuePair<TranslationString, string>(TranslationString.TryingAgainIn,
                "Trying again in {0} seconds..."),
            new KeyValuePair<TranslationString, string>(TranslationString.AccountNotVerified,
                "Account not verified! Exiting..."),
            new KeyValuePair<TranslationString, string>(TranslationString.OpeningGoogleDevicePage,
                "Opening Google Device page. Please paste the code using CTRL+V"),
            new KeyValuePair<TranslationString, string>(TranslationString.CouldntCopyToClipboard,
                "Couldnt copy to clipboard, do it manually"),
            new KeyValuePair<TranslationString, string>(TranslationString.CouldntCopyToClipboard2,
                "Goto: {0} & enter {1}"),
            new KeyValuePair<TranslationString, string>(TranslationString.RealisticTravelDetected,
                "Detected realistic Traveling , using UserSettings.settings"),
            new KeyValuePair<TranslationString, string>(TranslationString.NotRealisticTravel,
                "Not realistic Traveling at {0}, using last saved Coords.ini"),
            new KeyValuePair<TranslationString, string>(TranslationString.CoordinatesAreInvalid,
                "Coordinates in \"Coords.ini\" file are invalid, using the default coordinates"),
            new KeyValuePair<TranslationString, string>(TranslationString.GotUpToDateVersion,
                "Perfect! You already have the newest Version {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.AutoUpdaterDisabled,
                "AutoUpdater is disabled. Get the latest release from: {0}\n "),
            new KeyValuePair<TranslationString, string>(TranslationString.DownloadingUpdate,
                "Downloading and apply Update..."),
            new KeyValuePair<TranslationString, string>(TranslationString.FinishedDownloadingRelease,
                "Finished downloading newest Release..."),
            new KeyValuePair<TranslationString, string>(TranslationString.FinishedUnpackingFiles,
                "Finished unpacking files..."),
            new KeyValuePair<TranslationString, string>(TranslationString.FinishedTransferringConfig,
                "Finished transferring your config to the new version..."),
            new KeyValuePair<TranslationString, string>(TranslationString.UpdateFinished,
                "Update finished, you can close this window now."),
            new KeyValuePair<TranslationString, string>(TranslationString.LookingForIncensePokemon,
                "Looking for incense Pokemon..."),
            new KeyValuePair<TranslationString, string>(TranslationString.LookingForPokemon, "Looking for Pokemon..."),
            new KeyValuePair<TranslationString, string>(TranslationString.LookingForLurePokemon,
                "Looking for lure Pokemon..."),
            new KeyValuePair<TranslationString, string>(TranslationString.PokemonSkipped, "Skipped {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.ZeroPokeballInv,
                "You have no pokeballs in your inventory, no more Pokemon can be caught!"),
            new KeyValuePair<TranslationString, string>(TranslationString.CurrentPokeballInv,
                "[Current Inventory] Pokeballs: {0} | Greatballs: {1} | Ultraballs: {2} | Masterballs: {3}"),
            new KeyValuePair<TranslationString, string>(TranslationString.CheckingForBallsToRecycle,
                "Checking for balls to recycle, keeping {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.CheckingForPotionsToRecycle,
                "Checking for potions to recycle, keeping {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.CheckingForRevivesToRecycle,
                "Checking for revives to recycle, keeping {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.PokeballsToKeepIncorrect,
                "TotalAmountOfPokeballsToKeep is configured incorrectly. The number is smaller than 1."),
            new KeyValuePair<TranslationString, string>(TranslationString.PotionsToKeepIncorrect,
                "TotalAmountOfPotionsToKeep is configured incorrectly. The number is smaller than 1."),
            new KeyValuePair<TranslationString, string>(TranslationString.RevivesToKeepIncorrect,
                "TotalAmountOfRevivesToKeep is configured incorrectly. The number is smaller than 1."),
            new KeyValuePair<TranslationString, string>(TranslationString.InvFullTransferring,
                "Pokemon Inventory is full, transferring Pokemon..."),
            new KeyValuePair<TranslationString, string>(TranslationString.InvFullTransferManually,
                "Pokemon Inventory is full! Please transfer Pokemon manually or set TransferDuplicatePokemon to true in settings..."),
            new KeyValuePair<TranslationString, string>(TranslationString.InvFullPokestopLooting,
                "Inventory is full, no items looted!"),
            new KeyValuePair<TranslationString, string>(TranslationString.EncounterProblem, "Encounter problem: {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.EncounterProblemLurePokemon,
                "Encounter problem: Lure Pokemon {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.DesiredDestTooFar,
                "Your desired destination of {0}, {1} is too far from your current position of {2}, {3}"),
            new KeyValuePair<TranslationString, string>(TranslationString.PokemonRename,
                "Pokemon {0} ({1}) renamed from {2} to {3}."),
            new KeyValuePair<TranslationString, string>(TranslationString.PokemonIgnoreFilter,
                "[Pokemon ignore filter] - Ignoring {0} as defined in settings"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchStatusAttempt, "CatchAttempt"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchStatusError, "CatchError"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchStatusEscape, "CatchEscape"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchStatusFlee, "CatchFlee"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchStatusMissed, "CatchMissed"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchStatusSuccess, "CatchSuccess"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchTypeNormal, "Normal"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchTypeLure, "Lure"),
            new KeyValuePair<TranslationString, string>(TranslationString.CatchTypeIncense, "Incense"),
            new KeyValuePair<TranslationString, string>(TranslationString.WebSocketFailStart,
                "Failed to start WebSocketServer on port : {0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.StatsTemplateString,
                "{0} - Runtime {1} - Lvl: {2} | EXP/H: {3:n0} | P/H: {4:n0} | Stardust: {5:n0} | Transfered: {6:n0} | Recycled: {7:n0}"),
            new KeyValuePair<TranslationString, string>(TranslationString.StatsXpTemplateString,
                "{0} (Advance in {1}h {2}m | {3:n0}/{4:n0} XP)"),
            new KeyValuePair<TranslationString, string>(TranslationString.RequireInputText,
                "Program will continue after the key press..."),
            new KeyValuePair<TranslationString, string>(TranslationString.GoogleTwoFactorAuth,
                "As you have Google Two Factor Auth enabled, you will need to insert an App Specific Password into the auth.json"),
            new KeyValuePair<TranslationString, string>(TranslationString.GoogleTwoFactorAuthExplanation,
                "Opening Google App-Passwords. Please make a new App Password (use Other as Device)"),
            new KeyValuePair<TranslationString, string>(TranslationString.GoogleError,
                "Make sure you have entered the right Email & Password."),
            new KeyValuePair<TranslationString, string>(TranslationString.MissingCredentialsGoogle,
                "You need to fill out GoogleUsername and GooglePassword in auth.json!"),
            new KeyValuePair<TranslationString, string>(TranslationString.MissingCredentialsPtc,
                "You need to fill out PtcUsername and PtcPassword in auth.json!"),
            new KeyValuePair<TranslationString, string>(TranslationString.SnipeScan,
                "[Sniper] Scanning for Snipeable Pokemon at {0}..."),
            new KeyValuePair<TranslationString, string>(TranslationString.SnipeScanEx,
                "[Sniper] Sniping a {0} with {1} IV at {2}..."),
            new KeyValuePair<TranslationString, string>(TranslationString.NoPokemonToSnipe,
                "[Sniper] No Pokemon found to snipe!"),
            new KeyValuePair<TranslationString, string>(TranslationString.NotEnoughPokeballsToSnipe,
                "Not enough Pokeballs to start sniping! ({0}/{1})"),
            new KeyValuePair<TranslationString, string>(TranslationString.DisplayHighestMove1Header, "MOVE1"),
            new KeyValuePair<TranslationString, string>(TranslationString.DisplayHighestMove2Header, "MOVE2"),
            new KeyValuePair<TranslationString, string>(TranslationString.UseBerry,
                "Using Razzberry. Berries left: {0}")
        };

        [JsonProperty("Pokemon",
            ItemTypeNameHandling = TypeNameHandling.Arrays,
            ItemConverterType = typeof(KeyValuePairConverter),
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Populate)]
        //Default Translations (ENGLISH)        
        private readonly List<KeyValuePair<int, string>> _pokemons = new List<KeyValuePair<int, string>>
        {
            new KeyValuePair<int, string>(0, "Missingno"),
            new KeyValuePair<int, string>(1, "Bulbasaur"),
            new KeyValuePair<int, string>(2, "Ivysaur"),
            new KeyValuePair<int, string>(3, "Venusaur"),
            new KeyValuePair<int, string>(4, "Charmander"),
            new KeyValuePair<int, string>(5, "Charmeleon"),
            new KeyValuePair<int, string>(6, "Charizard"),
            new KeyValuePair<int, string>(7, "Squirtle"),
            new KeyValuePair<int, string>(8, "Wartortle"),
            new KeyValuePair<int, string>(9, "Blastoise"),
            new KeyValuePair<int, string>(10, "Caterpie"),
            new KeyValuePair<int, string>(11, "Metapod"),
            new KeyValuePair<int, string>(12, "Butterfree"),
            new KeyValuePair<int, string>(13, "Weedle"),
            new KeyValuePair<int, string>(14, "Kakuna"),
            new KeyValuePair<int, string>(15, "Beedrill"),
            new KeyValuePair<int, string>(16, "Pidgey"),
            new KeyValuePair<int, string>(17, "Pidgeotto"),
            new KeyValuePair<int, string>(18, "Pidgeot"),
            new KeyValuePair<int, string>(19, "Rattata"),
            new KeyValuePair<int, string>(20, "Raticate"),
            new KeyValuePair<int, string>(21, "Spearow"),
            new KeyValuePair<int, string>(22, "Fearow"),
            new KeyValuePair<int, string>(23, "Ekans"),
            new KeyValuePair<int, string>(24, "Arbok"),
            new KeyValuePair<int, string>(25, "Pikachu"),
            new KeyValuePair<int, string>(26, "Raichu"),
            new KeyValuePair<int, string>(27, "Sandshrew"),
            new KeyValuePair<int, string>(28, "Sandslash"),
            new KeyValuePair<int, string>(29, "Nidoran♀"),
            new KeyValuePair<int, string>(30, "Nidorina"),
            new KeyValuePair<int, string>(31, "Nidoqueen"),
            new KeyValuePair<int, string>(32, "Nidoran♂"),
            new KeyValuePair<int, string>(33, "Nidorino"),
            new KeyValuePair<int, string>(34, "Nidoking"),
            new KeyValuePair<int, string>(35, "Clefairy"),
            new KeyValuePair<int, string>(36, "Clefable"),
            new KeyValuePair<int, string>(37, "Vulpix"),
            new KeyValuePair<int, string>(38, "Ninetales"),
            new KeyValuePair<int, string>(39, "Jigglypuff"),
            new KeyValuePair<int, string>(40, "Wigglytuff"),
            new KeyValuePair<int, string>(41, "Zubat"),
            new KeyValuePair<int, string>(42, "Golbat"),
            new KeyValuePair<int, string>(43, "Oddish"),
            new KeyValuePair<int, string>(44, "Gloom"),
            new KeyValuePair<int, string>(45, "Vileplume"),
            new KeyValuePair<int, string>(46, "Paras"),
            new KeyValuePair<int, string>(47, "Parasect"),
            new KeyValuePair<int, string>(48, "Venonat"),
            new KeyValuePair<int, string>(49, "Venomoth"),
            new KeyValuePair<int, string>(50, "Diglett"),
            new KeyValuePair<int, string>(51, "Dugtrio"),
            new KeyValuePair<int, string>(52, "Meowth"),
            new KeyValuePair<int, string>(53, "Persian"),
            new KeyValuePair<int, string>(54, "Psyduck"),
            new KeyValuePair<int, string>(55, "Golduck"),
            new KeyValuePair<int, string>(56, "Mankey"),
            new KeyValuePair<int, string>(57, "Primeape"),
            new KeyValuePair<int, string>(58, "Growlithe"),
            new KeyValuePair<int, string>(59, "Arcanine"),
            new KeyValuePair<int, string>(60, "Poliwag"),
            new KeyValuePair<int, string>(61, "Poliwhirl"),
            new KeyValuePair<int, string>(62, "Poliwrath"),
            new KeyValuePair<int, string>(63, "Abra"),
            new KeyValuePair<int, string>(64, "Kadabra"),
            new KeyValuePair<int, string>(65, "Alakazam"),
            new KeyValuePair<int, string>(66, "Machop"),
            new KeyValuePair<int, string>(67, "Machoke"),
            new KeyValuePair<int, string>(68, "Machamp"),
            new KeyValuePair<int, string>(69, "Bellsprout"),
            new KeyValuePair<int, string>(70, "Weepinbell"),
            new KeyValuePair<int, string>(71, "Victreebel"),
            new KeyValuePair<int, string>(72, "Tentacool"),
            new KeyValuePair<int, string>(73, "Tentacruel"),
            new KeyValuePair<int, string>(74, "Geodude"),
            new KeyValuePair<int, string>(75, "Graveler"),
            new KeyValuePair<int, string>(76, "Golem"),
            new KeyValuePair<int, string>(77, "Ponyta"),
            new KeyValuePair<int, string>(78, "Rapidash"),
            new KeyValuePair<int, string>(79, "Slowpoke"),
            new KeyValuePair<int, string>(80, "Slowbro"),
            new KeyValuePair<int, string>(81, "Magnemite"),
            new KeyValuePair<int, string>(82, "Magneton"),
            new KeyValuePair<int, string>(83, "Farfetch'd"),
            new KeyValuePair<int, string>(84, "Doduo"),
            new KeyValuePair<int, string>(85, "Dodrio"),
            new KeyValuePair<int, string>(86, "Seel"),
            new KeyValuePair<int, string>(87, "Dewgong"),
            new KeyValuePair<int, string>(88, "Grimer"),
            new KeyValuePair<int, string>(89, "Muk"),
            new KeyValuePair<int, string>(90, "Shellder"),
            new KeyValuePair<int, string>(91, "Cloyster"),
            new KeyValuePair<int, string>(92, "Gastly"),
            new KeyValuePair<int, string>(93, "Haunter"),
            new KeyValuePair<int, string>(94, "Gengar"),
            new KeyValuePair<int, string>(95, "Onix"),
            new KeyValuePair<int, string>(96, "Drowzee"),
            new KeyValuePair<int, string>(97, "Hypno"),
            new KeyValuePair<int, string>(98, "Krabby"),
            new KeyValuePair<int, string>(99, "Kingler"),
            new KeyValuePair<int, string>(100, "Voltorb"),
            new KeyValuePair<int, string>(101, "Electrode"),
            new KeyValuePair<int, string>(102, "Exeggcute"),
            new KeyValuePair<int, string>(103, "Exeggutor"),
            new KeyValuePair<int, string>(104, "Cubone"),
            new KeyValuePair<int, string>(105, "Marowak"),
            new KeyValuePair<int, string>(106, "Hitmonlee"),
            new KeyValuePair<int, string>(107, "Hitmonchan"),
            new KeyValuePair<int, string>(108, "Lickitung"),
            new KeyValuePair<int, string>(109, "Koffing"),
            new KeyValuePair<int, string>(110, "Weezing"),
            new KeyValuePair<int, string>(111, "Rhyhorn"),
            new KeyValuePair<int, string>(112, "Rhydon"),
            new KeyValuePair<int, string>(113, "Chansey"),
            new KeyValuePair<int, string>(114, "Tangela"),
            new KeyValuePair<int, string>(115, "Kangaskhan"),
            new KeyValuePair<int, string>(116, "Horsea"),
            new KeyValuePair<int, string>(117, "Seadra"),
            new KeyValuePair<int, string>(118, "Goldeen"),
            new KeyValuePair<int, string>(119, "Seaking"),
            new KeyValuePair<int, string>(120, "Staryu"),
            new KeyValuePair<int, string>(121, "Starmie"),
            new KeyValuePair<int, string>(122, "Mr. Mime"),
            new KeyValuePair<int, string>(123, "Scyther"),
            new KeyValuePair<int, string>(124, "Jynx"),
            new KeyValuePair<int, string>(125, "Electabuzz"),
            new KeyValuePair<int, string>(126, "Magmar"),
            new KeyValuePair<int, string>(127, "Pinsir"),
            new KeyValuePair<int, string>(128, "Tauros"),
            new KeyValuePair<int, string>(129, "Magikarp"),
            new KeyValuePair<int, string>(130, "Gyarados"),
            new KeyValuePair<int, string>(131, "Lapras"),
            new KeyValuePair<int, string>(132, "Ditto"),
            new KeyValuePair<int, string>(133, "Eevee"),
            new KeyValuePair<int, string>(134, "Vaporeon"),
            new KeyValuePair<int, string>(135, "Jolteon"),
            new KeyValuePair<int, string>(136, "Flareon"),
            new KeyValuePair<int, string>(137, "Porygon"),
            new KeyValuePair<int, string>(138, "Omanyte"),
            new KeyValuePair<int, string>(139, "Omastar"),
            new KeyValuePair<int, string>(140, "Kabuto"),
            new KeyValuePair<int, string>(141, "Kabutops"),
            new KeyValuePair<int, string>(142, "Aerodactyl"),
            new KeyValuePair<int, string>(143, "Snorlax"),
            new KeyValuePair<int, string>(144, "Articuno"),
            new KeyValuePair<int, string>(145, "Zapdos"),
            new KeyValuePair<int, string>(146, "Moltres"),
            new KeyValuePair<int, string>(147, "Dratini"),
            new KeyValuePair<int, string>(148, "Dragonair"),
            new KeyValuePair<int, string>(149, "Dragonite"),
            new KeyValuePair<int, string>(150, "Mewtwo"),
            new KeyValuePair<int, string>(151, "Mew")
        };

        public string GetTranslation(TranslationString translationString, params object[] data)
        {
            var translation = _translationStrings.FirstOrDefault(t => t.Key.Equals(translationString)).Value;
            return translation != default(string)
                ? string.Format(translation, data)
                : $"Translation for {translationString} is missing";
        }

        public string GetTranslation(TranslationString translationString)
        {
            var translation = _translationStrings.FirstOrDefault(t => t.Key.Equals(translationString)).Value;
            return translation != default(string) ? translation : $"Translation for {translationString} is missing";
        }

        public string GetPokemonName(PokemonId pkmnId)
        {
            var name = _pokemons.FirstOrDefault(p => p.Key == (int)pkmnId).Value;

            return name != default(string)
                ? name
                : $"Translation for pokemon name {pkmnId} is missing";
        }

        public static Translation Load(ILogicSettings logicSettings)
        {
            var translationsLanguageCode = logicSettings.TranslationLanguageCode;
            var translationPath = Path.Combine(logicSettings.GeneralConfigPath, "translations");
            var fullPath = Path.Combine(translationPath, "translation." + translationsLanguageCode + ".json");

            Translation translations;
            if (File.Exists(fullPath))
            {
                var input = File.ReadAllText(fullPath);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;
                translations = JsonConvert.DeserializeObject<Translation>(input, jsonSettings);
                //TODO make json to fill default values as it won't do it now

                var defaultTranslation = new Translation();

                defaultTranslation._translationStrings.Where(
                    item => translations._translationStrings.All(a => a.Key != item.Key))
                    .ToList()
                    .ForEach(translations._translationStrings.Add);

                defaultTranslation._pokemons.Where(
                    item => translations._pokemons.All(a => a.Key != item.Key))
                    .ToList()
                    .ForEach(translations._pokemons.Add);
            }
            else
            {
                translations = new Translation();
                translations.Save(Path.Combine(translationPath, "translation.en.json"));
            }
            return translations;
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
}