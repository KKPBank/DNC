﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CSM.Service.LeadService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Header", Namespace="http://schemas.datacontract.org/2004/07/SLS.Service")]
    [System.SerializableAttribute()]
    public partial class Header : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ChannelIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EncodingField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PasswordField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UsernameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string VersionField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ChannelID {
            get {
                return this.ChannelIDField;
            }
            set {
                if ((object.ReferenceEquals(this.ChannelIDField, value) != true)) {
                    this.ChannelIDField = value;
                    this.RaisePropertyChanged("ChannelID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Encoding {
            get {
                return this.EncodingField;
            }
            set {
                if ((object.ReferenceEquals(this.EncodingField, value) != true)) {
                    this.EncodingField = value;
                    this.RaisePropertyChanged("Encoding");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Password {
            get {
                return this.PasswordField;
            }
            set {
                if ((object.ReferenceEquals(this.PasswordField, value) != true)) {
                    this.PasswordField = value;
                    this.RaisePropertyChanged("Password");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Username {
            get {
                return this.UsernameField;
            }
            set {
                if ((object.ReferenceEquals(this.UsernameField, value) != true)) {
                    this.UsernameField = value;
                    this.RaisePropertyChanged("Username");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Version {
            get {
                return this.VersionField;
            }
            set {
                if ((object.ReferenceEquals(this.VersionField, value) != true)) {
                    this.VersionField = value;
                    this.RaisePropertyChanged("Version");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="LeadService.ILeadService")]
    public interface ILeadService {
        
        // CODEGEN: Generating message contract since the wrapper namespace (www.kiatnakinbank.com/services/SlmLeadService/InsertLead) of message InsertLeadRequest does not match the default value (http://tempuri.org/)
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILeadService/InsertLead", ReplyAction="http://tempuri.org/ILeadService/InsertLeadResponse")]
        CSM.Service.LeadService.InsertLeadResponse InsertLead(CSM.Service.LeadService.InsertLeadRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILeadService/InsertLead", ReplyAction="http://tempuri.org/ILeadService/InsertLeadResponse")]
        System.Threading.Tasks.Task<CSM.Service.LeadService.InsertLeadResponse> InsertLeadAsync(CSM.Service.LeadService.InsertLeadRequest request);
        
        // CODEGEN: Generating message contract since the wrapper namespace (www.kiatnakinbank.com/services/SlmLeadService/UpdateLead) of message UpdateLeadRequest does not match the default value (http://tempuri.org/)
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILeadService/UpdateLead", ReplyAction="http://tempuri.org/ILeadService/UpdateLeadResponse")]
        CSM.Service.LeadService.UpdateLeadResponse UpdateLead(CSM.Service.LeadService.UpdateLeadRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILeadService/UpdateLead", ReplyAction="http://tempuri.org/ILeadService/UpdateLeadResponse")]
        System.Threading.Tasks.Task<CSM.Service.LeadService.UpdateLeadResponse> UpdateLeadAsync(CSM.Service.LeadService.UpdateLeadRequest request);
        
        // CODEGEN: Generating message contract since the wrapper namespace (www.kiatnakinbank.com/services/SlmLeadService/SearchLead) of message SearchLeadRequest does not match the default value (http://tempuri.org/)
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILeadService/SearchLead", ReplyAction="http://tempuri.org/ILeadService/SearchLeadResponse")]
        CSM.Service.LeadService.SearchLeadResponse SearchLead(CSM.Service.LeadService.SearchLeadRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILeadService/SearchLead", ReplyAction="http://tempuri.org/ILeadService/SearchLeadResponse")]
        System.Threading.Tasks.Task<CSM.Service.LeadService.SearchLeadResponse> SearchLeadAsync(CSM.Service.LeadService.SearchLeadRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="InsertLeadRequest", WrapperNamespace="www.kiatnakinbank.com/services/SlmLeadService/InsertLead", IsWrapped=true)]
    public partial class InsertLeadRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/InsertLead")]
        public CSM.Service.LeadService.Header RequestHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/InsertLead", Order=0)]
        public string RequestXml;
        
        public InsertLeadRequest() {
        }
        
        public InsertLeadRequest(CSM.Service.LeadService.Header RequestHeader, string RequestXml) {
            this.RequestHeader = RequestHeader;
            this.RequestXml = RequestXml;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="InsertLeadResponse", WrapperNamespace="www.kiatnakinbank.com/services/SlmLeadService/InsertLead", IsWrapped=true)]
    public partial class InsertLeadResponse {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/InsertLead")]
        public CSM.Service.LeadService.Header ResponseHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/InsertLead", Order=0)]
        public string ResponseStatus;
        
        public InsertLeadResponse() {
        }
        
        public InsertLeadResponse(CSM.Service.LeadService.Header ResponseHeader, string ResponseStatus) {
            this.ResponseHeader = ResponseHeader;
            this.ResponseStatus = ResponseStatus;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="UpdateLeadRequest", WrapperNamespace="www.kiatnakinbank.com/services/SlmLeadService/UpdateLead", IsWrapped=true)]
    public partial class UpdateLeadRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/UpdateLead")]
        public CSM.Service.LeadService.Header RequestHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/UpdateLead", Order=0)]
        public string RequestXml;
        
        public UpdateLeadRequest() {
        }
        
        public UpdateLeadRequest(CSM.Service.LeadService.Header RequestHeader, string RequestXml) {
            this.RequestHeader = RequestHeader;
            this.RequestXml = RequestXml;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="UpdateLeadResponse", WrapperNamespace="www.kiatnakinbank.com/services/SlmLeadService/UpdateLead", IsWrapped=true)]
    public partial class UpdateLeadResponse {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/UpdateLead")]
        public CSM.Service.LeadService.Header ResponseHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/UpdateLead", Order=0)]
        public string ResponseStatus;
        
        public UpdateLeadResponse() {
        }
        
        public UpdateLeadResponse(CSM.Service.LeadService.Header ResponseHeader, string ResponseStatus) {
            this.ResponseHeader = ResponseHeader;
            this.ResponseStatus = ResponseStatus;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="SearchLeadRequest", WrapperNamespace="www.kiatnakinbank.com/services/SlmLeadService/SearchLead", IsWrapped=true)]
    public partial class SearchLeadRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/SearchLead")]
        public CSM.Service.LeadService.Header RequestHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/SearchLead", Order=0)]
        public string RequestXml;
        
        public SearchLeadRequest() {
        }
        
        public SearchLeadRequest(CSM.Service.LeadService.Header RequestHeader, string RequestXml) {
            this.RequestHeader = RequestHeader;
            this.RequestXml = RequestXml;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="SearchLeadResponse", WrapperNamespace="www.kiatnakinbank.com/services/SlmLeadService/SearchLead", IsWrapped=true)]
    public partial class SearchLeadResponse {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/SearchLead")]
        public CSM.Service.LeadService.Header ResponseHeader;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="www.kiatnakinbank.com/services/SlmLeadService/SearchLead", Order=0)]
        public string ResponseStatus;
        
        public SearchLeadResponse() {
        }
        
        public SearchLeadResponse(CSM.Service.LeadService.Header ResponseHeader, string ResponseStatus) {
            this.ResponseHeader = ResponseHeader;
            this.ResponseStatus = ResponseStatus;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ILeadServiceChannel : CSM.Service.LeadService.ILeadService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class LeadServiceClient : System.ServiceModel.ClientBase<CSM.Service.LeadService.ILeadService>, CSM.Service.LeadService.ILeadService {
        
        public LeadServiceClient() {
        }
        
        public LeadServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public LeadServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public LeadServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public LeadServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        CSM.Service.LeadService.InsertLeadResponse CSM.Service.LeadService.ILeadService.InsertLead(CSM.Service.LeadService.InsertLeadRequest request) {
            return base.Channel.InsertLead(request);
        }
        
        public CSM.Service.LeadService.Header InsertLead(CSM.Service.LeadService.Header RequestHeader, string RequestXml, out string ResponseStatus) {
            CSM.Service.LeadService.InsertLeadRequest inValue = new CSM.Service.LeadService.InsertLeadRequest();
            inValue.RequestHeader = RequestHeader;
            inValue.RequestXml = RequestXml;
            CSM.Service.LeadService.InsertLeadResponse retVal = ((CSM.Service.LeadService.ILeadService)(this)).InsertLead(inValue);
            ResponseStatus = retVal.ResponseStatus;
            return retVal.ResponseHeader;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<CSM.Service.LeadService.InsertLeadResponse> CSM.Service.LeadService.ILeadService.InsertLeadAsync(CSM.Service.LeadService.InsertLeadRequest request) {
            return base.Channel.InsertLeadAsync(request);
        }
        
        public System.Threading.Tasks.Task<CSM.Service.LeadService.InsertLeadResponse> InsertLeadAsync(CSM.Service.LeadService.Header RequestHeader, string RequestXml) {
            CSM.Service.LeadService.InsertLeadRequest inValue = new CSM.Service.LeadService.InsertLeadRequest();
            inValue.RequestHeader = RequestHeader;
            inValue.RequestXml = RequestXml;
            return ((CSM.Service.LeadService.ILeadService)(this)).InsertLeadAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        CSM.Service.LeadService.UpdateLeadResponse CSM.Service.LeadService.ILeadService.UpdateLead(CSM.Service.LeadService.UpdateLeadRequest request) {
            return base.Channel.UpdateLead(request);
        }
        
        public CSM.Service.LeadService.Header UpdateLead(CSM.Service.LeadService.Header RequestHeader, string RequestXml, out string ResponseStatus) {
            CSM.Service.LeadService.UpdateLeadRequest inValue = new CSM.Service.LeadService.UpdateLeadRequest();
            inValue.RequestHeader = RequestHeader;
            inValue.RequestXml = RequestXml;
            CSM.Service.LeadService.UpdateLeadResponse retVal = ((CSM.Service.LeadService.ILeadService)(this)).UpdateLead(inValue);
            ResponseStatus = retVal.ResponseStatus;
            return retVal.ResponseHeader;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<CSM.Service.LeadService.UpdateLeadResponse> CSM.Service.LeadService.ILeadService.UpdateLeadAsync(CSM.Service.LeadService.UpdateLeadRequest request) {
            return base.Channel.UpdateLeadAsync(request);
        }
        
        public System.Threading.Tasks.Task<CSM.Service.LeadService.UpdateLeadResponse> UpdateLeadAsync(CSM.Service.LeadService.Header RequestHeader, string RequestXml) {
            CSM.Service.LeadService.UpdateLeadRequest inValue = new CSM.Service.LeadService.UpdateLeadRequest();
            inValue.RequestHeader = RequestHeader;
            inValue.RequestXml = RequestXml;
            return ((CSM.Service.LeadService.ILeadService)(this)).UpdateLeadAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        CSM.Service.LeadService.SearchLeadResponse CSM.Service.LeadService.ILeadService.SearchLead(CSM.Service.LeadService.SearchLeadRequest request) {
            return base.Channel.SearchLead(request);
        }
        
        public CSM.Service.LeadService.Header SearchLead(CSM.Service.LeadService.Header RequestHeader, string RequestXml, out string ResponseStatus) {
            CSM.Service.LeadService.SearchLeadRequest inValue = new CSM.Service.LeadService.SearchLeadRequest();
            inValue.RequestHeader = RequestHeader;
            inValue.RequestXml = RequestXml;
            CSM.Service.LeadService.SearchLeadResponse retVal = ((CSM.Service.LeadService.ILeadService)(this)).SearchLead(inValue);
            ResponseStatus = retVal.ResponseStatus;
            return retVal.ResponseHeader;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<CSM.Service.LeadService.SearchLeadResponse> CSM.Service.LeadService.ILeadService.SearchLeadAsync(CSM.Service.LeadService.SearchLeadRequest request) {
            return base.Channel.SearchLeadAsync(request);
        }
        
        public System.Threading.Tasks.Task<CSM.Service.LeadService.SearchLeadResponse> SearchLeadAsync(CSM.Service.LeadService.Header RequestHeader, string RequestXml) {
            CSM.Service.LeadService.SearchLeadRequest inValue = new CSM.Service.LeadService.SearchLeadRequest();
            inValue.RequestHeader = RequestHeader;
            inValue.RequestXml = RequestXml;
            return ((CSM.Service.LeadService.ILeadService)(this)).SearchLeadAsync(inValue);
        }
    }
}
