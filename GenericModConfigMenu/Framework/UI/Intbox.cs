namespace GenericModConfigMenu.Framework.UI
{
    internal class Intbox : Textbox
    {
        /*********
        ** Accessors
        *********/
        public int Value
        {
            get => (this.String is "" or "-") ? 0 : int.Parse(this.String);
            set => this.String = value.ToString();
        }


        /*********
        ** Protected methods
        *********/
        /// <inheritdoc />
        protected override void ReceiveInput(string str)
        {
            bool valid = true;
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if (!char.IsDigit(c) && !(c == '-' && this.String == "" && i == 0))
                {
                    valid = false;
                    break;
                }
            }
            if (!valid)
                return;

            this.String += str;
            this.Callback?.Invoke(this);
        }
    }
}
