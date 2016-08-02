﻿#region using directives

using System;
using System.Collections.Generic;
using POGOProtos.Data;
using POGOProtos.Enums;

#endregion

namespace PoGo.PokeMobBot.Logic.Event
{
    public class DisplayHighestsPokemonEvent : IEvent
    {
        //PokemonData | CP | IV | Level | MOVE1 | MOVE2 | AverageRankVsTypes
        public List<Tuple<PokemonData, int, double, double, PokemonMove, PokemonMove,int>> PokemonList;
        public string SortedBy;
    }
}