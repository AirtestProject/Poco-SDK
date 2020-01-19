using System;
using System.Net.Sockets;

namespace TcpServer
{
    /// <summary>
    /// 接收到数据报文事件参数
    /// </summary>
    /// <typeparam name="T">报文类型</typeparam>
    public class TcpDatagramReceivedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// 接收到数据报文事件参数
        /// </summary>
        /// <param name="tcpClientState">客户端状态</param>
        /// <param name="datagram">报文</param>
        public TcpDatagramReceivedEventArgs(TcpClientState tcpClientState, T datagram)
        {
            this.Client = tcpClientState;
            this.TcpClient = tcpClientState.TcpClient;
            this.Datagram = datagram;
        }

        /// <summary>
        /// 客户端状态
        /// </summary>
        public TcpClientState Client { get; private set; }

        /// <summary>
        /// 客户端
        /// </summary>
        public TcpClient TcpClient { get; private set; }

        /// <summary>
        /// 报文
        /// </summary>
        public T Datagram { get; private set; }
    }
}