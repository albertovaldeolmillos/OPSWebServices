using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using OPS.Comm;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// The signature for handlers of the NewConnection Port event
	/// </summary>
	/// <param name="newConnection">The communication channel for the new 
	/// connection</param>
	public delegate void ConnectionHandler(IChannel newConnection);

	/// <summary>
	/// A well-known end point to establish possibly bidirectional communications
	/// </summary>
	public class Port
	{
		#region Public API
		/// <summary>
		/// Constructor that takes the URI where the port listens for connections
		/// </summary>
		/// <param name="uri">The uniform resource identifier where the port listens for 
		/// connections to arrive</param>
		/// <remarks>The current version only supports TCP/IP URIs and SocketChannel
		/// </remarks>
		public Port(String uri)
		{
			TcpIpUri.UriAsAddressPort(uri, out _ipAddress, out _port);
		}
		public Port(IPAddress addr, int port)
		{
			_addr = addr;
			_port = port;
		}
		/// <summary>
		/// The manager for the connections established through this port
		/// </summary>
		public IChannelManager ChannelMgr
		{
			get { return _channelMgr; }
			set { _channelMgr = value; }
		}
		/// <summary>
		/// The event that fires when a new connection arrives
		/// </summary>
		public event ConnectionHandler NewConnection
		{
			add { _connHandler += value; }
			remove { _connHandler -= value; } 
		}
		/// <summary>
		/// Begins listening for connections
		/// </summary>
		public void Open()
		{
			if (_srvSocket == null)
			{
				try
				{
					if (_ipAddress != null)
					{
						_srvSocket = new Socket(
							AddressFamily.InterNetwork,
							SocketType.Stream,
							ProtocolType.Tcp);
						_srvSocket.Bind(new IPEndPoint(IPAddress.Parse(_ipAddress), _port));
						_srvSocket.Listen(20);
						ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadConnectionProc));
					}
					else
					{
						if (_addr != null)
						{
							_srvSocket = new Socket(
								AddressFamily.InterNetwork,
								SocketType.Stream,
								ProtocolType.Tcp);
							_srvSocket.Bind(new IPEndPoint(_addr, _port));
							_srvSocket.Listen(20);
							ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadConnectionProc));
						}
					}
				}
				catch (SocketException sockEx)
				{
					//Debug.WriteLine("Port.Open socket error - " + sockEx.Message);
					CommMain.Logger.AddLog(sockEx);
				}
				catch (Exception ex)
				{
					//Debug.WriteLine("Port.Open - " + ex.Message);
					CommMain.Logger.AddLog(ex);
				}
			}
		}
		/// <summary>
		/// Stops listening for connections
		/// </summary>
		public void Close()
		{
			_closing = true;
			if (_srvSocket != null)
			{
				try
				{
					_srvSocket.Shutdown(SocketShutdown.Both);
				}
				catch (SocketException sockEx)
				{
					//Debug.WriteLine("Port.Close - shutting down : " + sockEx.Message);
					CommMain.Logger.AddLog(sockEx);
				}
				try
				{
					_srvSocket.Close();
				}
				catch (SocketException sockEx)
				{
					//Debug.WriteLine("Port.Close - closing : " + sockEx.Message);
					CommMain.Logger.AddLog(sockEx);
				}
				_srvSocket = null;
			}
		}
		/// <summary>
		/// Indicates whether the Port is being closed or not
		/// </summary>
		public bool Closing
		{
			get { return _closing; }
		}
		/// <summary>
		/// The local address to bind the port to
		/// </summary>
		public string LocalAddress
		{
			get { return _ipAddress; }
			set { _ipAddress = value; }
		}
		#endregion // Public API

		#region Private data members
		private void ThreadConnectionProc(object state)
		{
			while (true)
			{
				try
				{
					Socket s = _srvSocket.Accept();
					if (s != null)
					{
						IChannel ch = new SocketChannel(s, true); /// TODO: Implement a generic mechanism to decouple Port from the type of channel to be created
						if (_channelMgr != null)
						{
							IPEndPoint p = (IPEndPoint) s.RemoteEndPoint;
							String uri = TcpIpUri.AddressPortAsUri(p.Address.ToString(), p.Port);
							ch.Uri = uri;
							//>> LDRTEST
							// Check if there is an old version of the channel
							// This means that the closing of the socket was not detected
							if (_channelMgr.GetChannel(uri) != null)
							{
								_channelMgr.CloseChannel(uri);
								CommMain.Logger.AddLog("Port.ThreadConnectionProc: Forced to close channel: " + uri, LoggerSeverities.Error);
							}
							//<< LDRTEST
							_channelMgr.AddChannel(uri, ch);
						}
						if (_connHandler != null)
						{
							_connHandler(ch);
						}
						else
						{
							CommMain.Logger.AddLog("WARNING: Port.NotifySendError: ThreadConnectionProc is null.", LoggerSeverities.Error);
                        }
						//>> LDR 2004.07.16
						SocketChannel sch = (SocketChannel)ch;
						sch.Run();
						//<< LDR 2004.07.16
					}
				}
				catch (SocketException sockEx)
				{
					CommMain.Logger.AddLog(sockEx);
					if (Closing)
						break;
				}
				catch (Exception ex)
				{
					CommMain.Logger.AddLog(ex);
				}
			}
		}
		#endregion // Private members

		#region Private data members
		/// <summary>
		/// The server socket (connection requests arrive to this socket)
		/// </summary>
		private Socket _srvSocket;
		/// <summary>
		/// The channel factory
		/// </summary>
		private IChannelManager _channelMgr;
		/// <summary>
		/// The local endpoint address
		/// </summary>
		private String _ipAddress;
		/// <summary>
		/// The local endpoint port number
		/// </summary>
		private int _port;
		/// <summary>
		/// The objects handling new connections
		/// </summary>
		private ConnectionHandler _connHandler;
		/// <summary>
		/// Indicates that the close operation has been started
		/// </summary>
		private bool _closing = false;
		private IPAddress _addr;
		#endregion //Private data members
	}
}
