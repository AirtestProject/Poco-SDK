using TcpServer;

namespace Game.SDKs.PocoSDK
{
    public struct ResultStruct
    {
        public TcpClientState client;
        public object idAction;
        public object result;
        public PocoManager.SubThreadCallBack callBack;
    }
}