using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace BeatStripper
{
    // Shamelessly stolen from ModAssistant

    internal class InstallDir
    {
        internal const string GorillaTagAPPID = "1533390";
        internal const string GorillaTagEXE = "Gorilla Tag.exe";
        internal const string GorillaTagDataFolder = "Gorilla Tag_Data";

        public static string GetInstallDir()
        {
            string InstallDir = null;
            
            try
            {
                InstallDir = GetSteamDir();
            }
            catch { }

            if (!string.IsNullOrEmpty(InstallDir))
                return InstallDir;
            
            return null;
        }

        public static string GetSteamDir()
        {
            string SteamInstall = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)?.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam")?.GetValue("InstallPath").ToString();
            if (string.IsNullOrEmpty(SteamInstall))
            {
                SteamInstall = Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam")?.GetValue("InstallPath").ToString();
            }

            if (string.IsNullOrEmpty(SteamInstall)) return null;

            string vdf = Path.Combine(SteamInstall, @"steamapps\libraryfolders.vdf");
            if (!File.Exists(@vdf)) return null;

            Regex regex = new Regex("\\s\"\\d\"\\s+\"(.+)\"");
            List<string> SteamPaths = new List<string>
            {
                Path.Combine(SteamInstall, @"steamapps")
            };

            using (StreamReader reader = new StreamReader(@vdf))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        SteamPaths.Add(Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), @"steamapps"));
                    }
                }
            }

            regex = new Regex("\\s\"installdir\"\\s+\"(.+)\"");
            foreach (string path in SteamPaths)
            {
                if (File.Exists(Path.Combine(@path, @"appmanifest_" + GorillaTagAPPID + ".acf")))
                {
                    using (StreamReader reader = new StreamReader(Path.Combine(@path, @"appmanifest_" + GorillaTagAPPID + ".acf")))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Match match = regex.Match(line);
                            if (match.Success)
                            {
                                if (File.Exists(Path.Combine(@path, @"common", match.Groups[1].Value, GorillaTagEXE)))
                                {
                                    return Path.Combine(@path, @"common", match.Groups[1].Value);
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
