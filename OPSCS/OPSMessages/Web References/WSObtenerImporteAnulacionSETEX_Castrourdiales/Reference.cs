﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// Microsoft.VSDesigner generó automáticamente este código fuente, versión=4.0.30319.42000.
// 
#pragma warning disable 1591

namespace OPSMessages.WSObtenerImporteAnulacionSETEX_Castrourdiales {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="SETEXSoap", Namespace="http://tempuri.org/")]
    public partial class SETEX : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback EsExpedienteAnulableOperationCompleted;
        
        private System.Threading.SendOrPostCallback ObtenerImporteAnulacionOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public SETEX() {
            this.Url = global::OPSMessages.Properties.Settings.Default.OPS_Messages_1_2_1_1_WSObtenerImporteAnulacionSETEX_Castrourdiales_SETEX;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event EsExpedienteAnulableCompletedEventHandler EsExpedienteAnulableCompleted;
        
        /// <remarks/>
        public event ObtenerImporteAnulacionCompletedEventHandler ObtenerImporteAnulacionCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/EsExpedienteAnulable", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int EsExpedienteAnulable(string Expediente, string FechaParquimetro) {
            object[] results = this.Invoke("EsExpedienteAnulable", new object[] {
                        Expediente,
                        FechaParquimetro});
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginEsExpedienteAnulable(string Expediente, string FechaParquimetro, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("EsExpedienteAnulable", new object[] {
                        Expediente,
                        FechaParquimetro}, callback, asyncState);
        }
        
        /// <remarks/>
        public int EndEsExpedienteAnulable(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public void EsExpedienteAnulableAsync(string Expediente, string FechaParquimetro) {
            this.EsExpedienteAnulableAsync(Expediente, FechaParquimetro, null);
        }
        
        /// <remarks/>
        public void EsExpedienteAnulableAsync(string Expediente, string FechaParquimetro, object userState) {
            if ((this.EsExpedienteAnulableOperationCompleted == null)) {
                this.EsExpedienteAnulableOperationCompleted = new System.Threading.SendOrPostCallback(this.OnEsExpedienteAnulableOperationCompleted);
            }
            this.InvokeAsync("EsExpedienteAnulable", new object[] {
                        Expediente,
                        FechaParquimetro}, this.EsExpedienteAnulableOperationCompleted, userState);
        }
        
        private void OnEsExpedienteAnulableOperationCompleted(object arg) {
            if ((this.EsExpedienteAnulableCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.EsExpedienteAnulableCompleted(this, new EsExpedienteAnulableCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/ObtenerImporteAnulacion", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public decimal ObtenerImporteAnulacion(string Expediente, string FechaParquimetro) {
            object[] results = this.Invoke("ObtenerImporteAnulacion", new object[] {
                        Expediente,
                        FechaParquimetro});
            return ((decimal)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginObtenerImporteAnulacion(string Expediente, string FechaParquimetro, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("ObtenerImporteAnulacion", new object[] {
                        Expediente,
                        FechaParquimetro}, callback, asyncState);
        }
        
        /// <remarks/>
        public decimal EndObtenerImporteAnulacion(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((decimal)(results[0]));
        }
        
        /// <remarks/>
        public void ObtenerImporteAnulacionAsync(string Expediente, string FechaParquimetro) {
            this.ObtenerImporteAnulacionAsync(Expediente, FechaParquimetro, null);
        }
        
        /// <remarks/>
        public void ObtenerImporteAnulacionAsync(string Expediente, string FechaParquimetro, object userState) {
            if ((this.ObtenerImporteAnulacionOperationCompleted == null)) {
                this.ObtenerImporteAnulacionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnObtenerImporteAnulacionOperationCompleted);
            }
            this.InvokeAsync("ObtenerImporteAnulacion", new object[] {
                        Expediente,
                        FechaParquimetro}, this.ObtenerImporteAnulacionOperationCompleted, userState);
        }
        
        private void OnObtenerImporteAnulacionOperationCompleted(object arg) {
            if ((this.ObtenerImporteAnulacionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ObtenerImporteAnulacionCompleted(this, new ObtenerImporteAnulacionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    public delegate void EsExpedienteAnulableCompletedEventHandler(object sender, EsExpedienteAnulableCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class EsExpedienteAnulableCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal EsExpedienteAnulableCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public int Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((int)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    public delegate void ObtenerImporteAnulacionCompletedEventHandler(object sender, ObtenerImporteAnulacionCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ObtenerImporteAnulacionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ObtenerImporteAnulacionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public decimal Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((decimal)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591