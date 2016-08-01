#region using directives

using System;
using System.IO;
using PoGo.PokeMobBot.Logic.State;

#endregion

namespace PoGo.PokeMobBot.Logic.DataDumper
{
    public class Dumper
    {
        /// <summary>
        ///     Clears the specified dumpfile.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="filename" />
        /// File to clear/param>
        public void ClearDumpFile(ISession session, string filename)
        {
            var path = Path.Combine(session.LogicSettings.ProfilePath, "Dumps");
            var file = Path.Combine(path,
                $"PokeMobBot-{filename}-{DateTime.Today.ToString("yyyy-MM-dd")}-{DateTime.Now.ToString("HH")}.txt");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            // Clears all contents of a file first if overwrite is true
            File.WriteAllText(file, string.Empty);
        }

        /// <summary>
        ///     Dumps data to a file
        /// </summary>
        /// <param name="session"></param>
        /// <param name="data">Dumps the string data to the file</param>
        /// <param name="filename">Filename to be used for naming the file.</param>
        public void Dump(ISession session, string data, string filename)
        {
            string uniqueFileName = $"{filename}";

            DumpToFile(session, data, uniqueFileName);
        }

        /// <summary>
        ///     This is used for dumping contents to a file stored in the Logs folder.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="data">Dumps the string data to the file</param>
        /// <param name="filename">Filename to be used for naming the file.</param>
        private void DumpToFile(ISession session, string data, string filename)
        {
            var path = Path.Combine(session.LogicSettings.ProfilePath, "Dumps",
                $"PokeMobBot-{filename}-{DateTime.Today.ToString("yyyy-MM-dd")}-{DateTime.Now.ToString("HH")}.txt");

            using (
                var dumpFile =
                    File.AppendText(path)
                )
            {
                dumpFile.WriteLine(data);
                dumpFile.Flush();
            }
        }

        /// <summary>
        ///     Set the dumper.
        /// </summary>
        /// <param name="dumper"></param>
        /// <param name="subPath"></param>
        public void SetDumper(IDumper dumper, string subPath = "")
        {
        }
    }
}
