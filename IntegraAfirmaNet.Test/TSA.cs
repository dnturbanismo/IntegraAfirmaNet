using System;
using System.Diagnostics;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using IntegraAfirmaNet.Authentication;
using System.Security.Cryptography.X509Certificates;
using IntegraAfirmaNet.Services;
using IntegraAfirmaNet.Exceptions;
using IntegraAfirmaNet.Schemas;

namespace IntegraAfirmaNet.Test
{
    [TestClass]
    public class TSA
    {
        private string _appId = "dipualba.sellado_genera";
        private string _certPath = @"c:\Temp\sello_componente_DIPUALBA.ES.p12";
        private string _passwordFile = @"C:\temp\pin.txt";
        
        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]        
        public void CreateTimeStamp()
        {
            /*ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            using (var servicio = new CreateTimeStampBinding())
            {
                //servicio.SoapVersion = SoapProtocolVersion.Soap11; 
                var resultado = servicio.createTimeStampCertificado(
                    @"c:\Temp\sello_componente_DIPUALBA.ES.p12",
                    System.IO.File.ReadAllText(@"C:\temp\pin.txt"),
                    GenerarSignRequest());
                TestContext.WriteLine(DateTime.Now.ToShortTimeString() + " " + resultado.Result.ResultMajor);
                Assert.AreEqual("urn:oasis:names:tc:dss:1.0:resultmajor:Success", resultado.Result.ResultMajor);
            }*/

            try
            {                
                Identity identity = new Identity(GetCertificate(), _appId);

                TsaService tsaService = new TsaService("https://des-tsafirma.redsara.es/tsamap", identity, null);

                var timeStamp = tsaService.CreateTimeStamp(RequestSignatureType.ASN1, CrearHashTexto("BLABLABLA"), "http://www.w3.org/2001/04/xmlenc#sha256");
            }
            catch (AfirmaResultException afirmaEx)
            {
                Assert.Fail(string.Format("Error devuelto por @firma: {0}", afirmaEx.Message));
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format("Unexpected exception of type {0} caught: {1}", ex.GetType(), ex.Message));                
            }
        }

        private X509Certificate2 GetCertificate()
        {
            return new X509Certificate2(_certPath, System.IO.File.ReadAllText(_passwordFile));
        }

/*        private static SignRequest GenerarSignRequest()
        {
            var signRequest = new SignRequest { OptionalInputs = new AnyType() };


            //Dim applicationIdentity As New ApplicationIdentity("dipualba.sellado_general")


            XmlDocument xmlSeccionOptionalDoc = new XmlDocument();

            xmlSeccionOptionalDoc.LoadXml("<OptionalInputs xmlns=\"urn:oasis:names:tc:dss:1.0:core:schema\">"
                + "<SignatureType>urn:oasis:names:tc:dss:1.0:core:schema</SignatureType>" 
                + "<ClaimedIdentity>" 
                + "<idAplicacion>dipualba.sellado_general</idAplicacion>" 
                + "</ClaimedIdentity></OptionalInputs>");

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
        }*/

        protected static byte[] CrearHashTexto(string texto)
        {
            System.Security.Cryptography.SHA256Managed sha256 = new System.Security.Cryptography.SHA256Managed();

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            return sha256.ComputeHash(enc.GetBytes(texto));
        }
    }
}
