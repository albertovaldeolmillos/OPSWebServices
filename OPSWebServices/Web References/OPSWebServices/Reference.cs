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

namespace OPSWebServices.OPSWebServices {
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
    [System.Web.Services.WebServiceBindingAttribute(Name="MessagesSoap", Namespace="http://tempuri.org/")]
    public partial class Messages : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback M59OperationCompleted;
        
        private System.Threading.SendOrPostCallback M3OperationCompleted;
        
        private System.Threading.SendOrPostCallback MessageOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Messages() {
            this.Url = "http://localhost/OPSWebServices/Messages.asmx";
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
        public event M59CompletedEventHandler M59Completed;
        
        /// <remarks/>
        public event M3CompletedEventHandler M3Completed;
        
        /// <remarks/>
        public event MessageCompletedEventHandler MessageCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/M59", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string M59(string sMessage) {
            object[] results = this.Invoke("M59", new object[] {
                        sMessage});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginM59(string sMessage, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("M59", new object[] {
                        sMessage}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndM59(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void M59Async(string sMessage) {
            this.M59Async(sMessage, null);
        }
        
        /// <remarks/>
        public void M59Async(string sMessage, object userState) {
            if ((this.M59OperationCompleted == null)) {
                this.M59OperationCompleted = new System.Threading.SendOrPostCallback(this.OnM59OperationCompleted);
            }
            this.InvokeAsync("M59", new object[] {
                        sMessage}, this.M59OperationCompleted, userState);
        }
        
        private void OnM59OperationCompleted(object arg) {
            if ((this.M59Completed != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.M59Completed(this, new M59CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/M3", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string M3(string sMessage) {
            object[] results = this.Invoke("M3", new object[] {
                        sMessage});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginM3(string sMessage, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("M3", new object[] {
                        sMessage}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndM3(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void M3Async(string sMessage) {
            this.M3Async(sMessage, null);
        }
        
        /// <remarks/>
        public void M3Async(string sMessage, object userState) {
            if ((this.M3OperationCompleted == null)) {
                this.M3OperationCompleted = new System.Threading.SendOrPostCallback(this.OnM3OperationCompleted);
            }
            this.InvokeAsync("M3", new object[] {
                        sMessage}, this.M3OperationCompleted, userState);
        }
        
        private void OnM3OperationCompleted(object arg) {
            if ((this.M3Completed != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.M3Completed(this, new M3CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/Message", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string Message(string sMessage) {
            object[] results = this.Invoke("Message", new object[] {
                        sMessage});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginMessage(string sMessage, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("Message", new object[] {
                        sMessage}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndMessage(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void MessageAsync(string sMessage) {
            this.MessageAsync(sMessage, null);
        }
        
        /// <remarks/>
        public void MessageAsync(string sMessage, object userState) {
            if ((this.MessageOperationCompleted == null)) {
                this.MessageOperationCompleted = new System.Threading.SendOrPostCallback(this.OnMessageOperationCompleted);
            }
            this.InvokeAsync("Message", new object[] {
                        sMessage}, this.MessageOperationCompleted, userState);
        }
        
        private void OnMessageOperationCompleted(object arg) {
            if ((this.MessageCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.MessageCompleted(this, new MessageCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    public delegate void M59CompletedEventHandler(object sender, M59CompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class M59CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal M59CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    public delegate void M3CompletedEventHandler(object sender, M3CompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class M3CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal M3CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    public delegate void MessageCompletedEventHandler(object sender, MessageCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class MessageCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal MessageCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591