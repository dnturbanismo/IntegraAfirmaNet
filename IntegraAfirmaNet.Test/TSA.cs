using System;
using System.Diagnostics;
using System.Net;
using IntegraAfirmaNet.TSA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

namespace IntegraAfirmaNet.Test
{
    [TestClass]
    public class TSA
    {
        [TestMethod]
        public void Prueba1()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            using (var servicio = new CreateTimeStampBinding())
            {
                //servicio.SoapVersion = SoapProtocolVersion.Soap11; 
                var resultado = servicio.createTimeStampCertificado(
                    @"\\nas01vnx\usuarios\INFORMATICA\informatica\Sistemas\Sede Electrónica\old\sello_componente_DIPUALBA.ES.p12",
                    System.IO.File.ReadAllText(@"\\nas01vnx\usuarios\INFORMATICA\informatica\Sistemas\Sede Electrónica\old\pin.txt"),
                    GenerarSignRequest());
                Debug.WriteLine(resultado.SignatureObject.SchemaRefs);
            }
        }

        private static SignRequest GenerarSignRequest()
        {
            var signRequest = new SignRequest { OptionalInputs = new AnyType() };


            //Dim applicationIdentity As New ApplicationIdentity("dipualba.sellado_general")


            XmlDocument xmlSeccionOptionalDoc = new XmlDocument();

            xmlSeccionOptionalDoc.LoadXml("<OptionalInputs xmlns=\"urn:oasis:names:tc:dss:1.0:core:schema\"><SignatureType>urn:oasis:names:tc:dss:1.0:core:schema</SignatureType>" + "<ClaimedIdentity>" + "<idAplicacion>dipualba.sellado_general</idAplicacion>" + "</ClaimedIdentity></OptionalInputs>");

            signRequest.OptionalInputs.Any = new XmlElement[] {
                (XmlElement)xmlSeccionOptionalDoc.ChildNodes.Item(0).FirstChild,
                (XmlElement)xmlSeccionOptionalDoc.ChildNodes.Item(0).LastChild
            };

            DocumentHash documentHash = new DocumentHash();
            documentHash.DigestMethod = new DigestMethodType();
            documentHash.DigestMethod.Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
            documentHash.DigestValue = new DigestValueType();
            documentHash.DigestValue.Value = CrearHashTexto("BLABLABLA");
            signRequest.InputDocuments = new InputDocuments();
            signRequest.InputDocuments.Items = new object[] { documentHash };

            return signRequest;
        }

        protected static byte[] CrearHashTexto(string texto)
        {
            System.Security.Cryptography.SHA256Managed sha256 = new System.Security.Cryptography.SHA256Managed();

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            return sha256.ComputeHash(enc.GetBytes(texto));
        }
    }
}
