using System;
using System.Security.Cryptography;
using OPS.Comm.Codify.Celes;
using OPS.Comm.Cryptography.TripleDes;
using OPS.Comm.Channel;

namespace OPS.Comm.Channel.Concrete
{
	/// <summary>
	/// A Process Channel that uses:
	///		.) Celes codify method (as EndPointSink)
	///		.) Triple DES encription (as an InternalSink)
	/// </summary>
	public class CelesProcessChannel : ProcessChannelBase
	{
		public CelesProcessChannel() : base (new OPSCelesCodifier ()) 
		{
			// Add the concrete IInternalSinks.

			TripleDESCryptoServiceProvider cs = new TripleDESCryptoServiceProvider();
			cs.GenerateKey();
			cs.GenerateIV();
			OPSTripleDesEncryptor encriptor = new OPSTripleDesEncryptor(cs);
			_sinks.Add (encriptor);
		}
	}
}
