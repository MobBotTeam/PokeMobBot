#region using directives

using System;
using System.Threading.Tasks;

#endregion

namespace PoGo.PokeMobBot.Logic.Utils
{
    public class JitterUtils
    {
        private readonly Random _randomDevice = new Random();

        public Task RandomDelay(int min, int max)
        {
            return Task.Delay(_randomDevice.Next(min, max));
        }
    }
}