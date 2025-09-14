using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace BackstoryQuestionsFramework
{
    public interface IApi
    {
        List<string> GetQuestionsAskedFor(Farmer perspective, NPC npc);
    }
    public class Api : IApi
    {
        public List<string> GetQuestionsAskedFor(Farmer perspective, NPC npc)
        {
            return perspective.friendshipData[npc.Name].get_questionsAsked().ToList();
        }
    }
}
