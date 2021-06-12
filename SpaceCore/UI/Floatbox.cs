using System.Linq;

namespace SpaceCore.UI
{
    public class FloatBox : Textbox
    {
        public float Value
        {
            get { return (this.String == "" || this.String == "-") ? 0 : float.Parse(this.String); }
            set { this.String = value.ToString(); }
        }

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
            if (this.Callback != null)
                this.Callback.Invoke(this);
        }
    }
}
