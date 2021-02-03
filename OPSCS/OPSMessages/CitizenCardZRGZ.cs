using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;



//namespace OPS.Comm.Becs.CitizenCardZRGZ
namespace OPS.Comm.Becs.Messages
{

	public enum TagName
	{
		Resultado,
		MensajeID,
		DispositivoID,
		NumeroOperacionID,
		NumeroOperacionAnulaID,
		TarjetaID,
		BloqueTarjeta100,
		BloqueTarjeta101,
		BloqueTarjeta102,
		BloqueTarjeta110,
		BloqueTarjeta111,
		BloqueTarjeta112,
		BloqueTarjeta120,
		BloqueTarjeta121,
		BloqueTarjeta122,
		BloqueTarjeta130,
		BloqueTarjeta131,
		BloqueTarjeta132,
		BloqueTarjeta140,
		BloqueTarjeta141,
		BloqueTarjeta142,
		BloqueTarjeta150,
		BloqueTarjeta151,
		BloqueTarjeta152,
		BloqueTarjeta160,
		BloqueTarjeta161,
		BloqueTarjeta162,
		BloqueTarjeta170,
		BloqueTarjeta171,
		BloqueTarjeta172,
		BloqueTarjeta180,
		BloqueTarjeta181,
		BloqueTarjeta182,
		BloqueTarjeta190,
		BloqueTarjeta191,
		BloqueTarjeta192,
		BloqueTarjeta200,
		BloqueTarjeta201,
		BloqueTarjeta202,
		BloqueTarjeta210,
		BloqueTarjeta211,
		BloqueTarjeta212,
		BloqueTarjeta220,
		BloqueTarjeta221,
		BloqueTarjeta222,
		BloqueTarjeta230,
		BloqueTarjeta231,
		BloqueTarjeta232,
		BloqueTarjeta240,
		BloqueTarjeta241,
		BloqueTarjeta242,
		BloqueTarjeta250,
		BloqueTarjeta251,
		BloqueTarjeta252,
		Importe,
		IndiceDescuento,
		Mensaje,
		FechaHora,
		Actualizacion,
		Saldo

	};

	public enum DataType
	{
		MSG_BOOL, // 0x00 or 0xFF
		MSG_NUMERIC_HEXA,
		MSG_NUMERIC_DEC,
		MSG_TEXT,
		MSG_DATE_TIME
	};

	// Associa los tipos de tags con su ID y longitud
	public class TagHelper
	{
		private  TagName[] BlockTagList = new TagName[48] 
		{
			TagName.BloqueTarjeta100, 
			TagName.BloqueTarjeta101,
			TagName.BloqueTarjeta102,
			TagName.BloqueTarjeta110,
			TagName.BloqueTarjeta111,
			TagName.BloqueTarjeta112,
			TagName.BloqueTarjeta120,
			TagName.BloqueTarjeta121,
			TagName.BloqueTarjeta122,
			TagName.BloqueTarjeta130,
			TagName.BloqueTarjeta131,
			TagName.BloqueTarjeta132,
			TagName.BloqueTarjeta140,
			TagName.BloqueTarjeta141,
			TagName.BloqueTarjeta142,
			TagName.BloqueTarjeta150,
			TagName.BloqueTarjeta151,
			TagName.BloqueTarjeta152,
			TagName.BloqueTarjeta160,
			TagName.BloqueTarjeta161,
			TagName.BloqueTarjeta162,
			TagName.BloqueTarjeta170,
			TagName.BloqueTarjeta171,
			TagName.BloqueTarjeta172,
			TagName.BloqueTarjeta180,
			TagName.BloqueTarjeta181,
			TagName.BloqueTarjeta182,
			TagName.BloqueTarjeta190,
			TagName.BloqueTarjeta191,
			TagName.BloqueTarjeta192,
			TagName.BloqueTarjeta200,
			TagName.BloqueTarjeta201,
			TagName.BloqueTarjeta202,
			TagName.BloqueTarjeta210,
			TagName.BloqueTarjeta211,
			TagName.BloqueTarjeta212,
			TagName.BloqueTarjeta220,
			TagName.BloqueTarjeta221,
			TagName.BloqueTarjeta222,
			TagName.BloqueTarjeta230,
			TagName.BloqueTarjeta231,
			TagName.BloqueTarjeta232,
			TagName.BloqueTarjeta240,
			TagName.BloqueTarjeta241,
			TagName.BloqueTarjeta242,
			TagName.BloqueTarjeta250,
			TagName.BloqueTarjeta251,
			TagName.BloqueTarjeta252
		};

		public TagName[] GetTagBlockArray()
		{
			return BlockTagList;
		}
		private Hashtable htBlockToNum = null;
		private Hashtable htNameID = null;
		private Hashtable htIDName = null;
		private Hashtable htNameLength = null;
		private Hashtable htNameDataType = null;

		#region TAG_ID
		private const string Resultado_TAG_ID = "0x23";
		private const string MensajeID_TAG_ID = "0x28";
		private const string DispositivoID_TAG_ID = "0x33";
		private const string NumeroOperacionID_TAG_ID = "0x35";
		private const string NumeroOperacionAnulaID_TAG_ID = "0x36";
		private const string TarjetaID_TAG_ID = "0x37";
		private const string BloqueTarjeta100_TAG_ID = "0x46";
		private const string BloqueTarjeta101_TAG_ID = "0x47";
		private const string BloqueTarjeta102_TAG_ID = "0x48";
		private const string BloqueTarjeta110_TAG_ID = "0x49";
		private const string BloqueTarjeta111_TAG_ID = "0x4A";
		private const string BloqueTarjeta112_TAG_ID = "0x4B";
		private const string BloqueTarjeta120_TAG_ID = "0x4C";
		private const string BloqueTarjeta121_TAG_ID = "0x4D";
		private const string BloqueTarjeta122_TAG_ID = "0x4E";
		private const string BloqueTarjeta130_TAG_ID = "0x4F";
		private const string BloqueTarjeta131_TAG_ID = "0x50";
		private const string BloqueTarjeta132_TAG_ID = "0x51";
		private const string BloqueTarjeta140_TAG_ID = "0x52";
		private const string BloqueTarjeta141_TAG_ID = "0x53";
		private const string BloqueTarjeta142_TAG_ID = "0x54";
		private const string BloqueTarjeta150_TAG_ID = "0x55";
		private const string BloqueTarjeta151_TAG_ID = "0x56";
		private const string BloqueTarjeta152_TAG_ID = "0x57";
		private const string BloqueTarjeta160_TAG_ID = "0x58";
		private const string BloqueTarjeta161_TAG_ID = "0x59";
		private const string BloqueTarjeta162_TAG_ID = "0x5A";
		private const string BloqueTarjeta170_TAG_ID = "0x5B";
		private const string BloqueTarjeta171_TAG_ID = "0x5C";
		private const string BloqueTarjeta172_TAG_ID = "0x5D";
		private const string BloqueTarjeta180_TAG_ID = "0x5E";
		private const string BloqueTarjeta181_TAG_ID = "0x5F";
		private const string BloqueTarjeta182_TAG_ID = "0x60";
		private const string BloqueTarjeta190_TAG_ID = "0x61";
		private const string BloqueTarjeta191_TAG_ID = "0x62";
		private const string BloqueTarjeta192_TAG_ID = "0x63";
		private const string BloqueTarjeta200_TAG_ID = "0x64";
		private const string BloqueTarjeta201_TAG_ID = "0x65";
		private const string BloqueTarjeta202_TAG_ID = "0x66";
		private const string BloqueTarjeta210_TAG_ID = "0x67";
		private const string BloqueTarjeta211_TAG_ID = "0x68";
		private const string BloqueTarjeta212_TAG_ID = "0x69";
		private const string BloqueTarjeta220_TAG_ID = "0x6A";
		private const string BloqueTarjeta221_TAG_ID = "0x6B";
		private const string BloqueTarjeta222_TAG_ID = "0x6C";
		private const string BloqueTarjeta230_TAG_ID = "0x6D";
		private const string BloqueTarjeta231_TAG_ID = "0x6E";
		private const string BloqueTarjeta232_TAG_ID = "0x6F";
		private const string BloqueTarjeta240_TAG_ID = "0x70";
		private const string BloqueTarjeta241_TAG_ID = "0x71";
		private const string BloqueTarjeta242_TAG_ID = "0x72";
		private const string BloqueTarjeta250_TAG_ID = "0x73";
		private const string BloqueTarjeta251_TAG_ID = "0x74";
		private const string BloqueTarjeta252_TAG_ID = "0x75";
		private const string Importe_TAG_ID = "0x39";
		private const string IndiceDescuento_TAG_ID = "0x3A";
		private const string Mensaje_TAG_ID = "0x41";
		private const string FechaHora_TAG_ID = "0x42";
		private const string Actualizacion_TAG_ID = "0x44";
		private const string Saldo_TAG_ID = "0x45";
		#endregion

		#region TAG_LEN
		private const int Resultado_TAG_LEN = 2;
		private const int MensajeID_TAG_LEN = 2;
		private const int DispositivoID_TAG_LEN = 10;
		private const int NumeroOperacionID_TAG_LEN = 12;
		private const int NumeroOperacionAnulaID_TAG_LEN = 12;
		private const int TarjetaID_TAG_LEN = 16;
		private const int BloqueTarjeta100_TAG_LEN = 32;
		private const int BloqueTarjeta101_TAG_LEN = 32;
		private const int BloqueTarjeta102_TAG_LEN = 32;
		private const int BloqueTarjeta110_TAG_LEN = 32;
		private const int BloqueTarjeta111_TAG_LEN = 32;
		private const int BloqueTarjeta112_TAG_LEN = 32;
		private const int BloqueTarjeta120_TAG_LEN = 32;
		private const int BloqueTarjeta121_TAG_LEN = 32;
		private const int BloqueTarjeta122_TAG_LEN = 32;
		private const int BloqueTarjeta130_TAG_LEN = 32;
		private const int BloqueTarjeta131_TAG_LEN = 32;
		private const int BloqueTarjeta132_TAG_LEN = 32;
		private const int BloqueTarjeta140_TAG_LEN = 32;
		private const int BloqueTarjeta141_TAG_LEN = 32;
		private const int BloqueTarjeta142_TAG_LEN = 32;
		private const int BloqueTarjeta150_TAG_LEN = 32;
		private const int BloqueTarjeta151_TAG_LEN = 32;
		private const int BloqueTarjeta152_TAG_LEN = 32;
		private const int BloqueTarjeta160_TAG_LEN = 32;
		private const int BloqueTarjeta161_TAG_LEN = 32;
		private const int BloqueTarjeta162_TAG_LEN = 32;
		private const int BloqueTarjeta170_TAG_LEN = 32;
		private const int BloqueTarjeta171_TAG_LEN = 32;
		private const int BloqueTarjeta172_TAG_LEN = 32;
		private const int BloqueTarjeta180_TAG_LEN = 32;
		private const int BloqueTarjeta181_TAG_LEN = 32;
		private const int BloqueTarjeta182_TAG_LEN = 32;
		private const int BloqueTarjeta190_TAG_LEN = 32;
		private const int BloqueTarjeta191_TAG_LEN = 32;
		private const int BloqueTarjeta192_TAG_LEN = 32;
		private const int BloqueTarjeta200_TAG_LEN = 32;
		private const int BloqueTarjeta201_TAG_LEN = 32;
		private const int BloqueTarjeta202_TAG_LEN = 32;
		private const int BloqueTarjeta210_TAG_LEN = 32;
		private const int BloqueTarjeta211_TAG_LEN = 32;
		private const int BloqueTarjeta212_TAG_LEN = 32;
		private const int BloqueTarjeta220_TAG_LEN = 32;
		private const int BloqueTarjeta221_TAG_LEN = 32;
		private const int BloqueTarjeta222_TAG_LEN = 32;
		private const int BloqueTarjeta230_TAG_LEN = 32;
		private const int BloqueTarjeta231_TAG_LEN = 32;
		private const int BloqueTarjeta232_TAG_LEN = 32;
		private const int BloqueTarjeta240_TAG_LEN = 32;
		private const int BloqueTarjeta241_TAG_LEN = 32;
		private const int BloqueTarjeta242_TAG_LEN = 32;
		private const int BloqueTarjeta250_TAG_LEN = 32;
		private const int BloqueTarjeta251_TAG_LEN = 32;
		private const int BloqueTarjeta252_TAG_LEN = 32;
		private const int Importe_TAG_LEN = 12;
		private const int IndiceDescuento_TAG_LEN = 5;
		private const int Mensaje_TAG_LEN = 134;
		private const int FechaHora_TAG_LEN = 15;
		private const int Actualizacion_TAG_LEN = 50;
		private const int Saldo_TAG_LEN = 12;
		#endregion

		#region TAG_DATA_TYPE
		private const DataType Resultado_TAG_DATATYPE = DataType.MSG_BOOL;
		private const DataType MensajeID_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType DispositivoID_TAG_DATATYPE = DataType.MSG_NUMERIC_DEC;
		private const DataType NumeroOperacionID_TAG_DATATYPE = DataType.MSG_NUMERIC_DEC;
		private const DataType NumeroOperacionAnulaID_TAG_DATATYPE = DataType.MSG_NUMERIC_DEC;
		private const DataType TarjetaID_TAG_DATATYPE = DataType.MSG_NUMERIC_DEC;
		private const DataType BloqueTarjeta100_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta101_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta102_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta110_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta111_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta112_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta120_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta121_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta122_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta130_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta131_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta132_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta140_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta141_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta142_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta150_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta151_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta152_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta160_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta161_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta162_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta170_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta171_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta172_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta180_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta181_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta182_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta190_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta191_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta192_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta200_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta201_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta202_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta210_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta211_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta212_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta220_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta221_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta222_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta230_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta231_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta232_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta240_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta241_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta242_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta250_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta251_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType BloqueTarjeta252_TAG_DATATYPE = DataType.MSG_NUMERIC_HEXA;
		private const DataType Importe_TAG_DATATYPE = DataType.MSG_NUMERIC_DEC;
		private const DataType IndiceDescuento_TAG_DATATYPE = DataType.MSG_NUMERIC_DEC;
		private const DataType Mensaje_TAG_DATATYPE = DataType.MSG_TEXT;
		private const DataType FechaHora_TAG_DATATYPE = DataType.MSG_DATE_TIME;
		private const DataType Actualizacion_TAG_DATATYPE = DataType.MSG_TEXT;
		private const DataType Saldo_TAG_DATATYPE = DataType.MSG_NUMERIC_DEC;
		#endregion

		public TagHelper()
		{
			htIDName = new Hashtable();

			#region INIT_ID_NAME
			htIDName.Add(Resultado_TAG_ID               , TagName.Resultado);
			htIDName.Add(MensajeID_TAG_ID               , TagName.MensajeID);
			htIDName.Add(DispositivoID_TAG_ID           , TagName.DispositivoID);
			htIDName.Add(NumeroOperacionID_TAG_ID       , TagName.NumeroOperacionID);
			htIDName.Add(NumeroOperacionAnulaID_TAG_ID  , TagName.NumeroOperacionAnulaID);
			htIDName.Add(TarjetaID_TAG_ID               , TagName.TarjetaID);
			htIDName.Add(BloqueTarjeta100_TAG_ID, TagName.BloqueTarjeta100);
			htIDName.Add(BloqueTarjeta101_TAG_ID, TagName.BloqueTarjeta101);
			htIDName.Add(BloqueTarjeta102_TAG_ID, TagName.BloqueTarjeta102);
			htIDName.Add(BloqueTarjeta110_TAG_ID, TagName.BloqueTarjeta110);
			htIDName.Add(BloqueTarjeta111_TAG_ID, TagName.BloqueTarjeta111);
			htIDName.Add(BloqueTarjeta112_TAG_ID, TagName.BloqueTarjeta112);
			htIDName.Add(BloqueTarjeta120_TAG_ID, TagName.BloqueTarjeta120);
			htIDName.Add(BloqueTarjeta121_TAG_ID, TagName.BloqueTarjeta121);
			htIDName.Add(BloqueTarjeta122_TAG_ID, TagName.BloqueTarjeta122);
			htIDName.Add(BloqueTarjeta130_TAG_ID, TagName.BloqueTarjeta130);
			htIDName.Add(BloqueTarjeta131_TAG_ID, TagName.BloqueTarjeta131);
			htIDName.Add(BloqueTarjeta132_TAG_ID, TagName.BloqueTarjeta132);
			htIDName.Add(BloqueTarjeta140_TAG_ID, TagName.BloqueTarjeta140);
			htIDName.Add(BloqueTarjeta141_TAG_ID, TagName.BloqueTarjeta141);
			htIDName.Add(BloqueTarjeta142_TAG_ID, TagName.BloqueTarjeta142);
			htIDName.Add(BloqueTarjeta150_TAG_ID, TagName.BloqueTarjeta150);
			htIDName.Add(BloqueTarjeta151_TAG_ID, TagName.BloqueTarjeta151);
			htIDName.Add(BloqueTarjeta152_TAG_ID, TagName.BloqueTarjeta152);
			htIDName.Add(BloqueTarjeta160_TAG_ID, TagName.BloqueTarjeta160);
			htIDName.Add(BloqueTarjeta161_TAG_ID, TagName.BloqueTarjeta161);
			htIDName.Add(BloqueTarjeta162_TAG_ID, TagName.BloqueTarjeta162);
			htIDName.Add(BloqueTarjeta170_TAG_ID, TagName.BloqueTarjeta170);
			htIDName.Add(BloqueTarjeta171_TAG_ID, TagName.BloqueTarjeta171);
			htIDName.Add(BloqueTarjeta172_TAG_ID, TagName.BloqueTarjeta172);
			htIDName.Add(BloqueTarjeta180_TAG_ID, TagName.BloqueTarjeta180);
			htIDName.Add(BloqueTarjeta181_TAG_ID, TagName.BloqueTarjeta181);
			htIDName.Add(BloqueTarjeta182_TAG_ID, TagName.BloqueTarjeta182);
			htIDName.Add(BloqueTarjeta190_TAG_ID, TagName.BloqueTarjeta190);
			htIDName.Add(BloqueTarjeta191_TAG_ID, TagName.BloqueTarjeta191);
			htIDName.Add(BloqueTarjeta192_TAG_ID, TagName.BloqueTarjeta192);
			htIDName.Add(BloqueTarjeta200_TAG_ID, TagName.BloqueTarjeta200);
			htIDName.Add(BloqueTarjeta201_TAG_ID, TagName.BloqueTarjeta201);
			htIDName.Add(BloqueTarjeta202_TAG_ID, TagName.BloqueTarjeta202);
			htIDName.Add(BloqueTarjeta210_TAG_ID, TagName.BloqueTarjeta210);
			htIDName.Add(BloqueTarjeta211_TAG_ID, TagName.BloqueTarjeta211);
			htIDName.Add(BloqueTarjeta212_TAG_ID, TagName.BloqueTarjeta212);
			htIDName.Add(BloqueTarjeta220_TAG_ID, TagName.BloqueTarjeta220);
			htIDName.Add(BloqueTarjeta221_TAG_ID, TagName.BloqueTarjeta221);
			htIDName.Add(BloqueTarjeta222_TAG_ID, TagName.BloqueTarjeta222);
			htIDName.Add(BloqueTarjeta230_TAG_ID, TagName.BloqueTarjeta230);
			htIDName.Add(BloqueTarjeta231_TAG_ID, TagName.BloqueTarjeta231);
			htIDName.Add(BloqueTarjeta232_TAG_ID, TagName.BloqueTarjeta232);
			htIDName.Add(BloqueTarjeta240_TAG_ID, TagName.BloqueTarjeta240);
			htIDName.Add(BloqueTarjeta241_TAG_ID, TagName.BloqueTarjeta241);
			htIDName.Add(BloqueTarjeta242_TAG_ID, TagName.BloqueTarjeta242);
			htIDName.Add(BloqueTarjeta250_TAG_ID, TagName.BloqueTarjeta250);
			htIDName.Add(BloqueTarjeta251_TAG_ID, TagName.BloqueTarjeta251);
			htIDName.Add(BloqueTarjeta252_TAG_ID, TagName.BloqueTarjeta252);
			htIDName.Add(Importe_TAG_ID        , TagName.Importe             );
			htIDName.Add(IndiceDescuento_TAG_ID, TagName.IndiceDescuento     );
			htIDName.Add(Mensaje_TAG_ID        , TagName.Mensaje             );
			htIDName.Add(FechaHora_TAG_ID      , TagName.FechaHora           );
			htIDName.Add(Actualizacion_TAG_ID  , TagName.Actualizacion       );
			htIDName.Add(Saldo_TAG_ID          , TagName.Saldo               );
			#endregion

			htNameID = new Hashtable();

			#region INIT_NAME_ID
			htNameID.Add(TagName.Resultado              , Resultado_TAG_ID);
			htNameID.Add(TagName.MensajeID              , MensajeID_TAG_ID);
			htNameID.Add(TagName.DispositivoID          , DispositivoID_TAG_ID);
			htNameID.Add(TagName.NumeroOperacionID      , NumeroOperacionID_TAG_ID);
			htNameID.Add(TagName.NumeroOperacionAnulaID , NumeroOperacionAnulaID_TAG_ID);
			htNameID.Add(TagName.TarjetaID              , TarjetaID_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta100, BloqueTarjeta100_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta101, BloqueTarjeta101_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta102, BloqueTarjeta102_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta110, BloqueTarjeta110_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta111, BloqueTarjeta111_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta112, BloqueTarjeta112_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta120, BloqueTarjeta120_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta121, BloqueTarjeta121_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta122, BloqueTarjeta122_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta130, BloqueTarjeta130_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta131, BloqueTarjeta131_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta132, BloqueTarjeta132_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta140, BloqueTarjeta140_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta141, BloqueTarjeta141_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta142, BloqueTarjeta142_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta150, BloqueTarjeta150_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta151, BloqueTarjeta151_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta152, BloqueTarjeta152_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta160, BloqueTarjeta160_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta161, BloqueTarjeta161_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta162, BloqueTarjeta162_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta170, BloqueTarjeta170_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta171, BloqueTarjeta171_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta172, BloqueTarjeta172_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta180, BloqueTarjeta180_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta181, BloqueTarjeta181_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta182, BloqueTarjeta182_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta190, BloqueTarjeta190_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta191, BloqueTarjeta191_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta192, BloqueTarjeta192_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta200, BloqueTarjeta200_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta201, BloqueTarjeta201_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta202, BloqueTarjeta202_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta210, BloqueTarjeta210_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta211, BloqueTarjeta211_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta212, BloqueTarjeta212_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta220, BloqueTarjeta220_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta221, BloqueTarjeta221_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta222, BloqueTarjeta222_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta230, BloqueTarjeta230_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta231, BloqueTarjeta231_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta232, BloqueTarjeta232_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta240, BloqueTarjeta240_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta241, BloqueTarjeta241_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta242, BloqueTarjeta242_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta250, BloqueTarjeta250_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta251, BloqueTarjeta251_TAG_ID);
			htNameID.Add(TagName.BloqueTarjeta252, BloqueTarjeta252_TAG_ID);
			htNameID.Add(TagName.Importe, Importe_TAG_ID);
			htNameID.Add(TagName.IndiceDescuento, IndiceDescuento_TAG_ID);
			htNameID.Add(TagName.Mensaje, Mensaje_TAG_ID);
			htNameID.Add(TagName.FechaHora, FechaHora_TAG_ID);
			htNameID.Add(TagName.Actualizacion, Actualizacion_TAG_ID);
			htNameID.Add(TagName.Saldo, Saldo_TAG_ID);
			#endregion

			htNameLength = new Hashtable();

			#region INIT_NAME_LEN
			htNameLength.Add(TagName.Resultado, Resultado_TAG_LEN);
			htNameLength.Add(TagName.MensajeID, MensajeID_TAG_LEN);
			htNameLength.Add(TagName.DispositivoID, DispositivoID_TAG_LEN);
			htNameLength.Add(TagName.NumeroOperacionID, NumeroOperacionID_TAG_LEN);
			htNameLength.Add(TagName.NumeroOperacionAnulaID, NumeroOperacionAnulaID_TAG_LEN);
			htNameLength.Add(TagName.TarjetaID, TarjetaID_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta100, BloqueTarjeta100_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta101, BloqueTarjeta101_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta102, BloqueTarjeta102_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta110, BloqueTarjeta110_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta111, BloqueTarjeta111_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta112, BloqueTarjeta112_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta120, BloqueTarjeta120_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta121, BloqueTarjeta121_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta122, BloqueTarjeta122_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta130, BloqueTarjeta130_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta131, BloqueTarjeta131_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta132, BloqueTarjeta132_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta140, BloqueTarjeta140_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta141, BloqueTarjeta141_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta142, BloqueTarjeta142_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta150, BloqueTarjeta150_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta151, BloqueTarjeta151_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta152, BloqueTarjeta152_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta160, BloqueTarjeta160_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta161, BloqueTarjeta161_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta162, BloqueTarjeta162_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta170, BloqueTarjeta170_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta171, BloqueTarjeta171_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta172, BloqueTarjeta172_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta180, BloqueTarjeta180_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta181, BloqueTarjeta181_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta182, BloqueTarjeta182_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta190, BloqueTarjeta190_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta191, BloqueTarjeta191_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta192, BloqueTarjeta192_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta200, BloqueTarjeta200_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta201, BloqueTarjeta201_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta202, BloqueTarjeta202_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta210, BloqueTarjeta210_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta211, BloqueTarjeta211_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta212, BloqueTarjeta212_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta220, BloqueTarjeta220_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta221, BloqueTarjeta221_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta222, BloqueTarjeta222_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta230, BloqueTarjeta230_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta231, BloqueTarjeta231_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta232, BloqueTarjeta232_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta240, BloqueTarjeta240_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta241, BloqueTarjeta241_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta242, BloqueTarjeta242_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta250, BloqueTarjeta250_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta251, BloqueTarjeta251_TAG_LEN);
			htNameLength.Add(TagName.BloqueTarjeta252, BloqueTarjeta252_TAG_LEN);
			htNameLength.Add(TagName.Importe, Importe_TAG_LEN);
			htNameLength.Add(TagName.IndiceDescuento, IndiceDescuento_TAG_LEN);
			htNameLength.Add(TagName.Mensaje, Mensaje_TAG_LEN);
			htNameLength.Add(TagName.FechaHora, FechaHora_TAG_LEN);
			htNameLength.Add(TagName.Actualizacion, Actualizacion_TAG_LEN);
			htNameLength.Add(TagName.Saldo, Saldo_TAG_LEN);
			#endregion

			htNameDataType = new Hashtable();

			#region INIT_NAME_DATATYPE
			htNameDataType.Add(TagName.Resultado, Resultado_TAG_DATATYPE);
			htNameDataType.Add(TagName.MensajeID, MensajeID_TAG_DATATYPE);
			htNameDataType.Add(TagName.DispositivoID, DispositivoID_TAG_DATATYPE);
			htNameDataType.Add(TagName.NumeroOperacionID, NumeroOperacionID_TAG_DATATYPE);
			htNameDataType.Add(TagName.NumeroOperacionAnulaID, NumeroOperacionAnulaID_TAG_DATATYPE);
			htNameDataType.Add(TagName.TarjetaID, TarjetaID_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta100, BloqueTarjeta100_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta101, BloqueTarjeta101_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta102, BloqueTarjeta102_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta110, BloqueTarjeta110_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta111, BloqueTarjeta111_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta112, BloqueTarjeta112_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta120, BloqueTarjeta120_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta121, BloqueTarjeta121_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta122, BloqueTarjeta122_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta130, BloqueTarjeta130_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta131, BloqueTarjeta131_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta132, BloqueTarjeta132_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta140, BloqueTarjeta140_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta141, BloqueTarjeta141_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta142, BloqueTarjeta142_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta150, BloqueTarjeta150_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta151, BloqueTarjeta151_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta152, BloqueTarjeta152_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta160, BloqueTarjeta160_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta161, BloqueTarjeta161_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta162, BloqueTarjeta162_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta170, BloqueTarjeta170_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta171, BloqueTarjeta171_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta172, BloqueTarjeta172_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta180, BloqueTarjeta180_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta181, BloqueTarjeta181_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta182, BloqueTarjeta182_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta190, BloqueTarjeta190_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta191, BloqueTarjeta191_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta192, BloqueTarjeta192_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta200, BloqueTarjeta200_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta201, BloqueTarjeta201_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta202, BloqueTarjeta202_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta210, BloqueTarjeta210_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta211, BloqueTarjeta211_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta212, BloqueTarjeta212_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta220, BloqueTarjeta220_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta221, BloqueTarjeta221_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta222, BloqueTarjeta222_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta230, BloqueTarjeta230_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta231, BloqueTarjeta231_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta232, BloqueTarjeta232_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta240, BloqueTarjeta240_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta241, BloqueTarjeta241_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta242, BloqueTarjeta242_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta250, BloqueTarjeta250_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta251, BloqueTarjeta251_TAG_DATATYPE);
			htNameDataType.Add(TagName.BloqueTarjeta252, BloqueTarjeta252_TAG_DATATYPE);
			htNameDataType.Add(TagName.Importe, Importe_TAG_DATATYPE);
			htNameDataType.Add(TagName.IndiceDescuento, IndiceDescuento_TAG_DATATYPE);
			htNameDataType.Add(TagName.Mensaje, Mensaje_TAG_DATATYPE);
			htNameDataType.Add(TagName.FechaHora, FechaHora_TAG_DATATYPE);
			htNameDataType.Add(TagName.Actualizacion, Actualizacion_TAG_DATATYPE);
			htNameDataType.Add(TagName.Saldo, Saldo_TAG_DATATYPE);
			#endregion

			htBlockToNum = new Hashtable();

			#region INIT_BLOCK_TO_NUM
			htBlockToNum.Add(TagName.BloqueTarjeta100, "100");
			htBlockToNum.Add(TagName.BloqueTarjeta101, "101");
			htBlockToNum.Add(TagName.BloqueTarjeta102, "102");
			htBlockToNum.Add(TagName.BloqueTarjeta110, "110");
			htBlockToNum.Add(TagName.BloqueTarjeta111, "111");
			htBlockToNum.Add(TagName.BloqueTarjeta112, "112");
			htBlockToNum.Add(TagName.BloqueTarjeta120, "120");
			htBlockToNum.Add(TagName.BloqueTarjeta121, "121");
			htBlockToNum.Add(TagName.BloqueTarjeta122, "122");
			htBlockToNum.Add(TagName.BloqueTarjeta130, "130");
			htBlockToNum.Add(TagName.BloqueTarjeta131, "131");
			htBlockToNum.Add(TagName.BloqueTarjeta132, "132");
			htBlockToNum.Add(TagName.BloqueTarjeta140, "140");
			htBlockToNum.Add(TagName.BloqueTarjeta141, "141");
			htBlockToNum.Add(TagName.BloqueTarjeta142, "142");
			htBlockToNum.Add(TagName.BloqueTarjeta150, "150");
			htBlockToNum.Add(TagName.BloqueTarjeta151, "151");
			htBlockToNum.Add(TagName.BloqueTarjeta152, "152");
			htBlockToNum.Add(TagName.BloqueTarjeta160, "160");
			htBlockToNum.Add(TagName.BloqueTarjeta161, "161");
			htBlockToNum.Add(TagName.BloqueTarjeta162, "162");
			htBlockToNum.Add(TagName.BloqueTarjeta170, "170");
			htBlockToNum.Add(TagName.BloqueTarjeta171, "171");
			htBlockToNum.Add(TagName.BloqueTarjeta172, "172");
			htBlockToNum.Add(TagName.BloqueTarjeta180, "180");
			htBlockToNum.Add(TagName.BloqueTarjeta181, "181");
			htBlockToNum.Add(TagName.BloqueTarjeta182, "182");
			htBlockToNum.Add(TagName.BloqueTarjeta190, "190");
			htBlockToNum.Add(TagName.BloqueTarjeta191, "191");
			htBlockToNum.Add(TagName.BloqueTarjeta192, "192");
			htBlockToNum.Add(TagName.BloqueTarjeta200, "200");
			htBlockToNum.Add(TagName.BloqueTarjeta201, "201");
			htBlockToNum.Add(TagName.BloqueTarjeta202, "202");
			htBlockToNum.Add(TagName.BloqueTarjeta210, "210");
			htBlockToNum.Add(TagName.BloqueTarjeta211, "211");
			htBlockToNum.Add(TagName.BloqueTarjeta212, "212");
			htBlockToNum.Add(TagName.BloqueTarjeta220, "220");
			htBlockToNum.Add(TagName.BloqueTarjeta221, "221");
			htBlockToNum.Add(TagName.BloqueTarjeta222, "222");
			htBlockToNum.Add(TagName.BloqueTarjeta230, "230");
			htBlockToNum.Add(TagName.BloqueTarjeta231, "231");
			htBlockToNum.Add(TagName.BloqueTarjeta232, "232");
			htBlockToNum.Add(TagName.BloqueTarjeta240, "240");
			htBlockToNum.Add(TagName.BloqueTarjeta241, "241");
			htBlockToNum.Add(TagName.BloqueTarjeta242, "242");
			htBlockToNum.Add(TagName.BloqueTarjeta250, "250");
			htBlockToNum.Add(TagName.BloqueTarjeta251, "251");
			htBlockToNum.Add(TagName.BloqueTarjeta252, "252");
			#endregion
		}


		public string GetBlockNum( TagName tag )
		{
			return (string)htBlockToNum[tag];
		}

		public TagName GetTagNameById(string id)
		{

			return (TagName)htIDName[id];
		}

		public string GetTagId(TagName tagName)
		{
			string sId = String.Empty;
			try
			{
				if (htNameID != null)
				{
					sId = (string)htNameID[tagName];
				}
			}
			catch (Exception e)
			{
			}
			return sId;
		}

		public int GetTagLength(TagName tagName)
		{
			int iLength = 0;
			try
			{
				if (htNameLength != null)
				{
					iLength = (int)htNameLength[tagName];
				}
			}
			catch (Exception e)
			{
			}
			return iLength;
		}

		public DataType GetDataType(TagName tagName)
		{
			return (DataType)htNameDataType[tagName];
		}
	}


	public class Tag
	{

		private TagName Name;
		private string sTagId = String.Empty;
		private int iCharLength = 0;
		private DataType dataType;
		private string sTagValue = String.Empty; // Contiene el valor concatenable en el paquete
		private TagHelper helper = null;

		public TagName NAME
		{
			get { return Name; }
		}
		public string GetTLV()
		{
			string sId = sTagId.Substring(2, 2);
			string sLen = iCharLength.ToString("X" + 2);
			return sId + sLen + sTagValue;
		}


		public void SetTagValueFromString(string sVal)
		{
			sTagValue = sVal;
		}
	
		public bool SetTagValue(bool bVal)
		{
			bool bRslt = false;

			switch (dataType)
			{
				case DataType.MSG_BOOL:
				{
					if (bVal == true)
					{
						sTagValue = "FF";
					}
					else
					{
						sTagValue = "00";
					}
					break;
				}
				default:
				{
					bRslt = false;
					break;
				}
			}
			return bRslt;
		}

		public bool SetTagValue(string sVal)
		{
			bool bRslt = false;

			switch (dataType)
			{
				case DataType.MSG_TEXT:
				{
					if (sVal.Length <= iCharLength)
					{
						sTagValue = sVal.PadLeft(iCharLength, ' ');
					}
					else
					{
						bRslt = false;
					}
					break;
				}
				case DataType.MSG_NUMERIC_HEXA:
				{
					if (sVal.Length <= iCharLength)
					{
						sTagValue = sVal;
					}
					else
					{
						bRslt = false;
					}
					break;
				}
				default:
				{
					bRslt = false;
					break;
				}
			}

			return bRslt;
		}

		public bool SetTagValue(int iVal)
		{
			bool bRslt = false;

			switch (dataType)
			{
				case DataType.MSG_NUMERIC_DEC:
				{
					if (iVal.ToString().Length <= iCharLength)
					{
						if(iVal < 0)
						{
							sTagValue = "-";

							sTagValue += Math.Abs(iVal).ToString().PadLeft(iCharLength -1, '0');
						}
						else
						{
							sTagValue = iVal.ToString().PadLeft(iCharLength, '0');
						}
					}
					else
					{
						bRslt = false;
					}
					break;
				}
				case DataType.MSG_NUMERIC_HEXA:
				{
					sTagValue = iVal.ToString("X" + iCharLength);
					break;
				}
				default:
				{
					bRslt = false;
					break;
				}
			}
			return bRslt;
		}

		public bool SetTagValue(uint iVal)
		{
			bool bRslt = false;

			switch (dataType)
			{
				case DataType.MSG_NUMERIC_DEC:
				{
					if (iVal.ToString().Length <= iCharLength)
					{
						sTagValue = iVal.ToString().PadLeft(iCharLength, '0');
					}
					else
					{
						bRslt = false;
					}
					break;
				}
				case DataType.MSG_NUMERIC_HEXA:
				{
					sTagValue = iVal.ToString("X" + iCharLength);
					break;
				}
				default:
				{
					bRslt = false;
					break;
				}
			}
			return bRslt;
		}

		
		public string TAG_VALUE
		{
			get { return sTagValue; }
		}

		public Tag(TagName tagName, TagHelper hlpr)
		{
			Name = tagName;
			helper = hlpr;

			sTagId = helper.GetTagId(tagName);
			iCharLength = helper.GetTagLength(tagName);
			dataType = helper.GetDataType(tagName);

			if (sTagId == String.Empty || iCharLength == 0)
			{
				// El tag està mal definido a nivel de protocolo
			}
		}

	}

	public enum MESSAGE_TYPE
	{
		MSG_OPEINS,
		MSG_OPEINS_RESP_OK,
		MSG_OPEINS_RESP_ERR,
		MSG_OPENOTI,
		MSG_OPENOTI_RESP_OK,
		MSG_OPENOTI_RESP_ERR,
		MSG_RECINS,
		MSG_RECINS_RESP_OK,
		MSG_RECINS_RESP_ERR,
		MSG_RECNOTI,
		MSG_RECNOTI_RESP_OK,
		MSG_RECNOTI_RESP_ERR,
		MSG_TARSAL,
		MSG_TARSAL_RESP_OK,
		MSG_TARSAL_RESP_ERR
	};

	public class MessageHelper
	{
		private const int MSG_OPEINS_ID = 0x1E;
		private const int MSG_OPEINS_ID_RESP_OK = 0x01;
		private const int MSG_OPEINS_ID_RESP_ERR = 0x02;

		private const int MSG_OPENOTI_ID = 0x22;
		private const int MSG_OPENOTI_ID_RESP_OK = 0x03;
		private const int MSG_OPENOTI_ID_RESP_ERR = 0x04;

		private const int MSG_RECINS_ID = 0x1C;
		private const int MSG_RECINS_ID_RESP_OK = 0x05;
		private const int MSG_RECINS_ID_RESP_ERR = 0x06;

		private const int MSG_RECNOTI_ID = 0x23;
		private const int MSG_RECNOTI_ID_RESP_OK = 0x07;
		private const int MSG_RECNOTI_ID_RESP_ERR = 0x08;

		private const int MSG_TARSAL_ID = 0x25;
		private const int MSG_TARSAL_ID_RESP_OK = 0x09;
		private const int MSG_TARSAL_ID_RESP_ERR = 0x0A;

		private Hashtable htMsgID = null;

		public MessageHelper()
		{
			htMsgID = new Hashtable();
			htMsgID.Add(MESSAGE_TYPE.MSG_OPEINS, MSG_OPEINS_ID);
			htMsgID.Add(MESSAGE_TYPE.MSG_OPEINS_RESP_OK, MSG_OPEINS_ID_RESP_OK);
			htMsgID.Add(MESSAGE_TYPE.MSG_OPEINS_RESP_ERR, MSG_OPEINS_ID_RESP_ERR);

			htMsgID.Add(MESSAGE_TYPE.MSG_OPENOTI, MSG_OPENOTI_ID);
			htMsgID.Add(MESSAGE_TYPE.MSG_OPENOTI_RESP_OK, MSG_OPENOTI_ID_RESP_OK );
			htMsgID.Add(MESSAGE_TYPE.MSG_OPENOTI_RESP_ERR, MSG_OPENOTI_ID_RESP_ERR );

			htMsgID.Add(MESSAGE_TYPE.MSG_RECINS, MSG_RECINS_ID);
			htMsgID.Add(MESSAGE_TYPE.MSG_RECINS_RESP_OK, MSG_RECINS_ID);
			htMsgID.Add(MESSAGE_TYPE.MSG_RECINS_RESP_ERR, MSG_RECINS_ID);

			htMsgID.Add(MESSAGE_TYPE.MSG_RECNOTI, MSG_RECNOTI_ID);
			htMsgID.Add(MESSAGE_TYPE.MSG_RECNOTI_RESP_OK, MSG_RECNOTI_ID_RESP_OK );
			htMsgID.Add(MESSAGE_TYPE.MSG_RECNOTI_RESP_ERR, MSG_RECNOTI_ID_RESP_ERR );

			htMsgID.Add(MESSAGE_TYPE.MSG_TARSAL, MSG_TARSAL_ID);
			htMsgID.Add(MESSAGE_TYPE.MSG_TARSAL_RESP_OK, MSG_RECNOTI_ID_RESP_OK );
			htMsgID.Add(MESSAGE_TYPE.MSG_TARSAL_RESP_ERR, MSG_RECNOTI_ID_RESP_OK );
		}


		public int GetMessageID(MESSAGE_TYPE type)
		{
			return (int)htMsgID[type];
		}

	}

	public class Message
	{
		private MESSAGE_TYPE messageType;
		private int iMessageId = -1;
		private ArrayList lstTagList = null;

		public MESSAGE_TYPE GetMessageType()
		{
			return messageType;
		}

		public Message(MESSAGE_TYPE mType, MessageHelper helper)
		{
			messageType = mType;

			iMessageId = helper.GetMessageID(messageType);
			lstTagList = new ArrayList();
		}

		public void SetTagList(ArrayList lst)
		{
			lstTagList = lst;
		}

		public void AddTag(Tag tag)
		{
			if (tag.NAME == TagName.MensajeID)
			{
				tag.SetTagValue(iMessageId);
			}

			lstTagList.Add(tag);
		}

		public Tag GetTagByName(TagName tagName)
		{
			Tag result = null;
			foreach (Tag tag in lstTagList)
			{
				if (tag.NAME == tagName)
				{
					result = tag;
					break;
				}
			}
			return result;
		}

		public string GetFullMessage()
		{
			string res = String.Empty;
			foreach (Tag tag in lstTagList)
			{
				res += tag.GetTLV(); 
			}

			return res;
		}

		public bool SetTagValue(TagName tagName, bool val)
		{
			bool bRslt = false;
			foreach (Tag tag in lstTagList)
			{
				if (tag.NAME == tagName)
				{
					tag.SetTagValue(val);
					bRslt = true;
					break;
				}
			}
			return bRslt;
		}

		public bool SetTagValue(TagName tagName, string val)
		{
			bool bRslt = false;
			foreach (Tag tag in lstTagList)
			{
				if (tag.NAME == tagName)
				{
					tag.SetTagValue(val);
					bRslt = true;
					break;
				}
			}
			return bRslt;
		}

		public bool SetTagValue(TagName tagName, int val)
		{
			bool bRslt = false;
			foreach (Tag tag in lstTagList)
			{
				if (tag.NAME == tagName && tag.NAME != TagName.MensajeID)
				{
					tag.SetTagValue(val);
					bRslt = true;
					break;
				}
			}
			return bRslt;
		}

		public bool SetTagValue(TagName tagName, uint val)
		{
			bool bRslt = false;
			foreach (Tag tag in lstTagList)
			{
				if (tag.NAME == tagName && tag.NAME != TagName.MensajeID)
				{
					tag.SetTagValue(val);
					bRslt = true;
					break;
				}
			}
			return bRslt;
		}

	}


	public class WDMessageFactory
	{

		static private TagHelper tagHelper     = new TagHelper();
		static private MessageHelper msgHelper = new MessageHelper();

		static public bool ParseResponseStream(string sStream, out Message msg, MESSAGE_TYPE SentMessageType)
		{
			bool bRslt = true;
			ArrayList tagList = new ArrayList();
			TagHelper tagHelper = new TagHelper();
			MessageHelper msgHelper = new MessageHelper();
			int iOffset = 0;
			bool bTagsFormatOK = true;
			bool bMessageResult = false;
			msg = null;
			try
			{

				while(iOffset < sStream.Length - 4 && bTagsFormatOK)
				{
					string sTagId = sStream.Substring(iOffset, 2);
					iOffset += 2;
					int inVal = System.Int32.Parse(sTagId, System.Globalization.NumberStyles.AllowHexSpecifier);
					
					string sTagLen =  sStream.Substring(iOffset, 2);
					iOffset += 2;
					int iLen = System.Int32.Parse(sTagLen, System.Globalization.NumberStyles.AllowHexSpecifier);

					TagName tagName = tagHelper.GetTagNameById( "0x" + sTagId );
					
					if( iLen == tagHelper.GetTagLength(tagName))
					{
						//La longitud de tag recibida es la esperada
						string sValue = sStream.Substring(iOffset, iLen);
						iOffset += iLen;
						Tag tag = new Tag(tagName,tagHelper);
						tag.SetTagValueFromString(sValue);
						tagList.Add(tag);

						if(tagName == TagName.Resultado)
						{
							if(sValue == "FF")
							{
								bMessageResult = true;
							}
							else
							{
								bMessageResult = false;
							}
						}

					}
					else
					{
						bTagsFormatOK = false;
					}
				}


				switch(SentMessageType)
				{
					case MESSAGE_TYPE.MSG_OPEINS:
					{
						if(bMessageResult)
						{
							msg = new Message(MESSAGE_TYPE.MSG_OPEINS_RESP_OK, msgHelper);
							msg.SetTagList(tagList);
						}
						else
						{
							msg = new Message(MESSAGE_TYPE.MSG_OPEINS_RESP_ERR, msgHelper);
							msg.SetTagList(tagList);
						}
						break;
					}

					case MESSAGE_TYPE.MSG_OPENOTI:
					{
						if(bMessageResult)
						{
							msg = new Message(MESSAGE_TYPE.MSG_OPENOTI_RESP_OK, msgHelper);
							msg.SetTagList(tagList);
						}
						else
						{
							msg = new Message(MESSAGE_TYPE.MSG_OPENOTI_RESP_ERR, msgHelper);
							msg.SetTagList(tagList);
						}
						break;
					}

					case MESSAGE_TYPE.MSG_RECINS:
					{
						if(bMessageResult)
						{
							msg = new Message(MESSAGE_TYPE.MSG_RECINS_RESP_OK, msgHelper);
							msg.SetTagList(tagList);
						}
						else
						{
							msg = new Message(MESSAGE_TYPE.MSG_RECINS_RESP_ERR, msgHelper);
							msg.SetTagList(tagList);
						}
						break;
					}

					case MESSAGE_TYPE.MSG_RECNOTI:
					{
						if(bMessageResult)
						{
							msg = new Message(MESSAGE_TYPE.MSG_RECNOTI_RESP_OK, msgHelper);
							msg.SetTagList(tagList);
						}
						else
						{
							msg = new Message(MESSAGE_TYPE.MSG_RECNOTI_RESP_ERR, msgHelper);
							msg.SetTagList(tagList);
						}
						break;
					}
					
					case MESSAGE_TYPE.MSG_TARSAL:
					{
						if(bMessageResult)
						{
							msg = new Message(MESSAGE_TYPE.MSG_TARSAL_RESP_OK, msgHelper);
							msg.SetTagList(tagList);
						}
						else
						{
							msg = new Message(MESSAGE_TYPE.MSG_TARSAL_RESP_ERR, msgHelper);
							msg.SetTagList(tagList);
						}
						break;
					}
					default:
					{
						msg = null;
						break;
					}
				}
			}
			catch(Exception e)
			{
				bRslt = false;
			}

			return bRslt;
		}

		static public Message CreateMessage(MESSAGE_TYPE msgType)
		{
			Message msg = null;
			switch(msgType)
			{
				case MESSAGE_TYPE.MSG_OPEINS:
				{
					msg = new Message(MESSAGE_TYPE.MSG_OPEINS, msgHelper);

            
					msg.AddTag(new Tag(TagName.MensajeID, tagHelper));
					msg.AddTag(new Tag(TagName.DispositivoID, tagHelper));
					msg.AddTag(new Tag(TagName.NumeroOperacionID, tagHelper));
					msg.AddTag(new Tag(TagName.TarjetaID, tagHelper));

					TagName[] blockArray = tagHelper.GetTagBlockArray();

					foreach(TagName blockName in blockArray)
					{
						msg.AddTag(new Tag(blockName, tagHelper));
					}

					msg.AddTag(new Tag(TagName.Importe, tagHelper));
            

					break;
				}

				case MESSAGE_TYPE.MSG_OPEINS_RESP_OK:
				{
					msg = new Message(MESSAGE_TYPE.MSG_OPEINS_RESP_OK, msgHelper);

					TagName[] blockArray = tagHelper.GetTagBlockArray();

					foreach(TagName blockName in blockArray)
					{
						msg.AddTag(new Tag(blockName, tagHelper));
					}

					msg.AddTag(new Tag(TagName.Actualizacion, tagHelper));
					msg.AddTag(new Tag(TagName.Saldo, tagHelper));
					msg.AddTag(new Tag(TagName.Resultado, tagHelper));

					break;
				}

				case MESSAGE_TYPE.MSG_OPEINS_RESP_ERR:
				{
					msg = new Message(MESSAGE_TYPE.MSG_OPEINS_RESP_ERR, msgHelper);

					TagName[] blockArray = tagHelper.GetTagBlockArray();

					foreach(TagName blockName in blockArray)
					{
						msg.AddTag(new Tag(blockName, tagHelper));
					}

					msg.AddTag(new Tag(TagName.Resultado, tagHelper));
					msg.AddTag(new Tag(TagName.Mensaje, tagHelper));

					break;
				}

				case MESSAGE_TYPE.MSG_OPENOTI:
				{
					msg = new Message(MESSAGE_TYPE.MSG_OPENOTI, msgHelper);

					msg.AddTag(new Tag(TagName.MensajeID, tagHelper));
					msg.AddTag(new Tag(TagName.DispositivoID, tagHelper));
					msg.AddTag(new Tag(TagName.NumeroOperacionID, tagHelper));
					msg.AddTag(new Tag(TagName.TarjetaID, tagHelper));
					msg.AddTag(new Tag(TagName.Actualizacion, tagHelper));
					break;
				}

				case MESSAGE_TYPE.MSG_OPENOTI_RESP_OK:
				{
					msg = new Message(MESSAGE_TYPE.MSG_OPENOTI_RESP_OK, msgHelper);

					msg.AddTag(new Tag(TagName.Resultado, tagHelper));
					break;
				}

				case MESSAGE_TYPE.MSG_OPENOTI_RESP_ERR:
				{
					msg = new Message(MESSAGE_TYPE.MSG_OPENOTI_RESP_ERR, msgHelper);

					msg.AddTag(new Tag(TagName.Resultado, tagHelper));
					msg.AddTag(new Tag(TagName.Mensaje, tagHelper));
					break;
				}

				case MESSAGE_TYPE.MSG_RECINS:
				{
					msg = new Message(MESSAGE_TYPE.MSG_RECINS, msgHelper);

					msg.AddTag(new Tag(TagName.MensajeID, tagHelper));
					msg.AddTag(new Tag(TagName.DispositivoID, tagHelper));
					msg.AddTag(new Tag(TagName.NumeroOperacionID, tagHelper));
					msg.AddTag(new Tag(TagName.TarjetaID, tagHelper));
					
					TagName[] blockArray = tagHelper.GetTagBlockArray();

					foreach(TagName blockName in blockArray)
					{
						msg.AddTag(new Tag(blockName, tagHelper));
					}

					msg.AddTag(new Tag(TagName.Importe, tagHelper));

					break;
				}

				case MESSAGE_TYPE.MSG_RECINS_RESP_OK:
				{
					msg = new Message(MESSAGE_TYPE.MSG_RECINS_RESP_OK, msgHelper);


					TagName[] blockArray = tagHelper.GetTagBlockArray();

					foreach(TagName blockName in blockArray)
					{
						msg.AddTag(new Tag(blockName, tagHelper));
					}


					msg.AddTag(new Tag(TagName.Actualizacion, tagHelper));
					msg.AddTag(new Tag(TagName.Importe, tagHelper));
					msg.AddTag(new Tag(TagName.Saldo, tagHelper));
					msg.AddTag(new Tag(TagName.Resultado, tagHelper));

					break;
				}

				case MESSAGE_TYPE.MSG_RECINS_RESP_ERR:
				{
					msg = new Message(MESSAGE_TYPE.MSG_RECINS_RESP_ERR, msgHelper);

					msg.AddTag(new Tag(TagName.Resultado, tagHelper));
					msg.AddTag(new Tag(TagName.Mensaje, tagHelper));

					break;
				}

				case MESSAGE_TYPE.MSG_RECNOTI:
				{
					msg = new Message(MESSAGE_TYPE.MSG_RECNOTI, msgHelper);

					msg.AddTag(new Tag(TagName.MensajeID, tagHelper));
					msg.AddTag(new Tag(TagName.DispositivoID, tagHelper));
					msg.AddTag(new Tag(TagName.NumeroOperacionID, tagHelper));
					msg.AddTag(new Tag(TagName.TarjetaID, tagHelper));
					msg.AddTag(new Tag(TagName.Actualizacion, tagHelper));
					break;
				}

				case MESSAGE_TYPE.MSG_RECNOTI_RESP_OK:
				{
					msg = new Message(MESSAGE_TYPE.MSG_RECNOTI_RESP_OK, msgHelper);

					msg.AddTag(new Tag(TagName.Resultado, tagHelper));
					break;
				}

				case MESSAGE_TYPE.MSG_RECNOTI_RESP_ERR:
				{
					msg = new Message(MESSAGE_TYPE.MSG_RECNOTI_RESP_ERR, msgHelper);

					msg.AddTag(new Tag(TagName.Resultado, tagHelper));
					msg.AddTag(new Tag(TagName.Mensaje, tagHelper));
					break;
				}

				case MESSAGE_TYPE.MSG_TARSAL:
				{
					msg = new Message(MESSAGE_TYPE.MSG_TARSAL, msgHelper);

					msg.AddTag(new Tag(TagName.MensajeID, tagHelper));
					msg.AddTag(new Tag(TagName.DispositivoID, tagHelper));
					msg.AddTag(new Tag(TagName.TarjetaID, tagHelper));

					TagName[] blockArray = tagHelper.GetTagBlockArray();

					foreach(TagName blockName in blockArray)
					{
						msg.AddTag(new Tag(blockName, tagHelper));
					}

					break;
				}

				case MESSAGE_TYPE.MSG_TARSAL_RESP_OK:
				{
					msg = new Message(MESSAGE_TYPE.MSG_TARSAL_RESP_OK, msgHelper);

					msg.AddTag(new Tag(TagName.Resultado, tagHelper));
					msg.AddTag(new Tag(TagName.Saldo, tagHelper));
					break;
				}

				case MESSAGE_TYPE.MSG_TARSAL_RESP_ERR:
				{
					msg = new Message(MESSAGE_TYPE.MSG_TARSAL_RESP_ERR, msgHelper);

					msg.AddTag(new Tag(TagName.Resultado, tagHelper));
					msg.AddTag(new Tag(TagName.Mensaje, tagHelper));
					break;
				}


			}

			return msg;
		}

	}


}


namespace OPS.Comm.Becs.Messages
{
	
	public class WDSender
	{
		
		/* a.	Servidor de prepago:
				i.	IP: 93.92.169.118
				ii.	Puerto: 8081
			b.	Servidor de pospago:
				i.	IP: 93.92.169.118
				ii.	Puerto: 8082
		 */

		private string sPrepayServerAddress = String.Empty;
		private int iPrepayServerPort = -1;

		private string sPostpayServerAddress = String.Empty;
		private int	   iPostpayServerPort = -1;

		private int	   iTimeout=10;

		public string PREPAY_SERVER_ADDRESS
		{
			get{return sPrepayServerAddress; }
			set{sPrepayServerAddress = value;}
		}

		public string POSTPAY_SERVER_ADDRESS
		{
			get{return sPostpayServerAddress; }
			set{sPostpayServerAddress = value;}
		}

		public int PREPAY_SERVER_PORT
		{
			get{return iPrepayServerPort;}
			set{iPrepayServerPort = value;}
		}

		public int POSTPAY_SERVER_PORT
		{
			get{return iPostpayServerPort;}
			set{iPostpayServerPort = value;}
		}

		public int TIMEOUT
		{
			get{return iTimeout;}
			set{iTimeout = value;}
		}

		public bool SendMessage(string sMsg, CARD_TYPE ct, out Socket socket )
		{
			bool bRslt = false;
			
			IPHostEntry address = null;
			IPEndPoint Ep = null;
			Byte[] RecvBytes = new Byte[255];

			if(ct == CARD_TYPE.PRE_PAY)
			{
				address = Dns.Resolve(sPrepayServerAddress);
				Ep = new IPEndPoint(address.AddressList[0], iPrepayServerPort);
			}
			else if(ct == CARD_TYPE.POST_PAY)
			{
				address = Dns.Resolve(sPostpayServerAddress);
				Ep = new IPEndPoint(address.AddressList[0], iPostpayServerPort);
			}

			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


			

			try
			{

				socket.Connect(Ep); //Conectamos

				Byte[] SendBytes = new byte[sMsg.Length + 2]; 

				Byte[] tmp = Encoding.ASCII.GetBytes(sMsg);


				for(int i = 0; i < tmp.Length; i++)
				{
					SendBytes[i] = tmp[i];
				}

				SendBytes[ sMsg.Length + 1] = 0x0D;
				SendBytes[ sMsg.Length]		= 0x0A;

				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, iTimeout * 1000 );
				int bytes = socket.Receive(RecvBytes,RecvBytes.Length,SocketFlags.None); //Recibimos la respuesta en bloques de 255 bytes
				string response = Encoding.ASCII.GetString(RecvBytes, 0, bytes);
				
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, iTimeout * 1000 );
				int iSent = socket.Send(SendBytes,SendBytes.Length,SocketFlags.None);

				bRslt = iSent != 0;

			}
			catch(Exception e)
			{
				bRslt = false;
			}
			return bRslt;
		}


		public bool WaitForResponse(out string response, Socket socket)
		{
			bool bRslt = false;
			response = String.Empty;
			Byte[] RecvBytes = null;
			int bytes = 0;
			try
			{
				RecvBytes = new Byte[2048];

				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, iTimeout * 1000 );
				bytes = socket.Receive(RecvBytes);//,RecvBytes.Length,SocketFlags.None); //Recibimos la respuesta en bloques de 255 bytes
				response = Encoding.ASCII.GetString(RecvBytes, 0, bytes);

				bRslt = bytes > 0;
			}
			catch(Exception e)
			{
			}

			return bRslt;
		}
/*
		public bool SendMessage(string sMsg, int iSecondsTimeOut, CARD_TYPE ct, out string response)
		{
			bool bRslt = false;
			response = String.Empty;
			IPHostEntry address = null;
			IPEndPoint Ep = null;
			

			if(ct == CARD_TYPE.PRE_PAY)
			{
				address = Dns.Resolve(sPrepayServerAddress);
				Ep = new IPEndPoint(address.AddressList[0], iPrepayServerPort);
			}
			else if(ct == CARD_TYPE.POST_PAY)
			{
				address = Dns.Resolve(sPostpayServerAddress);
				Ep = new IPEndPoint(address.AddressList[0], iPostpayServerPort);
			}

			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, iSecondsTimeOut * 1000 );

			try
			{

				socket.Connect(Ep); //Conectamos

				Byte[] SendBytes = Encoding.ASCII.GetBytes(sMsg);

				socket.Send(SendBytes,SendBytes.Length,SocketFlags.None); 

				int bytes = socket.Receive(RecvBytes,RecvBytes.Length,SocketFlags.None); //Recibimos la respuesta en bloques de 255 bytes
				response = Encoding.ASCII.GetString(RecvBytes, 0, bytes);

				bRslt = bytes > 0;

			}
			catch(Exception e)
			{
				bRslt = false;
			}
			return bRslt;
		}
		*/
	}

}