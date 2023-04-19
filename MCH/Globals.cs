using System;
using System.IO;

namespace MCH
{
    internal static class Globals
    {
        public static string myDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MCH");
        /*public static string BP_cloudDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpaceEngineers", "Blueprints", "cloud");
        public static string BP_localDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpaceEngineers", "Blueprints", "local");*/
        public static string BP_exportDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpaceEngineers", "Export");
        public static string ModDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpaceEngineers", "Mods");

        public static uint modId = uint.MaxValue;



        public static bool NoModSelected() => modId == uint.MaxValue;
    }
}
