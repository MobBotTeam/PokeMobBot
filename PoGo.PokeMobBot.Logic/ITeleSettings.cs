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