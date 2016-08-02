﻿#region using directives

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Utils;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class TransferLowStatPokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Refresh inventory so that the player stats are fresh
            await session.Inventory.RefreshCachedInventory();

            var pokemons = await session.Inventory.GetPokemons();

            var pokemonSettings = await session.Inventory.GetPokemonSettings();
            var pokemonFamilies = await session.Inventory.GetPokemonFamilies();

            foreach (var pokemon in pokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ((pokemon.Cp >= session.LogicSettings.KeepMinCp) ||
                    (PokemonInfo.CalculatePokemonPerfection(pokemon) >= session.LogicSettings.KeepMinIvPercentage && session.LogicSettings.PrioritizeIvOverCp))
                {
                    continue;
                }

                await session.Client.Inventory.TransferPokemon(pokemon.Id);
                await session.Inventory.DeletePokemonFromInvById(pokemon.Id);

                var bestPokemonOfType = (session.LogicSettings.PrioritizeIvOverCp
                    ? await session.Inventory.GetHighestPokemonOfTypeByIv(pokemon)
                    : await session.Inventory.GetHighestPokemonOfTypeByCp(pokemon)) ?? pokemon;

                var setting = pokemonSettings.Single(q => q.PokemonId == pokemon.PokemonId);
                var family = pokemonFamilies.First(q => q.FamilyId == setting.FamilyId);

                family.Candy_++;

                session.EventDispatcher.Send(new TransferPokemonEvent
                {
                    Id = pokemon.PokemonId,
                    Perfection = PokemonInfo.CalculatePokemonPerfection(pokemon),
                    Cp = pokemon.Cp,
                    BestCp = bestPokemonOfType.Cp,
                    BestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                    FamilyCandies = family.Candy_
                });
                if (session.LogicSettings.Teleport)
                    await Task.Delay(session.LogicSettings.DelayTransferPokemon);
                else
                    await DelayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 0);
            }
        }
    }
}