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
