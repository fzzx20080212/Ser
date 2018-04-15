using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class RoomMgr
    {
        public List<Room> roomList = new List<Room>();
        public static RoomMgr instance = new RoomMgr();
        
        private RoomMgr() { }

        //获取房间列表
        public ProtocolBase GetRoomList()
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetRoomList");
            int count = roomList.Count;
            protocol.AddInt(count);
            lock (roomList)
            {
                foreach (Room room in roomList)
                {
                    protocol.AddInt(room.playerNum);
                    protocol.AddInt((int)room.state);
                }
            }
            return protocol;
        }

        //创建房间
        public bool CreateRoom(Player player)
        {
            if (player.tempData.state == PlayerTempData.State.None)
            {
                Room room = new Room(player);
                room.state = Room.State.Readying;
                room.AddPlayer(player);
                lock(roomList){
                    roomList.Add(room);
                }
                ServNet.getInstance().Broadcast(GetRoomList());
                return true;
            }

            return false;
        }

        void Broadcast()
        {

        }
    }
}
