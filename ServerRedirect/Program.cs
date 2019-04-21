using huqiang;
using Redirection.Data;
using System;
using System.Net;

namespace ServerRedirect
{
    public class Req
    {
        public const Int32 Cmd = 0;
        public const Int32 Type = 1;
        public const Int32 Error = 2;
        public const Int32 Args = 3;
        public const Int32 Length = 4;
    }
    public class MessageType
    {
        public const Int32 Pro = -1;
        public const Int32 Def = 0;
        public const Int32 Rpc = 1;
        public const Int32 Query = 2;
    }
    public class ProCmd
    {
        public const Int32 Guid = -2;
        public const Int32 Server = -1;//设置服务器ip
        public const Int32 ServerIp = 0;//获取服务器ip
        public const Int32 AllServer = 1;//获取所有服务器
    }
    [Serializable]
    public class RServer
    {
        public string key;
        public string name;
    }
    class Program
    {
       
        static void Main(string[] args)
        {
            KcpServer.CreateLink = (o) => { return new KcpUser(o); };
            var kcp = new KcpServer(8888);
            kcp.OpenHeart();
            RServer rs = new RServer();
            rs.name = "斗地主";
            rs.key = "ert125dsaqwqf43bvrwurx24354tq245sd32dfkh348shdjfhs234sf5345";
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("193.112.70.170"),6666);
            kcp.Send(KcpPack.PackObject<RServer>(ProCmd.Server,MessageType.Pro,rs),EnvelopeType.AesDataBuffer, endPoint);
            while(true)
            {
                var cmd = Console.ReadLine();
                if (cmd == "close" | cmd == "Close")
                    break;
            }
        }
    }
}
