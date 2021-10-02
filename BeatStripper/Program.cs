using System;
using System.IO;
using System.Linq;

namespace BeatStripper
{
    class Program
    {
        internal static string InstallDirectory;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0] != null)
                {
                    InstallDirectory = Path.GetDirectoryName(args[0]);
                    if (File.Exists(Path.Combine(InstallDirectory, InstallDir.GorillaTagEXE)) == false)
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    Logger.Log("Resolving Gorilla Tag install directory");
                    InstallDirectory = InstallDir.GetInstallDir();
                    if (InstallDirectory == null)
                    {
                        throw new Exception();
                    }
                }
                
                string bepinexLibsDir = Path.Combine(InstallDirectory, @"BepInEx", @"core");
                string managedDir = Path.Combine(InstallDirectory, InstallDir.GorillaTagDataFolder, @"Managed");

                //Logger.Log("Resolving Gorilla Tag version");
                //string version = VersionFinder.FindVersion(InstallDirectory);

                string outDir = Path.Combine(Directory.GetCurrentDirectory(), "stripped");//, version);
                Logger.Log("Creating output directory");
                Directory.CreateDirectory(outDir);

                string[] whitelist = new string[]
                {
                    "TextMeshPro",
                    "UnityEngine.",
                    "Assembly-CSharp",
                    "0Harmony",
                    "Newtonsoft.Json",
                    "Cinemachine",
                    "Photon",
                    "Unity.",
                    "BepInEx",
                };

                foreach (string f in ResolveDLLs(managedDir, whitelist))
                {
                    StripDLL(f, outDir, bepinexLibsDir, managedDir);
                }

                if (Directory.Exists(bepinexLibsDir))
                {
                    foreach (string f in ResolveDLLs(bepinexLibsDir, whitelist))
                    {
                        StripDLL(f, outDir, bepinexLibsDir, managedDir);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        internal static string[] ResolveDLLs(string managedDir, string[] whitelist)
        {
            var files = Directory.GetFiles(managedDir).Where(path =>
            {
                FileInfo info = new FileInfo(path);
                if (info.Extension != ".dll") return false;

                foreach (string substr in whitelist)
                {
                    if (info.Name.Contains(substr)) return true;
                }

                return false;
            });

            return files.ToArray();
        }

        internal static void StripDLL(string f, string outDir, params string[] resolverDirs)
        {
            if (File.Exists(f) == false) return;
            var file = new FileInfo(f);
            Logger.Log($"Stripping {file.Name}");

            var mod = ModuleProcessor.Load(file.FullName, resolverDirs);
            mod.Virtualize();
            mod.Strip();

            string outFile = Path.Combine(outDir, file.Name);
            mod.Write(outFile);
        }
    }
}
