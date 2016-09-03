using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoGo.PokeMobBot.Logic.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace PoGo.PokeMobBot.Logic.API
{
    public class GeoLatLonAlt
    {
        public string lat { get; set; }
        public string lon { get; set; }
        public string alt { get; set; }
    }
    public class MapzenAPI
    {
        private static Random r = new Random();
        protected string api = "https://elevation.mapzen.com/height";
        protected string[] options = new string[3] { "?json={\"shape\":[{\"lat\":", ",\"lon\":", "}]}&api_key=" };

        protected string[] _data;
        protected string url(string lat, string lon, string key)
        {
            _data = new string[3] { lat, lon, "" };
            return api + options[0] + lat + options[1] + lon + options[2] + key;
        }
        protected string request(string httpUrl)
        {
            try
            {
                string get = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(httpUrl);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    get = reader.ReadToEnd();
                }
                JObject json = JObject.Parse(get);
#if DEBUG
                Logger.Write(json.ToString(), LogLevel.Debug);
                Logger.Write("Altitude: " + json["height"][0], LogLevel.Debug);
#endif
                return (string)json["height"][0];
            }
            catch (Exception ex)
            {
                Logger.Write("ERROR: " + ex.Message, LogLevel.Error);
                return "ERROR";
            }

        }
        protected double height(string h)
        {
            if (h.Equals("ERROR"))
            {
                Logger.Write("There was an error grabbing Altitude from Mapzen API! Check your Elevation API Key!", LogLevel.Warning);
                return r.Next(8, 12);
            }
            Logger.Write("Successfully grabbed new Mapzen Elevation: " + h + " Meters.", LogLevel.Info);
            _data[2] = h;
            List<GeoLatLonAlt> data = new List<GeoLatLonAlt>();
            GeoLatLonAlt latLonAlt = new GeoLatLonAlt()
            {
                lat = _data[0],
                lon = _data[1],
                alt = _data[2]
            };
            data.Add(latLonAlt);
            string json = JsonConvert.SerializeObject(data.ToArray());
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "MapzenAPI"));
            string filepath = "MapzenAPI/" + latLonAlt.lat + "_" + latLonAlt.lon;
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), filepath.Replace(".", ",") + ".json"), json);
            return double.Parse(h);
        }
        public bool checkForExistingAltitude(string possibleFile)
        {
            if (File.Exists(possibleFile))
            {
                return true;
            }
            return false;
        }
        public double getExistingAltitude(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                List<GeoLatLonAlt> fileData = JsonConvert.DeserializeObject<List<GeoLatLonAlt>>(json);
                Logger.Write("Found old Altitude data, using this to avoid Mapzen API call.", LogLevel.Info);
                return double.Parse(fileData[0].alt);
            }
        }
        public double getAltitude(double lat, double lon, string key)
        {
            Logger.Write("Using MapzenAPI to obtian Altitude based on Longitude and Latitude.", LogLevel.Info);
            string possibleFile = Path.Combine(Directory.GetCurrentDirectory(), "MapzenAPI/" + lat.ToString().Replace(".", ",") + "_" + lon.ToString().Replace(".", ",") + ".json");
            if (checkForExistingAltitude(possibleFile))
            {
                return getExistingAltitude(possibleFile);
            }
            if (Equals(lat, default(double)) || Equals(lon, default(double)) || !key.Equals(""))
            {
                return height(request(url(lat.ToString(), lon.ToString(), key)));
            }
            else
            {
                List<string> invalids = new List<string>();
                if (!Equals(lat, default(double)))
                {
                    invalids.Add("DefaultLatitude");
                }
                if (!Equals(lon, default(double)))
                {
                    invalids.Add("DefaultLongitude");
                }
                if (!key.Equals(""))
                {
                    invalids.Add("Mapzen Elevation API Key");
                }
                string message = invalids.Count > 0 ? "Invalid Settings: " : "";
                if (message.Equals("Invalid Settings: "))
                {
                    foreach (string invalid in invalids)
                    {
                        message += invalid + ", ";
                    }
                    message = message.Substring(0, message.Length - 3);
                }
                if (message.Length > 0)
                {
                    Logger.Write(message, LogLevel.Error);
                }

                return r.Next(8, 12);
            }
        }
    }

}
