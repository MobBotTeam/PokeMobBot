#region using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class PokemonSettingsTask
    {
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;

        public PokemonSettingsTask(Inventory inventory, ILogicSettings logicSettings)
        {
            _inventory = inventory;
            _logicSettings = logicSettings;
        }

        public async Task Execute(Action<IEvent> action)
        {
            var settings = await _inventory.GetPokemonSettings();

            action(new PokemonSettingsEvent
            {
                Data = settings.ToList()
            });

            await Task.Delay(_logicSettings.DelayBetweenPlayerActions);
        }
    }
}