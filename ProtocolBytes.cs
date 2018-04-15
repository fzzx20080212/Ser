using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    //字节流协议
    class ProtocolBytes:ProtocolBase
    {
        public byte[] bytes;
        public override ProtocolBase Decode(byte[] readbuff, int start, int length)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.bytes = new byte[length];
            Array.Copy(readbuff, start, protocol.bytes, 0, length);
            return protocol;
        }

        public override byte[] Encode()
        {
            return bytes;
        }

        public override string GetName()
        {
            return GetString(0);
        }

        public override string GetDesc()
        {
            string str = "";
            if (bytes == null) return str;
            for(int i=0;i<bytes.Length;i++)
            {
                str += (int)bytes[i]+" ";
            }
            return str;
        }

        public void AddString(string str)
        {
            Int32 len = str.Length;
            byte[] lenbyte = BitConverter.GetBytes(len);
            byte[] strbyte = System.Text.Encoding.UTF8.GetBytes(str);
            if (bytes == null)
                bytes = lenbyte.Concat(strbyte).ToArray();
            else
                bytes = bytes.Concat(lenbyte).Concat(strbyte).ToArray();

        }

        //从字节数组的start处开始读取字符串
        public string GetString(int start, ref int end)
        {
            if (bytes == null)
                return "";
            if (bytes.Length < start + sizeof(Int32))
                return "";
            Int32 strlen = BitConverter.ToInt32(bytes, start);
            if (bytes.Length < start + sizeof(Int32) + strlen)
                return "";
            string str = System.Text.Encoding.UTF8.GetString(bytes, start + sizeof(Int32), strlen);
            end = start + sizeof(Int32) + strlen;
            return str;
        }

        public string GetString(int start)
        {
            int end = 0;
            return GetString(start, ref end);
        }

        public void AddInt(int num)
        {
            byte[] numbyte = BitConverter.GetBytes(num);
            if (bytes == null)
                bytes = numbyte;
            else
                bytes = bytes.Concat(numbyte).ToArray();
        }

        public int GetInt(int start,ref int end)
        {
            if (bytes == null)
                return 0;
            if (bytes.Length < sizeof(Int32) + start)
                return 0;
            end = start + sizeof(Int32);
            return BitConverter.ToInt32(bytes, start);
        }

        public int GetInt(int start)
        {
            int end = 0;
            return GetInt(start, ref end);
        }

        public void AddFloat(float num)
        {
            byte[] numbyte = BitConverter.GetBytes(num);
            if (bytes == null)
                bytes = numbyte;
            else
                bytes = bytes.Concat(numbyte).ToArray();
        }

        public float GetFloat(int start, ref int end)
        {
            if (bytes == null)
                return 0;
            if (bytes.Length < sizeof(float) + start)
                return 0;
            end = start + sizeof(float);
            return BitConverter.ToSingle(bytes, start);
        }

        public float GetFloat(int start)
        {
            int end = 0;
            return GetFloat(start, ref end);
        }


    }
}
