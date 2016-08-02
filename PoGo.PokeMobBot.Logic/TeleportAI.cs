using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoGo.PokeMobBot.Logic;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.PokeMobBot.Logic
{
    public class TeleportAI
    {
        ISession session;


            public int getDelay(double distance)
              {
            return session.TeleSetting.teleWaiter(distance);
              }
            
        


             TeleDelay teleWait = new TeleDelay();

        public int addDelay(int distance)
        {
            return session.TeleSetting.teleDelayer(distance);
        }
    }
}
