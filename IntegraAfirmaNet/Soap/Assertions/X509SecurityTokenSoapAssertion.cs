using IntegraAfirmaNet.SignatureFramework;
using IntegraAfirmaNet.Soap.Filters;
using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Design;
using Microsoft.Web.Services3.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.Soap.Assertions
{
    public class X509SecurityTokenSoapAssertion : PolicyAssertion
    {
        private X509SecurityToken token;
        public X509SecurityToken Token { get { return token; } }

        public X509SecurityTokenSoapAssertion(string keystorePath, string keystorePassword)
        {
            Tools.ExternalX509TokenProvider tokenProvider = new Tools.ExternalX509TokenProvider(keystorePath, keystorePassword);
            token = tokenProvider.GetToken();
        }

        public X509SecurityTokenSoapAssertion(X509SecurityToken Token)
        {
            token = Token;
        }

        /// <summary>
        /// Crea un filtro de salida para las peticiones SOAP firmadas dirigidas a la plataforma @firma5
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>SignedSoapFilter</returns>
        public override SoapFilter CreateClientOutputFilter(FilterCreationContext context)
        {
            return new SignedSoapFilter(this);
        }

        /// <summary>
        /// No se implementa la entrada al cliente
        /// </summary>
        /// <remarks>Si se desea implementar un filtro para validar las respuestas de la plataforma, es aquí donde debería instanciarse.</remarks>
        /// <returns>null</returns>
        public override SoapFilter CreateClientInputFilter(FilterCreationContext context)
        {
            return null; // new InputSoapFilter();
        }

        /// <summary>
        /// No se implementa la entrada de servicio web
        /// </summary>
        /// <returns>null</returns>
        public override SoapFilter CreateServiceInputFilter(FilterCreationContext context)
        {
            return null;
        }

        /// <summary>
        /// No se implementa la salida de servicio web
        /// </summary>
        /// <returns>null</returns>
        public override SoapFilter CreateServiceOutputFilter(FilterCreationContext context)
        {
            return null;
        }
    }
}
