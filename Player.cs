using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Player
    {
        public string id;
        public Conn conn;
        public PlayerData data;
        public PlayerTempData tempData;

        public Player(string id,Conn conn)
        {
            this.id = id;
            this.conn = conn;
            tempData = new PlayerTempData();
        }

        //发送
        public void Send(ProtocolBase proto)
        {
            if (conn == null)
                return;
            ServNet.getInstance().Send(conn,proto);
        }

        //下线
        public bool Logout()
        {
            //事件处理
            //ServNet.getInstance().handlePlayerEvent.OnLogout(this);
            //保存
            if (!DataMgr.instance.SavePlayer(this))
            {
                return false;
            }
            //下线
            ServNet.getInstance().HandlePlayerEvent.OnLogout(this);
            conn.player = null;
            conn.Close();
            return true;
        }

        //踢下线
        public static bool KickOff(string id,ProtocolBase protocol)
        {
            Conn[] conns = ServNet.getInstance().conns;
            for(int i=0;i<conns.Length;i++)
            {
                if (conns[i] == null) continue;
                if (!conns[i].isUse) continue;
                if (conns[i].player == null) continue;
                if(conns[i].player.id==id)
                {
                    lock (conns[i].player)
                    {
                        if (protocol != null)
                            conns[i].player.Send(protocol);
                        return conns[i].player.Logout();
                    }
                }
            }
            return true;
        }

        //获取角色信息
        public ProtocolBase GetInfo()
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetInfo");
            protocol.AddString(id);
            protocol.AddInt(data.win);
            protocol.AddInt(data.lost);
            return protocol;
        }
    }
}
