using System;

using DidasUtils.Logging;

namespace MCH.Data
{
    internal class DescriptionTag
    {
        public string Tag { get; }
        public string Value { get; set; }



        public DescriptionTag(string tag, string value)
        {
            Tag = tag;
            Value = value;
        }



        public override string ToString()
        {
            //: used as a separator bc of tag format
            return $"{Tag}:{Value}";
        }
        public static bool TryFromString(string src, out DescriptionTag tag)
        {
            tag = null;
            string[] parts = src.Split(':', 2);

            if (parts.Length != 2)
            {
                Log.LogEvent($"Invalid description tag '{src}'.", "DescriptionTag.TryFromString");
                return false;
            }

            tag = new(parts[0], parts[1]);
            return true;
        }
    }
}
