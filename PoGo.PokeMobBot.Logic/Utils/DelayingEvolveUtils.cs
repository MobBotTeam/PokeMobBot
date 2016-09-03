#region using directives

using System;
using System.Threading.Tasks;

#endregion

namespace PoGo.PokeMobBot.Logic.Utils
{
    public class DelayingEvolveUtils
    {
        private readonly Random _randomDevice = new Random();

        public async Task Delay(int delay, int defdelay, double evolvevariation)
        {
            if (delay > defdelay)
            {
                var randomFactor = evolvevariation;
                var randomMin = (int)(delay * (1 - randomFactor));
                var randomMax = (int)(delay * (1 + randomFactor));
                var randomizedDelay = _randomDevice.Next(randomMin, randomMax);

                await Task.Delay(randomizedDelay);
            }
            else if (defdelay > 0)
            {
                await Task.Delay(defdelay);
            }
        }
    }
}