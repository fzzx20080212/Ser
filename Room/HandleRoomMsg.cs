using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    partial class HandlePlayerMsg
    {
        public void MsgGetRoomList(Player player,ProtocolBase protocol)
        {
            ProtocolBytes proto = (ProtocolBytes)RoomMgr.instance.GetRoomList();
            player.Send(proto);
            
        }

        public void MsgCreateRoom(Player player,ProtocolBase protocol)
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("CreateRoom");
            if (RoomMgr.instance.CreateRoom(player))
            {
                proto.AddInt(0);
            }
            else
                proto.AddInt(-1);
            player.Send(proto);

        }
    }
}
