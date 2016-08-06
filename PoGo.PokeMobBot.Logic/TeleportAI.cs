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
    public class TeleportAI : TeleSettings
    {



        public void getDelay(double distance)
              {
            Random rnd = new Random();
            int rand = rnd.Next(0, 250);
            if (distance > 2000)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime2000 + rand) + "ms");
                 System.Threading.Thread.Sleep(waitTime2000 + rand);
            }
            else if (distance > 1500)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime1500 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime1500 + rand);
            }
            else if (distance > 1250)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime1250 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime1250 + rand);
            }
            else if (distance > 1000)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime1000 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime1000 + rand);
            }
            else if (distance > 900)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime900 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime900 + rand);
            }
            else if (distance > 800)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime800 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime800 + rand);
            }
            else if (distance > 700)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime700 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime700 + rand);
            }
            else if (distance > 600)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime600 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime600 + rand);
            }
            else if (distance > 500)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime500 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime500 + rand);
            }
            else if (distance > 400)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime400 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime400 + rand);
            }
            else if (distance > 300)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime300 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime300 + rand);
            }
            else if (distance > 200)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime200 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime200 + rand);
            }
            else if (distance > 100)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime100 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime100 + rand);
            }
            else if (distance > 50)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + (waitTime50 + rand) + "ms");
                System.Threading.Thread.Sleep(waitTime50 + rand);
            }
            

              }


        

        public void addDelay(int distance)

        {  
            var profilePath = Path.Combine(Directory.GetCurrentDirectory());
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "TeleAI.json");
            Save(configFile);

            if (distance > 2000)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime2000 + "ms.");
                int i = waitTime2000 + 100;
                this.waitTime2000 = i;
            }
            else if (distance > 1500)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime1500 + "ms.");
                int i = waitTime1500 + 100;
                this.waitTime1500 = i;
            }
            else if (distance > 1250)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime1250 + "ms.");
                int i = waitTime1250 + 100;
                this.waitTime1000 = i;
            }
            else if (distance > 1000)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime1000 + "ms.");
                waitTime1000 = waitTime1000 + 100;
            }
            else if (distance > 900)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime900 + "ms.");
                int i = waitTime900 + 100;
                this.waitTime1000 = i;
            }
            else if (distance > 800)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime800 + "ms.");
                int i = waitTime800 + 100;
                this.waitTime900 = i;
            }
            else if (distance > 700)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime700 + "ms.");
                int i = waitTime700 + 100;
                this.waitTime700 = i;
            }
            else if (distance > 600)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime600 + "ms.");
                int i = waitTime600 + 100;
                this.waitTime600 = i;
            }
            else if (distance > 500)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime500 + "ms.");
                int i = waitTime500 + 100;
                this.waitTime500 = i;
            }
            else if (distance > 400)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime400 + "ms.");
                int i = waitTime400 + 100;
                this.waitTime500 = i;
            }
            else if (distance > 300)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime300 + "ms.");
                int i = waitTime300 + 100;
                this.waitTime300 = i;
            }
            else if (distance > 200)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime200 + "ms.");

                int i = waitTime200 + 100;
                this.waitTime200 = i;
            }
            else if (distance > 100)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime100 + "ms.");
                int i = waitTime100 + 100;
                this.waitTime100 = i;
            }
            else if (distance > 50)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime100 + "ms.");
                int i = waitTime50 + 50;
                this.waitTime50 = i;
            }
            
        }
    }
}
