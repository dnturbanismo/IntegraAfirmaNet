using System;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Design;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;

namespace IntegraAfirmaNet.TSA
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

        internal class UsernameOutputFilter : SendSecurityFilter
        {
            private UsernameTokenSoapAssertion parentAssertion;
            private UsernameToken token;

            public UsernameOutputFilter(UsernameTokenSoapAssertion parent)
                :base("",true)
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
                IdAttTs.Value = "Timestamp-"+Guid.NewGuid().ToString();

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

                SignedXml signature = new SignedXml(envelope);
                signature.Signature.Id = "Signature-" + Guid.NewGuid().ToString();

                KeyInfo ki = new KeyInfo();
                ki.Id = "KeyInfo-" + Guid.NewGuid().ToString();
                SecurityTokenReference sectokenReference = new SecurityTokenReference(token, SecurityTokenReference.SerializationOptions.Reference);
                ki.AddClause(sectokenReference);

                signature.KeyInfo = ki;

                SignedInfo si = new SignedInfo();
                si.CanonicalizationMethod = XmlSignatureConstants.XmlDsigExcC14NTransformUrl;

                String bodyId = "Body-" + Guid.NewGuid().ToString();
                envelope.Body.SetAttribute("Id", bodyId);

                Reference bsBody = new Reference();
                bsBody.Uri = "#" + bodyId;
                bsBody.AddTransform(new XmlDsigExcC14NTransform());
                si.AddReference(bsBody);

                Reference refTimestamp = new Reference();
                refTimestamp.Uri = "#" + IdAttTs.Value;
                refTimestamp.AddTransform(new XmlDsigExcC14NTransform());
                si.AddReference(refTimestamp);

                signature.SignedInfo = si;
                signature.SigningKey = parentAssertion.Token.Certificate.PrivateKey;

                securityNode.AppendChild(binarySecurityTokenNode);
                securityNode.AppendChild(timestampNode);
                envelope.ImportNode(securityNode, true);
                XmlNode node = envelope.Header;

                node.RemoveAll();


                node.AppendChild(securityNode);

                signature.ComputeSignature();
                signature.CheckSignature();
                securityNode.AppendChild(envelope.ImportNode(signature.GetXml(), true));

                return SoapFilterResult.Continue;
            }
        }

    }
}
