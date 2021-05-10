using System;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Contains constants for the predefined message tags
	/// </summary>
	public class Tags
	{
		#region All uses

		public const string AckProcessed = "ap";
		public const string AckError= "ae";
		public const string AckOK = "ao";
		public const string AckDeferred = "ad";
		public const string AckJammed = "aj";
		public const string NackMsg = "ne";
		public const string NackServer = "nb";
		public const string Packet = "p";
		public const string MessageId = "id";
		
		#endregion // All uses
		
		#region Messages tags
		
		public const string QueryVehicleMsg = "m50";
		public const string QueryVehicleResp = "r50";
		public const string MailMsg = "m51";
		public const string RequestTableMsg = "m52";
		public const string PunishVehicleMsg = "m53";
		public const string RequestReplicationMsg = "m57";
		public const string TimeSyncMsg = "m59";
		public const string ReplTablesVersionsMsg = "m100";
		
		#endregion // Messages tags
		
		#region Message attributes

		public const string PacketSrcAttr = "src";
		public const string PacketDateAttr = "dtx";
		public const string MsgIdAttr = "id";
		public const string RetriesAttr = "ret";
		public const string DestinationAttr = "dst";
		public const string PriorityAttr = "pty";

		#endregion // Message attributes
	}
}
