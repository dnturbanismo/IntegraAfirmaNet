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
    public class UsernameTokenSoapAssertion : PolicyAssertion
    {
        private UsernameToken token;
        public UsernameToken Token { get { return token; } }

        public UsernameTokenSoapAssertion(UsernameToken Token)
        {
            token = Token;
        }

        public UsernameTokenSoapAssertion(string username, string password, PasswordOption passwordmode)
        {
            token = new UsernameToken(username, password, passwordmode);
        }

        /// <summary>
        /// Implementa la autenticación mediante usuario y password
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override SoapFilter CreateClientOutputFilter(FilterCreationContext context)
        {
            return new UsernameOutputFilter(this);
        }

        /// <summary>
        /// No se implementa la entrada al cliente
        /// </summary>
        /// <remarks>Si se desea implementar un filtro para validar las respuestas de la plataforma, es aquí donde debería instanciarse.</remarks>
        /// <returns>null</returns>
        public override SoapFilter CreateClientInputFilter(FilterCreationContext context)
        {
            return null;
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
