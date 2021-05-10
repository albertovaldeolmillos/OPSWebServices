using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using OPS.Comm;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// A implementation of the IChannel interface that uses sockets as
	/// the communication device
	/// </summary>
	public class SocketChannel : IChannel
	{
		#region Public API
		/// <summary>
		/// The known socket errors
		/// </summary>
		public enum SocketErrors : int
		{ 
			RemoteClosed = 1, SendingError, ReceivingError 
		};

		/// <summary>
		/// Constructs a Socket channel based on the supplied socket
		/// </summary>
		/// <param name="socket">The socket that supports the communication
		/// through this channel</param>
		/// <param name="ownSocket">If true, the channel owns the socket and therefore
		/// it is closed when the channel is closed</param>
		public SocketChannel(Socket socket, bool ownSocket)
		{
			_socket = socket;
			LingerOption opt = new LingerOption(false, 0);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, opt);
			_ownedSocket = ownSocket;
/// JLB 2004.07.23 - Exclude thread availability from log
/// 
//			//>> LDRTEST2
//			int workerThreads = 0;
//			int completionPortThreads = 0;
//			ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
//			CommMain.Logger.AddLog("SocketChannel constructor: Available threads = " + 
//				workerThreads + " : " + completionPortThreads, LogLevels.Verbose);
//			//<< LDRTEST2
			//>> LDR 2004.07.16
			//ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadSendProc), this);
			//ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadReceiveProc), this);
			//<< LDR 2004.07.16
		}

		//>> LDR 2004.07.16
		public void Run()
		{
			if (!ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadSendProc), this))
				CommMain.Logger.AddLog("SocketChannel - Couldn't start SendProc",LoggerSeverities.Error);

			if (!ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadReceiveProc), this))
				CommMain.Logger.AddLog("SocketChannel - Couldn't start ReceiveProc", LoggerSeverities.Error);
		}
		//<< LDR 2004.07.16

		/// <summary>
		/// Sends a message to the remote end point
		/// </summary>
		/// <param name="msg">The message to send</param>
		public void SendMessage(OPSTelegrama msg)
		{
			Monitor.Enter(_outQueue);
			try
			{
				_outQueue.Enqueue(msg);
				_sendEvent.Set();
				NotifySend(msg);
			}
			catch (Exception ex)
			{
				//Debug.WriteLine(ex.Message);
				CommMain.Logger.AddLog(ex);
				throw;
			}
			finally
			{
				Monitor.Exit(_outQueue);
			}
		}
		/// <summary>
		/// Releases any resource owned by the channel
		/// </summary>
		public void Close()
		{
			Debug.WriteLine("SocketChannel Close - " + _uri);
			_closing = true;
			_sendEvent.Set();
			Thread.Sleep(150);
			_sendEvent.Close();
			if (_ownedSocket && _socket.Connected)
			{
				//AbortReception();
				try
				{
				//	_socket.Shutdown(SocketShutdown.Both);
				}
				catch (Exception ex)
				{
					//Debug.WriteLine("SocketChannel.Close: " + sex.Message);
					CommMain.Logger.AddLog(ex);
				}
				try
				{
					_socket.Close();
				}
				catch (Exception ex)
				{
					//Debug.WriteLine("SocketChannel Close - " + cex.Message);
					CommMain.Logger.AddLog(ex);
				}
				Thread.Sleep(150);
				_socket = null;
			}
		}
		/// <summary>
		/// The remote end point URI
		/// </summary>
		public String Uri
		{
			get { return _uri; }
			set { _uri = value; }
		}
		/// <summary>
		/// Read-only property that returns whether the Channel
		/// is being closed or not
		/// </summary>
		public bool Closing
		{
			get { return _closing; }
		}
		/// <summary>
		/// The event fired when a message has been sent
		/// </summary>
		public event MessageHandler OutcomingMessage
		{
			add { _outcomingMsgHandler += value; }
			remove { _outcomingMsgHandler -= value; }
		}
		/// <summary>
		/// The event fired when a message has been received
		/// </summary>
		public event MessageHandler IncomingMessage
		{
			add { _incomingMsgHandler += value; }
			remove { _incomingMsgHandler -= value; }
		}
		/// <summary>
		/// The event fired when a error occurs sending a message
		/// </summary>
		public event ErrorHandler SendError
		{
			add { _sendErrorHandler += value; }
			remove { _sendErrorHandler -= value; }
		}
		/// <summary>
		/// The event fired when a error occurs receiving a message
		/// </summary>
		public event ErrorHandler ReceiveError
		{
			add { _receiveErrorHandler += value; }
			remove { _receiveErrorHandler -= value; }
		}
		#endregion // Public API

		#region Private methods
		/// <summary>
		/// The thread responsible for sending messages
		/// </summary>
		/// <param name="state">Is a SocketChannel object</param>
		private void ThreadSendProc(object state)
		{
			bool send = true;
			Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
			while (send && !_closing)
			{
				Debug.WriteLine("SocketChannel - Blocking to send data");
				try
				{
					_sendEvent.WaitOne();
				}
				catch (Exception ex)
				{
					//Debug.WriteLine("SocketChannel.ThreadSendProc - Exception: " + ex.Message);
					CommMain.Logger.AddLog(ex);
				}
				Debug.WriteLine("SocketChannel - Unblocked to send data");
				if (_closing)
				{
					NotifySendError(SocketErrors.SendingError);
					break;
				}
				Monitor.Enter(_outQueue);
				try
				{
					while (_outQueue.Count > 0)
					{
						OPSTelegrama t = (OPSTelegrama)_outQueue.Dequeue();
						byte[] data = t.FullData;
						//Debug.WriteLine("SocketChannel - About to send, length=" + data.Length.ToString());
						//CommMain.Logger.AddLog("SocketChannel - About to send, length=" + data.Length.ToString(),
						//	LogLevels.DEBUG);
						_socket.Send(data, 0, data.Length, SocketFlags.None);
						Thread.Sleep(0);
					}
				}
				catch (SocketException sockEx)
				{
					//Debug.WriteLine(sockEx.Message);
					CommMain.Logger.AddLog(sockEx);
					if (sockEx.ErrorCode == WSAECONNRESET)
					{
						// The remote end-point closed the connection
						NotifySendError(SocketErrors.RemoteClosed);
						send = false;
					}
				}
				catch (Exception ex)
				{
					//Debug.WriteLine(ex.Message);
					CommMain.Logger.AddLog(ex);
					NotifySendError(SocketErrors.SendingError);
					send = false;
				}
				finally
				{
					_outQueue.Clear();
					Monitor.Exit(_outQueue);
				}
			}
			Debug.WriteLine("SocketChannel - ThreadSendProc ending");
		}
		/// <summary>
		/// The thread responsible for receiving messages
		/// </summary>
		/// <param name="state">Is a SocketChannel object</param>
		private void ThreadReceiveProc(object state)
		{
			Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
			while (!_closing)
			{
				try
				{
					//Debug.WriteLine("SocketChannel - Blocking to receive data");
					//CommMain.Logger.AddLog("SocketChannel - Blocking to receive data", LogLevels.DEBUG);
					int received = _socket.Receive(_rcvBuffer, _rcvBufferCount, 
						MaxBufferSize, SocketFlags.None);
					if (!_closing)
					{
						if (received > 0)
						{
							//Debug.WriteLine("SocketChannel - Received data, length="+received.ToString());
							//CommMain.Logger.AddLog("SocketChannel - Received data, length="+received.ToString(), 
							//	LogLevels.DEBUG);
							int byteCount = received + _rcvBufferCount;
							byte[] bytesReceived = GetReceivedBytes(byteCount);
							ProcessReceivedData(bytesReceived);
						}
						else
						{
							// Possibly the remote end-point closed the connection
							CommMain.Logger.AddLog(string.Format("Receive count=0 from {0}", this.Uri), LoggerSeverities.Debug);
							NotifyReceiveError(SocketErrors.RemoteClosed);
							break;
						}
					}
				}
				catch (SocketException sockEx)
				{
					//Debug.WriteLine("SocketChannel - ThreadReceiveProc socket error: " + sockEx.Message);
					if (sockEx.ErrorCode == WSAECONNRESET)
					{
						//Debug.WriteLine("SocketChannel - ThreadReceiveProc WSAECONNRESET");
						CommMain.Logger.AddLog("WARNING: SocketChannel.ThreadReceiveProc: WSAECONNRESET at " + this.Uri, LoggerSeverities.Info);

						// The remote end-point closed the connection
						NotifyReceiveError(SocketErrors.RemoteClosed);
						break;
					}
					else if (sockEx.ErrorCode == WSAECONNABORTED)
					{
						CommMain.Logger.AddLog("WARNING: SocketChannel.ThreadReceiveProc: WSAECONNABORTED at " + this.Uri, LoggerSeverities.Info);

						// The remote end-point closed the connection
						NotifyReceiveError(SocketErrors.RemoteClosed);
						break;
					}
					else
					{
						CommMain.Logger.AddLog(sockEx);
					}
				}
				catch (Exception ex)
				{
					//Debug.WriteLine("SocketChannel - ThreadReceiveProc error: " + ex.Message);
					CommMain.Logger.AddLog(ex);
					NotifyReceiveError(SocketErrors.ReceivingError);
					break;
				}
			}
			if (_closing)
			{
				//>> LDR 2004.07.16
				//NotifyReceiveError(SocketErrors.ReceivingError);
				//<< LDR 2004.07.16
			}
			//Debug.WriteLine("SocketChannel - ThreadReceiveProc ending");
			//CommMain.Logger.AddLog("SocketChannel - ThreadReceiveProc ending", LogLevels.DEBUG);
		}
		/// <summary>
		/// Sends the notification that the message has been sent
		/// </summary>
		/// <param name="msg">The message just sent</param>
		private void NotifySend(OPSTelegrama msg)
		{
			try
			{
				if (_outcomingMsgHandler != null)
					_outcomingMsgHandler(msg, this);
				else
					CommMain.Logger.AddLog("WARNING: SocketChannel.NotifySend: _outcomingMsgHandler is null for ["  + msg.XmlData + "].", LoggerSeverities.Error);
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("SocketChannel - Exception NotifySend: " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
		}
		/// <summary>
		/// Sends the notification that the message has been received
		/// </summary>
		/// <param name="msg">The message just received</param>
		private void NotifyReceive(OPSTelegrama msg)
		{
			//CommMain.Logger.AddLog("NotifyReceive " + msg.XmlData, LogLevels.DEBUG);
			try
			{
				if (_incomingMsgHandler != null)
					_incomingMsgHandler(msg, this);
				else
					CommMain.Logger.AddLog("WARNING: SocketChannel.NotifyReceive: _incomingMsgHandler is null for [" + msg.XmlData + "].", LoggerSeverities.Error);
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("SocketChannel - Exception NotifySend: " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
		}
		/// <summary>
		/// Fires the handlers for a ReceiveError event
		/// </summary>
		/// <param name="error"></param>
		private void NotifyReceiveError(SocketErrors error)
		{
			try
			{
				if (_receiveErrorHandler != null)
					///_receiveErrorHandler((int) error, error.ToString(), this);
					_receiveErrorHandler((int) error, this.Uri, this);
				else
					CommMain.Logger.AddLog("WARNING: SocketChannel.NotifyReceiveError: _receiveErrorHandler is null.", LoggerSeverities.Error);
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("SocketChannel - Exception NotifySend: " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
		}
		/// <summary>
		/// Fires the handlers for a SendError event
		/// </summary>
		/// <param name="error"></param>
		private void NotifySendError(SocketErrors error)
		{
			try
			{
				if (_sendErrorHandler != null)
					///_sendErrorHandler((int) error, error.ToString(), this);
					_sendErrorHandler((int) error, this.Uri, this);
				else
					CommMain.Logger.AddLog("WARNING: SocketChannel.NotifySendError: _sendErrorHandler is null.", LoggerSeverities.Error);
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("SocketChannel - Exception NotifySend: " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
		}
		/// <summary>
		/// Handles received data and manages the eventually fragmented Telegrama
		/// </summary>
		/// <param name="rcvData"></param>
		private void ProcessReceivedData(byte[] rcvData)
		{
			int receivedCount = rcvData.Length;
			int headerSize;

			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			int iFrameType=(int)appSettings.GetValue("FrameType",typeof(int));

			switch (iFrameType)
			{
				case OPSTelegramaFactory.FRAMETYPE_NOENCRYPT:
					headerSize=OPSTelegramaFrame1.Header.ByteSize;
					break;
				case OPSTelegramaFactory.FRAMETYPE_ENCRYPT:
					headerSize=OPSTelegramaFrame2.Header.ByteSize;
					break;
				default:
					headerSize=OPSTelegramaFrame1.Header.ByteSize;
					break;
			}



			if (receivedCount > headerSize)
			{
				Debug.WriteLine("Process - Received enough data");
				_rcvBufferCount = 0;
				// At least the header has been received
				if (_telegram == null)
				{
					Debug.WriteLine("Process - Starting new telegram");
					// A new telegram must be built
					_telegram = OPSTelegramaFactory.CreateOPSTelegrama(rcvData);
					Debug.WriteLine("Process - t: " + _telegram.XmlData);
				}
				else
				{
					Debug.WriteLine("Process - Still adding data to telegram");
					// Still receiving data for the current telegram
					_telegram.AddData(rcvData);
					Debug.WriteLine("Process - t: " + _telegram.XmlData);
				}

				if (_telegram.Completed)
				{
					Debug.WriteLine("Process - Telegram completed");
					// Notify the completion of the telegram
					NotifyReceive(_telegram);
				
					// handle eventual extra bytes belonging to a following telegram
					int extraBytes = _telegram.GetLastExtraBytes();
					_telegram = null;
					if (extraBytes > 0)
					{
						Debug.WriteLine("Process - Extra bytes to process");
						ProcessExtraBytes(rcvData, receivedCount - extraBytes);
					}
				}
			}
			else
			{
				Debug.WriteLine("Process - Not enough data received");
				// It's likely a fragment of a header came at the end of a 
				// previous telegram. Add the partial data to the incoming data
				// buffer for further processing
				for (int i = 0; i < receivedCount; i++)
				{
					_rcvBuffer[_rcvBufferCount++] = rcvData[i];
				}
			}
		}
		/// <summary>
		/// Processes the extra bytes at the end of a telegram belonging to
		/// the following telegram
		/// </summary>
		/// <param name="rcvData">The buffer containing the extra bytes</param>
		/// <param name="index">The position where the extra bytes are</param>
		private void ProcessExtraBytes(byte[] rcvData, int index)
		{
			byte[] newBuffer = new byte[rcvData.Length - index];
			Array.Copy(rcvData, index, newBuffer, 0, newBuffer.Length);
			ProcessReceivedData(newBuffer);
		}
		/// <summary>
		/// Returns a copy of the received bytes in the received buffer with
		/// the exact length of the received count (OPSTelegrama needs such
		/// an array of bytes to work)
		/// </summary>
		/// <param name="byteCount"></param>
		/// <returns></returns>
		private byte[] GetReceivedBytes(int byteCount)
		{
			byte[] rcvBytes = new byte[byteCount];
			Array.Copy(_rcvBuffer, 0, rcvBytes, 0, byteCount);
			return rcvBytes;
		}

		private void AbortReception()
		{
			try
			{
				byte[] buffer = new byte[] { 1 };
				Socket socket = new Socket(
					AddressFamily.InterNetwork,
					SocketType.Dgram,
					ProtocolType.Udp);
				IPEndPoint ep = (IPEndPoint) _socket.LocalEndPoint;
				string msg = string.Format("SocketChannel aborting reception for {0}:{1} ",
					ep.Address.ToString(), ep.Port.ToString());
				Debug.WriteLine(msg);
				socket.SendTo(buffer,0, 1, SocketFlags.None, ep);
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("SocketChannel aborting reception error - " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
		}
		#endregion // Private methods

		#region Private data members
		/// <summary>
		/// The end point to which the Channels connects to
		/// </summary>
		private String _uri;
		/// <summary>
		/// The socket to send messages and receive responses
		/// </summary>
		private Socket _socket;
		/// <summary>
		/// Indicates if the socket is responsible for closing the
		/// socket
		/// </summary>
		private bool _ownedSocket;
		/// <summary>
		/// Buffers the outgoing messages 
		/// </summary>
		private Queue _outQueue = new Queue(5);
		/// <summary>
		/// Event set when there are messages ready to sent
		/// </summary>
		private AutoResetEvent _sendEvent = new AutoResetEvent(false);
		/// <summary>
		/// The maximun size for incoming messages
		/// </summary>
		private static int MaxBufferSize = 4*1024;
		/// <summary>
		/// Indicates that the Channel is about to be closed
		/// </summary>
		private bool _closing = false;
		/// <summary>
		/// Holds the last message received or in process to be received
		/// </summary>
		private OPSTelegrama _telegram;
		/// <summary>
		/// The mumber of bytes received for the last message
		/// </summary>
		private int _rcvBufferCount = 0;
		/// <summary>
		/// Holds the bytes received
		/// </summary>
		private byte[] _rcvBuffer = new byte[MaxBufferSize];
		/// <summary>
		/// Handlers of the IncomingMessage event
		/// </summary>
		private MessageHandler _incomingMsgHandler;
		/// <summary>
		/// Handlers of the OutcomingMessage event
		/// </summary>
		private MessageHandler _outcomingMsgHandler;
		/// <summary>
		/// Handlers of the ReceiveError event
		/// </summary>
		private ErrorHandler _receiveErrorHandler;
		/// <summary>
		/// Handlers of the SendError event
		/// </summary>
		private ErrorHandler _sendErrorHandler;
		/// <summary>
		/// Windows Sockets error codes
		/// </summary>
		private static int WSAECONNRESET = 10054;
		private static int WSAECONNABORTED = 10053;
		#endregion // Private data members
	}
}
