#region using directives

using PoGo.PokeMobBot.Logic.Event;
using POGOProtos.Data.Player;
using System.Collections.Generic;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class PlayerStatsEvent : IEvent

    {
        public List<PlayerStats> PlayerStats { get; set; }
    }
}
