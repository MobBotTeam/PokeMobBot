#region using directives

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PokemonGo.RocketAPI;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class RenamePokemonTask
    {
        private readonly PokemonInfo _pokemonInfo;
        private readonly ITranslation _translation;
        private readonly ILogicSettings _logicSettings;
        private readonly Inventory _inventory;
        private readonly Client _client;
        private readonly IEventDispatcher _eventDispatcher;

        public RenamePokemonTask(PokemonInfo pokemonInfo, ITranslation translation, ILogicSettings logicSettings, Inventory inventory, Client client, IEventDispatcher eventDispatcher)
        {
            _pokemonInfo = pokemonInfo;
            _translation = translation;
            _logicSettings = logicSettings;
            _inventory = inventory;
            _client = client;
            _eventDispatcher = eventDispatcher;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pokemons = await _inventory.GetPokemons();

            foreach (var pokemon in pokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var perfection = Math.Round(_pokemonInfo.CalculatePokemonPerfection(pokemon));
                var pokemonName = _translation.GetPokemonName(pokemon.PokemonId);
                // iv number + templating part + pokemonName <= 12
                var nameLength = 12 -
                                 (perfection.ToString(CultureInfo.InvariantCulture).Length +
                                  _logicSettings.RenameTemplate.Length - 6);
                if (pokemonName.Length > nameLength)
                {
                    pokemonName = pokemonName.Substring(0, nameLength);
                }
                var newNickname = string.Format(_logicSettings.RenameTemplate, pokemonName, perfection);
                var oldNickname = pokemon.Nickname.Length != 0 ? pokemon.Nickname : _translation.GetPokemonName(pokemon.PokemonId);

                // If "RenameOnlyAboveIv" = true only rename pokemon with IV over "KeepMinIvPercentage"
                // Favorites will be skipped
                if ((!_logicSettings.RenameOnlyAboveIv || perfection >= _logicSettings.KeepMinIvPercentage) &&
                    newNickname != oldNickname && pokemon.Favorite == 0)
                {
                    await _client.Inventory.NicknamePokemon(pokemon.Id, newNickname);

                    _eventDispatcher.Send(new NoticeEvent
                    {
                        Message =
                            _translation.GetTranslation(TranslationString.PokemonRename, _translation.GetPokemonName(pokemon.PokemonId),
                                pokemon.Id, oldNickname, newNickname)
                    });
                }
            }
        }
    }
}