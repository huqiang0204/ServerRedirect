using huqiang;
using huqiang.Data;
using MessagePack.LZ4;
using Redirection.Data;
using System;
using System.Net;
using System.Text;

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
    public const Int32 NewLink = 2;
}
public class ServerDataControll
{
    public static void Dispatch(KcpUser linker, byte[] dat, byte tag)
    {
        switch (tag)
        {
            case EnvelopeType.AesDataBuffer:
                if (dat.Length > 2)
                {
                    try
                    {
                        dat = AES.Instance.Decrypt(dat);
                        int len = dat.ReadInt32(0);
                        int all = dat.ReadInt32(4);
                        byte[] buf = new byte[all];
                        int o = LZ4Codec.Decode32Unsafe(dat, 8, len, buf, 0, all);
                        dat = new byte[o];
                        for (int i = 0; i < o; i++)
                            dat[i] = buf[i];
                        DispatchDataBuffer(linker, dat);
                    }
                    catch
                    {
                    }
                }
                break;
            case EnvelopeType.DataBuffer:
                DispatchDataBuffer(linker, dat);
                break;
        }
    }
    static void DispatchDataBuffer(KcpUser linker, byte[] dat)
    {
        var buffer = new DataBuffer(dat);
        var fake = buffer.fakeStruct;
        if (fake != null)
        {
            switch (fake[Req.Type])
            {
                case MessageType.Pro:
                    ProData(linker,buffer);
                    break;
            }
        }
    }
    static void ProData(KcpUser linker,DataBuffer data)
    {
        switch(data.fakeStruct[Req.Cmd])
        {
            case ProCmd.Server:
                RedirectServer(linker,data);
                break;
            case ProCmd.ServerIp:
                GetServerIp(linker,data);
                break;
            case ProCmd.AllServer:
                GetAllServerIp(linker,data);
                break;
            case ProCmd.NewLink:
                NewLink(linker, data);
                break;
        }
    }
    static readonly string key = "ert125dsaqwqf43bvrwurx24354tq245sd32dfkh348shdjfhs234sf5345";
    static void RedirectServer(KcpUser linker,DataBuffer data)
    {
        try
        {
            byte[] dat = data.fakeStruct.GetData<byte[]>(Req.Args);
            var rs = KcpPack.UnPackMsg<RServer>(dat);
            if(rs!=null)
            {
                if(rs.key==key)
                {
                    ServerTable.AddServer(linker.ip, linker.port, rs.name);
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
        }
    }
    static void GetServerIp(KcpUser linker, DataBuffer data)
    {
        linker.SendObject<ServerInfo>(ProCmd.ServerIp, MessageType.Pro,ServerTable.GetServer(0));
    }
    static void GetAllServerIp(KcpUser linker, DataBuffer data)
    {
        //int index = data.fakeStruct[Req.Args];
        //var info = ServerTable.GetAllServer();
        //int c = info.Count;
        //if(c>0)
        //{
        //    DataBuffer db = new DataBuffer();
        //    FakeStruct fake = new FakeStruct(db, Req.Length);
        //    fake[Req.Cmd] = ProCmd.AllServer;
        //    fake[Req.Type] = MessageType.Pro;
        //    FakeStructArray array = new FakeStructArray(db, 3, c);
        //    for(int i=0;i<c;i++)
        //    {
        //        array[i, 0] = info[i].ip;
        //        array[i, 1] = info[i].port;
        //        array.SetData(i, 2, info[i].name);
        //    }
        //    fake.SetData(Req.Args, array);
        //    db.fakeStruct = fake;
        //    linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);
        //}
    }
    static void NewLink(KcpUser linker, DataBuffer data)
    {
        byte[] dat = data.fakeStruct.GetData<byte[]>(Req.Args);
        if(dat!=null)
        {
            var link = KcpPack.UnPackMsg<LinkInfo>(dat);
            if(link!=null)
            {
                new IPAddress(link.ip.ToBytes());
                IPEndPoint endPoint = new IPEndPoint(new IPAddress(link.ip.ToBytes()), link.port);
                KcpServer.Instance.Send(new byte[1],EnvelopeType.Mate,endPoint);
            }
        }
    }
}
[Serializable]
public class LinkInfo
{
    public int ip;
    public int port;
}