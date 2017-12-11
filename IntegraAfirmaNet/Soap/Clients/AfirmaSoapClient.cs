using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Web.Services3;
using System.Xml;
using System.Security.Cryptography.Xml;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;
using Microsoft.Web.Services3.Design;
using System.Web.Services.Protocols;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;
using System.Xml.Serialization;
using IntegraAfirmaNet.Authentication;

namespace IntegraAfirmaNet.SignatureFramework
{

    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "DSSAfirmaVerifySoapBinding", Namespace = "urn:oasis:names:tc:dss:1.0:core:schema")]
    public partial class DSSAfirmaVerifyService : BaseSoapClient
    {
        public DSSAfirmaVerifyService(Identity identity, X509Certificate2 serverCert) :
            base(identity, serverCert)
        {
            base.Url = global::IntegraAfirmaNet.Properties.Settings.Default.DSSAfirmaVerifyBinding;
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

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "urn:oasis:names:tc:dss:1.0:core:schema", ResponseNamespace = "urn:oasis:names:tc:dss:1.0:core:schema", Use = System.Web.Services.Description.SoapBindingUse.Literal)]
        [return: System.Xml.Serialization.XmlElementAttribute("verifyReturn")]
        public string verify(string dssXML)
        {
            object[] results = this.Invoke("verify", new object[] { dssXML });

            return ((string)(results[0]));
        }
    }

    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "DSSAfirmaVerifyCertificateSoapBinding", Namespace = "urn:oasis:names:tc:dss:1.0:core:schema")]
    public partial class DSSAfirmaVerifyCertificateService : BaseSoapClient
    {
        public DSSAfirmaVerifyCertificateService(Identity identity, X509Certificate2 serverCert) :
            base(identity, serverCert)
        {
            base.Url = global::IntegraAfirmaNet.Properties.Settings.Default.DSSAfirmaVerifyCertificateBinding;
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

        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace = "urn:oasis:names:tc:dss:1.0:core:schema", ResponseNamespace = "urn:oasis:names:tc:dss:1.0:core:schema", Use = System.Web.Services.Description.SoapBindingUse.Literal)]
        [return: System.Xml.Serialization.XmlElementAttribute("verifyReturn")]
        public string verify(string dssXML)
        {
            object[] results = this.Invoke("verify", new object[] { dssXML });

            return ((string)(results[0]));
        }
    }
}
