using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    partial class HandlePlayerMsg
    {
        //获取玩家列表
        public void MsgGetList(Player player,ProtocolBase protocol)
        {
            Scene.instance.SendPlayerList(player);
        }

        //更新信息
        public void MsgUpdateInfo(Player player,ProtocolBase protocol)
        {
            int start = 0;
            ProtocolBytes proto = (ProtocolBytes)protocol;
            string name = proto.GetString(start, ref start);
            float x = proto.GetFloat(start, ref start);
            float y = proto.GetFloat(start, ref start);
            float z = proto.GetFloat(start, ref start);
            int score = player.data.score;
            Scene.instance.UpdateInfo(player.id, x, y, z, score);

            //广播
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("UpdateInfo");
            protocolRet.AddString(player.id);
            protocolRet.AddFloat(x);
            protocolRet.AddFloat(y);
            protocolRet.AddFloat(z);
            protocolRet.AddInt(score);
            ServNet.getInstance().Broadcast(protocolRet);
        }

        //获取玩家输赢信息
        public void MsgGetInfo(Player player,ProtocolBase protocol)
        {
            player.Send(player.GetInfo());
        }
    }
}
