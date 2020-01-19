using System;
using System.Net.Sockets;

namespace TcpServer
{
    /// <summary>
    /// 与客户端的连接已断开事件参数
    /// </summary>
    public class TcpClientDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// 与客户端的连接已断开事件参数
        /// </summary>
        /// <param name="tcpClient">客户端状态</param>
        public TcpClientDisconnectedEventArgs(TcpClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            this.TcpClient = tcpClient;
        }

        /// <summary>
        /// 客户端
        /// </summary>
        public TcpClient TcpClient { get; private set; }
    }
}