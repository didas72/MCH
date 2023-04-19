using System;

using DidasUtils.Logging;

namespace MCH.Data
{
    internal class Prefab
    {
        public string SubtypeId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Speed { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public string Behaviour { get; set; }



        public Prefab(string subtypeId, double x, double y, double z, double speed, string behaviour)
        {
            SubtypeId = subtypeId;
            X = x;
            Y = y;
            Z = z;
            Speed = speed;
            Behaviour = behaviour;
        }



        public override string ToString()
        {
            return $"{SubtypeId}@{X}@{Y}@{Z}@{Speed}@{Behaviour}";
        }
        public static bool TryFromString(string src, out Prefab prefab)
        {
            prefab = null;
            string[] parts = src.Split('@', 6);

            if (parts.Length != 6)
            {
                Log.LogEvent($"Invalid prefab '{src}'.", "Prefab.TryFromString");
                return false;
            }

            if (!double.TryParse(parts[1], out double x))
            {
                Log.LogEvent($"Could not parse x '{parts[1]}'.", "Prefab.TryFromString");
                return false;
            }
            if (!double.TryParse(parts[2], out double y))
            {
                Log.LogEvent($"Could not parse y '{parts[2]}'.", "Prefab.TryFromString");
                return false;
            }
            if (!double.TryParse(parts[3], out double z))
            {
                Log.LogEvent($"Could not parse z '{parts[3]}'.", "Prefab.TryFromString");
                return false;
            }
            if (!double.TryParse(parts[4], out double speed))
            {
                Log.LogEvent($"Could not parse speed '{parts[4]}'.", "Prefab.TryFromString");
                return false;
            }

            prefab = new(parts[0], x, y, z, speed, parts[5]);
            return true;
        }
    }
}
