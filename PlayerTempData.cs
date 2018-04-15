using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class PlayerTempData
    {
        public enum State
        {
            None=0,
            Prepare=2,
            InRoom=1,
            Playing=3,
        }

        public State state;
        public PlayerTempData()
        {
            state = State.None;
        }
    }
}
