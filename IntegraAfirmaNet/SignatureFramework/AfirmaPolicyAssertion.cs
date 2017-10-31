using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Web.Services3;
using System.Xml;
using Microsoft.Web.Services3.Design;
using Microsoft.Web.Services3.Security.Tokens;
using Microsoft.Web.Services3.Security;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Reflection;

namespace IntegraAfirmaNet.SignatureFramework
{
    class AfirmaPolicyAssertions
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

        public class AfirmaResponseAssertion : PolicyAssertion
        {
            private X509Certificate2 _serverCert;

            public AfirmaResponseAssertion(X509Certificate2 serverCert)
            {
                _serverCert = serverCert;
            }

            public override SoapFilter CreateClientInputFilter(FilterCreationContext context)
            {
                return new InputSoapFilter(_serverCert);
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
        }

        internal class UsernameOutputFilter : SendSecurityFilter
        {
            private UsernameTokenSoapAssertion parentAssertion;
            private UsernameToken token;

            public UsernameOutputFilter(UsernameTokenSoapAssertion parent)
                : base("", true)
            {
                parentAssertion = parent;
                token = parent.Token;
            }

            public override void SecureMessage(SoapEnvelope envelope, Security security)
            {
                security.Tokens.Add(parentAssertion.Token);
                security.MustUnderstand = false;
            }
        }

        internal class SignedSoapFilter : SoapFilter
        {

            private AfirmaPolicyAssertions.X509SecurityTokenSoapAssertion parentAssertion;
            private X509SecurityToken token;

            public SignedSoapFilter(AfirmaPolicyAssertions.X509SecurityTokenSoapAssertion parent)
            {
                parentAssertion = parent;
                token = parentAssertion.Token;
            }

            public override SoapFilterResult ProcessMessage(SoapEnvelope envelope)
            {
                return Sign(envelope);
            }

            private SoapFilterResult Sign(SoapEnvelope envelope)
            {
                XmlNode securityNode = envelope.CreateNode(XmlNodeType.Element, "wsse:Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                envelope.PreserveWhitespace = false;

                /*
               * <wsu:Timestamp wsu:Id="Timestamp-10ba255c-ea41-4041-ab07-2b7a3220ef88">
              <wsu:Created>2011-01-07T07:49:08Z</wsu:Created>
              <wsu:Expires>2011-01-07T07:54:08Z</wsu:Expires>
          </wsu:Timestamp>
               */
                XmlNode timestampNode = envelope.CreateNode(
                    XmlNodeType.Element,
                    "wsu:Timestamp", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                XmlElement timestampElement = timestampNode as XmlElement;

                XmlAttribute IdAttTs = envelope.CreateAttribute("wsu", "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                IdAttTs.Value = "Timestamp-" + Guid.NewGuid().ToString();

                XmlNode created = envelope.CreateNode(XmlNodeType.Element, "wsu:Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                created.InnerText = DateTime.Now.ToString("o");

                DateTime expiration = DateTime.Now.AddMinutes(3);
                XmlNode expires = envelope.CreateNode(XmlNodeType.Element, "wsu:Expires", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                expires.InnerText = expiration.ToString("o");
                timestampElement.Attributes.Append(IdAttTs);
                timestampElement.AppendChild(expires);
                timestampElement.AppendChild(created);

                XmlNode binarySecurityTokenNode = envelope.CreateNode(
                    XmlNodeType.Element,
                    "wsse:BinarySecurityToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                XmlElement binarySecurityTokenElement = binarySecurityTokenNode as XmlElement;
                binarySecurityTokenElement.SetAttribute(
                    "xmlns:wsu",
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                binarySecurityTokenElement.SetAttribute(
                    "EncodingType",
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
                binarySecurityTokenElement.SetAttribute(
                    "ValueType",
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

                XmlAttribute IdAtt = envelope.CreateAttribute("wsu", "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                IdAtt.Value = parentAssertion.Token.Id;
                binarySecurityTokenElement.Attributes.Append(IdAtt);

                byte[] publicCert = parentAssertion.Token.Certificate.GetRawCertData();
                binarySecurityTokenElement.InnerXml = Convert.ToBase64String(publicCert, Base64FormattingOptions.None);

                SignatureFramework.SignedXml signature = new SignatureFramework.SignedXml(envelope);
                signature.Signature.Id = "Signature-" + Guid.NewGuid().ToString();

                KeyInfo ki = new KeyInfo();
                ki.Id = "KeyInfo-" + Guid.NewGuid().ToString();
                SecurityTokenReference sectokenReference = new SecurityTokenReference(token, SecurityTokenReference.SerializationOptions.Reference);
                ki.AddClause(sectokenReference);

                signature.KeyInfo = ki;

                SignatureFramework.SignedInfo si = new SignatureFramework.SignedInfo();
                
                si.SignatureMethod = SignatureFramework.XmlSignatureConstants.XmlDsigSHA256Url;

                si.CanonicalizationMethod = SignatureFramework.XmlSignatureConstants.XmlDsigExcC14NTransformUrl;

                String bodyId = "Body-" + Guid.NewGuid().ToString();
                envelope.Body.SetAttribute("Id", bodyId);

                System.Security.Cryptography.Xml.Reference bsBody = new System.Security.Cryptography.Xml.Reference();
                bsBody.Uri = "#" + bodyId;
                bsBody.AddTransform(new System.Security.Cryptography.Xml.XmlDsigExcC14NTransform());
                si.AddReference(bsBody);

                System.Security.Cryptography.Xml.Reference refTimestamp = new System.Security.Cryptography.Xml.Reference();
                refTimestamp.Uri = "#" + IdAttTs.Value;
                refTimestamp.AddTransform(new System.Security.Cryptography.Xml.XmlDsigExcC14NTransform());
                si.AddReference(refTimestamp);

                signature.SignedInfo = si;

                bool disposeCryptoProvider = false;

                var key = (RSACryptoServiceProvider)parentAssertion.Token.Certificate.PrivateKey;

                if (key.CspKeyContainerInfo.ProviderName == "Microsoft Strong Cryptographic Provider" ||
                    key.CspKeyContainerInfo.ProviderName == "Microsoft Enhanced Cryptographic Provider v1.0" ||
                    key.CspKeyContainerInfo.ProviderName == "Microsoft Base Cryptographic Provider v1.0" ||
                    key.CspKeyContainerInfo.ProviderName == "Microsoft RSA SChannel Cryptographic Provider")
                {
                    Type CspKeyContainerInfo_Type = typeof(CspKeyContainerInfo);

                    FieldInfo CspKeyContainerInfo_m_parameters = CspKeyContainerInfo_Type.GetField("m_parameters", BindingFlags.NonPublic | BindingFlags.Instance);
                    CspParameters parameters = (CspParameters)CspKeyContainerInfo_m_parameters.GetValue(key.CspKeyContainerInfo);

                    var cspparams = new CspParameters(24, "Microsoft Enhanced RSA and AES Cryptographic Provider", key.CspKeyContainerInfo.KeyContainerName);
                    cspparams.KeyNumber = parameters.KeyNumber;
                    cspparams.Flags = parameters.Flags;
                    signature.SigningKey = new RSACryptoServiceProvider(cspparams);

                    disposeCryptoProvider = true;
                }
                else
                {
                    signature.SigningKey = parentAssertion.Token.Certificate.PrivateKey;
                }

                securityNode.AppendChild(binarySecurityTokenNode);
                securityNode.AppendChild(timestampNode);
                envelope.ImportNode(securityNode, true);
                XmlNode node = envelope.Header;
                node.AppendChild(securityNode);

                signature.ComputeSignature();
                signature.CheckSignature();
                securityNode.AppendChild(envelope.ImportNode(signature.GetXml(), true));

                if (disposeCryptoProvider)
                {
                    signature.SigningKey.Dispose();
                }

                return SoapFilterResult.Continue;
            }
        }

    }
}
