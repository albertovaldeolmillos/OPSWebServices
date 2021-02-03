using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections;

using OPS.Comm.Common.Codify;
using OPS.Comm.Common.Channel;

namespace OPS.Comm.Common.Codify.Celes
{

	public enum CodifyMarks
	{
		/// <summary>
		/// Init tag mark
		/// </summary>
		Mi = 0x80, 
		/// <summary>
		/// Text data mark
		/// </summary>
		Mt = 0x40, 
		/// <summary>
		/// Number data mark
		/// </summary>
		Mn = 0x20, 
		/// <summary>
		/// End tag mark
		/// </summary>
		Mf = 0x10,
		/// <summary>
		/// Mask for getting the size of a mask
		/// </summary>
		MaskSize = 0xF
	}

	internal class OPSCelesCodificador
	{
		// Codification used vars
		protected XmlDocument _xmlDoc;
		protected XmlNode _root;
		protected int _idx;
		protected ByteArray _buffer;

		// Indicates the MAX value that the field size can have in a Mt.
		// If a Mt with size == MAX_SIZE_PER_MT appears indicate that the following bytes indicates the real size.
		// i.e.		Mt(MAX_SIZE_PER_MT), 0, 12 ==> indicates a size of (MAX_SIZE_PER_MT - 1) + 256 + 12
		//			Mt(MAX_SIZE_PER_MT), 0, 0, 22 ==> indicates a size of (MAX_SIZE_PER_MT - 1) + 256 + 256 + 22
		//			Mt(MAX_SIZE_PER_MT), 1 ==> indicates a size of (MAX_SIZE_PER_MT - 1) + 1
		private static int MAX_SIZE_PER_MT = 15;
		private static int ZERO_IN_EXTRA_LENGTH_MEANS = 256;


		internal OPSCelesCodificador()
		{
			_idx = 0;
		}

		#region Codification section

		/// <summary>
		/// Codifies the specified data with the "Celes codify method".
		/// </summary>
		/// <param name="data">Data to codify (MUST be an XML string)</param>
		/// <returns>A newly created array of bytes with data codified</returns>
		internal  byte[] CodifyData(string data)
		{
			_xmlDoc = new XmlDocument();
			_xmlDoc.LoadXml (data);
			_root = _xmlDoc.DocumentElement;
			// We don't use an ArrayList because we will insert a lot of byte items and boxing/unboxing could be unacceptable
			// So is better to use a reasonably large byte array, and if it is not enought, allocate a second larger buffer and
			// copy all data.
			_buffer = new ByteArray(data.Length * 2);			// double the length of string is a reasonably larger buffer
			CodifyNode (_root);
			byte[] retArray = new byte[_idx];
			Array.Copy (_buffer, 0, retArray, 0, _idx);
			return retArray;
		}

		/// <summary>
		/// Codify one node and all his childs.
		/// </summary>
		/// <param name="node">Node to codify</param>
		protected virtual void CodifyNode(XmlNode node)
		{
			// Codify the current node, and all childs of current node (with recursive calls)
			// First we have to add the Mark for the following node as it follows:
			string nodename = GetNodeNameWithAttrs (node);
			PutMarcaInicioNode (nodename);
			// Next step is add the NAME of the node
			for (int i=0; i< nodename.Length; i++) 
			{
				_buffer[_idx++] = (byte)nodename[i];		// TODO: What to do with unicode chars > 255 ???????????
			}
			// Next step is add the DATA of the node.
			//  Data is added as a mark (specifying type and length of data) and a n-bytes with data.
			PutMarcaAndDatosNode (node);

			// Now processes the childs
			int childNodesCount = 0;
			foreach (XmlNode child in node.ChildNodes) 
			{
				if (child.NodeType == XmlNodeType.Element) 
				{
					CodifyNode (child);
					childNodesCount++;
				}
			}

			// Finally closes the node (only nodes with no childs are "closed")
			
			if (childNodesCount == 0)
			{
				// Node has no childs ==> it is time to put the close tag mark.
				// Close all pending nodes (1 minimum).
				int pendingNodes = 1;
				XmlNode current = node;
				XmlNode parent = current.ParentNode;
				if (parent!=null && node!=_root)							
				{
					while (parent.NodeType != XmlNodeType.Document && current == parent.LastChild)
					{
						current=parent;
						parent = current.ParentNode;
						pendingNodes++;
					}
				}
				_buffer[_idx++] = (byte)((byte)CodifyMarks.Mf | (byte)pendingNodes);
			}			
		}

		/// <summary>
		/// Gets the name of a node, including its attributes. 
		/// (i.e. the node <a x="y"></a> will have a nodname of a x="y"
		/// </summary>
		/// <param name="node">Node to get its nodename</param>
		/// <returns>Nodename</returns>
		protected virtual string GetNodeNameWithAttrs (XmlNode node)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append (node.Name);
			foreach (XmlAttribute attr in node.Attributes)
			{
				sb.Append (' ');
				sb.Append (attr.Name);
				sb.Append ("=\"");
				sb.Append (attr.Value);
				sb.Append ('"');
			}
			return sb.ToString();
		}

		/// <summary>
		/// Puts the Mark for a new Node (A byte with the mark M(init, text, length) where length is the length of node's name)
		/// If node name's length > 15 the mark will have more than one byte.
		/// </summary>
		/// <param name="nodeName">Name of the node</param>
		protected virtual void PutMarcaInicioNode (string nodeName)
		{
			if (nodeName.Length > MAX_SIZE_PER_MT - 1)
			{
				_buffer[_idx++] = (byte) (( (int)CodifyMarks.Mi | (int)CodifyMarks.Mt ) | MAX_SIZE_PER_MT);	
				int length = nodeName.Length-(MAX_SIZE_PER_MT-1);		// 14 first bytes are marked with Mt(15).
				while (length > 0)
				{
					if (length > Byte.MaxValue) 
					{
						_buffer[_idx++] = 0;			// A 0 in a length byte indicates 256 bytes more of length
						length-=ZERO_IN_EXTRA_LENGTH_MEANS;
					}
					else
					{
						_buffer[_idx++] = (byte)length;		
						length =0;
					}
				}
			}
			else
			{
				_buffer[_idx++] = (byte) (( (int)CodifyMarks.Mi | (int)CodifyMarks.Mt ) | nodeName.Length);
			}
			 
		}


		/// <summary>
		/// Put the mark for the data of a node (NOT the name) AND the data.
		/// </summary>
		/// <param name="node">Node to codify its data</param>
		protected virtual void PutMarcaAndDatosNode (XmlNode node)
		{
			bool bIsNumber = true;
			string innerText = GetTextOfNode(node);
			ulong lv= 0UL;
			try 
			{
				lv = Convert.ToUInt64(innerText);
			}
			catch (Exception)
			{
				// Some exception has ocurred... we will assume that innerText is a string
				bIsNumber = false;
			}

			byte ret;
			if (bIsNumber)
			{
				ret = (byte)CodifyMarks.Mn;
				// Get the size (in bytes) of the number, and writes number to the next bytes
				int iretidx = _idx++;
				byte nBytes = GetBytesAndWriteNumber(lv);
				ret |= nBytes;
				_buffer[iretidx] = ret;
				_idx+=nBytes;
			}
			else
			{
				ret = (byte)CodifyMarks.Mt;
				// Get the size (in bytes) of the text
				if (innerText.Length > MAX_SIZE_PER_MT - 1)  
				{
					// More than 15 chars ==> One Mt with length 16 + n bytes with extra length.
					ret |= (byte)MAX_SIZE_PER_MT;
					_buffer[_idx++] = ret;
					int length = innerText.Length-(MAX_SIZE_PER_MT -1);		// 14 first bytes are marked with Mt(15).
					while (length > 0)
					{
						if (length > Byte.MaxValue) 
						{
							_buffer[_idx++] = 0;			// A 0 in a length byte indicates 256 bytes more of length
							length-=ZERO_IN_EXTRA_LENGTH_MEANS;
						}
						else
						{
							_buffer[_idx++] = (byte)length;		
							length =0;
						}
					}
				}
				else
				{
					// Less or equal than 15 chars ==> only one Mt with lentgh
					ret |= (byte)innerText.Length;
					_buffer[_idx++] = ret;
				}
				// Put the text
				for (int i=0; i< innerText.Length;i++) 
				{
					_buffer[_idx++] = (byte)innerText[i];
				}
			}
		}

		/// <summary>
		/// Gets the text inside an xml node (i.e. <a>100<b>200</b></a> returns 100 if node 'a' is passed)
		/// </summary>
		/// <param name="n">Node to get its text</param>
		/// <returns>Text inside a node or empty string ("") if node has no text</returns>
		protected string GetTextOfNode(XmlNode n)
		{
			string ret = null;
			foreach (XmlNode child in n.ChildNodes)
			{
				if (child.NodeType == XmlNodeType.Text) 
				{
					ret = child.Value;		// Only ONE text node
					break;
				}
			}

			return ret!=null ? ret : string.Empty;
		}

		/// <summary>
		/// Writes a number in the buffer (in binary format) and returns the minimum number
		/// of bytes needed to store that number
		/// </summary>
		/// <param name="number">Number to write to the buffer</param>
		/// <returns>A byte with number of bytes needed to store the number (1-8)</returns>
		protected virtual byte GetBytesAndWriteNumber (ulong number)
		{
			// 1st ensure we have enought space (we can need at maximum 8 bytes).
			_buffer.EnsureCapacity (_idx + 8);
			MemoryStream ms = new MemoryStream (_buffer,_idx, _buffer.Length - _idx);
			BinaryWriter bw = new BinaryWriter (ms);
			bw.Write (number);
			int i=7;
			for (; i>=0; i--)
			{
				if (_buffer[_idx + i] != 0) break;
			}
			return (byte)(i+1);
		}

		#endregion
	}

	internal class OPSCelesDecodificador
	{
		
		
		// Decodification used vars
		protected StringBuilder _decoded;
		protected Stack _openTags;
		protected byte[] _ulongBytes;
		protected byte[] _buffer;
		protected int _idx;

		// Indicates the MAX value that the field size can have in a Mt.
		// If a Mt with size == MAX_SIZE_PER_MT appears indicate that the following bytes indicates the real size.
		// i.e.		Mt(MAX_SIZE_PER_MT), 0, 12 ==> indicates a size of (MAX_SIZE_PER_MT - 1) + 256 + 12
		//			Mt(MAX_SIZE_PER_MT), 0, 0, 22 ==> indicates a size of (MAX_SIZE_PER_MT - 1) + 256 + 256 + 22
		//			Mt(MAX_SIZE_PER_MT), 1 ==> indicates a size of (MAX_SIZE_PER_MT - 1) + 1
		private static int MAX_SIZE_PER_MT = 15;
		private static int ZERO_IN_EXTRA_LENGTH_MEANS = 256;



		internal OPSCelesDecodificador()
		{
			_ulongBytes = new byte[8];				// 8 == sizeof(ulong);
		}

		#region DEcodification Section

		/// <summary>
		/// Decodify the specified data (that has been previosuly codified with OPSCelesCodifier::CodifyData())
		/// </summary>
		/// <param name="codifiedData">Array of bytes with data to decodify</param>
		/// <param name="offset">Index inside codifiedData where codified data starts</param>
		/// <param name="length">Size (in bytes) of the codified data</param>
		/// <returns>string with uncodified data</returns>
		internal string DecodifyData(byte[] codifiedData, int offset, int length)
		{
			_openTags = new Stack();
			_decoded = new StringBuilder();

			// All codified data starts with a Mark.
			_buffer = new byte[length];
			Array.Copy (codifiedData, offset, _buffer, 0, length);
			_idx = 0;
			while (_idx < _buffer.Length)
			{
				ProcessMark();
			}
			return _decoded.ToString();
		}

		/// <summary>
		/// Process a mark (and all his associated data).
		/// </summary>
		protected void ProcessMark()
		{
			// read the Mark
			byte mark = _buffer[_idx++];
			int length = (int)mark & (int)CodifyMarks.MaskSize;

			if ((mark & (byte)(CodifyMarks.Mf)) != (byte)0)
			{
				// Is a closing mark... we have to close n pendings nodes from the list.
				ProcessClosingMark(length);
				return;
			}
			// Is a text mark. A text mark can be also an init mark (all init marks are text).
			if ((mark & (byte)(CodifyMarks.Mt)) != (byte)0)
			{
				// Is a init mark -> Get the length.
				bool bInitMark = ((mark & (byte)(CodifyMarks.Mi)) != (byte)0);
				ProcessTextMark (length, bInitMark);
				return;
			}
			if ((mark & (byte)(CodifyMarks.Mn)) != (byte)0)
			{
				// Is a numeric Mark
				ProcessNumericMark (length); 
			}
		}

		/// <summary>
		/// Process a (previosuly read) numeric mark
		/// </summary>
		/// <param name="size"># bytes containing the number</param>
		protected void ProcessNumericMark (int size)
		{
			// Get the size first bytes
			for (int i=0;i<size;i++)
			{
				_ulongBytes[i] = _buffer[_idx++];
			}
			// Rest of bytes set to 0
			for (int i=size;i<8;i++)					// 8 == sizeof(ulong)
			{
				_ulongBytes[i] = 0;
			}
			// Gets the ulong value of _ulongBytes.
			MemoryStream ms = new MemoryStream (_ulongBytes);
			BinaryReader br = new BinaryReader (ms);
			ulong uvalue = br.ReadUInt64 ();
			_decoded.Append (uvalue);
		}

		/// <summary>
		/// Processes a (previously read) close mark
		/// </summary>
		/// <param name="ntags"># tags to close (MUST be previously open)</param>
		protected void ProcessClosingMark (int ntags)
		{
			for (int i=0; i< ntags; i++)
			{
				string stag = (string)_openTags.Pop();
				// stag can contain spaces (because attributes are processed LIKE name of the node).
				int indexspace = stag.IndexOf (' ') ;
				if (indexspace!=-1)
				{
					stag = stag.Substring (0, indexspace);
				}
				_decoded.Append ("</");
				_decoded.Append (stag);
				_decoded.Append ('>');
			}
		}

		/// <summary>
		/// Processes a (previously read) text mark. 
		/// </summary>
		/// <param name="size">Size of the text</param>
		/// <param name="bWasInitMark">If true the mark is a Init Mark (so < and > must be appended)</param>
		protected void ProcessTextMark(int size, bool bWasInitMark)
		{
			// We have the size, so just get the size next items of buffer
			if (bWasInitMark) _decoded.Append ('<');
			string sname = null;
			
			if (size == MAX_SIZE_PER_MT)
			{
				// If size is 16 we have to read the extra bytes containing more size
				size = MAX_SIZE_PER_MT -1;				// A Mt(16) does not indicates 16 of length. Indicates 15 plus extra bytes.
				int nsizeblock = 0;
				do
				{
					nsizeblock = _buffer[_idx++];
					size+=nsizeblock;
					if (nsizeblock == 0) size+=ZERO_IN_EXTRA_LENGTH_MEANS;
				} while (nsizeblock==0);
			}
			
			char[] charval = new char [size];

			for (int i=0; i< size; i++)
			{
				charval [i] = (char)_buffer[_idx++];
			}
			sname = new string(charval);
			_decoded.Append (sname);
			if (bWasInitMark) 
			{
				_decoded.Append('>');
				_openTags.Push (sname);
			}
		}

		#endregion
	}

	/// <summary>
	/// That class implements the "Celes codify method" as described in [INF][CFE]XMLCodif.doc.
	/// </summary>
	public class OPSCelesCodifier : EndPointSinkAdapter
	{
		private OPSCelesCodificador _codificador;
		private OPSCelesDecodificador _decodificador;

		/// <summary>
		/// Codifies the specified data with the "Celes codify method".
		/// </summary>
		/// <param name="data">Data to codify (MUST be an XML string)</param>
		/// <returns>A newly created array of bytes with data codified</returns>
		public override byte[] CodifyData(string data)
		{
			return _codificador.CodifyData (data);
		}

		/// <summary>
		/// Decodify the specified data (that has been previosuly codified with OPSCelesCodifier::CodifyData())
		/// </summary>
		/// <param name="codifiedData">Array of bytes with data to decodify</param>
		/// <param name="offset">Index inside codifiedData where codified data starts</param>
		/// <param name="length">Size (in bytes) of the codified data</param>
		/// <returns>string with uncodified data</returns>
		public override string DecodifyData(byte[] codifiedData, int offset, int length)
		{
			return _decodificador.DecodifyData(codifiedData,offset,length);
		}
		
		public OPSCelesCodifier()
		{
			_codificador = new OPSCelesCodificador();
			_decodificador = new OPSCelesDecodificador();
		}

	}
	
}
