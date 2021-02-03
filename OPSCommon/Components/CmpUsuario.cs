using System;
using System.Data;
using System.Collections;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Mantains info about a user
	/// </summary>
	public sealed class CmpUsuario
	{
		private int _id;
		private string _login;
		private string _nombre;
		private string _apellido1;
		private string _apellido2;
		private int _rol;
		private int _lanid;
		private string _password;

		// TO DO: role id of a supervisor
		public const int SupervisorRoleId = 3;



		public CmpUsuario() {}

		public int Rol
		{
			get {return _rol;}
		}

		public int Id 
		{
			get {return _id;}
		}
		
		public string Login 
		{
			get {return _login;}
		}

		public string Password 
		{
			get {return _password;}
		}

		public string Nombre 
		{
			get {return _nombre;}
		}
		public string Apellido1 
		{
			get {return _apellido1;}
		}
		public string Apellido2
		{
			get {return _apellido2;}
		}

		public int LanId
		{
			get {return _lanid;}
		}

		/// <summary>
		/// Validates a login-password account
		/// </summary>
		/// <param name="login">Login to validate</param>
		/// <param name="pwd">Unencrypted password to validate</param>
		/// <param name="usrer">Info about validated user (applies only if login-password has been validated)</param>
		/// <returns><c>true</c> if login-password is validated <c>false</c> otherwise</returns>
		
		public static bool ValidateUser (string login, string pwd, out CmpUsuario usr)
		{
			CmpUsuarioDB udb = new CmpUsuarioDB();
			DataSet ds = udb.ValidateUser(login,pwd);
			DataTable dt = ds.Tables[0];
			usr = null;
			if (dt.Rows.Count == 0) return false;			// User not found, so login or password is incorrect!
			usr = new CmpUsuario();
			usr._id = Convert.ToInt32(dt.Rows[0]["USR_ID"]);
			usr._login  = dt.Rows[0]["USR_LOGIN"].ToString();
			usr._nombre = dt.Rows[0]["USR_NAME"].ToString();
			usr._apellido1 = dt.Rows[0]["USR_SURNAME1"].ToString();
			usr._rol = Convert.ToInt32(dt.Rows[0]["USR_ROL_ID"]);
			usr._password =  dt.Rows[0]["USR_PASSWORD"].ToString();

			usr._lanid = Convert.ToInt32(dt.Rows[0]["USR_LAN_ID"]);
			return true; 
		}

		public static bool GetUserData (string login, out CmpUsuario usr)
		{
			CmpUsuarioDB udb = new CmpUsuarioDB();
			DataSet ds = udb.GetUserData(login);
			DataTable dt = ds.Tables[0];
			usr = null;
			if (dt.Rows.Count == 0) return false;			// User not found, so login or password is incorrect!
			usr = new CmpUsuario();
			usr._id = Convert.ToInt32(dt.Rows[0]["USR_ID"]);
			usr._login  = dt.Rows[0]["USR_LOGIN"].ToString();
			usr._nombre = dt.Rows[0]["USR_NAME"].ToString();
			usr._apellido1 = dt.Rows[0]["USR_SURNAME1"].ToString();
			usr._rol = Convert.ToInt32(dt.Rows[0]["USR_ROL_ID"]);
			usr._password =  dt.Rows[0]["USR_PASSWORD"].ToString();
			usr._lanid = Convert.ToInt32(dt.Rows[0]["USR_LAN_ID"]);
			return true; 
		}
		
		/// <summary>
		/// Creates a new CmpUsuario with the info of a specified user
		/// The user MUST exist in the database
		/// </summary>
		/// <param name="usrId"></param>
		public CmpUsuario (int usrId)
		{
			DataTable dt = new CmpUsuarioDB().GetData(null, "USERS.USR_ID = @USERS.USR_ID@",null,new object[] {usrId});
			if (dt.Rows.Count == 1)
			{
				DataRow dr = dt.Rows[0];
				_id = Convert.ToInt32(dr["USR_ID"]);
				_nombre = dr["USR_NAME"].ToString();
				_apellido1 = dr["USR_SURNAME1"].ToString();
				_apellido2 = dr["USR_SURNAME2"].ToString();
				_login = dr["USR_LOGIN"].ToString();
				_lanid = Convert.ToInt32(dr["USR_LAN_ID"]);
				_rol = Convert.ToInt32(dr["USR_ROL_ID"]);
			}
			else throw new System.Exception ("CmpUsuario::<ctor>: Not found USER with login: " + usrId);
		}

		/// <summary>
		/// Fills a datatable with all Supervisors information ordered by (surname 1, surname 2, name) asc
		/// </summary>
		/// <param name="condenseName">True to get a single string for surnames and name (FULLNAME), false otherwise.</param>
		public DataTable GetAllSupevisors (bool condensedName) 
		{
			if (condensedName)
				return new CmpUsuarioDB().GetData(				
					new String[] {"USR_ID","USR_SURNAME1 || ' ' || USR_SURNAME2 || ', ' || USR_NAME as FULLNAME","USR_LOGIN","USR_LAN_ID","USR_ROL_ID"},
					"USR_ROL_ID = @USERS.USR_ROL_ID@", "USR_SURNAME1, USR_SURNAME2, USR_NAME", new Object[] {CmpUsuario.SupervisorRoleId});
			else
				return new CmpUsuarioDB().GetData(
					null, 
					"USR_ROL_ID = @USERS.USR_ROL_ID@", "USR_SURNAME1, USR_SURNAME2, USR_NAME", new object[] {CmpUsuario.SupervisorRoleId});
		}

	}
}
