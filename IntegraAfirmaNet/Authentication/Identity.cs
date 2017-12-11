using Microsoft.Web.Services3.Design;
using Microsoft.Web.Services3.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using IntegraAfirmaNet.SignatureFramework;
using IntegraAfirmaNet.Soap.Assertions;

namespace IntegraAfirmaNet.Authentication
{
    enum AuthenticationType
    {
        UsernameToken,
        BinarySecurityToken
    }

    public class Identity
    {
        /// <summary>
        /// Aplicación a la que va dirigida la petición
        /// </summary>
        private string _applicationId;

        /// <summary>
        /// Usuario 
        /// </summary>
        private string _user;

        /// <summary>
        /// Password
        /// </summary>
        private string _password;

        private X509Certificate2 _certificate;

        /// <summary>
        /// Tipo de autenticacion
        /// </summary>
        private AuthenticationType _authenticationType;

        private PasswordOption _passwordMode;

        public string ApplicationId
        {
            get
            {
                return _applicationId;
            }

            set
            {
                _applicationId = value;
            }
        }

        public Identity(string user, string password, PasswordOption mode, string applicationId)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "El valor no puede ser nulo.");
            }

            if (user == "")
            {
                throw new ArgumentException("user", "El valor no puede ser una cadena vacía.");
            }

            if (password == null)
            {
                throw new ArgumentNullException("password", "El valor no puede ser nulo.");
            }

            if (password == "")
            {
                throw new ArgumentException("password", "El valor no puede ser una cadena vacía.");
            }

            if (applicationId == null)
            {
                throw new ArgumentNullException("applicationId", "El valor no puede ser nulo.");
            }

            if (applicationId == "")
            {
                throw new ArgumentException("applicationId", "El valor no puede ser una cadena vacía.");
            } 
            
            _user = user;
            _password = password;
            _passwordMode = mode;
            _authenticationType = AuthenticationType.UsernameToken;
            _applicationId = applicationId;
        }

        public Identity(X509Certificate2 certificate, string applicationId)
        {
            if (applicationId == null)
            {
                throw new ArgumentNullException("applicationId", "El valor no puede ser nulo.");
            }

            if (applicationId == "")
            {
                throw new ArgumentException("applicationId", "El valor no puede ser una cadena vacía.");
            }

            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "El valor no puede ser nulo.");
            }
            
            _certificate = certificate;
            _authenticationType = AuthenticationType.BinarySecurityToken;
            _applicationId = applicationId;
        }


        public PolicyAssertion GetPolicyAssertion()
        {
            PolicyAssertion assertion;

            if (_authenticationType == AuthenticationType.UsernameToken)
            {
                UsernameToken token = new UsernameToken(_user, _password, _passwordMode);
                assertion = new UsernameTokenSoapAssertion(token);
            }
            else
            {
                X509SecurityToken token = new X509SecurityToken(_certificate);
                assertion = new X509SecurityTokenSoapAssertion(token);
            }

            return assertion;
        }
    }
}
