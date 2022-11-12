using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceShared.APIs
{
    public interface IAdvancedSocialInteractionsApi
    {
        public event EventHandler<Action<string, Action>> AdvancedInteractionStarted;
    }
}
