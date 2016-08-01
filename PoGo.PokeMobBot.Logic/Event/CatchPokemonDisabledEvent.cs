#region using directives

using POGOProtos.Enums;

#endregion

namespace PoGo.PokeMobBot.Logic.Event
{
    public class CatchPokemonDisabledEvent : IEvent
    {
        public PokemonId Id;
    }
}