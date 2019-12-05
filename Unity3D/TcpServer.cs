using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace TcpServer
{
    public interface ProtoFilter
    {
        void input(byte[] data);

        List<string> swap_msgs();
    }

    public class SimpleProtocolFilter : ProtoFilter
    {
        /* 简单协议过滤器
		协议按照 [有效数据字节数][有效数据] 这种协议包的格式进行打包和解包
		[有效数据字节数]长度HEADER_SIZE字节
		[有效数据]长度有效数据字节数字节
		本类按照这种方式，顺序从数据流中取出数据进行拼接，一旦接收完一个完整的协议包，就会将协议包返回
		[有效数据]字段接收到后会按照utf-8进行解码，因为在传输过程中是用utf-8进行编码的
		所有编解码的操作在该类中完成
		*/

        private byte[] buf = new byte[0];
        private int HEADER_SIZE = 4;
        private List<string> msgs = new List<string>();

        public void input(byte[] data)
        {
            buf = Combine(buf, data);

            while (buf.Length > HEADER_SIZE)
            {
                int data_size = BitConverter.ToInt32(buf, 0);
                if (buf.Length >= data_size + HEADER_SIZE)
                {
                    byte[] data_body = Slice(buf, HEADER_SIZE, data_size + HEADER_SIZE);
                    string content = System.Text.Encoding.Default.GetString(data_body);
                    msgs.Add(content);
                    buf = Slice(buf, data_size + HEADER_SIZE, buf.Length);
                }
                else
                {
                    break;
                }
            }
        }

        public List<string> swap_msgs()
        {
            List<string> ret = msgs;
            msgs = new List<string>();
            return ret;
        }

        public byte[] pack(String content)
        {
            int len = content.Length;
            byte[] size = BitConverter.GetBytes(len);
            if (!BitConverter.IsLittleEndian)
            {
                //reverse it so we get little endian.
                Array.Reverse(size);
            }
            byte[] body = System.Text.Encoding.Default.GetBytes(content);
            byte[] ret = Combine(size, body);
            return ret;
        }

        private static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public byte[] Slice(byte[] source, int start, int end)
        {
            int length = end - start;
            byte[] ret = new byte[length];
            Array.Copy(source, start, ret, 0, length);
            return ret;
        }
    }


    /// <summary>
    /// 异步TCP服务器
    /// </summary>
    public class AsyncTcpServer : IDisposable
    {
        #region Fields

        private TcpListener _listener;
        private ConcurrentDictionary<string, TcpClientState> _clients;
        private bool _disposed = false;

        #endregion

        #region Ctors

        /// <summary>
        /// 异步TCP服务器
        /// </summary>
        /// <param name="listenPort">监听的端口</param>
        public AsyncTcpServer(int listenPort)
            : this(IPAddress.Any, listenPort)
        {
        }

        /// <summary>
        /// 异步TCP服务器
        /// </summary>
        /// <param name="localEP">监听的终结点</param>
        public AsyncTcpServer(IPEndPoint localEP)
            : this(localEP.Address, localEP.Port)
        {
        }

        /// <summary>
        /// 异步TCP服务器
        /// </summary>
        /// <param name="localIPAddress">监听的IP地址</param>
        /// <param name="listenPort">监听的端口</param>
        public AsyncTcpServer(IPAddress localIPAddress, int listenPort)
        {
            this.Address = localIPAddress;
            this.Port = listenPort;
            this.Encoding = Encoding.Default;

            _clients = new ConcurrentDictionary<string, TcpClientState>();

            _listener = new TcpListener(Address, Port);
            // _listener.AllowNatTraversal(true);
        }

        #endregion

        #region Properties

        /// <summary>
        /// 服务器是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 监听的IP地址
        /// </summary>
        public IPAddress Address { get; private set; }

        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 通信使用的编码
        /// </summary>
        public Encoding Encoding { get; set; }

        #endregion

        #region Server

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <returns>异步TCP服务器</returns>
        public AsyncTcpServer Start()
        {
            Debug.Log("start server");
            return Start(10);
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="backlog">服务器所允许的挂起连接序列的最大长度</param>
        /// <returns>异步TCP服务器</returns>
        public AsyncTcpServer Start(int backlog)
        {
            if (IsRunning)
                return this;

            IsRunning = true;

            _listener.Start(backlog);
            ContinueAcceptTcpClient(_listener);

            return this;
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        /// <returns>异步TCP服务器</returns>
        public AsyncTcpServer Stop()
        {
            if (!IsRunning)
                return this;

            try
            {
                _listener.Stop();

                foreach (var client in _clients.Values)
                {
                    client.TcpClient.Client.Disconnect(false);
                }
                _clients.Clear();
            }
            catch (ObjectDisposedException ex)
            {
                Debug.LogException(ex);
            }
            catch (SocketException ex)
            {
                Debug.LogException(ex);
            }

            IsRunning = false;

            return this;
        }

        private void ContinueAcceptTcpClient(TcpListener tcpListener)
        {
            try
            {
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(HandleTcpClientAccepted), tcpListener);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.LogException(ex);
            }
            catch (SocketException ex)
            {
                Debug.LogException(ex);
            }
        }

        #endregion

        #region Receive

        private void HandleTcpClientAccepted(IAsyncResult ar)
        {
            if (!IsRunning)
                return;

            TcpListener tcpListener = (TcpListener)ar.AsyncState;

            TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
            if (!tcpClient.Connected)
                return;

            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            SimpleProtocolFilter prot = new SimpleProtocolFilter();
            TcpClientState internalClient = new TcpClientState(tcpClient, buffer, prot);

            // add client connection to cache
            string tcpClientKey = internalClient.TcpClient.Client.RemoteEndPoint.ToString();
            _clients.AddOrUpdate(tcpClientKey, internalClient, (n, o) =>
            {
                return internalClient;
            });
            RaiseClientConnected(tcpClient);

            // begin to read data
            NetworkStream networkStream = internalClient.NetworkStream;
            ContinueReadBuffer(internalClient, networkStream);

            // keep listening to accept next connection
            ContinueAcceptTcpClient(tcpListener);
        }

        private void HandleDatagramReceived(IAsyncResult ar)
        {
            if (!IsRunning)
                return;

            try
            {
                TcpClientState internalClient = (TcpClientState)ar.AsyncState;
                if (!internalClient.TcpClient.Connected)
                    return;

                NetworkStream networkStream = internalClient.NetworkStream;

                int numberOfReadBytes = 0;
                try
                {
                    // if the remote host has shutdown its connection, 
                    // read will immediately return with zero bytes.
                    numberOfReadBytes = networkStream.EndRead(ar);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    numberOfReadBytes = 0;
                }

                if (numberOfReadBytes == 0)
                {
                    // connection has been closed
                    TcpClientState internalClientToBeThrowAway;
                    string tcpClientKey = internalClient.TcpClient.Client.RemoteEndPoint.ToString();
                    _clients.TryRemove(tcpClientKey, out internalClientToBeThrowAway);
                    RaiseClientDisconnected(internalClient.TcpClient);
                    return;
                }

                // received byte and trigger event notification
                var receivedBytes = new byte[numberOfReadBytes];
                Array.Copy(internalClient.Buffer, 0, receivedBytes, 0, numberOfReadBytes);
                // input bytes into protofilter
                internalClient.Prot.input(receivedBytes);
                RaiseDatagramReceived(internalClient, receivedBytes);
                // RaisePlaintextReceived(internalClient.TcpClient, receivedBytes);

                // continue listening for tcp datagram packets
                ContinueReadBuffer(internalClient, networkStream);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogException(ex);
            }
        }

        private void ContinueReadBuffer(TcpClientState internalClient, NetworkStream networkStream)
        {
            try
            {
                networkStream.BeginRead(internalClient.Buffer, 0, internalClient.Buffer.Length, HandleDatagramReceived, internalClient);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.LogException(ex);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// 接收到数据报文事件
        /// </summary>
        public event EventHandler<TcpDatagramReceivedEventArgs<byte[]>> DatagramReceived;
        /// <summary>
        /// 接收到数据报文明文事件
        /// </summary>
        public event EventHandler<TcpDatagramReceivedEventArgs<string>> PlaintextReceived;

        private void RaiseDatagramReceived(TcpClientState sender, byte[] datagram)
        {
            if (DatagramReceived != null)
            {
                DatagramReceived(this, new TcpDatagramReceivedEventArgs<byte[]>(sender, datagram));
            }
        }

        private void RaisePlaintextReceived(TcpClientState sender, byte[] datagram)
        {
            if (PlaintextReceived != null)
            {
                PlaintextReceived(this, new TcpDatagramReceivedEventArgs<string>(sender, this.Encoding.GetString(datagram, 0, datagram.Length)));
            }
        }

        /// <summary>
        /// 与客户端的连接已建立事件
        /// </summary>
        public event EventHandler<TcpClientConnectedEventArgs> ClientConnected;
        /// <summary>
        /// 与客户端的连接已断开事件
        /// </summary>
        public event EventHandler<TcpClientDisconnectedEventArgs> ClientDisconnected;

        private void RaiseClientConnected(TcpClient tcpClient)
        {
            if (ClientConnected != null)
            {
                ClientConnected(this, new TcpClientConnectedEventArgs(tcpClient));
            }
        }

        private void RaiseClientDisconnected(TcpClient tcpClient)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, new TcpClientDisconnectedEventArgs(tcpClient));
            }
        }

        #endregion

        #region Send

        private void GuardRunning()
        {
            if (!IsRunning)
                throw new InvalidProgramException("This TCP server has not been started yet.");
        }

        /// <summary>
        /// 发送报文至指定的客户端
        /// </summary>
        /// <param name="tcpClient">客户端</param>
        /// <param name="datagram">报文</param>
        public void Send(TcpClient tcpClient, byte[] datagram)
        {
            GuardRunning();

            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            if (datagram == null)
                throw new ArgumentNullException("datagram");

            try
            {
                NetworkStream stream = tcpClient.GetStream();
                if (stream.CanWrite)
                {
                    stream.BeginWrite(datagram, 0, datagram.Length, HandleDatagramWritten, tcpClient);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// 发送报文至指定的客户端
        /// </summary>
        /// <param name="tcpClient">客户端</param>
        /// <param name="datagram">报文</param>
        public void Send(TcpClient tcpClient, string datagram)
        {
            Send(tcpClient, this.Encoding.GetBytes(datagram));
        }

        /// <summary>
        /// 发送报文至所有客户端
        /// </summary>
        /// <param name="datagram">报文</param>
        public void SendToAll(byte[] datagram)
        {
            GuardRunning();

            foreach (var client in _clients.Values)
            {
                Send(client.TcpClient, datagram);
            }
        }

        /// <summary>
        /// 发送报文至所有客户端
        /// </summary>
        /// <param name="datagram">报文</param>
        public void SendToAll(string datagram)
        {
            GuardRunning();

            SendToAll(this.Encoding.GetBytes(datagram));
        }

        private void HandleDatagramWritten(IAsyncResult ar)
        {
            try
            {
                ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.LogException(ex);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogException(ex);
            }
            catch (IOException ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// 发送报文至指定的客户端
        /// </summary>
        /// <param name="tcpClient">客户端</param>
        /// <param name="datagram">报文</param>
        public void SyncSend(TcpClient tcpClient, byte[] datagram)
        {
            GuardRunning();

            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            if (datagram == null)
                throw new ArgumentNullException("datagram");

            try
            {
                NetworkStream stream = tcpClient.GetStream();
                if (stream.CanWrite)
                {
                    stream.Write(datagram, 0, datagram.Length);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// 发送报文至指定的客户端
        /// </summary>
        /// <param name="tcpClient">客户端</param>
        /// <param name="datagram">报文</param>
        public void SyncSend(TcpClient tcpClient, string datagram)
        {
            SyncSend(tcpClient, this.Encoding.GetBytes(datagram));
        }

        /// <summary>
        /// 发送报文至所有客户端
        /// </summary>
        /// <param name="datagram">报文</param>
        public void SyncSendToAll(byte[] datagram)
        {
            GuardRunning();

            foreach (var client in _clients.Values)
            {
                SyncSend(client.TcpClient, datagram);
            }
        }

        /// <summary>
        /// 发送报文至所有客户端
        /// </summary>
        /// <param name="datagram">报文</param>
        public void SyncSendToAll(string datagram)
        {
            GuardRunning();

            SyncSendToAll(this.Encoding.GetBytes(datagram));
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Stop();

                        if (_listener != null)
                        {
                            _listener = null;
                        }
                    }
                    catch (SocketException ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                _disposed = true;
            }
        }

        #endregion
    }
}