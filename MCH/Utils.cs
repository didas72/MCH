using System.IO;

namespace MCH
{
    internal static class Utils
    {
        public static void CopyFilesRecursive(string src, string dest)
        {
            Directory.CreateDirectory(dest);

            foreach (string dir in Directory.GetDirectories(src))
                CopyFilesRecursive(dir, Path.Combine(dest, Path.GetFileName(dir)));

            foreach (string file in Directory.GetFiles(src))
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
        }
    }
}
