using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.UI
{
    class Floatbox : Textbox
    {
        public float Value
        {
            get { return (String == "" || String == "-") ? 0 : float.Parse(String); }
            set { String = value.ToString(); }
        }

        protected override void receiveInput(string str)
        {
            bool hasDot = String.Contains('.');
            bool valid = true;
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if ( !char.IsDigit(c) && !(c == '.' && !hasDot) && !(c == '-' && String == "" && i == 0))
                {
                    valid = false;
                    break;
                }
                if (c == '.')
                    hasDot = true;
            }
            if (!valid)
                return;

            String += str;
            if (Callback != null)
                Callback.Invoke(this);
        }
    }
}
