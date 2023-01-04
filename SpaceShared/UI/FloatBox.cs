using System.Linq;

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
