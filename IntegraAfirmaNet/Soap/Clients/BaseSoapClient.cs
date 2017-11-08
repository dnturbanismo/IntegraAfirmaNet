using IntegraAfirmaNet.Authentication;
using IntegraAfirmaNet.Soap.Assertions;
using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.SignatureFramework
{
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public abstract class BaseSoapClient : WebServicesClientProtocol
    {
        public BaseSoapClient(string url, Identity identity, X509Certificate2 serverCert)
        {
            this.Url = url;
            Policy policy = new Policy();
            policy.Assertions.Add(identity.GetPolicyAssertion());
            policy.Assertions.Add(new AfirmaResponseAssertion(serverCert));

            this.SetPolicy(policy);            
        }
    }
}
