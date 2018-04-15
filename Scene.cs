using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Scene
    {
        public static Scene instance=new Scene();
        private Scene() { }
        List<ScenePlayer> list = new List<ScenePlayer>();

        //根据名称获取ScenePlayer
        public ScenePlayer GetScenePlayer(string id)
        {
            foreach(ScenePlayer p in list)
            {
                if (p.id == id)
                    return p;
            }
            return null;
        }

        //添加玩家
        public void AddPlayer(string id)
        {
            lock (list)
            {
                ScenePlayer p = new ScenePlayer(id);
                list.Add(p);
            }
        }

        //删除玩家
        public void DelPlayer(string id)
        {
            lock (list)
            {
                ScenePlayer p = GetScenePlayer(id);
                if (p != null)
                    list.Remove(p);
            }
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("PlayerLeave");
            protocol.AddString(id);
            ServNet.getInstance().Broadcast(protocol);
        }

        public void SendPlayerList(Player player)
        {
            int count = list.Count;
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetList");
            protocol.AddInt(count);
            for (int i = 0; i < count; i++)
            {
                ScenePlayer p = list[i];
                protocol.AddString(p.id);
                protocol.AddFloat(p.x);
                protocol.AddFloat(p.y);
                protocol.AddFloat(p.z);
                protocol.AddInt(p.score);
            }
            player.Send(protocol);
        }

        public void UpdateInfo(string id, float x, float y, float z, int score)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            ScenePlayer p = GetScenePlayer(id);
            if (p == null)
                return;
            p.x = x;
            p.y = y;
            p.z = z;
            p.score = score;
        }


    }
}

