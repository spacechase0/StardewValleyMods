namespace GenericModConfigMenu.UI
{
    internal class Intbox : Textbox
    {
        public int Value
        {
            get { return (this.String == "" || this.String == "-") ? 0 : int.Parse(this.String); }
            set { this.String = value.ToString(); }
        }

        protected override void receiveInput(string str)
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
            if (this.Callback != null)
                this.Callback.Invoke(this);
        }
    }
}
