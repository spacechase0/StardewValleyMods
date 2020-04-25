using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.UI
{
    class Intbox : Textbox
    {
        public int Value
        {
            get { return (String == "" || String == "-") ? 0 : int.Parse(String); }
            set { String = value.ToString(); }
        }

        protected override void receiveInput(string str)
        {
            bool valid = true;
            for ( int i = 0; i < str.Length; ++i )
            {
                char c = str[i];
                if ( !char.IsDigit(c) && !(c == '-' && String == "" && i == 0) )
                {
                    valid = false;
                    break;
                }
            }
            if (!valid)
                return;

            String += str;
            if (Callback != null)
                Callback.Invoke(this);
        }
    }
}
