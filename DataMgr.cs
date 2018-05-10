using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
//数据库相关
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

//序列化相关
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace Server
{
    class DataMgr
    {
        MySqlConnection sqlConn;
        public static DataMgr instance=new DataMgr();
        private DataMgr()
        {
            instance = this;
            Connect();
        }

        public void Connect()
        {
            string connStr = "Database=game;DataSource=127.0.0.1;";
            connStr += "User Id=root;Password=123456;port=3306";
            sqlConn = new MySqlConnection(connStr);
            try
            {
                sqlConn.Open();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

  

        //判断安全字符串
        public bool IsSafeStr(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\{|\}|%|@|\*|!|\']");
        }
        //是否存在该用户
        private bool CanRegister(string id)
        {
            //查询id是否存在
            string cmdStr = string.Format("Select*from VRUser where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return !hasRows;

            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]CanRegisterfail" + e.Message);
                return false;
            }
        }
        public bool Register(string id,string pw)
        {
            if (!IsSafeStr(id) || !IsSafeStr(pw))
            {
                Console.WriteLine("注册使用非法字符");
                return false;
            }
            if(!CanRegister(id))
            {
                Console.WriteLine("注册失败，已存在的用户");
                return false;
            }
            string cmdStr = string.Format("insert into VRUser set id='{0}',pw='{1}';", id,pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                cmd.ExecuteNonQuery();
                
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CanRegisterfail" + e.Message);
                return false;
            }
            return true;
        }

        public bool CreatePlayer(string id)
        {
            if (!IsSafeStr(id))
                return false;
            //序列化
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            PlayerData player = new PlayerData();
            try
            {
                formatter.Serialize(stream,player);
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer 序列化" + e.Message);
                return false;
            }
            byte[] streamArr = stream.ToArray();
            string cmdStr = string.Format("insert into player set id='{0}',data=@data;", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = streamArr;
            try
            {
                cmd.ExecuteNonQuery();
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer" + e.Message);
                return false;
            }
        }


        //登入校验
        public bool CheckPassWord(string id,string pw)
        {
            if (!IsSafeStr(id) || !IsSafeStr(pw))
            {
                return false;
            }
            string cmdStr = string.Format("Select*from VRUser where id='{0}'and pw='{1}';", id,pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return hasRows;

            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] CheckPassWord" + e.Message);
                return false;
            }
        }

        //获取角色数据
        public PlayerData GetPlayerData(string id)
        {
            PlayerData playerData = null;
            if (!IsSafeStr(id))
                return null;
            string cmdStr = string.Format("select * from player where id='{0}';", id);
            MySqlCommand mySqlCommand = new MySqlCommand(cmdStr,sqlConn);
            byte[] buff;
            try
            {
                MySqlDataReader dataReader = mySqlCommand.ExecuteReader();
                if (!dataReader.HasRows)
                {
                    dataReader.Close();
                    return playerData;
                }
                dataReader.Read();
                long len = dataReader.GetBytes(1, 0, null, 0, 0);
                buff = new byte[len];
                dataReader.GetBytes(1, 0, buff, 0, (int)len);
                dataReader.Close();

            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData 查询" + e.Message);
                return playerData;
            }

            //反序列化
            MemoryStream stream = new MemoryStream(buff);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                playerData = (PlayerData)formatter.Deserialize(stream);
                return playerData;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData 反序列化" + e.Message);
                return playerData;
            }
        }


        //保存角色
        public bool SavePlayer(Player player)
        {
            string id = player.id;
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            PlayerData playerData = player.data;
            try
            {
                formatter.Serialize(stream, playerData);
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 序列化" + e.Message);
                return false;
            }
            byte[] streamArr = stream.ToArray();
            string cmdStr = string.Format("update player set data=@data where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = streamArr;
            try
            {
                cmd.ExecuteNonQuery();
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer" + e.Message);
                return false;
            }
        }





    }
}
