#if !DEPENDENCY_HAS_SPACESHARED
using System.Linq;
using System.Text;
using StardewValley.Menus;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
         class Floatbox : Textbox
    {
        /*********
        ** Accessors
        *********/
        public float Value
        {
            get => float.TryParse(this.String, out float value) ? value : 0f;
            set => this.String = value.ToString();
        }

        public bool IsValid => float.TryParse(this.String, out _);


        /*********
        ** Protected methods
        *********/
        /// <inheritdoc />
        protected override void FilterInputText(TextBox _, List<TextBoxSnippet> contents)
        {
            bool hasDot = false;
            StringBuilder sb = new();
            for (int si = 0; si < contents.Count; ++si)
            {
                if (contents[si] is not TextBoxTextSnippet text)
                {
                    contents.RemoveAt(si);
                    si -= 1;
                    continue;
                }

                sb.Append(text.Message);
                bool modified = false;
                for (int i = 0; i < sb.Length; ++i)
                {
                    char c = sb[i];
                    if (!char.IsDigit(c) && !(c == '.' && !hasDot) && !(c == '-' && si == 0 && i == 0))
                    {
                        modified = true;
                        sb.Remove(i, 1);
                        i -= 1;
                        continue;
                    }
                    if (c == '.')
                        hasDot = true;
                }
                if (modified)
                {
                    text.Message = sb.ToString();
                }
                sb.Clear();
            }
        }
    }
}
#endif
