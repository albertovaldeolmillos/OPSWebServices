using System;

namespace OPS.Comm.Common.Channel
{

	/// <summary>
	/// When processing data using an IInternalSink indicates if channel is being processes in forward order
	/// or in reverse order.
	/// </summary>
	public enum InternalSinkProcessOrder
	{
		/// <summary>
		/// Channel is being processed in forward order
		/// </summary>
		forward = 1,
		/// <summary>
		/// Channel is being processed in reverse order
		/// </summary>
		reverse = 2
	}

	/// <summary>
	/// A Sink is a process that is attached to a CommChannel in order to process the data.
	/// There are two types of sinks
	///		EndPointSinks that act with another client.
	///		InternalSinks that act with another sinks
	/// </summary>
	public interface ISink
	{
	}

	/// <summary>
	/// An EndPointSink is located at the end (or beginning) of a channel, translating data
	/// that goes into the channel (byte[]) to external data (string).
	/// </summary>
	public interface IEndPointSink : ISink
	{
		/// <summary>
		/// Transforms the internal data to external representation (string)
		/// </summary>
		/// <param name="data">Internal data to transform</param>
		/// <returns>External representation of data (as a string)</returns>
		string DecodifyData (byte[] data);
		/// <summary>
		/// Transforms the part of internal data to external representation (string)
		/// </summary>
		/// <param name="data">Internal data to transform</param>
		/// <param name="offset">Offset where data to transform starts</param>
		/// <param name="len">Number of bytes to transform.</param>
		/// <returns>External representation of data (as a string)</returns>
		string DecodifyData (byte[] data, int offset, int len);

		/// <summary>
		/// Transforms external data (string) to the internal representation allowed by the channel
		/// </summary>
		/// <param name="data">Data to transform</param>
		/// <returns>Array of bytes with the internal data.</returns>
		byte[] CodifyData (string data);
	}

	/// <summary>
	/// An InternalSink is located inside the Channel, doing some process of the data.
	/// It acts with the internal data (byte[]).
	/// </summary>
	public interface IInternalSink : ISink
	{
		/// <summary>
		/// Processes the data that is passing for the Channel
		/// </summary>
		/// <param name="data">Data to process</param>
		/// <param name="order">Order which channel is processed.</param>
		/// <returns>Data after process. Can be NULL if process does not modify the data</returns>
		byte[] ProcessData (byte[] data, InternalSinkProcessOrder order);
	}


	/// <summary>
	/// That class is an adapter to implement IEndPointSink interface.
	/// Implements all overloads, making implementation of IEndPointSink easier.
	/// </summary>
	public abstract class EndPointSinkAdapter : IEndPointSink
	{
		/// <summary>
		/// Transforms external data (string) to the internal representation allowed by the channel
		/// </summary>
		/// <param name="data">Data to transform</param>
		/// <returns>Array of bytes with the internal data.</returns>
		public abstract byte[] CodifyData (string data);
		
		/// <summary>
		/// Decodify all data conteined in data calling IEndPointSkink::DecodifyData (data, 0, data.Length).
		/// Not virtual. If you need to override that overload, don't inherit from EndPointSinkAdapter: just implement IEndPointSink
		/// </summary>
		/// <param name="data">Internal data to transform</param>
		/// <returns>External representation of data (as a string)</returns>		
		public string DecodifyData (byte[] data)				
		{
			return DecodifyData (data, 0, data.Length);
		}
		
		/// <summary>
		/// Transforms the part of internal data to external representation (string)
		/// </summary>
		/// <param name="data">Internal data to transform</param>
		/// <param name="offset">Offset where data to transform starts</param>
		/// <param name="len">Number of bytes to transform.</param>
		/// <returns>External representation of data (as a string)</returns>
		public abstract string DecodifyData (byte[] data, int offset, int len);
	}

}
