#region using directives

using System.Collections.Generic;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic
{
    public class TeleDelay
    {
        public int teleWait;
        public TeleDelay()
        {
        }

        public int teleWaiter(double distance)
        {
            if (distance > 2000)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime2000 + "ms");
                return teleWait = waitTime2000;
            }
            else if (distance > 1500)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime1500 + "ms");
                return teleWait = waitTime1500;
            }
            else if (distance > 1250)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime1250 + "ms");
                return teleWait = waitTime1250;
            }
            else if (distance > 1000)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime1000 + "ms");
                return teleWait = waitTime1000;
            }
            else if (distance > 900)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime900 + "ms");
                return teleWait = waitTime900;
            }
            else if (distance > 800)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime800 + "ms");
                return teleWait = waitTime800;
            }
            else if (distance > 700)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime700 + "ms");
               return teleWait = waitTime700;
            }
            else if (distance > 600)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime600 + "ms");
                return teleWait = waitTime600;
            }
            else if (distance > 500)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime500 + "ms");
                return teleWait = waitTime500;
            }
            else if (distance > 400)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime400 + "ms");
                return teleWait = waitTime700;
            }
            else if (distance > 300)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime300 + "ms");
                return teleWait = waitTime600;
            }
            else if (distance > 200)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime200 + "ms");
                return teleWait = waitTime500;
            }
            else if (distance > 100)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime100 + "ms");
                return teleWait = waitTime400;
            }
            else if (distance > 50)
            {
                return teleWait = waitTime50;
            }

            else teleWait = 0;
            return teleWait;
        }
        public int teleDelayer(double distance)
        {
            if (distance > 2000)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime2000 + "ms");
                waitTime2000 = waitTime2000 + 100;
            }
            else if (distance > 1500)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime1500 + "ms");
                waitTime1500 = waitTime1500 + 100;
            }
            else if (distance > 1250)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime1250 + "ms");
                teleWait = waitTime1250;
            }
            else if (distance > 1000)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime1000 + "ms");
                teleWait = waitTime1000;
            }
            else if (distance > 900)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime900 + "ms");
                teleWait = waitTime900;
            }
            else if (distance > 800)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime800 + "ms");
                teleWait = waitTime800;
            }
            else if (distance > 700)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime700 + "ms");
                teleWait = waitTime700;
            }
            else if (distance > 600)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime600 + "ms");
                teleWait = waitTime600;
            }
            else if (distance > 500)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime500 + "ms");
                teleWait = waitTime500;
            }
            else if (distance > 400)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime400 + "ms");
                teleWait = waitTime700;
            }
            else if (distance > 300)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime300 + "ms");
                teleWait = waitTime600;
            }
            else if (distance > 200)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime200 + "ms");
                teleWait = waitTime500;
            }
            else if (distance > 100)
            {
                Logging.Logger.Write("We Got SoftBanned From Jumping  " + distance + " meters so we will add  " + waitTime100 + "ms");
                teleWait = waitTime400;
            }
            else if (distance > 50)
            {
                teleWait = waitTime50;
            }

            else teleWait = 0;
            return teleWait;
        }


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
      //  public int teleWaiter { get; internal set; }
    }


    }



    public interface ITeleSettings
    {
        int waitTime50 { get; set; }
        int waitTime100  { get; set; }
        int waitTime200  { get; set; }
        int waitTime300  { get; set; }
        int waitTime400  { get; set; }
        int waitTime500  { get; set; }
        int waitTime600  { get; set; }
        int waitTime700  { get; set; }
        int waitTime800  { get; set; }
        int waitTime900  { get; set; }
        int waitTime1000  { get; set; }
        int waitTime1250  { get; set; }
        int waitTime1500  { get; set; }
        int waitTime2000  { get; set; }


}