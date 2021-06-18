using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace SpaceShared
    {
    public class EscapeChecker
        {
        public bool Requested { get; private set; } = false;
        private readonly IModHelper Helper;
        private readonly string Invoker;
        private bool Activated = false;
        public EscapeChecker(IMod mod, string invoker = null, bool activate = false) {
            this.Helper = mod.Helper;
            this.Invoker = invoker;
            if (activate) this.Activate();
            }
        public void Activate() {
            if (this.Activated) return;
            this.Activated = true;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            Log.Trace($"{this.Invoker ?? ""} EscapeChecker activated");
            }
        public void Deactivate() {
            this.Activated = false;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            Log.Trace($"{this.Invoker ?? ""} EscapeChecker deactivated");
            }
        public void OnButtonsChanged(object sender, ButtonsChangedEventArgs args) {
            if (args.Released.Contains(SButton.Escape)) {
                Log.Trace("Detected SButton.Escape");
                this.Deactivate();
                this.Requested = true;
                }
            }
        }
    }
