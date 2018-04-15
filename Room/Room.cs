using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    
    class Room
    {
        public int RoomId;
        public List<Player> playerList = new List<Player>();
        public int playerNum;
        public int maxPlayer;
        public State state;
        //房间拥有者
        private Player roomOwner;
        public Player RoomOwner
        {
            get
            {
                return roomOwner;
            }
        }
        public enum State
        {
            Playing=1,
            Readying=0,
        }


        public Room(Player owner)
        {
            playerNum = 0;
            state = State.Readying;
            maxPlayer = 6;
            roomOwner = owner;
        }

        public void AddPlayer(Player player)
        {
            playerNum++;
            playerList.Add(player);
        }
    }
}
