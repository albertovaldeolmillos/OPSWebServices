using System;
using System.Text.RegularExpressions;

namespace PDMHelpers
{
    public class COPSPlate
    {
        public const int OPSPLATE_LEN = 20;

        private string plate;

        public COPSPlate() { }
        public COPSPlate(string szPlate)
        {

            if (szPlate != null)
            {
                plate = szPlate;
                NormalizePlate();
            }

        }

        public static bool operator ==(COPSPlate x, object y)
        {

            if (x is null && y is null)
            {
                return true;
            }
            else if (x is null || y is null)
            {
                return false;
            }
            else
            {
                return (x.plate == y.ToString());
            }
        }
        public static bool operator !=(COPSPlate x, object y)
        {

            if (x is null && y is null)
            {
                return false;
            }
            else if (x is null || y is null)
            {
                return true;
            }
            else
            {
                return (x.plate != y.ToString());
            }
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            COPSPlate plateToCompare = obj as COPSPlate;
            return plate == plateToCompare.ToString();
        }
        public override int GetHashCode()
        {
            return plate.GetHashCode();
        }
        public override string ToString()
        {
            if (plate == string.Empty)
                return string.Empty;

            return plate;
        }

        public char[] ToCharArray(int length)
        {
            char[] bRdo;
            try
            {
                NormalizePlate();
                char[] plateChars = new char[length];
                Array.Copy(plate.ToCharArray(), plateChars, plate.ToCharArray().Length);
                bRdo = plateChars;

            }
            catch (Exception error)
            {
                bRdo = null;
            }

            return bRdo;
        }
        // Una matrícula normalizada es aquella que:
        //
        //	.- Tiene letras en mayuscula [A...Z]
        //	.- Tiene números [0...9]
        //  .- Longitud máxima es de OPSPLATE_LEN
        // 
        protected bool NormalizePlate()
        {
            bool bRdo = true;

            try
            {
                int nLen = this.plate.Length;

                if (nLen == 0)
                    throw new InvalidOperationException("Plate length is 0");


                char[] plateNormalized = new char[plate.Length];
                char[] plateCharacters = plate.ToUpper().ToCharArray();
                int newLen = 0;
                for (int i = 0; i < plateCharacters.Length; i++)
                {
                    string pattern = @"^[a-zA-Z0-9]*$";
                    RegexOptions options = RegexOptions.IgnoreCase;

                    Match m = Regex.Match(plateCharacters[i].ToString(), pattern, options);
                    if (m.Success)
                    {
                        plateNormalized[newLen++] = plateCharacters[i];
                    }
                }

                plate = new string(plateNormalized);

                if (plate.Length > OPSPLATE_LEN)
                {
                    throw new InvalidOperationException("Plate length is too long");
                }
            }
            catch (Exception error)
            {
                bRdo = false;
            }

            return bRdo;
        }

        public void Empty()
        {
            plate = string.Empty;
        }

        public bool IsEmpty()
        {
            return String.IsNullOrWhiteSpace(plate);
        }
    }
}