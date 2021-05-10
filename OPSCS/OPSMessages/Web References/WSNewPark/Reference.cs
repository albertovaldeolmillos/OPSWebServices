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

namespace OPSMessages.WSNewPark {
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
    [System.Web.Services.WebServiceBindingAttribute(Name="WSOPS2NewParkSoap", Namespace="http://tempuri.org/")]
    public partial class WSOPS2NewPark : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback IsDBAliveOperationCompleted;
        
        private System.Threading.SendOrPostCallback HeartBeatOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetCurrentVersionOperationCompleted;
        
        private System.Threading.SendOrPostCallback NewVehicleOperationOperationCompleted;
        
        private System.Threading.SendOrPostCallback PenaltyControlNoticeOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public WSOPS2NewPark() {
            this.Url = global::OPSMessages.Properties.Settings.Default.OPS_Messages_1_2_1_1_WSNewPark_WSOPS2NewPark;
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
        public event IsDBAliveCompletedEventHandler IsDBAliveCompleted;
        
        /// <remarks/>
        public event HeartBeatCompletedEventHandler HeartBeatCompleted;
        
        /// <remarks/>
        public event GetCurrentVersionCompletedEventHandler GetCurrentVersionCompleted;
        
        /// <remarks/>
        public event NewVehicleOperationCompletedEventHandler NewVehicleOperationCompleted;
        
        /// <remarks/>
        public event PenaltyControlNoticeCompletedEventHandler PenaltyControlNoticeCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/IsDBAlive", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool IsDBAlive() {
            object[] results = this.Invoke("IsDBAlive", new object[0]);
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginIsDBAlive(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("IsDBAlive", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public bool EndIsDBAlive(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void IsDBAliveAsync() {
            this.IsDBAliveAsync(null);
        }
        
        /// <remarks/>
        public void IsDBAliveAsync(object userState) {
            if ((this.IsDBAliveOperationCompleted == null)) {
                this.IsDBAliveOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsDBAliveOperationCompleted);
            }
            this.InvokeAsync("IsDBAlive", new object[0], this.IsDBAliveOperationCompleted, userState);
        }
        
        private void OnIsDBAliveOperationCompleted(object arg) {
            if ((this.IsDBAliveCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsDBAliveCompleted(this, new IsDBAliveCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/HeartBeat", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string HeartBeat() {
            object[] results = this.Invoke("HeartBeat", new object[0]);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginHeartBeat(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("HeartBeat", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public string EndHeartBeat(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void HeartBeatAsync() {
            this.HeartBeatAsync(null);
        }
        
        /// <remarks/>
        public void HeartBeatAsync(object userState) {
            if ((this.HeartBeatOperationCompleted == null)) {
                this.HeartBeatOperationCompleted = new System.Threading.SendOrPostCallback(this.OnHeartBeatOperationCompleted);
            }
            this.InvokeAsync("HeartBeat", new object[0], this.HeartBeatOperationCompleted, userState);
        }
        
        private void OnHeartBeatOperationCompleted(object arg) {
            if ((this.HeartBeatCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.HeartBeatCompleted(this, new HeartBeatCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetCurrentVersion", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetCurrentVersion() {
            object[] results = this.Invoke("GetCurrentVersion", new object[0]);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGetCurrentVersion(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetCurrentVersion", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public string EndGetCurrentVersion(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetCurrentVersionAsync() {
            this.GetCurrentVersionAsync(null);
        }
        
        /// <remarks/>
        public void GetCurrentVersionAsync(object userState) {
            if ((this.GetCurrentVersionOperationCompleted == null)) {
                this.GetCurrentVersionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetCurrentVersionOperationCompleted);
            }
            this.InvokeAsync("GetCurrentVersion", new object[0], this.GetCurrentVersionOperationCompleted, userState);
        }
        
        private void OnGetCurrentVersionOperationCompleted(object arg) {
            if ((this.GetCurrentVersionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetCurrentVersionCompleted(this, new GetCurrentVersionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/NewVehicleOperation", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public VehiclePDMOperationAnswer NewVehicleOperation(VehiclePDMOperation vPDMo) {
            object[] results = this.Invoke("NewVehicleOperation", new object[] {
                        vPDMo});
            return ((VehiclePDMOperationAnswer)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginNewVehicleOperation(VehiclePDMOperation vPDMo, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("NewVehicleOperation", new object[] {
                        vPDMo}, callback, asyncState);
        }
        
        /// <remarks/>
        public VehiclePDMOperationAnswer EndNewVehicleOperation(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((VehiclePDMOperationAnswer)(results[0]));
        }
        
        /// <remarks/>
        public void NewVehicleOperationAsync(VehiclePDMOperation vPDMo) {
            this.NewVehicleOperationAsync(vPDMo, null);
        }
        
        /// <remarks/>
        public void NewVehicleOperationAsync(VehiclePDMOperation vPDMo, object userState) {
            if ((this.NewVehicleOperationOperationCompleted == null)) {
                this.NewVehicleOperationOperationCompleted = new System.Threading.SendOrPostCallback(this.OnNewVehicleOperationOperationCompleted);
            }
            this.InvokeAsync("NewVehicleOperation", new object[] {
                        vPDMo}, this.NewVehicleOperationOperationCompleted, userState);
        }
        
        private void OnNewVehicleOperationOperationCompleted(object arg) {
            if ((this.NewVehicleOperationCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.NewVehicleOperationCompleted(this, new NewVehicleOperationCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/PenaltyControlNotice", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public GeneralAnswer PenaltyControlNotice(VehiclePCNOperation vPCNo) {
            object[] results = this.Invoke("PenaltyControlNotice", new object[] {
                        vPCNo});
            return ((GeneralAnswer)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginPenaltyControlNotice(VehiclePCNOperation vPCNo, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("PenaltyControlNotice", new object[] {
                        vPCNo}, callback, asyncState);
        }
        
        /// <remarks/>
        public GeneralAnswer EndPenaltyControlNotice(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((GeneralAnswer)(results[0]));
        }
        
        /// <remarks/>
        public void PenaltyControlNoticeAsync(VehiclePCNOperation vPCNo) {
            this.PenaltyControlNoticeAsync(vPCNo, null);
        }
        
        /// <remarks/>
        public void PenaltyControlNoticeAsync(VehiclePCNOperation vPCNo, object userState) {
            if ((this.PenaltyControlNoticeOperationCompleted == null)) {
                this.PenaltyControlNoticeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPenaltyControlNoticeOperationCompleted);
            }
            this.InvokeAsync("PenaltyControlNotice", new object[] {
                        vPCNo}, this.PenaltyControlNoticeOperationCompleted, userState);
        }
        
        private void OnPenaltyControlNoticeOperationCompleted(object arg) {
            if ((this.PenaltyControlNoticeCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PenaltyControlNoticeCompleted(this, new PenaltyControlNoticeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2612.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class VehiclePDMOperation {
        
        private string sIDField;
        
        private int nTypeField;
        
        private string sPlateField;
        
        private string sPaymentDateField;
        
        private string sExpiryDateField;
        
        private int nTariffIDField;
        
        private int nQuantityField;
        
        private bool bOnlineField;
        
        /// <comentarios/>
        public string SID {
            get {
                return this.sIDField;
            }
            set {
                this.sIDField = value;
            }
        }
        
        /// <comentarios/>
        public int NType {
            get {
                return this.nTypeField;
            }
            set {
                this.nTypeField = value;
            }
        }
        
        /// <comentarios/>
        public string SPlate {
            get {
                return this.sPlateField;
            }
            set {
                this.sPlateField = value;
            }
        }
        
        /// <comentarios/>
        public string SPaymentDate {
            get {
                return this.sPaymentDateField;
            }
            set {
                this.sPaymentDateField = value;
            }
        }
        
        /// <comentarios/>
        public string SExpiryDate {
            get {
                return this.sExpiryDateField;
            }
            set {
                this.sExpiryDateField = value;
            }
        }
        
        /// <comentarios/>
        public int NTariffID {
            get {
                return this.nTariffIDField;
            }
            set {
                this.nTariffIDField = value;
            }
        }
        
        /// <comentarios/>
        public int NQuantity {
            get {
                return this.nQuantityField;
            }
            set {
                this.nQuantityField = value;
            }
        }
        
        /// <comentarios/>
        public bool BOnline {
            get {
                return this.bOnlineField;
            }
            set {
                this.bOnlineField = value;
            }
        }
    }
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2612.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class GeneralAnswer {
        
        private int nRdoField;
        
        private string sRdoField;
        
        /// <comentarios/>
        public int NRdo {
            get {
                return this.nRdoField;
            }
            set {
                this.nRdoField = value;
            }
        }
        
        /// <comentarios/>
        public string SRdo {
            get {
                return this.sRdoField;
            }
            set {
                this.sRdoField = value;
            }
        }
    }
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2612.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class VehiclePCNOperation {
        
        private string sIDField;
        
        private int nTypeField;
        
        /// <comentarios/>
        public string SID {
            get {
                return this.sIDField;
            }
            set {
                this.sIDField = value;
            }
        }
        
        /// <comentarios/>
        public int NType {
            get {
                return this.nTypeField;
            }
            set {
                this.nTypeField = value;
            }
        }
    }
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2612.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class VehiclePDMOperationAnswer {
        
        private int nRdoField;
        
        private string sRdoField;
        
        private int nTariffIDField;
        
        private string sIDField;
        
        /// <comentarios/>
        public int NRdo {
            get {
                return this.nRdoField;
            }
            set {
                this.nRdoField = value;
            }
        }
        
        /// <comentarios/>
        public string SRdo {
            get {
                return this.sRdoField;
            }
            set {
                this.sRdoField = value;
            }
        }
        
        /// <comentarios/>
        public int NTariffID {
            get {
                return this.nTariffIDField;
            }
            set {
                this.nTariffIDField = value;
            }
        }
        
        /// <comentarios/>
        public string SID {
            get {
                return this.sIDField;
            }
            set {
                this.sIDField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    public delegate void IsDBAliveCompletedEventHandler(object sender, IsDBAliveCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class IsDBAliveCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal IsDBAliveCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    public delegate void HeartBeatCompletedEventHandler(object sender, HeartBeatCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class HeartBeatCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal HeartBeatCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    public delegate void GetCurrentVersionCompletedEventHandler(object sender, GetCurrentVersionCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetCurrentVersionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetCurrentVersionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    public delegate void NewVehicleOperationCompletedEventHandler(object sender, NewVehicleOperationCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class NewVehicleOperationCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal NewVehicleOperationCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public VehiclePDMOperationAnswer Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((VehiclePDMOperationAnswer)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    public delegate void PenaltyControlNoticeCompletedEventHandler(object sender, PenaltyControlNoticeCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PenaltyControlNoticeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PenaltyControlNoticeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public GeneralAnswer Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((GeneralAnswer)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591