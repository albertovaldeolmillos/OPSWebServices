using System;
using System.Collections;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// The types of channels
	/// </summary>
	public enum ChannelType { Socket, FecsQueue, BecsQueue };

	/// <summary>
	/// The interface for all channel factories.
	/// </summary>
	public interface IChannelManager
	{
		/// <summary>
		/// Creates a channel to exchange data with the end-point represented by
		/// the supplied uniform resource identifier.</summary>
		/// <param name="uri">The logical endpoint identifier</param>
		/// <param name="type">The type of channel to open. From the ChannelType enumeration</param>
		/// <returns>The channel requested or null if an error occurs</returns>
		IChannel OpenChannel(string uri, ChannelType type);
		/// <summary>
		/// Creates a channel to exchange data with the end-point represented by
		/// the supplied uniform resource identifier.</summary>
		/// <param name="uri">The logical endpoint identifier</param>
		/// <returns>The channel requested or null if an error occurs</returns>
		/// <remarks>The channel type is the manager's default channel type</remarks>
		IChannel OpenChannel(string uri);
		/// <summary>
		/// Returns the channel associated with the specified uri
		/// </summary>
		/// <param name="uri">The logical endpoint identifier</param>
		/// <returns>The channel requested or null if not found</returns>
		IChannel GetChannel(string uri);
		/// <summary>
		/// Gets rid of the channel associated with the specified uri
		/// </summary>
		/// <param name="uri">The logical endpoint identifier</param>
		void CloseChannel(string uri);
		/// <summary>
		/// Incorporates an already open channel and associates it with the specified uri
		/// </summary>
		/// <param name="uri">The logical endpoint identifier</param>
		/// <param name="channel">The channel associated to the uri parameter</param>
		void AddChannel(string uri, IChannel channel);
		/// <summary>
		/// Enumerates the channels
		/// </summary>
		/// <returns>An enumerator object allowing access to all the managed
		/// channels</returns>
		IEnumerator EnumChannels();
		/// <summary>
		/// Closes all channels
		/// </summary>
		void CloseAll();
	}
}
