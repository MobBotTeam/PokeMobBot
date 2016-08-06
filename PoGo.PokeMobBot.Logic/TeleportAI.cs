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
using PoGo.PokeMobBot.Logic.Event;

namespace PoGo.PokeMobBot.Logic
{
    public class TeleportAI : TeleSettings
    {



        public void getDelay(double distance)
              {
            EventDispatcher msg = new EventDispatcher();
            Random rnd = new Random();
            int rand = rnd.Next(0, 250);
            int time = 0;
            if (distance > 2000)
            {
                time = waitTime2000 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                 System.Threading.Thread.Sleep(waitTime2000);
            }
            else if (distance > 1500)
            {
                time = waitTime1500 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime1500);
            }
            else if (distance > 1250)
            {
                time = waitTime1250 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime1250);
            }
            else if (distance > 1000)
            {
                time = waitTime1000 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime1000);
            }
            else if (distance > 900)
            {
                time = waitTime900 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime900);
            }
            else if (distance > 800)
            {
                time = waitTime800 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime800);
            }
            else if (distance > 700)
            {
                time = waitTime700 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime700);
            }
            else if (distance > 600)
            {
                time = waitTime600 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime600);
            }
            else if (distance > 500)
            {
                time = waitTime500 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime500);
            }
            else if (distance > 400)
            {
                time = waitTime400 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime400);
            }
            else if (distance > 300)
            {
                time = waitTime300 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime300);
            }
            else if (distance > 200)
            {
                time = waitTime200 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime200);
            }
            else if (distance > 100)
            {
                time = waitTime100 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime100);
            }
            else if (distance > 50)
            {
                time = waitTime50 + rand;
                msg.Send(new TeleAI
                {
                    Disance = distance,
                    Delay = time
                });
                System.Threading.Thread.Sleep(waitTime50);
            }
            

              }


        

        public void addDelay(int distance)

        {
            var profilePath = Path.Combine(Directory.GetCurrentDirectory());
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "TeleAI.json");
            EventDispatcher msg = new EventDispatcher();
            Save(configFile);

            if (distance > 2000)
            {
                int i = waitTime2000 + 100;
                this.waitTime2000 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            
            }
            else if (distance > 1500)
            {
                int i = waitTime1500 + 100;
                this.waitTime1500 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 1250)
            {
                int i = waitTime1250 + 100;
                this.waitTime1000 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 1000)
            {
                int i = waitTime1000 + 100;
                this.waitTime1000 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });

            }
            else if (distance > 900)
            {
                int i = waitTime900 + 100;
                this.waitTime1000 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 800)
            {
                int i = waitTime800 + 100;
                this.waitTime900 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 700)
            {
                int i = waitTime700 + 100;
                this.waitTime700 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 600)
            {
                int i = waitTime600 + 100;
                this.waitTime600 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 500)
            {
                int i = waitTime500 + 100;
                this.waitTime500 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 400)
            {
                int i = waitTime400 + 100;
                this.waitTime400 = i; 
                    msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 300)
            {
                int i = waitTime300 + 100;
                this.waitTime300 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 200)
            {
                int i = waitTime200 + 100;
                this.waitTime200 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 100)
            {
                int i = waitTime100 + 100;
                this.waitTime100 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            else if (distance > 50)
            {
                int i = waitTime50 + 50;
                this.waitTime50 = i;
                msg.Send(new TeleAIBan
                {
                    Disance = distance,
                    Delay = i
                });
            }
            
        }
    }

    internal class TeleAI : IEvent
    {
        public int Delay { get; set; }
        public double Disance { get; set; }
    }

    internal class TeleAIBan : IEvent
    {
        public int Delay { get; internal set; }
        public int Disance { get; set; }
    }
}
