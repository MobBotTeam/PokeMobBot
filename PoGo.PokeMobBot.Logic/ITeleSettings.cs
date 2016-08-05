#region using directives

using System;
using System.Collections.Generic;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic
{
    public class TeleDelay
    {
        private int teleWait;

        public int teleWaiter()
        {
            Logging.Logger.Write("Test");
            return waitTime100;
        }

         public int teleWaiter(double distance)
        {
            if (distance > 2000)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime2000 + "ms");
                return waitTime2000;
            }
            else if (distance > 1500)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime1500 + "ms");
                return waitTime1500;
            }
            else if (distance > 1250)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime1250 + "ms");
                return waitTime1250;
            }
            else if (distance > 1000)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime1000 + "ms");
                return waitTime1000;
            }
            else if (distance > 900)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime900 + "ms");
                return waitTime900;
            }
            else if (distance > 800)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime800 + "ms");
                return waitTime800;
            }
            else if (distance > 700)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime700 + "ms");
               return waitTime700;
            }
            else if (distance > 600)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime600 + "ms");
                return waitTime600;
            }
            else if (distance > 500)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime500 + "ms");
                return waitTime500;
            }
            else if (distance > 400)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime400 + "ms");
                return waitTime400;
            }
            else if (distance > 300)
            {
                Logging.Logger.Write("We are teleporting " + distance + " meters so we will wait " + waitTime300 + "ms");
                return waitTime300;
            }
            else if (distance > 200)
            {
                return waitTime200;
            }
            else if (distance > 100)
            {
                return waitTime100;
            }
            else if (distance > 50)
            {
                return waitTime50;
            }

            else teleWait = 0;
            return teleWait;
        }
        public class waitTime
        {
           

            public  waitTime(int WaitTime100)
            {
                waitTime100 = WaitTime100;
            }

            public int waitTime100 { get; set; }
            
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