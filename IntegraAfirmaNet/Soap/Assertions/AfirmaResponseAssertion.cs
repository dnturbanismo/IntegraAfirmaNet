using IntegraAfirmaNet.SignatureFramework;
using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Design;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.Soap.Assertions
{
    public class AfirmaResponseAssertion : PolicyAssertion
    {
        private X509Certificate2 _serverCert;
        private bool _checkResponseSignature;

        public AfirmaResponseAssertion(X509Certificate2 serverCert)
        {
            _serverCert = serverCert;
            ReadConfiguration();
        }

        public override SoapFilter CreateClientInputFilter(FilterCreationContext context)
        {
            return new InputSoapFilter(_serverCert, _checkResponseSignature);
        }

        public override SoapFilter CreateClientOutputFilter(FilterCreationContext context)
        {
            return null;
        }

        public override SoapFilter CreateServiceInputFilter(FilterCreationContext context)
        {
            return null;
        }

        public override SoapFilter CreateServiceOutputFilter(FilterCreationContext context)
        {
            return null;
        }

        private void ReadConfiguration()
        {
            string appSetting = ConfigurationManager.AppSettings["IntegraAfirmaNet_CheckResponseSignature"];

            if (!string.IsNullOrEmpty(appSetting))
            {
                _checkResponseSignature = Convert.ToBoolean(appSetting);
            }
            else
            {
                _checkResponseSignature = true;
            }
        }
    }
}
