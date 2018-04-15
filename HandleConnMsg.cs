using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    partial class HandleConnMsg
    {
        public void MsgHeartBeat(Conn conn,ProtocolBase protocolBase)
        {
            conn.lastTickTime = Sys.GetTimeStamp();
            Console.WriteLine("[更新心跳时间]" + conn.GetAdress());
        }

        public void MsgRegister(Conn conn,ProtocolBase protocolBase)
        {
            //获取数值
            ProtocolBytes protocolBytes = (ProtocolBytes)protocolBase;
            int start = 0;
            string name = protocolBytes.GetString(start,ref start);
            string id = protocolBytes.GetString(start,ref start);
            string pw = protocolBytes.GetString(start, ref start);
            Console.WriteLine("[注册协议]" + "用户名：" + id + "密码:" + pw);
            protocolBytes = new ProtocolBytes();
            protocolBytes.AddString("Register");

            //注册
            if (DataMgr.instance.Register(id, pw))
            {
                protocolBytes.AddInt(0);
                //创建角色
                DataMgr.instance.CreatePlayer(id);
            }
            else
                protocolBytes.AddInt(-1);

            
            conn.Send(protocolBytes);
        }

        public void MsgLogin(Conn conn,ProtocolBase protocolBase)
        {
            int start = 0;
            ProtocolBytes protocolBytes = (ProtocolBytes)protocolBase;
            string name = protocolBytes.GetString(start, ref start);
            string id = protocolBytes.GetString(start, ref start);
            string pw = protocolBytes.GetString(start, ref start);
            Console.WriteLine("[登入协议]" + "用户名：" + id + "密码:" + pw);
            protocolBytes = new ProtocolBytes();
            protocolBytes.AddString("Login");
            //验证
            if (!DataMgr.instance.CheckPassWord(id, pw))
            {
                protocolBytes.AddInt(-1);
                conn.Send(protocolBytes);
                return;
            }
            //是否已经登入,如果已登入，强制退出，并且此次登入失效
            ProtocolBytes protocolLogout = new ProtocolBytes();
            protocolLogout.AddString("Logout");
            if(!Player.KickOff(id, protocolLogout))
            {
                protocolBytes.AddInt(-1);
                conn.Send(protocolBytes);
                return;
            }

            //获取玩家数据
            PlayerData playerData = DataMgr.instance.GetPlayerData(id);
            if (playerData == null)
            {
                protocolBytes.AddInt(-1);
                conn.Send(protocolBytes);
                return;
            }

            conn.player = new Player(id, conn);
            conn.player.data = playerData;
            //事件触发
            ServNet.getInstance().HandlePlayerEvent.OnLogin(conn.player);
            protocolBytes.AddInt(0);
            conn.Send(protocolBytes);
            return;
        }


        //登出
        public void MsgLogout(Conn conn,ProtocolBase protocolBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("Logout");
            protocol.AddInt(0);
            conn.Send(protocol);
            if (conn.player == null)
            {
                conn.Close();
            }
            else
                conn.player.Logout();
        }

    }
}
