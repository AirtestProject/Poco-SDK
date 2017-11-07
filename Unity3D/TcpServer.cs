using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System;
using UnityEngine;

  // for debug only

namespace TcpServer
{
	public interface ProtoFilter
	{
		void input (byte[] data);

		List<string> swap_msgs ();
	}

	public class SimpleProtocolFilter: ProtoFilter
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
		private List<string> msgs = new List<string> ();

		public void input (byte[] data)
		{
			buf = Combine (buf, data);

			while (buf.Length > HEADER_SIZE) {
				int data_size = BitConverter.ToInt32 (buf, 0);
				if (buf.Length >= data_size + HEADER_SIZE) {
					byte[] data_body = Slice (buf, HEADER_SIZE, buf.Length);
					string content = System.Text.Encoding.Default.GetString (data_body);
					Debug.Log ("got content:" + content);
					msgs.Add (content);
					buf = Slice (buf, data_size + HEADER_SIZE, buf.Length);
				} else {
					break;
				}
			}
		}

		public List<string> swap_msgs ()
		{
			List<string> ret = msgs;
			msgs = new List<string> ();
			return ret;
		}

		public byte[] pack (String content)
		{
			int len = content.Length;
			byte[] size = BitConverter.GetBytes (len);
			if (!BitConverter.IsLittleEndian) {
				//reverse it so we get little endian.
				Array.Reverse (size); 
			}
			byte[] body = System.Text.Encoding.Default.GetBytes (content);
			byte[] ret = Combine (size, body);
			Console.WriteLine (BitConverter.ToString (ret));
			return ret;
		}

		private static byte[] Combine (byte[] first, byte[] second)
		{
			byte[] ret = new byte[first.Length + second.Length];
			Buffer.BlockCopy (first, 0, ret, 0, first.Length);
			Buffer.BlockCopy (second, 0, ret, first.Length, second.Length);
			return ret;
			// return first.Concat(second);
		}

		public byte[] Slice (byte[] source, int start, int end)
		{
			int length = end - start;
			byte[] ret = new byte[length];
			Array.Copy (source, start, ret, 0, length);
			return ret;
			// return source.Skip(start).Take(end - start);
		}
	}

	/// <summary>
	/// Internal class to join the TCP client and buffer together
	/// for easy management in the server
	/// </summary>
	public class TcpClientState
	{
		/// <summary>
		/// Constructor for a new Client
		/// </summary>
		/// <param name="tcpClient">The TCP client</param>
		/// <param name="buffer">The byte array buffer</param> 

		public TcpClientState (TcpClient tcpClient, byte[] buffer, ProtoFilter prot)
		{
			if (tcpClient == null)
				throw new ArgumentNullException ("tcpClient");
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			this.TcpClient = tcpClient;
			this.Buffer = buffer;
			this.Prot = prot;
		}

		/// <summary>
		/// Gets the TCP Client
		/// </summary>
		public TcpClient TcpClient { get; private set; }

		/// <summary>
		/// Gets the Buffer.
		/// </summary>
		public byte[] Buffer { get; private set; }

		public ProtoFilter Prot { get; private set; }

		/// <summary>
		/// Gets the network stream
		/// </summary>
		public NetworkStream NetworkStream {
			get { return TcpClient.GetStream (); }
		}
	}

	/// <summary>
	/// 接收到数据报文事件参数
	/// </summary>
	/// <typeparam name="T">报文类型</typeparam>
	public class TcpDatagramReceivedEventArgs<T> : EventArgs
	{
		/// <summary>
		/// 接收到数据报文事件参数
		/// </summary>
		/// <param name="tcpClient">客户端</param>
		/// <param name="datagram">报文</param>
		//		public TcpDatagramReceivedEventArgs(TcpClient tcpClient, T datagram)
		public TcpDatagramReceivedEventArgs (TcpClientState client, T datagram)
		{
			Client = client;
			TcpClient = client.TcpClient;
			Datagram = datagram;
		}

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

	/// <summary>
	/// 与客户端的连接已建立事件参数
	/// </summary>
	public class TcpClientConnectedEventArgs : EventArgs
	{
		/// <summary>
		/// 与客户端的连接已建立事件参数
		/// </summary>
		/// <param name="tcpClient">客户端</param>
		public TcpClientConnectedEventArgs (TcpClient tcpClient)
		{
			if (tcpClient == null)
				throw new ArgumentNullException ("tcpClient");

			this.TcpClient = tcpClient;
		}

		/// <summary>
		/// 客户端
		/// </summary>
		public TcpClient TcpClient { get; private set; }
	}

	/// <summary>
	/// 与客户端的连接已断开事件参数
	/// </summary>
	public class TcpClientDisconnectedEventArgs : EventArgs
	{
		/// <summary>
		/// 与客户端的连接已断开事件参数
		/// </summary>
		/// <param name="tcpClient">客户端</param>
		public TcpClientDisconnectedEventArgs (TcpClient tcpClient)
		{
			if (tcpClient == null)
				throw new ArgumentNullException ("tcpClient");

			this.TcpClient = tcpClient;
		}

		/// <summary>
		/// 客户端
		/// </summary>
		public TcpClient TcpClient { get; private set; }
	}

	/// <summary>
	/// 异步TCP服务器
	/// </summary>
	public class AsyncTcpServer : IDisposable
	{
		#region Fields

		private TcpListener listener;
		private List<TcpClientState> clients;
		private bool disposed = false;

		#endregion

		#region Ctors

		/// <summary>
		/// 异步TCP服务器
		/// </summary>
		/// <param name="listenPort">监听的端口</param>
		public AsyncTcpServer (int listenPort)
			: this (IPAddress.Any, listenPort)
		{
		}

		/// <summary>
		/// 异步TCP服务器
		/// </summary>
		/// <param name="localEP">监听的终结点</param>
		public AsyncTcpServer (IPEndPoint localEP)
			: this (localEP.Address, localEP.Port)
		{
		}

		/// <summary>
		/// 异步TCP服务器
		/// </summary>
		/// <param name="localIPAddress">监听的IP地址</param>
		/// <param name="listenPort">监听的端口</param>
		public AsyncTcpServer (IPAddress localIPAddress, int listenPort)
		{
			Address = localIPAddress;
			Port = listenPort;
			this.Encoding = Encoding.Default;

			clients = new List<TcpClientState> ();

			listener = new TcpListener (Address, Port);
			//		listener.AllowNatTraversal(true);
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
		public AsyncTcpServer Start ()
		{
			if (!IsRunning) {
				IsRunning = true;
				listener.Start ();
				listener.BeginAcceptTcpClient (
					new AsyncCallback (HandleTcpClientAccepted), listener);
			}
			return this;
		}

		/// <summary>
		/// 启动服务器
		/// </summary>
		/// <param name="backlog">
		/// 服务器所允许的挂起连接序列的最大长度
		/// </param>
		/// <returns>异步TCP服务器</returns>
		public AsyncTcpServer Start (int backlog)
		{
			if (!IsRunning) {
				IsRunning = true;
				listener.Start (backlog);
				listener.BeginAcceptTcpClient (
					new AsyncCallback (HandleTcpClientAccepted), listener);
			}
			return this;
		}

		/// <summary>
		/// 停止服务器
		/// </summary>
		/// <returns>异步TCP服务器</returns>
		public AsyncTcpServer Stop ()
		{
			if (IsRunning) {
				IsRunning = false;
				listener.Stop ();

				lock (this.clients) {
					for (int i = 0; i < this.clients.Count; i++) {
						this.clients [i].TcpClient.Client.Disconnect (false);
					}
					this.clients.Clear ();
				}

			}
			return this;
		}

		#endregion

		#region Receive

		private void HandleTcpClientAccepted (IAsyncResult ar)
		{
			if (IsRunning) {
				TcpListener tcpListener = (TcpListener)ar.AsyncState;

				TcpClient tcpClient = tcpListener.EndAcceptTcpClient (ar);
				byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

				SimpleProtocolFilter prot = new SimpleProtocolFilter ();
				TcpClientState internalClient = new TcpClientState (tcpClient, buffer, prot);
				lock (this.clients) {
					this.clients.Add (internalClient);
					RaiseClientConnected (tcpClient);
				}

				NetworkStream networkStream = internalClient.NetworkStream;
				networkStream.BeginRead (
					internalClient.Buffer, 
					0, 
					internalClient.Buffer.Length, 
					HandleDatagramReceived, 
					internalClient);

				tcpListener.BeginAcceptTcpClient (
					new AsyncCallback (HandleTcpClientAccepted), ar.AsyncState);
			}
		}

		private void HandleDatagramReceived (IAsyncResult ar)
		{
			if (IsRunning) {
				TcpClientState internalClient = (TcpClientState)ar.AsyncState;
				NetworkStream networkStream = internalClient.NetworkStream;

				int numberOfReadBytes = 0;
				try {
					numberOfReadBytes = networkStream.EndRead (ar);
				} catch {
					numberOfReadBytes = 0;
				}

				if (numberOfReadBytes == 0) {
					// connection has been closed
					lock (this.clients) {
						this.clients.Remove (internalClient);
						RaiseClientDisconnected (internalClient.TcpClient);
						return;
					}
				}

				// received byte 
				byte[] receivedBytes = new byte[numberOfReadBytes];
				Buffer.BlockCopy (
					internalClient.Buffer, 0, 
					receivedBytes, 0, numberOfReadBytes);
				// input bytes into protofilter
				internalClient.Prot.input (receivedBytes);
				// trigger event notification
//				RaiseDatagramReceived(internalClient.TcpClient, receivedBytes);
//				RaisePlaintextReceived(internalClient.TcpClient, receivedBytes);
				RaiseDatagramReceived (internalClient, receivedBytes);

				// continue listening for tcp datagram packets
				networkStream.BeginRead (
					internalClient.Buffer, 
					0, 
					internalClient.Buffer.Length, 
					HandleDatagramReceived, 
					internalClient);
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
		//		public event EventHandler<TcpDatagramReceivedEventArgs<string>> PlaintextReceived;

		//		private void RaiseDatagramReceived(TcpClient sender, byte[] datagram)
		private void RaiseDatagramReceived (TcpClientState sender, byte[] datagram)
		{
			if (DatagramReceived != null) {
				DatagramReceived (this, new TcpDatagramReceivedEventArgs<byte[]> (sender, datagram));
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

		private void RaiseClientConnected (TcpClient tcpClient)
		{
			if (ClientConnected != null) {
				ClientConnected (this, new TcpClientConnectedEventArgs (tcpClient));
			}
		}

		private void RaiseClientDisconnected (TcpClient tcpClient)
		{
			if (ClientDisconnected != null) {
				ClientDisconnected (this, new TcpClientDisconnectedEventArgs (tcpClient));
			}
		}

		#endregion

		#region Send

		/// <summary>
		/// 发送报文至指定的客户端
		/// </summary>
		/// <param name="tcpClient">客户端</param>
		/// <param name="datagram">报文</param>
		public void Send (TcpClient tcpClient, byte[] datagram)
		{
			if (!IsRunning)
				throw new InvalidProgramException ("This TCP server has not been started.");

			if (tcpClient == null)
				throw new ArgumentNullException ("tcpClient");

			if (datagram == null)
				throw new ArgumentNullException ("datagram");

			tcpClient.GetStream ().BeginWrite (
				datagram, 0, datagram.Length, HandleDatagramWritten, tcpClient);
		}

		private void HandleDatagramWritten (IAsyncResult ar)
		{
			((TcpClient)ar.AsyncState).GetStream ().EndWrite (ar);
		}

		/// <summary>
		/// 发送报文至指定的客户端
		/// </summary>
		/// <param name="tcpClient">客户端</param>
		/// <param name="datagram">报文</param>
		public void Send (TcpClient tcpClient, string datagram)
		{
			Send (tcpClient, this.Encoding.GetBytes (datagram));
		}

		/// <summary>
		/// 发送报文至所有客户端
		/// </summary>
		/// <param name="datagram">报文</param>
		public void SendAll (byte[] datagram)
		{
			if (!IsRunning)
				throw new InvalidProgramException ("This TCP server has not been started.");

			for (int i = 0; i < this.clients.Count; i++) {
				Send (this.clients [i].TcpClient, datagram);
			}
		}

		/// <summary>
		/// 发送报文至所有客户端
		/// </summary>
		/// <param name="datagram">报文</param>
		public void SendAll (string datagram)
		{
			if (!IsRunning)
				throw new InvalidProgramException ("This TCP server has not been started.");

			SendAll (this.Encoding.GetBytes (datagram));
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, 
		/// releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release 
		/// both managed and unmanaged resources; <c>false</c> 
		/// to release only unmanaged resources.</param>
		protected virtual void Dispose (bool disposing)
		{
			if (!this.disposed) {
				if (disposing) {
					try {
						Stop ();

						if (listener != null) {
							listener = null;
						}
					} catch (SocketException ex) {
						Console.Write (ex);
					}
				}

				disposed = true;
			}
		}

		#endregion
	}
}