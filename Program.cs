using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServNet serv = ServNet.getInstance();
            serv.Start("192.168.50.31",1234);
            Console.ReadLine();
        }
    }
}
