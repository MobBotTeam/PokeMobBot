#region using directives

using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Utils;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic
{
    public class StatisticsAggregator
    {
        private readonly Statistics _stats;
        private readonly Inventory _inventory;

        public StatisticsAggregator(Statistics stats, Inventory inventory)
        {
            _stats = stats;
            _inventory = inventory;
        }

        public void HandleEvent(ProfileEvent evt)
        {
            _stats.SetUsername(evt.Profile);
            _stats.Dirty(_inventory);
            _stats.CheckLevelUp();
        }

        public void HandleEvent(ErrorEvent evt)
        {
        }

        public void HandleEvent(NoticeEvent evt)
        {
        }

        public void HandleEvent(WarnEvent evt)
        {
        }

        public void HandleEvent(UseLuckyEggEvent evt)
        {
        }

        public void HandleEvent(PokemonEvolveEvent evt)
        {
            _stats.TotalExperience += evt.Exp;
            _stats.Dirty(_inventory);
            _stats.CheckLevelUp();
        }

        public void HandleEvent(TransferPokemonEvent evt)
        {
            _stats.TotalPokemonsTransfered++;
            _stats.Dirty(_inventory);
            _stats.CheckLevelUp();
        }

        public void HandleEvent(ItemRecycledEvent evt)
        {
            _stats.TotalItemsRemoved++;
            _stats.Dirty(_inventory);
            _stats.CheckLevelUp();
        }

        public void HandleEvent(FortUsedEvent evt)
        {
            _stats.TotalExperience += evt.Exp;
            _stats.Dirty(_inventory);
            _stats.CheckLevelUp();
        }

        public void HandleEvent(FortTargetEvent evt)
        {
        }

        public void HandleEvent(PokemonCaptureEvent evt)
        {
            if (evt.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
            {
                _stats.TotalExperience += evt.Exp;
                _stats.TotalPokemons++;
                _stats.TotalStardust = evt.Stardust;
                _stats.Dirty(_inventory);
                _stats.CheckLevelUp();
            }
        }

        public void HandleEvent(NoPokeballEvent evt)
        {
        }

        public void HandleEvent(UseBerryEvent evt)
        {
        }

        public void HandleEvent(DisplayHighestsPokemonEvent evt)
        {
        }

        public void Listen(IEvent evt)
        {
            dynamic eve = evt;

            try
            {
                HandleEvent(eve);
            }
            catch
            {
                // ignored
            }
        }
    }
}