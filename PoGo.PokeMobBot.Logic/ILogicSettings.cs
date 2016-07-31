#region using directives

using System.Collections.Generic;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic
{
    public class Location
    {
        public Location()
        {
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class SnipeSettings
    {
        public SnipeSettings()
        {
        }

        public SnipeSettings(List<Location> locations, List<PokemonId> pokemon)
        {
            Locations = locations;
            Pokemon = pokemon;
        }

        public List<Location> Locations { get; set; }
        public List<PokemonId> Pokemon { get; set; }
    }

    public class TransferFilter
    {
        public TransferFilter()
        {
        }

        public TransferFilter(int keepMinCp, float keepMinIvPercentage, int keepMinDuplicatePokemon)
        {
            KeepMinCp = keepMinCp;
            KeepMinIvPercentage = keepMinIvPercentage;
            KeepMinDuplicatePokemon = keepMinDuplicatePokemon;
        }

        public int KeepMinCp { get; set; }
        public float KeepMinIvPercentage { get; set; }
        public int KeepMinDuplicatePokemon { get; set; }
    }

    public interface ILogicSettings
    {
        bool AutoUpdate { get; }
        bool TransferConfigAndAuthOnUpdate { get; }
        float KeepMinIvPercentage { get; }
        int KeepMinCp { get; }
        double WalkingSpeedInKilometerPerHour { get; }
        bool EvolveAllPokemonWithEnoughCandy { get; }
        bool KeepPokemonsThatCanEvolve { get; }
        bool TransferDuplicatePokemon { get; }
        bool UseEggIncubators { get; }
        int UseGreatBallAboveCp { get; }
        int UseUltraBallAboveCp { get; }
        int UseMasterBallAboveCp { get; }
        int DelayBetweenPokemonCatch { get; }
        bool AutomaticallyLevelUpPokemon { get; }
        string LevelUpByCPorIv { get; }
        float UpgradePokemonCpMinimum { get; }
        float UpgradePokemonIvMinimum { get; }
        int DelayBetweenPlayerActions { get; }
        bool UsePokemonToNotCatchFilter { get; }
        int KeepMinDuplicatePokemon { get; }
        bool PrioritizeIvOverCp { get; }
        int MaxTravelDistanceInMeters { get; }
        bool UseGpxPathing { get; }
        string GpxFile { get; }
        bool UseLuckyEggsWhileEvolving { get; }
        int UseLuckyEggsMinPokemonAmount { get; }
        bool EvolveAllPokemonAboveIv { get; }
        float EvolveAboveIvValue { get; }
        bool DumpPokemonStats { get; }
        bool RenamePokemon { get; }
        bool RenameOnlyAboveIv { get; }
        int TemplateUsage { get; }
        string RenameTemplate { get; }
        string RenameTemplate2 { get; }
        int AmountOfPokemonToDisplayOnStart { get; }
        string TranslationLanguageCode { get; }
        string ProfilePath { get; }
        string ProfileConfigPath { get; }
        string GeneralConfigPath { get; }
        bool SnipeAtPokestops { get; }
        int MinPokeballsToSnipe { get; }
        int MinPokeballsWhileSnipe { get; }
        int MaxPokeballsPerPokemon { get; }
        string SnipeLocationServer { get; }
        int SnipeLocationServerPort { get; }
        bool UseSnipeLocationServer { get; }
        bool UseTransferIvForSnipe { get; }
        bool SnipeIgnoreUnknownIv { get; }
        int MinDelayBetweenSnipes { get; }
        int TotalAmountOfPokeballsToKeep { get; }
        int TotalAmountOfPotionsToKeep { get; }
        int TotalAmountOfRevivesToKeep { get; }

        ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter { get; }

        ICollection<PokemonId> PokemonsToEvolve { get; }

        ICollection<PokemonId> PokemonsNotToTransfer { get; }

        ICollection<PokemonId> PokemonsNotToCatch { get; }

        ICollection<PokemonId> PokemonToUseMasterball { get; }

        Dictionary<PokemonId, TransferFilter> PokemonsTransferFilter { get; }
        SnipeSettings PokemonToSnipe { get; }

        bool StartupWelcomeDelay { get; }

        bool Teleport { get; }
        int DelayPositionCheckState { get; }
        int DelayCatchIncensePokemon { get; }
        int DelayCatchNearbyPokemon { get; }
        int DelayCatchLurePokemon { get; }
        int DelayCatchPokemon { get; }
        int DelayDisplayPokemon { get; }
        int DelayUseLuckyEgg { get; }
        int DelaySoftbanRetry { get; }
        int DelayPokestop { get; }
        int DelayRecyleItem { get; }
        int DelaySnipePokemon { get; }
        int DelayTransferPokemon { get; }
        double RecycleInventoryAtUsagePercentage { get; }
        bool HumanizeThrows { get; }
        double ThrowAccuracyMin { get; }
        double ThrowAccuracyMax { get; }
        double ThrowSpinFrequency { get; }
    }
}