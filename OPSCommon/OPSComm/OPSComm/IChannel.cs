using System;

namespace OPS.Comm.Channel
{
	/// <summary>
	/// A ProcessChannel is a set of processes (sinks) that are applied to data, in order to compress or encrypt it.
	/// </summary>
	public interface IProcessChannel
	{
		/// <summary>
		/// Gets the collection of Sinks that have that channel (does NOT include the IEndPointSink object).
		/// </summary>
		IReadOnlySinkCollection Sinks { get; }

		/// <summary>
		/// Gets the EndPointSink of that channel
		/// </summary>
		IEndPointSink EndPointSink { get; }

		/// <summary>
		/// Processes whole Channel in reverse order.
		/// First the byte[] data is processed passing by ALL IInternalSink objects (with reverse order) and finally is converted
		/// to string using IEndPointSink::DecodifyData
		/// Note that ProcessChannel (ProcessChannel(str)) DOES NOT have to return str (because the Channel does NOT have
		/// to be symmetric).
		/// </summary>
		/// <param name="data">Data to process</param>
		/// <returns>Data after processed by whole channel.</returns>
		string Process(byte[] data);

		/// <summary>
		/// Processes the whole Channel in forward order.
		/// First the string data is converted to string[] using IEndPointSink::CodifyData and after all IInternalSinks
		/// are invoked using the new byte[] data created (using forward order).
		/// Note that ProcessChannel (ProcessChannel(str)) DOES NOT have to return str (because the Channel does NOT have
		/// to be symmetric).
		/// </summary>
		/// <param name="data">Data to process</param>
		/// <returns>Data after processed by whole channel</returns>
		byte[] Process(string data);

	}

	/// <summary>
	/// A Custom Channel is a ProcessChannel that allows adding any InternalSink object,
	/// If a ProcessChannel is NOT a CustomChannel, InternalSinks cannot be added (and the Channel MUST
	/// provide its own).
	/// </summary>
	public interface ICustomProcessChannel : IProcessChannel
	{
		/// <summary>
		/// Add a InternalSink to the custom channel.
		/// InternalSinks are processed in the order they were added if channel is processed in 'forward' order.
		/// InternalSinks are processed in the inverse order of they were added if channel is processed in 'reverse' order.
		/// </summary>
		/// <param name="sink">InternalSink object to Add.</param>
		void AddSink (IInternalSink sink);
	}

	/// <summary>
	/// Implements the IProcessChannel interface allowing easy creation of specialized channels.
	/// </summary>
	public class ProcessChannelBase : IProcessChannel
	{

		protected  SinkCollection _sinks;
		protected IEndPointSink _endPointSink;

		/// <summary>
		/// Constructs a new ProcessChannelBase using the specified EndPointSink object
		/// </summary>
		/// <param name="endPointSink"></param>
		protected  ProcessChannelBase(IEndPointSink endPointSink)
		{
			_sinks = new SinkCollection();
			_endPointSink = endPointSink;
		}

		/// <summary>
		/// Constructs a new empty ProcessChannelBase
		/// </summary>
		protected ProcessChannelBase()
		{
			_sinks = new SinkCollection();
			_endPointSink = null;
		}

		/// <summary>
		/// Sets a new EndPointSink for the channel
		/// </summary>
		/// <param name="epSink">New EndPointSink to use</param>
		protected void SetEndPointSink (IEndPointSink epSink)
		{
			_endPointSink = epSink;
		}
		

		#region IProcessChannel Members

		/// <summary>
		/// Gets the collection of Sinks that have that channel (does NOT include the IEndPointSink object).
		/// </summary>
		public IReadOnlySinkCollection Sinks
		{
			get { return (IReadOnlySinkCollection)_sinks; }
		}

		/// <summary>
		/// Gets the EndPointSink of that channel
		/// </summary>
		public IEndPointSink EndPointSink
		{
			get { return _endPointSink; }
		}

		/// <summary>
		/// Processes whole Channel in reverse order.
		/// First the byte[] data is processed passing by ALL IInternalSink objects (with reverse order) and finally is converted
		/// to string using IEndPointSink::DecodifyData
		/// Note that ProcessChannel (ProcessChannel(str)) DOES NOT have to return str (because the Channel does NOT have
		/// to be symmetric).
		/// </summary>
		/// <param name="data">Data to process</param>
		/// <returns>Data after processed by whole channel.</returns>
		public string Process(byte[] data)
		{
			byte[] pdata = data;
			// Step 1: Apply all InternalSinks in reverse order
			int count = _sinks.Count;
			for (int i=count-1; i>=0; i--)
			{
				IInternalSink internalSink = (IInternalSink)_sinks[i];
				byte [] idata = internalSink.ProcessData (pdata,InternalSinkProcessOrder.reverse);
				pdata = idata!=null ? idata : pdata;
			}
			// Step 2: Apply the EndPointSink
			string sdata = _endPointSink.DecodifyData(pdata);
			return sdata;
		}

		/// <summary>
		/// Processes the whole Channel in forward order.
		/// First the string data is converted to string[] using IEndPointSink::CodifyData and after all IInternalSinks
		/// are invoked using the new byte[] data created (using forward order).
		/// Note that ProcessChannel (ProcessChannel(str)) DOES NOT have to return str (because the Channel does NOT have
		/// to be symmetric).
		/// </summary>
		/// <param name="data">Data to process</param>
		/// <returns>Data after processed by whole channel</returns>
		public byte[] Process(string data)
		{
			// Step 1: Apply the EndPointSink
			byte[] bdata = _endPointSink.CodifyData(data);
			// Step 2: Apply all InternalSinks
			foreach (ISink sink in _sinks)
			{
				byte[] idata  = ((IInternalSink)sink).ProcessData (bdata , InternalSinkProcessOrder.forward);
				bdata = idata!=null ? idata : bdata;
			}
			return bdata ;
		}

		#endregion
	}

	/// <summary>
	/// Implements the ICustomProcessChannel allowing easy creation of specialized custom channels.
	/// </summary>
	public class CustomProcessChannelBase : ProcessChannelBase, ICustomProcessChannel
	{	
		/// <summary>
		/// Constructs a new ProcessChannelBase using the specified EndPointSink object
		/// </summary>
		/// <param name="endPointSink"></param>
		protected  CustomProcessChannelBase(IEndPointSink endPointSink) : base (endPointSink) {}

		/// <summary>
		/// Constructs a new empty CustomProcessChannelBase
		/// </summary>
		protected CustomProcessChannelBase() : base() {}

		#region ICustomProcessChannel Members

		public void AddSink(IInternalSink sink)
		{
			_sinks.Add (sink);
		}

		#endregion
	
	}
 
}
