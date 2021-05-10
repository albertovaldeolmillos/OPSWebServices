using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using OPS.Comm;
using OPS.Components.Data;
using System.Text.RegularExpressions;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Interface that defines the properties and methods that ALL messages that CAN be received must support.
	/// Each message handler will be implemented with one class which must have this interface
	/// </summary>
	public interface IRecvMessage
	{
		/// <summary>
		/// The message is processed, and all responses are generated
		/// This method obtains the response(s). Each response is just an XML string
		/// </summary>
		/// <returns>All the responses generated (null if none)</returns>
		StringCollection Process();

		/// <summary>
		/// Returns the ID of the message
		/// </summary>
		long MsgId { get ; }

		/// <summary>
		/// The session with global data
		/// </summary>
		MessagesSession Session { get ; set ; }

		/// <summary>
		/// The message origin
		/// </summary>
		string MsgOrig { get ; set ; }
	}

	/// <summary>
	/// Class responsible of creating the object messages that process the messages received.
	/// </summary>
	public sealed class MessageFactory
	{
		/// <summary>
		/// Contains the delegates that create the messages (classes that implements IRecvMessage)
		/// based on the root tag of the string, indexed by tag.
		/// </summary>
		private static Hashtable _messages;

		private delegate IRecvMessage CreateMessageDelegate (XmlDocument data);
		private MessageFactory() {}

		/// <summary>                  
		/// Static constructor. Initializes the hashtable with the delegates
		/// </summary>
		static MessageFactory()
		{
			
			_messages = new Hashtable(40);// (+1):CFE - Add msg m12 
											// (+2):ORC - Add msg m20
											// (+3):IGU - Add msg m14
											// (+4):EBE - Add msg m6
											// (+5):EBE - Add msg m54
											// (+6):EBE - Add msg m55
											// (+7):EBE - Add msg m56
											// (+8):EBE - Add msg m7
											// (+9):EBE - Add msg m8
											// (+10):EBE - Add msg m9
											// (+11):EBE - Add msg m61
											// (+12):MRW - Add msg m62
											// (+13):EBE - Add msg m63
											// (+14):BCO - Add msg m90
											// (+15):EBE - Add msg m100
											// (+16):EBE - Add msg m101
											// (+17):EBE - Add msg m102
											// (+18):JTZ - Add msg m51
											// (+19):JTZ - Add msg m60

			//_messages = new Hashtable(1);
			// PDM messages
			
			_messages.Add(Msg01.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg01));
			_messages.Add(Msg02.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg02));
			_messages.Add(Msg03.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg03));
			_messages.Add(Msg04.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg04));
			_messages.Add(Msg05.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg05));
			_messages.Add(Msg06.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg06));
			//_messages.Add(Msg07.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg07));
			_messages.Add(Msg08.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg08));
			_messages.Add(Msg09.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg09));
			_messages.Add(Msg20.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg20));
			// CFE - Añado mensaje m12
			
			_messages.Add(Msg12.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg12));
			//--IGU--> 170305: M14 message added
			_messages.Add(Msg14.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg14));
			//--IGU--> 170305: M14 message added
			// PDA messages
			_messages.Add(Msg50.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg50));
			//--JTZ--> 120615: M51 message Added
			_messages.Add(Msg51.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg51));			
			_messages.Add(Msg52.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg52));
			_messages.Add(Msg53.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg53));
			_messages.Add(Msg54.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg54));
			_messages.Add(Msg55.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg55));
			_messages.Add(Msg56.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg56));
			_messages.Add(Msg57.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg57));
			_messages.Add(Msg58.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg58));
			_messages.Add(Msg59.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg59));
			//--JTZ--> 300615: M60 message Added
			_messages.Add(Msg60.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg60));
			_messages.Add(Msg61.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg61));
			_messages.Add(Msg62.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg62));
			_messages.Add(Msg63.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg63));
			_messages.Add(Msg70.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg70));
			_messages.Add(Msg71.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg71));
			_messages.Add(Msg72.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg72));
			_messages.Add(Msg73.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg73));
			_messages.Add(Msg79.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg79));
			_messages.Add(Msg80.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg80));
			_messages.Add(Msg81.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg81));
			_messages.Add(Msg83.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg83));
			_messages.Add(Msg90.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg90));
			_messages.Add(Msg100.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg100));
			_messages.Add(Msg101.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg101));
			_messages.Add(Msg102.DefinedRootTag, new CreateMessageDelegate(MessageFactory.CMsg102));

		}

		/// <summary>
		/// Returns an instance of the delegate class that handles the message.
		/// It's a factory method.
		/// </summary>
		/// <param name="xml">The source XML of the message</param>
		/// <returns>The instance of the class</returns>
		public static IRecvMessage GetReceivedMessage(string xml)
		{
			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			XmlDocument doc = new XmlDocument();
			string root = "";

			if(logger != null)
				logger.AddLog("[Message Factory] Loading Xml"  ,LoggerSeverities.Debug);

			// JTZ - 06/08/2015: Creado método CleanErrorsInDecryptedTags para limpiar tags con simbolos erroneos
			try
			{
				CleanErrorsInDecryptedTags(ref xml,logger);
			}
			catch (Exception ex)
			{
				if(logger != null)
					logger.AddLog("[Message Factory] Error cleaning symbols in tags: " + ex.ToString()  ,LoggerSeverities.Error);
			}

			try
			{
				doc.LoadXml(xml);
				root = doc.DocumentElement.Name;
			}
			catch (Exception ex)
			{
				if(logger != null)
					logger.AddLog("[Message Factory] Exception: " + ex.ToString() ,LoggerSeverities.Error);
				throw ex;
			}
			if(logger != null)
				logger.AddLog("[Message Factory] Loading Xml root: " + root  ,LoggerSeverities.Debug);
			// Retrieves and returns a IRecvMessage-implementing class based on root tag

			IRecvMessage msg;
			try
			{
				msg =  ((CreateMessageDelegate)_messages[root])(doc);
			}
			catch (Exception ex)
			{
				logger.AddLog("[Message Factory] Exception: " + ex.ToString(),LoggerSeverities.Error);
				throw ex;
			}

			
			if(logger != null)
				logger.AddLog("[Message Factory] GetReceivedMessage: " + msg.MsgId.ToString() ,LoggerSeverities.Debug);
			return msg;

			}

		/// <summary>
		/// Returns the id of the supplied message in XML format
		/// Useful when the associated message cannot be created for whatever reason
		/// (for example when the class is unable to load the parameters)
		/// </summary>
		/// <param name="xml">The source XML of the message</param>
		/// <returns>The id of the message</returns>
		public static int GetIdFromMessage(string xml)
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);
				return Convert.ToInt32(doc.DocumentElement.GetAttribute("id"));
			}
			catch (Exception)
			{
				return 0;
			}
		}

		private static int CleanErrorsInDecryptedTags(ref string xml, ILogger logger)
		{
			string[] tagList = new string[]{"tm", "tdd", "rtm"};
			int numberOfTagsReplaced = 0;

			foreach (string tag in tagList)
			{
				try
				{
					int posIni = xml.IndexOf("<" + tag + ">");
					if (posIni > 0) 
					{
						int posEnd = xml.IndexOf("</" + tag + ">");
						string innerText = xml.Substring(posIni+tag.Length+2, posEnd-(posIni+tag.Length+2));
						Regex rgx = new Regex("[^a-zA-Z0-9/_*. -]");
						if (rgx.IsMatch(innerText))
						{
							numberOfTagsReplaced++;
							if(logger != null)
								logger.AddLog("[Message Factory::CleanErrorsInDecryptedTags] Initial status of tag <" + tag + ">: " + innerText ,LoggerSeverities.Debug);
							string innerTextOutput = rgx.Replace(innerText, "*");
							xml = xml.Replace(innerText, innerTextOutput);
							if(logger != null)
								logger.AddLog("[Message Factory::CleanErrorsInDecryptedTags] Final status of tag <" + tag + ">: " + innerTextOutput ,LoggerSeverities.Debug);
						}
					}
				}
				catch (Exception ex)
				{
					if(logger != null)
						logger.AddLog("[Message Factory::CleanErrorsInDecryptedTags] Exception cleaning tag <" + tag + ">: " + ex.ToString() ,LoggerSeverities.Error);
				}
			}
			return numberOfTagsReplaced;
		}


		#region Methods that create messages

		// PDM messages
		private static IRecvMessage CMsg01(XmlDocument data) { return new Msg01(data); }
		private static IRecvMessage CMsg02(XmlDocument data) { return new Msg02(data); }
		private static IRecvMessage CMsg03(XmlDocument data) { return new Msg03(data); }
		private static IRecvMessage CMsg04(XmlDocument data) { return new Msg04(data); }
		private static IRecvMessage CMsg05(XmlDocument data) { return new Msg05(data); }
		private static IRecvMessage CMsg06(XmlDocument data) { return new Msg06(data); }
		//private static IRecvMessage CMsg07(XmlDocument data) { return new Msg07(data); }
		private static IRecvMessage CMsg08(XmlDocument data) { return new Msg08(data); }
		private static IRecvMessage CMsg09(XmlDocument data) { return new Msg09(data); }
		// CFE - Añado mensaje m12
		private static IRecvMessage CMsg12(XmlDocument data) { return new Msg12(data); }
		//--IGU--> 170305: M14 message added
		private static IRecvMessage CMsg14(XmlDocument data) { return new Msg14(data); }
		//--IGU--> 170305: M14 message added
		private static IRecvMessage CMsg20(XmlDocument data) { return new Msg20(data); }		
		// PDA messages
		private static IRecvMessage CMsg50(XmlDocument data) { return new Msg50(data); }
		// JTZ - Añadido mensaje M51
		private static IRecvMessage CMsg51(XmlDocument data) { return new Msg51(data); }
		private static IRecvMessage CMsg52(XmlDocument data) { return new Msg52(data); }
		private static IRecvMessage CMsg53(XmlDocument data) { return new Msg53(data); }
		private static IRecvMessage CMsg54(XmlDocument data) { return new Msg54(data); }
		private static IRecvMessage CMsg55(XmlDocument data) { return new Msg55(data); }
		private static IRecvMessage CMsg56(XmlDocument data) { return new Msg56(data); }
		private static IRecvMessage CMsg57(XmlDocument data) { return new Msg57(data); }
		private static IRecvMessage CMsg58(XmlDocument data) { return new Msg58(data); }
		private static IRecvMessage CMsg59(XmlDocument data) { return new Msg59(data); }
		// JTZ - Añadido mensaje M60
		private static IRecvMessage CMsg60(XmlDocument data) { return new Msg60(data); }
		private static IRecvMessage CMsg61(XmlDocument data) { return new Msg61(data); }
		private static IRecvMessage CMsg62(XmlDocument data) { return new Msg62(data); }
		private static IRecvMessage CMsg63(XmlDocument data) { return new Msg63(data); }
		private static IRecvMessage CMsg70(XmlDocument data) { return new Msg70(data); }
		private static IRecvMessage CMsg71(XmlDocument data) { return new Msg71(data); }
		private static IRecvMessage CMsg72(XmlDocument data) { return new Msg72(data); }
		private static IRecvMessage CMsg73(XmlDocument data) { return new Msg73(data); }
		private static IRecvMessage CMsg79(XmlDocument data) { return new Msg79(data); }
		private static IRecvMessage CMsg80(XmlDocument data) { return new Msg80(data); }
		private static IRecvMessage CMsg81(XmlDocument data) { return new Msg81(data); }
		private static IRecvMessage CMsg83(XmlDocument data) { return new Msg83(data); }
		private static IRecvMessage CMsg90(XmlDocument data) { return new Msg90(data); }
		private static IRecvMessage CMsg100(XmlDocument data) { return new Msg100(data); }
		private static IRecvMessage CMsg101(XmlDocument data) { return new Msg101(data); }
		private static IRecvMessage CMsg102(XmlDocument data) { return new Msg102(data); }
		#endregion
	}
}
