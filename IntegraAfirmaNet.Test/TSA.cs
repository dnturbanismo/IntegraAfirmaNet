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
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace IntegraAfirmaNet.Test
{
    [TestClass]
    public class TSA
    {
        private string _appId;
        private string _certPath;
        private string _password;
        private TsaService _tsaService;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        public TSA()
        {
            string[] lines = File.ReadAllLines(@"C:\Temp\ConfigTSA.txt");

            _appId = lines[0]; // Linea 1: identificador de la aplicacion
            _certPath = lines[1];  // Linea 2: ruta donde se encuentra el certificado para firmar las peticiones
            _password = lines[2]; // Linea 3: password del fichero

            Identity identity = new Identity(new X509Certificate2(_certPath, _password), _appId);

            _tsaService = new TsaService("https://des-tsafirma.redsara.es/tsamap", identity, null);
        }

        [TestMethod]
        public void CreateTimeStampASN1()
        {
            try
            {
                DocumentHash documentHash = new DocumentHash();
                documentHash.DigestMethod = new DigestMethodType();
                documentHash.DigestMethod.Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
                documentHash.DigestValue = CrearHashTexto("TEXTODEPRUEBA");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Creando sello de tiempo"));

                var timeStamp = _tsaService.CreateTimeStamp(RequestSignatureType.ASN1, documentHash);

                string resultado = TestContext.TestRunResultsDirectory + "\\Sello_Base64.txt";

                File.WriteAllText(resultado, Convert.ToBase64String(timeStamp.Item as byte[]));

                TestContext.AddResultFile(resultado);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Sello aplicado"));
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

        [TestMethod]
        public void CreateTimeStampXML()
        {
            try
            {
                DocumentHash documentHash = new DocumentHash();
                documentHash.DigestMethod = new DigestMethodType();
                documentHash.DigestMethod.Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
                documentHash.DigestValue = CrearHashTexto("TEXTODEPRUEBA");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Creando sello de tiempo"));

                var timeStamp = _tsaService.CreateTimeStamp(RequestSignatureType.XML, documentHash);
                
                string resultado = TestContext.TestRunResultsDirectory + "\\Sello.xml";

                SignatureType sello = timeStamp.Item as SignatureType;

                XmlSerializer serializer = new XmlSerializer(typeof(SignatureType));

                using (XmlWriter writer = XmlWriter.Create(resultado))
                {
                    serializer.Serialize(writer, sello);
                }
                
                TestContext.AddResultFile(resultado);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Sello aplicado"));
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

        [TestMethod]
        public void ValidarSelloASN1()
        {
            try
            {
                string sellob64 = Encoding.UTF8.GetString(ObtenerRecurso("IntegraAfirmaNet.Test.SellosTiempo.Sello_Base64.txt"));
                
                DocumentHash documentHash = new DocumentHash();
                documentHash.DigestMethod = new DigestMethodType();
                documentHash.DigestMethod.Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
                documentHash.DigestValue = CrearHashTexto("TEXTODEPRUEBA");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Validando sello de tiempo"));

                Timestamp timeStamp = new Timestamp();
                timeStamp.Item = Convert.FromBase64String(sellob64);

                _tsaService.VerifyTimestamp(documentHash, timeStamp);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Sello válido"));
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

        [TestMethod]
        public void ValidarSelloXML()
        {
            // NOTA: el servidor de desarrollo devuelve java.lang.NullPointerException, en producción funciona correctamente
            
            try
            {               
                XmlSerializer serializer = new XmlSerializer(typeof(SignatureType));
                SignatureType sello = (SignatureType)serializer.Deserialize(ObtenerStreamRecurso("IntegraAfirmaNet.Test.SellosTiempo.Sello.xml"));
              
                DocumentHash documentHash = new DocumentHash();
                documentHash.DigestMethod = new DigestMethodType();
                documentHash.DigestMethod.Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
                documentHash.DigestValue = CrearHashTexto("TEXTODEPRUEBA");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Validando sello de tiempo"));

                Timestamp timeStamp = new Timestamp();
                timeStamp.Item = sello;

                _tsaService.VerifyTimestamp(documentHash, timeStamp);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Sello válido"));
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

        private byte[] ObtenerRecurso(string nombre)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(nombre))
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);

                return ms.ToArray();
            }
        }

        private Stream ObtenerStreamRecurso(string nombre)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceStream(nombre);
        }

        private byte[] CrearHashTexto(string texto)
        {
            System.Security.Cryptography.SHA256Managed sha256 = new System.Security.Cryptography.SHA256Managed();

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            return sha256.ComputeHash(enc.GetBytes(texto));
        }
    }
}
