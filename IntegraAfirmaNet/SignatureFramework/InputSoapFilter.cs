using Microsoft.Web.Services3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IntegraAfirmaNet.SignatureFramework
{
    class InputSoapFilter : SoapFilter
    {
        private X509Certificate2 _serverCert;

        public InputSoapFilter(X509Certificate2 serverCert)
        {
            _serverCert = serverCert;
        }

        public override SoapFilterResult ProcessMessage(SoapEnvelope envelope)
        {
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(envelope.NameTable);
            xmlNamespaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            xmlNamespaceManager.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

            XmlNode securityNode = envelope.SelectSingleNode("soapenv:Envelope/soapenv:Header/wsse:Security", xmlNamespaceManager);

            if (securityNode != null)
            {
                // VALIDACIÓN DE LA FIRMA                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(envelope.OuterXml);

                XmlNode securityTokenNode = doc.SelectSingleNode("soapenv:Envelope/soapenv:Header/wsse:Security/wsse:BinarySecurityToken", xmlNamespaceManager);

                X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(securityTokenNode.InnerText));
                AsymmetricAlgorithm publicKey = null;

                // Si se establece el certificado del servidor se comprueba que sea el mismo que firma la respuesta
                if (_serverCert != null)
                {
                    if (cert.GetCertHashString() != _serverCert.GetCertHashString())
                    {
                        throw new Exception("El certificado que firma la respuesta no coincide con el dado de alta");
                    }

                    publicKey = _serverCert.PublicKey.Key;
                }
                else // se utiliza entonces el certificado de la respuesta
                {
                    publicKey = cert.PublicKey.Key;
                }

                XmlNode signatureNode = doc.SelectSingleNode("soapenv:Envelope/soapenv:Header/wsse:Security/ds:Signature", xmlNamespaceManager);

                SignedXml signedXml = new SignedXml(doc);
                signedXml.LoadXml((XmlElement)signatureNode);

                bool valid = signedXml.CheckSignature(publicKey);

                if (!valid)
                {
                    throw new Exception("Signature not valid");
                }

                // SE MODIFICA EL ATRIBUTO DESPUES DE VALIDAR LA FIRMA
                XmlNode attrMustUnderstand = securityNode.Attributes.GetNamedItem("mustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/");

                if (attrMustUnderstand != null && attrMustUnderstand.Value == "1")
                {
                    attrMustUnderstand.Value = "0";
                }
            }

            return SoapFilterResult.Continue;
        }
    }
}
