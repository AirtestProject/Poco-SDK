using System;
using System.Net.Sockets;

namespace TcpServer
{
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
        /// <param name="prot">The protocol filter</param>
        public TcpClientState(TcpClient tcpClient, byte[] buffer, ProtoFilter prot)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (prot == null)
                throw new ArgumentNullException("prot");

            this.TcpClient = tcpClient;
            this.Buffer = buffer;
            this.Prot = prot;
            // this.NetworkStream = tcpClient.GetStream ();
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
        public NetworkStream NetworkStream
        {
            get
            {
                return TcpClient.GetStream();
            }
        }
    }
}