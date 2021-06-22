using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace SpaceShared
{
    public class EscapeChecker
    {
        /*********
        ** Fields
        *********/
        private readonly IModHelper Helper;
        private readonly string Invoker;
        private bool Activated;


        /*********
        ** Accessors
        *********/
        public bool Requested { get; private set; }


        /*********
        ** Public methods
        *********/
        public EscapeChecker(IMod mod, string invoker = null, bool activate = false)
        {
            this.Helper = mod.Helper;
            this.Invoker = invoker;
            if (activate)
                this.Activate();
        }

        public void Activate()
        {
            if (this.Activated) return;
            this.Activated = true;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            Log.Trace($"{this.Invoker ?? ""} EscapeChecker activated");
        }

        public void Deactivate()
        {
            this.Activated = false;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            Log.Trace($"{this.Invoker ?? ""} EscapeChecker deactivated");
        }


        /*********
        ** Private methods
        *********/
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs args)
        {
            if (args.Released.Contains(SButton.Escape))
            {
                Log.Trace("Detected SButton.Escape");
                this.Deactivate();
                this.Requested = true;
            }
        }
    }
}
