using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ScenePlayer
    {
        public string id;
        public float x = 0,y=0,z=0;
        public int score = 0;

        public ScenePlayer(string id)
        {
            this.id = id;
        }
    }
}
