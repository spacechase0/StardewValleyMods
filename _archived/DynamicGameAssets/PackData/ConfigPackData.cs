using Microsoft.Xna.Framework;

namespace DynamicGameAssets.PackData
{
    public class ConfigPackData // Doesn't need dynamic fields or enable conditions, so not inheriting from BasePackData
    {
        public enum ConfigElementType
        {
            Label,
            Paragraph,
            Image,
            ConfigOption,
        }

        public enum ConfigValueType
        {
            Boolean,
            Integer,
            Float,
            String,
        }

        public string OnPage { get; set; } = "";
        public ConfigElementType ElementType { get; set; } = ConfigElementType.ConfigOption;

        public string PageToGoTo { get; set; }

        public string ImagePath { get; set; }
        public Rectangle? ImageRect { get; set; } = null;
        public int ImageScale { get; set; } = 4;

        public string Name { get; set; }
        public string Description { get; set; } = "";
        public ConfigValueType ValueType { get; set; }
        public string DefaultValue { get; set; }
        public string ValidValues { get; set; }
    }
}
