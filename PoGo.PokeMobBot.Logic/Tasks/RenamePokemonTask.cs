#region using directives

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PoGo.PokeMobBot.Logic.State;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class RenamePokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pokemons = await session.Inventory.GetPokemons();

            foreach (var pokemon in pokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon));
                var pokemonName = pokemon.PokemonId.ToString();
                // iv number + templating part + pokemonName <= 12
                var nameLength = 12 -
                                 (perfection.ToString(CultureInfo.InvariantCulture).Length +
                                  session.LogicSettings.RenameTemplate.Length - 6);
                if (pokemonName.Length > nameLength)
                {
                    pokemonName = pokemonName.Substring(0, nameLength);
                }

                if (session.LogicSettings.TemplateUsage == 1)
                {
                    var newNickname = string.Format(session.LogicSettings.RenameTemplate, pokemonName, perfection);
                    var oldNickname = pokemon.Nickname.Length != 0 ? pokemon.Nickname : pokemon.PokemonId.ToString();

                    // If "RenameOnlyAboveIv" = true only rename pokemon with IV over "KeepMinIvPercentage"
                    // Favorites will be skipped
                    if ((!session.LogicSettings.RenameOnlyAboveIv || perfection >= session.LogicSettings.KeepMinIvPercentage) &&
                        newNickname != oldNickname && pokemon.Favorite == 0)
                    {
                        await session.Client.Inventory.NicknamePokemon(pokemon.Id, newNickname);

                        session.EventDispatcher.Send(new NoticeEvent
                        {
                            Message =
                                session.Translation.GetTranslation(TranslationString.PokemonRename, pokemon.PokemonId,
                                    pokemon.Id, oldNickname, newNickname)
                        });
                    }
                } else if (session.LogicSettings.TemplateUsage == 2)
                {
                    var newNickname = string.Format(session.LogicSettings.RenameTemplate2, PokemonInfo.GetPokemonMove1(pokemon), PokemonInfo.GetPokemonMove2(pokemon), perfection);
                    var oldNickname = pokemon.Nickname.Length != 0 ? pokemon.Nickname : pokemon.PokemonId.ToString();

                    // If "RenameOnlyAboveIv" = true only rename pokemon with IV over "KeepMinIvPercentage"
                    // Favorites will be skipped
                    if ((!session.LogicSettings.RenameOnlyAboveIv || perfection >= session.LogicSettings.KeepMinIvPercentage) &&
                        newNickname != oldNickname && pokemon.Favorite == 0)
                    {
                        await session.Client.Inventory.NicknamePokemon(pokemon.Id, newNickname);

                        session.EventDispatcher.Send(new NoticeEvent
                        {
                            Message =
                                session.Translation.GetTranslation(TranslationString.PokemonRename, pokemon.PokemonId,
                                    pokemon.Id, oldNickname, newNickname)
                        });
                    }
                }
            }
        }
    }
}