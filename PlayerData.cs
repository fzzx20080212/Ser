using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    class PlayerData
    {
        public int score = 0;
        public int win, lost;
        public PlayerData()
        {
            score = 100;
            win = 0;
            lost = 0;
        }

    }
}
