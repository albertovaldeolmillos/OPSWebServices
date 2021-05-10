using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Manages the lifetime of channels
	/// </summary>
	public class ChannelManager : IChannelManager
	{
		#region Public API
		/// <summary>
		/// Creates a channel to exchange data with the end-point represented by
		/// the supplied uniform resource identifier. If a channel to this end-point is already open,
		/// it returns the existing channel
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="type">The type of channel to open. From the ChannelType enumeration</param>
		/// <returns>The channel requested or null if an error occurs</returns>
		/// <remarks>In this version it only handles SocketChannel</remarks>
		public virtual IChannel OpenChannel(string uri, ChannelType type)
		{
			String ipAddr;
			int port;
			IChannel ch = null;
			lock (_channels.SyncRoot)
			{
				try
				{
					if (type.Equals(ChannelType.Socket))
					{
						if (!_channels.ContainsKey(uri))
						{
							Socket socket = new Socket(
								AddressFamily.InterNetwork,
								SocketType.Stream,
								ProtocolType.Tcp);
							TcpIpUri.UriAsAddressPort(uri, out ipAddr, out port);
							socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddr), port));
							if (socket.Connected)
							{
								ch = new SocketChannel(socket, true); /// TODO: Implement a generic mechanism to decouple ChannelManager from the type of channel to be created
								ch.Uri = uri;
								AddToChannels(uri, ch);
							}
						}
						else
						{
							ch = (IChannel) _channels[uri];
						}
					}
				}
				catch (SocketException /*sockEx*/)
				{
					//Debug.WriteLine("OpenChannel socket error - " + sockEx.Message);
					//CommMain.Logger.AddLog(sockEx); --> not necessary as we throw the exception
					throw;
				}
				catch (Exception /*ex*/)
				{
					//Debug.WriteLine("OpenChannel error - " + ex.Message);
					//CommMain.Logger.AddLog(ex); --> not necessary as we throw the exception
					throw;
				}
			}
			return ch; 
		}

		/// <summary>
		/// Creates a channel to exchange data with the end-point represented by
		/// the supplied uniform resource identifier.</summary>
		/// <param name="uri">The logical endpoint identifier</param>
		/// <returns>The channel requested or null if an error occurs</returns>
		/// <remarks>The channel type is the manager's default channel type</remarks>
		public virtual IChannel OpenChannel(string uri)
		{
			return OpenChannel(uri, ChannelType.Socket);
		}

		/// <summary>
		/// Returns the channel associated with the specified uri
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public IChannel GetChannel(string uri)
		{
			IChannel ch = null;
			lock (_channels.SyncRoot)
			{
				try
				{
					ch = (IChannel) _channels[uri];
				}
				catch (Exception ex)
				{
					//Debug.WriteLine(string.Format("ChannelManager.GetChannel({0}) error: {1}", ch.Uri, ex.Message));
					CommMain.Logger.AddLog(ex);
				}
			}
			return ch;
		}

		/// <summary>
		/// Gets rid of the channel associated with the specified uri
		/// </summary>
		/// <param name="uri"></param>
		public void CloseChannel(string uri)
		{
			try
			{
				IChannel ch = null;
				lock (_channels.SyncRoot)
				{
					ch = (IChannel) _channels[uri];
					_channels.Remove(uri);
				}
				if (ch != null)
				{
					ch.Close();
				}
			}
			catch (Exception ex)
			{
				//Debug.WriteLine(string.Format("ChannelManager.CloseChannel error: {0}", ex.Message));
				CommMain.Logger.AddLog(ex);
			}
		}

		/// <summary>
		/// Incorporates an already open channel and associates it with the specified uri
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="channel"></param>
		public virtual void AddChannel(string uri, IChannel channel)
		{
			lock (_channels.SyncRoot)
			{
				try
				{
					if (!_channels.ContainsKey(uri))
					{
						AddToChannels(uri, channel);
					}
				}
				catch (Exception ex)
				{
					//Debug.WriteLine(string.Format("ChannelManager.AddChannel error: {0}", ex.Message));
					CommMain.Logger.AddLog(ex);
				}
			}
		}

		/// <summary>
		/// Enumerates the channels
		/// </summary>
		/// <returns></returns>
		public IEnumerator EnumChannels()
		{
			return _channels.Values.GetEnumerator();
		}

		/// <summary>
		/// Closes all channels
		/// </summary>
		public void CloseAll()
		{
			lock (_channels.SyncRoot)
			{
				try 
				{
					IEnumerator enumChannels = EnumChannels();
					while (enumChannels.MoveNext())
					{
						IChannel ch = (IChannel) enumChannels.Current;
						Debug.WriteLine("ChannelManager closing " + ch.Uri);
						ch.Close();
					}
					_channels.Clear();
				}
				catch (Exception ex)
				{
					//Debug.WriteLine(string.Format("ChannelManager.CloseAll error: {0}", ex.Message));
					CommMain.Logger.AddLog(ex);
				}
			}
		}
		#endregion // Public API
		
		#region Protected methods
		protected virtual void AddToChannels(string uri, IChannel channel)
		{
			_channels.Add(uri, channel);
		}
		#endregion // Protected methods

		#region Private data members
		private Hashtable _channels = new Hashtable(5);
		#endregion // Private data members
	}
}
