using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Reflection;
using System.Data;

namespace Server
{
    class Conn
    {
        public const int BUFFER_SIZE = 1024*1024;
        public Socket socket;
        public bool isUse = false;
        public byte[] readBuff = new byte[BUFFER_SIZE];
        public int buffCount = 0;

        //粘宝分包
        public byte[] lenBytes = new byte[sizeof(UInt32)];
        public Int32 msglength = 0;

        //心跳时间
        public long lastTickTime = long.MinValue;

        public Player player;

        public Conn()
        {

        }

        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;
            lastTickTime = Sys.GetTimeStamp();

        }

        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }

        //获取客户端地址
        public string GetAdress()
        {
            if (!isUse)
                return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }

        //关闭
        public void Close()
        {
            if (!isUse)
                return;
            if(player!=null)
            {
                player.Logout();
                return;
            }
            Console.WriteLine("[断开链接]" + GetAdress());
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isUse = false;
        }

        //发送协议
        public void Send(ProtocolBase protocol)
        {
            ServNet.getInstance().Send(this, protocol);
        }
    }
}
