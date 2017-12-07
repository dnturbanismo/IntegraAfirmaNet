using IntegraAfirmaNet.Authentication;
using IntegraAfirmaNet.Exceptions;
using IntegraAfirmaNet.Schemas;
using IntegraAfirmaNet.Services;
using IntegraAfirmaNet.SignatureFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.Test
{
    [TestClass]
    public class AFirma
    {
        private string _appId;
        private string _certPath;
        private string _password;
        private AfirmaService _afirmaService;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        public AFirma()
        {
            string[] lines = File.ReadAllLines(@"C:\Temp\ConfigAfirma.txt");

            _appId = lines[0]; // Linea 1: identificador de la aplicacion
            _certPath = lines[1];  // Linea 2: ruta donde se encuentra el certificado para firmar las peticiones
            _password = lines[2]; // Linea 3: password del fichero

            Identity identity = new Identity(new X509Certificate2(_certPath, _password), _appId);

//            _afirmaService = new AfirmaService(identity,
//                new X509Certificate2(ObtenerRecurso("IntegraAfirmaNet.Test.Certificados.SGAD_SE.cer")));

            _afirmaService = new AfirmaService(identity);
        }

        [TestMethod]
        public void AmpliarAXadesA()
        {
            try
            {
                byte[] firma = ObtenerRecurso("IntegraAfirmaNet.Test.Firmas.xades_internally_detached.xml");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Ampliando firma"));

                byte[] firmaAmpliada = _afirmaService.UpgradeSignature(firma, SignatureFormat.XAdES, ReturnUpdatedSignatureType.AdES_A);

                string resultado = TestContext.TestRunResultsDirectory + "\\FirmaXades-A.xml";

                File.WriteAllBytes(resultado, firmaAmpliada);

                TestContext.AddResultFile(resultado);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Firma ampliada"));
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
        public void AmpliarAXadesATargetSigner()
        {
            try
            {
                byte[] firma = ObtenerRecurso("IntegraAfirmaNet.Test.Firmas.xades_internally_detached.xml");

                byte[] cert = ObtenerRecurso("IntegraAfirmaNet.Test.Certificados.certificado_firma.cer");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Ampliando firma"));

                byte[] firmaAmpliada = _afirmaService.UpgradeSignature(firma, SignatureFormat.XAdES, ReturnUpdatedSignatureType.AdES_A, cert);

                string resultado = TestContext.TestRunResultsDirectory + "\\FirmaXades-A.xml";

                File.WriteAllBytes(resultado, firmaAmpliada);

                TestContext.AddResultFile(resultado);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Firma ampliada"));
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
        public void AmpliarACadesA()
        {
            try
            {
                byte[] firma = ObtenerRecurso("IntegraAfirmaNet.Test.Firmas.cades_attached_implicit.csig");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Ampliando firma"));

                byte[] firmaAmpliada = _afirmaService.UpgradeSignature(firma, SignatureFormat.CAdES, ReturnUpdatedSignatureType.AdES_A);

                string resultado = TestContext.TestRunResultsDirectory + "\\FirmaCades-A.csig";

                File.WriteAllBytes(resultado, firmaAmpliada);

                TestContext.AddResultFile(resultado);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Firma ampliada"));
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
        public void AmpliarACadesADetached()
        {
            try
            {
                byte[] firma = ObtenerRecurso("IntegraAfirmaNet.Test.Firmas.cades_detached_explicit.csig");

                DocumentHash docHash = new DocumentHash();
                docHash.DigestMethod = new DigestMethodType();
                docHash.DigestMethod.Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
                docHash.DigestValue = Convert.FromBase64String("11I1LxBkDvUpLwZqf2PHDmlQRM2vpf3t2Bg0kKODA8c=");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Ampliando firma"));

                byte[] firmaAmpliada = _afirmaService.UpgradeSignature(firma, SignatureFormat.CAdES, ReturnUpdatedSignatureType.AdES_A, null, new DocumentBaseType[] { docHash });

                string resultado = TestContext.TestRunResultsDirectory + "\\FirmaCades-Detached-A.csig";

                File.WriteAllBytes(resultado, firmaAmpliada);

                TestContext.AddResultFile(resultado);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Firma ampliada"));
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
        public void ValidarFirmaXades()
        {
            try
            {
                byte[] firma = ObtenerRecurso("IntegraAfirmaNet.Test.Firmas.xades_internally_detached.xml");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Verificando firma"));

                VerifyResponse resultado = _afirmaService.VerifySignature(firma, SignatureFormat.XAdES, true);

                Assert.AreEqual(ResultType.ValidSignature.Uri, resultado.Result.ResultMajor);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Firma válida"));
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
        public void ValidarFirmaCades()
        {
            try
            {
                byte[] firma = ObtenerRecurso("IntegraAfirmaNet.Test.Firmas.cades_attached_implicit.csig");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Verificando firma"));

                VerifyResponse resultado = _afirmaService.VerifySignature(firma, SignatureFormat.CAdES, true);

                Assert.AreEqual(ResultType.ValidSignature.Uri, resultado.Result.ResultMajor);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Firma válida"));
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
        public void ValidarFirmaCadesDetached()
        {
            try
            {
                byte[] firma = ObtenerRecurso("IntegraAfirmaNet.Test.Firmas.cades_detached_explicit.csig");

                DocumentHash docHash = new DocumentHash();
                docHash.DigestMethod = new DigestMethodType();
                docHash.DigestMethod.Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
                docHash.DigestValue = Convert.FromBase64String("11I1LxBkDvUpLwZqf2PHDmlQRM2vpf3t2Bg0kKODA8c=");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Verificando firma"));

                VerifyResponse resultado = _afirmaService.VerifySignature(firma, SignatureFormat.CAdES, true, new DocumentBaseType[] { docHash });

                Assert.AreEqual(ResultType.ValidSignature.Uri, resultado.Result.ResultMajor);

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Firma válida"));
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
        public void ValidarCertificado()
        {
            try
            {
                byte[] rawCert = ObtenerRecurso("IntegraAfirmaNet.Test.Certificados.lp_qse_es_sw_kpsc_valido.crt");

                TestContext.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToShortTimeString(), "Validando certificado"));

                VerifyResponse resultado = _afirmaService.ValidateCertificate(new X509Certificate2(rawCert), true, true);

                Assert.AreEqual("urn:oasis:names:tc:dss:1.0:profiles:XSS:resultminor:valid:certificate:Definitive", resultado.Result.ResultMinor);

                TestContext.WriteLine(string.Format("{0}: Resultado de validación - {1}", DateTime.Now.ToShortTimeString(), resultado.Result.ResultMessage.Value));
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
    }
}
