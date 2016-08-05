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
    public class TeleportAI : ITeleSettings
    {
        public int waitTime50 { get; set; }
        public int waitTime100 { get; set; }
        public int waitTime200 { get; set; }
        public int waitTime300 { get; set; }
        public int waitTime400 { get; set; }
        public int waitTime500 { get; set; }
        public int waitTime600 { get; set; }
        public int waitTime700 { get; set; }
        public int waitTime800 { get; set; }
        public int waitTime900 { get; set; }
        public int waitTime1000 { get; set; }
        public int waitTime1250 { get; set; }
        public int waitTime1500 { get; set; }
        public int waitTime2000 { get; set; }
        public int teleDelay { get; internal set; }


        public void getDelay(double distance)
              {
            if (distance > 2000)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime2000 + "ms");
                 System.Threading.Thread.Sleep(waitTime2000);
            }
            else if (distance > 1500)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime1500 + "ms");
                System.Threading.Thread.Sleep(waitTime1500);
            }
            else if (distance > 1250)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime1250 + "ms");
                System.Threading.Thread.Sleep(waitTime1250);
            }
            else if (distance > 1000)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime1000 + "ms");
                System.Threading.Thread.Sleep(waitTime1000);
            }
            else if (distance > 900)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime900 + "ms");
                System.Threading.Thread.Sleep(waitTime900);
            }
            else if (distance > 800)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime800 + "ms");
                System.Threading.Thread.Sleep(waitTime800);
            }
            else if (distance > 700)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime700 + "ms");
                System.Threading.Thread.Sleep(waitTime700);
            }
            else if (distance > 600)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime600 + "ms");
                System.Threading.Thread.Sleep(waitTime600);
            }
            else if (distance > 500)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime500 + "ms");
                System.Threading.Thread.Sleep(waitTime500);
            }
            else if (distance > 400)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime400 + "ms");
                System.Threading.Thread.Sleep(waitTime400);
            }
            else if (distance > 300)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime300 + "ms");
                System.Threading.Thread.Sleep(waitTime300);
            }
            else if (distance > 200)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime200 + "ms");
                System.Threading.Thread.Sleep(waitTime200);
            }
            else if (distance > 100)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime100 + "ms");
                System.Threading.Thread.Sleep(waitTime100);
            }
            else if (distance > 50)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime50 + "ms");
                System.Threading.Thread.Sleep(waitTime50);
            }
            

              }


        

        public void addDelay(int distance)

        {  
            TeleSettings settings2 = new TeleSettings();
            var profilePath = Path.Combine(Directory.GetCurrentDirectory());
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "teleai.json");
            String path2 = settings2.GeneralConfigPath;
            settings2.Save(configFile);

            if (distance > 2000)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime2000 + "ms.");
                waitTime2000 = waitTime2000 + 100;
            }
            else if (distance > 1500)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime1500 + "ms.");
                waitTime1500 = waitTime1500 + 100;
            }
            else if (distance > 1250)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime1250 + "ms.");
                waitTime1250 = waitTime1250 + 100;
            }
            else if (distance > 1000)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime1000 + "ms.");
                waitTime1000 = waitTime1000 + 100;
            }
            else if (distance > 900)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime900 + "ms.");
                waitTime900 = waitTime900 + 100;
            }
            else if (distance > 800)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime800 + "ms.");
                waitTime800 = waitTime800 + 100;
            }
            else if (distance > 700)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime700 + "ms.");
                waitTime700 = waitTime700 + 100;
            }
            else if (distance > 600)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime600 + "ms.");
                waitTime600 = waitTime600 + 100;
            }
            else if (distance > 500)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime500 + "ms.");
                waitTime500 = waitTime500 + 100;
            }
            else if (distance > 400)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime400 + "ms.");
                waitTime400 = waitTime400 + 100;
                this.waitTime500 = waitTime500;
            }
            else if (distance > 300)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime300 + "ms.");
                waitTime300 = waitTime300 + 100;
                this.waitTime300 = waitTime300;
            }
            else if (distance > 200)
            {
                Logging.Logger.Write("SoftBanned From Jumping  " + distance + " meters. Adding 100ms delay. Total: " + waitTime200 + "ms.");

                waitTime200 = waitTime200 + 100;
                this.waitTime200 = waitTime200;
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
