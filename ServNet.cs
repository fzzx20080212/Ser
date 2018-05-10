using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Reflection;

namespace Server
{
    class ServNet
    {
        public Socket socket;
        public Conn[] conns;
        public int maxConn = 50;
        private static ServNet instance;
        //主定时器
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        //协议
        public ProtocolBase proto;

        //心跳时间
        public long heartBeatTime = 30;

        //消息分发
        public HandleConnMsg handleConnMsg = new HandleConnMsg();
        public HandlePlayerMsg HandlePlayerMsg = new HandlePlayerMsg();
        public HandlePlayerEvent HandlePlayerEvent = new HandlePlayerEvent();
        private ServNet()
        {
            
        }

        public static ServNet getInstance()
        {
       
            if (instance == null)
                instance = new ServNet();
            return instance;
        }

        //获取连接池索引
        public int NewIndex()
        {
            if (conns == null)
                return -1;
            for (int i = 0; i < maxConn; i++)
            {
                if (conns[i] == null)
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false)
                    return i;
            }
            return -1;
        }


        //开启服务器
        public void Start(string host, int port)
        {
            conns = new Conn[maxConn];
            proto = new ProtocolBytes();
            for (int i = 0; i < maxConn; i++)
            {
                conns[i] = new Conn();
            }
            //Bind
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            socket.Bind(ipEp);

            //Listen
            socket.Listen(maxConn);

            //Accept
            socket.BeginAccept(AcceptCb, null);

            //定时器
            //timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
            //timer.AutoReset = false;
            //timer.Enabled = true;
        }

        //关闭服务器链接
        public void Close()
        {
            for(int i=0;i<conns.Length;i++)
            {
                Conn conn = conns[i];
                if (conn == null) continue;
                lock (conn)
                {
                    conn.Close();
                }
            }
        }

        //与客户端建立链接
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = socket.EndAccept(ar);
                int index = NewIndex();
                if (index < 0)
                {
                    clientSocket.Close();
                    Console.WriteLine("[警告]连接池已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(clientSocket);
                    Console.WriteLine("客户端连接[" + conn.GetAdress() + "]");
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                socket.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb失败：" + e.Message);
            }
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            try
            {
                //获取接受的字节数
                int count = conn.socket.EndReceive(ar);
                if (count <= 0)
                {
                    Console.WriteLine("收到[" + conn.GetAdress() + "]断开连接");
                    conn.Close();
                    return;
                }
                //数据处理
                //string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
                conn.buffCount += count;
                ProcessData(conn);

                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
        }
            catch (Exception e)
            {
                Console.WriteLine("收到[" + conn.GetAdress() + "]断开连接"+e.Message);
                conn.Close();
            }
}

        //粘包分包处理
        private void ProcessData(Conn conn)
        {
            if (conn.buffCount < sizeof(Int32))
                return;
            //将前四个字节复制到
            Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
            conn.msglength = BitConverter.ToInt32(conn.lenBytes, 0);
            if (conn.buffCount < conn.msglength + sizeof(Int32))
                return;
            //处理消息
            //string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, sizeof(Int32), conn.msglength);
            //Send(conn,str);
            ProtocolBase protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msglength);
            HandleMsg(conn, protocol);
            //清除已处理的消息
            int count = conn.buffCount - sizeof(Int32) - conn.msglength;
            Array.Copy(conn.readBuff, sizeof(Int32) + conn.msglength, conn.readBuff, 0, count);
            conn.buffCount = count;
            if (conn.buffCount > 0)
                ProcessData(conn);
        }

        private void HandleMsg(Conn conn,ProtocolBase protoBase)
        {
            string name = protoBase.GetName();
            string methodName = "Msg" + name;
            //连接协议分发
            if(conn.player==null||name=="HeartBeat"||name=="Logout")
            {
                MethodInfo methodInfo = handleConnMsg.GetType().GetMethod(methodName);
                if(methodInfo==null)
                {
                    Console.WriteLine("[警告]HandleMsg没有处理连接方法" + methodName);
                    return;
                }

                Object[] obj = new Object[] { conn, protoBase };
                methodInfo.Invoke(handleConnMsg, obj);
            }
            //角色协议分发
            else
            {
                MethodInfo methodInfo = HandlePlayerMsg.GetType().GetMethod(methodName);
                if (methodInfo == null)
                {
                    Console.WriteLine("[警告]HandleMsg没有处理连接方法" + methodName);
                    return;
                }

                Object[] obj = new Object[] { conn.player, protoBase };
                methodInfo.Invoke(HandlePlayerMsg, obj);
            }
        }

        public void Send(Conn conn,ProtocolBase protocol)
        {
            byte[] bytes = protocol.Encode();
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendbuff = length.Concat(bytes).ToArray();
            try
            {
                conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
            }
            catch(Exception e)
            {
                Console.WriteLine("[发送信息]" + conn.GetAdress() + ":" + e.Message);
            }
        }

        //广播
        public void Broadcast(ProtocolBase protocol)
        {
            for(int i=0;i<conns.Length;i++)
            {
                if (!conns[i].isUse)
                    continue;
                if (conns[i].player == null)
                    continue;
                Send(conns[i], protocol);
            }
        }

        //主定时器
        public void HandleMainTimer(object sender,System.Timers.ElapsedEventArgs e)
        {
            HeartBeat();
            timer.Start();
      
        }
        //心跳
        public void HeartBeat()
        {
            //Console.WriteLine("[主定时器执行]");
            long timeNow = Sys.GetTimeStamp();
            for(int i=0;i<conns.Length;i++)
            {
                Conn conn = conns[i];
                if (conn == null) return;
                if (!conn.isUse) return;
                if (conn.lastTickTime < timeNow - heartBeatTime)
                {
                    Console.WriteLine("[心跳引起断开连接]" + conn.GetAdress());
                    lock (conn)
                        conn.Close();
                }
            }
        }
    }
}
