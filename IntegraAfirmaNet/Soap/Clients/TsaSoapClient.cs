using IntegraAfirmaNet.Authentication;
using IntegraAfirmaNet.Schemas;
using IntegraAfirmaNet.SignatureFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace IntegraAfirmaNet.Soap.Clients
{
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "CreateTimeStampSoapBinding", Namespace = "http://www.map.es/TSA/V1/TSA.wsdl")]
    public partial class CreateTimeSoapClient : BaseSoapClient
    {
        public CreateTimeSoapClient(Identity identity, X509Certificate2 serverCert) :
            base(identity, serverCert)
        {
            base.Url = global::IntegraAfirmaNet.Properties.Settings.Default.CreateTimeStampBinding;
        }

        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                base.Url = value;
            }
        }

        [SoapDocumentMethod("", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: XmlElement("SignResponse", Namespace = "urn:oasis:names:tc:dss:1.0:core:schema")]
        public SignResponse createTimeStamp([XmlElement("SignRequest", Namespace = "urn:oasis:names:tc:dss:1.0:core:schema")] SignRequest request)
        {
            object[] results = this.Invoke("createTimeStamp", new object[] { request });

            return ((SignResponse)(results[0]));
        }
    }

    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "RenewTimeStampSoapBinding", Namespace = "http://www.map.es/TSA/V1/TSA.wsdl")]
    public partial class RenewTimeSoapClient : BaseSoapClient
    {
        public RenewTimeSoapClient(Identity identity, X509Certificate2 serverCert) :
            base(identity, serverCert)
        {
            base.Url = global::IntegraAfirmaNet.Properties.Settings.Default.RenewTimeStampBinding;
        }

        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                base.Url = value;
            }
        }

        [SoapDocumentMethod("", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: XmlElement("SignResponse", Namespace = "urn:oasis:names:tc:dss:1.0:core:schema")]
        public SignResponse renewTimeStamp([XmlElement("SignRequest", Namespace = "urn:oasis:names:tc:dss:1.0:core:schema")] SignRequest request)
        {
            object[] results = this.Invoke("renewTimeStamp", new object[] { request });

            return ((SignResponse)(results[0]));
        }
    }

    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "VerifyTimeStampSoapBinding", Namespace = "http://www.map.es/TSA/V1/TSA.wsdl")]
    public partial class VerifyTimeSoapClient : BaseSoapClient
    {
        public VerifyTimeSoapClient(Identity identity, X509Certificate2 serverCert) :
            base(identity, serverCert)
        {
            base.Url = global::IntegraAfirmaNet.Properties.Settings.Default.VerifyTimeStampBinding;
        }

        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                base.Url = value;
            }
        }

        [SoapDocumentMethod("", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: XmlElement("VerifyResponse", Namespace = "urn:oasis:names:tc:dss:1.0:core:schema")]
        public VerifyResponse verifyTimeStamp([XmlElement("VerifyRequest", Namespace = "urn:oasis:names:tc:dss:1.0:core:schema")] VerifyRequest request)
        {
            object[] results = this.Invoke("verifyTimeStamp", new object[] { request });

            return ((VerifyResponse)(results[0]));
        }
    }
}
