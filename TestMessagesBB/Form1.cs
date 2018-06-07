using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Security.Cryptography;

namespace OPSWebServices
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox txtIn;
		private System.Windows.Forms.Label In;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;

		internal const string KEY_MESSAGE_0	= "75o73K3%0=53?73*h>7*32<5";
		internal const string KEY_MESSAGE_1	= "35s03!*3!8H3j33*53)73*lf";
		internal const string KEY_MESSAGE_2	= "7*32z5$8j07!3*35f5%73(30";
		internal const string KEY_MESSAGE_3	= "j07!(*h>7*32<5y8n%=!g5/&";
		internal const string KEY_MESSAGE_4	= "3!*50g73*5=57*3j$8j07!3*";
		internal const string KEY_MESSAGE_5	= "*5%37*kj3!*50,=2*3(6&k3%";
		internal const string KEY_MESSAGE_6	= "!8H37t3*5*3(65k3%57*3j3!";
		internal const string KEY_MESSAGE_7	= "253)73*lf5%73(30*32z5$8j";
		private System.Windows.Forms.TextBox txtOut;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtInCrypted;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtOutCrypted;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.txtIn = new System.Windows.Forms.TextBox();
			this.In = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtOut = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.txtInCrypted = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtOutCrypted = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// txtIn
			// 
			this.txtIn.Location = new System.Drawing.Point(136, 40);
			this.txtIn.Multiline = true;
			this.txtIn.Name = "txtIn";
			this.txtIn.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtIn.Size = new System.Drawing.Size(520, 96);
			this.txtIn.TabIndex = 0;
			this.txtIn.Text = "";
			// 
			// In
			// 
			this.In.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.In.Location = new System.Drawing.Point(64, 40);
			this.In.Name = "In";
			this.In.Size = new System.Drawing.Size(64, 23);
			this.In.TabIndex = 1;
			this.In.Text = "In:";
			this.In.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(64, 400);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 23);
			this.label1.TabIndex = 3;
			this.label1.Text = "Out:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtOut
			// 
			this.txtOut.Location = new System.Drawing.Point(136, 408);
			this.txtOut.Multiline = true;
			this.txtOut.Name = "txtOut";
			this.txtOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtOut.Size = new System.Drawing.Size(520, 96);
			this.txtOut.TabIndex = 2;
			this.txtOut.Text = "";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(280, 520);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(208, 23);
			this.button1.TabIndex = 6;
			this.button1.Text = "Send Request";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(24, 144);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 23);
			this.label2.TabIndex = 8;
			this.label2.Text = "In Crypted:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtInCrypted
			// 
			this.txtInCrypted.Location = new System.Drawing.Point(136, 144);
			this.txtInCrypted.Multiline = true;
			this.txtInCrypted.Name = "txtInCrypted";
			this.txtInCrypted.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtInCrypted.Size = new System.Drawing.Size(520, 96);
			this.txtInCrypted.TabIndex = 7;
			this.txtInCrypted.Text = "";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(16, 296);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(112, 23);
			this.label3.TabIndex = 10;
			this.label3.Text = "Out Crypted:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtOutCrypted
			// 
			this.txtOutCrypted.Location = new System.Drawing.Point(136, 304);
			this.txtOutCrypted.Multiline = true;
			this.txtOutCrypted.Name = "txtOutCrypted";
			this.txtOutCrypted.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtOutCrypted.Size = new System.Drawing.Size(520, 96);
			this.txtOutCrypted.TabIndex = 9;
			this.txtOutCrypted.Text = "";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(736, 574);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.txtOutCrypted);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtInCrypted);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtOut);
			this.Controls.Add(this.In);
			this.Controls.Add(this.txtIn);
			this.Name = "Form1";
			this.Text = "Test MessageBB";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void button1_Click(object sender, System.EventArgs e)
		{

			TestMessagesBB.MessagesBB.MessagesBB objService=null;

			try
			{
				objService=new TestMessagesBB.MessagesBB.MessagesBB();		
				txtInCrypted.Text="";
				txtOutCrypted.Text="";
				txtOut.Text="";

				if (txtIn.Text.Length>0)
				{
					byte [] byIn = EncryptMsg(txtIn.Text);
					byte [] byOut = null;

					txtInCrypted.Text=string.Format("{0} bytes --> ",byIn.Length)+Bytes_To_HexString(byIn);

					if (objService.Message(byIn, out byOut))
					{
						txtOutCrypted.Text=string.Format("{0} bytes --> ",byOut.Length)+Bytes_To_HexString(byOut);
						txtOut.Text=DecryptMsg(byOut);
					}
				}

			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message);
			}

		
		}


		protected string DecryptMsg(byte [] msgIn)
		{
			TripleDESCryptoServiceProvider TripleDesProvider=  new TripleDESCryptoServiceProvider();
			int sizeKey = System.Text.Encoding.UTF8.GetByteCount (KEY_MESSAGE_5);
			byte [] byKey;
			byKey = new byte[sizeKey];	
			System.Text.Encoding.UTF8.GetBytes(KEY_MESSAGE_5,0, KEY_MESSAGE_5.Length,byKey, 0);
			TripleDesProvider.Mode=CipherMode.ECB;
			TripleDesProvider.Key=byKey;
			Array.Clear(TripleDesProvider.IV,0,TripleDesProvider.IV.Length);
					
			OPSTripleDesEncryptor OPSTripleDesEnc= new OPSTripleDesEncryptor(TripleDesProvider);
			byte [] byRes=null;
			byte [] byTemp;

			byTemp=OPSTripleDesEnc.Desencriptar(msgIn);

			byRes=null;
			byRes=new byte[byTemp.Length];
			Array.Copy(byTemp,0,byRes,0,byTemp.Length);
			return GetDataAsString(byRes);
		}


		protected byte [] EncryptMsg(string msgOut)
		{

			byte[] byMsgOut = new byte[System.Text.Encoding.Default.GetByteCount(msgOut.ToCharArray(), 0, msgOut.Length)];
			System.Text.Encoding.Default.GetBytes(msgOut, 0, msgOut.Length, byMsgOut, 0);
			byMsgOut = DropUnWantedChars(byMsgOut);
			TripleDESCryptoServiceProvider TripleDesProvider=  new TripleDESCryptoServiceProvider();
			int sizeKey = System.Text.Encoding.UTF8.GetByteCount (KEY_MESSAGE_5);
			byte [] byKey;
			byKey = new byte[sizeKey];	
			System.Text.Encoding.UTF8.GetBytes(KEY_MESSAGE_5,0, KEY_MESSAGE_5.Length,byKey, 0);
			TripleDesProvider.Mode=CipherMode.ECB;
			TripleDesProvider.Key=byKey;
			Array.Clear(TripleDesProvider.IV,0,TripleDesProvider.IV.Length);
					
			OPSTripleDesEncryptor OPSTripleDesEnc= new OPSTripleDesEncryptor(TripleDesProvider);
			byte [] byRes=null;
			byte [] byTemp;

			byTemp=OPSTripleDesEnc.Encriptar(byMsgOut);

			byRes=null;
			byRes=new byte[byTemp.Length];
			Array.Copy(byTemp,0,byRes,0,byTemp.Length);
			return byRes;
		}


		protected byte [] DropUnWantedChars(byte [] body)
		{

			byte [] byRes=null;
			byte [] byTemp=body;

			int iNewLen=0;
			byte temp;
			for (int i=0;i<byTemp.Length;i++)
			{
				if ((byTemp[i]!=10)&&(byTemp[i]!=13))
				{
					temp=byTemp[i];
					byTemp[iNewLen++]=temp;
				}
			}
			


			byRes=null;
			byRes=new byte[iNewLen];
			Array.Copy(byTemp,0,byRes,0,iNewLen);
			return byRes;
		}

		protected string GetDataAsString(byte[] data)
		{

			System.Text.Decoder utf8Decoder = System.Text.Encoding.UTF8.GetDecoder();
			int charCount = utf8Decoder.GetCharCount(data, 0, (data.Length));
			char[] recievedChars = new char[charCount];
			utf8Decoder.GetChars(data, 0, data.Length, recievedChars, 0);
			string recievedString = new String(recievedChars);
			return recievedString;
		}

		protected string Bytes_To_HexString(byte[] bytes_Input)
		{
			// convert the byte array back to a true string
			string strTemp = "";
			for (int x = 0; x <= bytes_Input.GetUpperBound(0); x++)
			{
				int number = int.Parse(bytes_Input[x].ToString());
				if(x>0)
				{
					strTemp += " ";
				}
				strTemp += number.ToString("X").PadLeft(2, '0');
			}
			// return the finished string of hex values
			return strTemp;
		}

	}
}
