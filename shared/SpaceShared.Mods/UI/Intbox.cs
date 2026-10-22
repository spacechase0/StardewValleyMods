using System.Text;
using StardewValley.Menus;

#if !DEPENDENCY_HAS_SPACESHARED
#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
         class Intbox : Textbox
    {
        /*********
        ** Accessors
        *********/
        public int Value
        {
            get => int.TryParse(this.String, out int value) ? value : 0;
            set => this.String = value.ToString();
        }

        public bool IsValid => int.TryParse(this.String, out _);


        /*********
        ** Protected methods
        *********/
        /// <inheritdoc />
        protected override void FilterInputText(TextBox _, List<TextBoxSnippet> contents)
        {
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
                    if (!char.IsDigit(c)&& !(c == '-' && si == 0 && i == 0))
                    {
                        modified = true;
                        sb.Remove(i, 1);
                        i -= 1;
                        continue;
                    }
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
