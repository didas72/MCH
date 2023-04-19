using System.Collections.Generic;

using DidasUtils.Logging;

namespace MCH.Data
{
    internal class SpawnGroup
    {
        public uint internalId;
        public string SubtypeId { get; set; }
        public List<DescriptionTag> DescriptionTags { get; }
        public double Frequency { get; set; }
        public List<Prefab> Prefabs { get; }



        public SpawnGroup(uint internalId, string subtypeId, double frequency)
        {
            this.internalId = internalId;
            SubtypeId = subtypeId;
            DescriptionTags = new();
            Frequency = frequency;
            Prefabs = new();
        }
        public SpawnGroup(uint internalId, string subtypeId, double frequency, List<DescriptionTag> tags, List<Prefab> prefabs)
        {
            this.internalId = internalId;
            SubtypeId = subtypeId;
            DescriptionTags = tags;
            Frequency = frequency;
            Prefabs = prefabs;
        }



        public override string ToString()
        {
            //<> used as separators since XML can't have them
            string ret = $"{internalId}<{SubtypeId}<{Frequency}<";

            foreach (DescriptionTag tag in DescriptionTags)
                ret += $"{tag}>";
            ret = ret.TrimEnd('>');
            ret += "<";

            foreach (Prefab prefab in Prefabs)
                ret += $"{prefab}>";
            ret = ret.TrimEnd('>');

            return ret;
        }
        public static bool TryFromString(string src, out SpawnGroup spawnGroup)
        {
            spawnGroup = null;
            string[] parts = src.Split('<', 5);

            if (parts.Length != 5)
            {
                Log.LogEvent($"Invalid spawn group line '{src}'.", "SpawnGroup.TryFromString");
                return false;
            }

            if (!uint.TryParse(parts[0], out uint id))
            {
                Log.LogEvent($"Could not parse id '{parts[0]}'.", "Mod.TryFromString");
                return false;
            }
            if (!double.TryParse(parts[2], out double freq))
            {
                Log.LogEvent($"Could not parse frequency '{parts[2]}'.", "Mod.TryFromString");
                return false;
            }

            string[] tagsS = parts[3].Split('>');
            List<DescriptionTag> tags = new(tagsS.Length);
            for (int i = 0; i < tagsS.Length; i++)
            {
                if (DescriptionTag.TryFromString(tagsS[i], out DescriptionTag tag))
                    tags.Add(tag);
            }
            tags.TrimExcess();

            string[] prefabsS = parts[4].Split('>');
            List<Prefab> prefabs = new(prefabsS.Length);
            for (int i = 0; i < prefabsS.Length; i++)
            {
                if (Prefab.TryFromString(prefabsS[i], out Prefab prefab))
                    prefabs.Add(prefab);
            }
            prefabs.TrimExcess();

            spawnGroup = new(id, parts[1], freq, tags, prefabs);
            return true;
        }



        public static string BuildSpawnGroupsFile(SpawnGroup[] groups)
        {
            string ret = "<?xml version=\"1.0\"?>\r\n" +
                "<Definitions xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n" +
                "\t<SpawnGroups>\r\n";

            foreach (SpawnGroup group in groups)
            {
                ret += FormatSpawnGroupXML(group);
            }

            return ret + "\t</SpawnGroups>\r\n</Definitions>\r\n";
        }
        public static string FormatSpawnGroupXML(SpawnGroup group)
        {
            string ret = "\t\t<SpawnGroup>\r\n\t\t\t<Id>\r\n\t\t\t\t<TypeId>SpawnGroupDefinition</TypeId>\r\n" +
                $"\t\t\t\t<SubtypeId>{group.SubtypeId}</SubtypeId>\r\n" +
                $"\t\t\t</Id>\r\n" +
                $"\t\t\t<Description>\r\n" +
                $"\t\t\t[Modular Encounters SpawnGroup]\r\n";

            foreach (DescriptionTag tag in group.DescriptionTags)
                ret += $"\t\t\t[{tag.Tag}:{tag.Value}]\r\n";

            ret += "\t\t\t</Description>\r\n" +
                "\t\t\t<IsPirate>true</IsPirate>\r\n" +
                $"\t\t\t<Frequency>{group.Frequency}</Frequency>\r\n" +
                "\t\t\t<Prefabs>\r\n";

            foreach (Prefab prefab in group.Prefabs)
                ret += $"\t\t\t\t<Prefab SubtypeId=\"{prefab.SubtypeId}\">\r\n" +
                    "\t\t\t\t\t<Position>\r\n" +
                    $"\t\t\t\t\t\t<X>{prefab.X}</X>\r\n" +
                    $"\t\t\t\t\t\t<Y>{prefab.Y}</Y>\r\n" +
                    $"\t\t\t\t\t\t<Z>{prefab.Z}</Z>\r\n" +
                    "\t\t\t\t\t</Position>\r\n" +
                    $"\t\t\t\t\t<Speed>{prefab.Speed}</Speed>\r\n" +
                    $"\t\t\t\t\t<Behaviour>{prefab.Behaviour}</Behaviour>\r\n" +
                    "\t\t\t\t</Prefab>\r\n";

            ret += "\t\t\t</Prefabs>\r\n" +
                "\t\t</SpawnGroup>\r\n";

            return ret;
        }
    }
}
