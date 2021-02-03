using System;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// Defines a set of functions that specify properties for a message that is put in a MSMQ queue
	/// </summary>
	public interface IPropertyManager
	{

		// Message to act with. This property has to be set before the rest (as Caducity and Priority)
		// could be received
		string Message { set; }
		/// <summary>
		/// Gets the MAXIMUM time that the message will be "valid". When the caducity of a message is over
		/// the message is discarded, regardless it were on target queue or not.
		/// </summary>
		TimeSpan Caducity { get; }

		/// <summary>
		/// Gets an integer that represents a priority for the given message. Higher values represents
		/// higher priorities.
		/// </summary>
		System.Messaging.MessagePriority Priority { get; }
	}
}
