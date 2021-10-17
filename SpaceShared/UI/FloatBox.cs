using System.Linq;

namespace SpaceShared.UI
{
    internal class Floatbox : Textbox
    {
        /*********
        ** Accessors
        *********/
        public float Value
        {
            get => (this.String is "" or "-") ? 0 : float.Parse(this.String);
            set => this.String = value.ToString();
        }


        /*********
        ** Protected methods
        *********/
        /// <inheritdoc />
        protected override void ReceiveInput(string str)
        {
            bool hasDot = this.String.Contains('.');
            bool valid = true;
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if (!char.IsDigit(c) && !(c == '.' && !hasDot) && !(c == '-' && this.String == "" && i == 0))
                {
                    valid = false;
                    break;
                }
                if (c == '.')
                    hasDot = true;
            }
            if (!valid)
                return;

            this.String += str;
            this.Callback?.Invoke(this);
        }
    }
}
