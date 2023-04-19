using System;

using DidasUtils.Logging;

namespace MCH.Data
{
    internal class Mod
    {
        public uint internalId;
        public string Name { get; set; }



        public Mod(uint internalId, string name)
        {
            this.internalId = internalId;
            Name = name;
        }


        public override string ToString()
        {
            //\ used as a separator since folders can't have it
            return $"{internalId}\\{Name}";
        }
        public static bool TryFromString(string src, out Mod mod)
        {
            mod = null;
            string[] parts = src.Split('\\', 2);

            if (parts.Length != 2)
            {
                Log.LogEvent($"Invalid mod list line '{src}'.", "Mod.TryFromString");
                return false;
            }

            if (!uint.TryParse(parts[0], out uint id))
            {
                Log.LogEvent($"Could not parse id '{parts[0]}'.", "Mod.TryFromString");
                return false;
            }

            mod = new(id, parts[1]);
            return true;
        }
    }
}
