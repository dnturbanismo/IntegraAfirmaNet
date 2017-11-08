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
    public partial class TsaSoapClient : BaseSoapClient
    {
        public TsaSoapClient(string url, Identity identity, X509Certificate2 serverCert) :
            base(url, identity, serverCert)
        {

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
}
