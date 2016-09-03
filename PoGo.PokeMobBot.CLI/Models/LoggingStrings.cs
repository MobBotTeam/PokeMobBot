using PoGo.PokeMobBot.Logic.Common;

namespace PoGo.PokeMobBot.CLI.Models
{
    public class LoggingStrings
    {
        private readonly ITranslation _translation;

        internal string Attention;

        internal string Berry;

        internal string Debug;

        internal string Egg;

        internal string Error;

        internal string Evolved;

        internal string Farming;

        internal string Info;

        internal string Pkmn;

        internal string Pokestop;

        internal string Recycling;

        internal string Transfered;

        internal string Update;

        public LoggingStrings(ITranslation translation)
        {
            _translation = translation;
        }

        internal void SetStrings()
        {
            Attention =
                _translation?.GetTranslation(
                    TranslationString.LogEntryAttention) ?? "ATTENTION";

            Berry =
                _translation?.GetTranslation(
                    TranslationString.LogEntryBerry) ?? "BERRY";

            Debug =
                _translation?.GetTranslation(
                    TranslationString.LogEntryDebug) ?? "DEBUG";

            Egg =
                _translation?.GetTranslation(
                    TranslationString.LogEntryEgg) ?? "EGG";

            Error =
                _translation?.GetTranslation(
                    TranslationString.LogEntryError) ?? "ERROR";

            Evolved =
                _translation?.GetTranslation(
                    TranslationString.LogEntryEvolved) ?? "EVOLVED";

            Farming =
                _translation?.GetTranslation(
                    TranslationString.LogEntryFarming) ?? "FARMING";

            Info =
                _translation?.GetTranslation(
                    TranslationString.LogEntryInfo) ?? "INFO";

            Pkmn =
                _translation?.GetTranslation(
                    TranslationString.LogEntryPkmn) ?? "PKMN";

            Pokestop =
                _translation?.GetTranslation(
                    TranslationString.LogEntryPokestop) ?? "POKESTOP";

            Recycling =
                _translation?.GetTranslation(
                    TranslationString.LogEntryRecycling) ?? "RECYCLING";

            Transfered =
                _translation?.GetTranslation(
                    TranslationString.LogEntryTransfered) ?? "TRANSFERED";

            Update =
                _translation?.GetTranslation(
                    TranslationString.LogEntryUpdate) ?? "UPDATE";
        }
    }
}
