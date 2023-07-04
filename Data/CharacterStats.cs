using System.Collections;
using System.Collections.Generic;

namespace GRPG.Data
{
    public struct CharacterStats
    {
        public CharacterStats(int MP)
        {
            this.MP = MP;
        }

        /// <summary>
        /// Movement points
        /// </summary>
        public int MP;
    }
}