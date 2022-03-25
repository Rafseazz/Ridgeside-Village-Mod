using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgesideVillage
{
    public interface IQuestFrameworkApi
    {

        /// <summary>
        /// Returns an quest id of managed quest
        /// </summary>
        /// <param name="fullQuestName">A fullqualified name of quest (questName@youdid)</param>
        /// <returns>
        /// Quest id if the quest with <param>fullQuestName</param> exists and it's managed, otherwise returns -1
        /// </returns>
        int ResolveQuestId(string fullQuestName);

        /// <summary>
        /// Resolves a fullqualified name (questName@youdid) of managed quest with this id
        /// </summary>
        /// <param name="questId"></param>
        /// <returns>
        /// Fullname of managed quest if this quest is managed by QF, otherwise returns null
        /// </returns>
        string ResolveQuestName(int questId);

        /// <summary>
        /// Is the quest with this id managed by QF?
        /// </summary>
        /// <param name="questId">A number representing quest id</param>
        /// <returns>True if this quest is managed, otherwise False</returns>
        bool IsManagedQuest(int questId);
    }
}
