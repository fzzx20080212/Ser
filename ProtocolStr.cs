using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    //字符串协议
    class ProtocolStr:ProtocolBase
    {
        //传输的字符串
        public string str;
        public override ProtocolBase Decode(byte[] readbuff, int start, int length)
        {
            ProtocolStr protocol = new ProtocolStr();
            protocol.str = System.Text.Encoding.UTF8.GetString(readbuff, start, length);
            return protocol;
        }

        public override byte[] Encode()
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        public override string GetName()
        {
            if (str.Length == 0) return "";
            return str.Split(',')[0];
        }

        public override string GetDesc()
        {
            return str;
        }
    }
}
