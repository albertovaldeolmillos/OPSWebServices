using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPSWebServicesAPI.Models
{
    public class UserOperationResponse
    {
        public O[] o { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class O
    {
        public string bns { get; set; }
        public string contid { get; set; }
        public string contname { get; set; }
        public string on { get; set; }
        public string ot { get; set; }
        public string pa { get; set; }
        public string pm { get; set; }
        public string pp { get; set; }
        public string rd { get; set; }
        public string zo { get; set; }
        public string zonecolor { get; set; }
        public string zonename { get; set; }
        public string farticle { get; set; }
        public string fcolor { get; set; }
        public string fmake { get; set; }
        public string fn { get; set; }
        public string fpd { get; set; }
        public string fs { get; set; }
        public string fstreet { get; set; }
        public string fstrnum { get; set; }
        public string pl { get; set; }
        public string ed { get; set; }
        public string sd { get; set; }
        public string sta { get; set; }
    }
}