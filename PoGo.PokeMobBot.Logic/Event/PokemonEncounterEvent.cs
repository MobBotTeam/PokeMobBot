#region using directives

using POGOProtos.Enums;

#endregion

namespace PoGo.PokeMobBot.Logic.Event
{
    public class PokemonEncounterEvent : IEvent
    {
        public POGOProtos.Data.PokemonData WildPokemon { get; set; }
        public POGOProtos.Map.Pokemon.MapPokemon MapPokemon { get; set; }
    }
}