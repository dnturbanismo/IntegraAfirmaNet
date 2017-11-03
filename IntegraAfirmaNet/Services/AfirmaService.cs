using IntegraAfirmaNet.Authentication;
using IntegraAfirmaNet.Exceptions;
using IntegraAfirmaNet.Schemas;
using IntegraAfirmaNet.SignatureFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AnyType = IntegraAfirmaNet.TSA.AnyType;
using Base64Data = IntegraAfirmaNet.TSA.Base64Data;
using Base64Signature = IntegraAfirmaNet.TSA.Base64Signature;
using DocumentType = IntegraAfirmaNet.TSA.DocumentType;
using InputDocuments = IntegraAfirmaNet.TSA.InputDocuments;
using SignatureObject = IntegraAfirmaNet.TSA.SignatureObject;
using SignaturePtr = IntegraAfirmaNet.TSA.SignaturePtr;
using VerifyRequest = IntegraAfirmaNet.TSA.VerifyRequest;

namespace IntegraAfirmaNet.Services
{
    public enum SignatureFormat
    {
        CAdES,
        XAdES,
        PAdES
    }

    public class AfirmaService
    {
        private string _baseUrlAfirma = null;
        private Identity _identity = null;
        private X509Certificate2 _serverCert = null;

        private XmlElement GetXmlElement<T>(T source)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(source.GetType());
                serializer.Serialize(ms, source);

                ms.Seek(0, SeekOrigin.Begin);

                XmlDocument doc = new XmlDocument();
                doc.Load(ms);

                return doc.DocumentElement;
            }
        }

        private T DeserializeXml<T>(string xml)
        {
            using (MemoryStream ms = new MemoryStream(UTF8Encoding.UTF8.GetBytes(xml)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T result = (T)serializer.Deserialize(ms);

                return result;
            }
        }


        private VerifyRequest BuildRequest(object signature, string updatedSignatureType = null)
        {
            VerifyRequest vr = new VerifyRequest();

            ClaimedIdentity identity = new ClaimedIdentity();
            identity.Name = new NameIdentifierType() { Value = _identity.ApplicationId };

            IgnoreGracePeriod igp = new IgnoreGracePeriod();

            vr.OptionalInputs = new AnyType();

            if (!string.IsNullOrEmpty(updatedSignatureType))
            {
                ReturnUpdatedSignature returnUpdated = new ReturnUpdatedSignature();
                returnUpdated.Type = updatedSignatureType;

                vr.OptionalInputs.Any = new XmlElement[] { GetXmlElement<ClaimedIdentity>(identity),
                GetXmlElement<ReturnUpdatedSignature>(returnUpdated),                
                GetXmlElement<IgnoreGracePeriod>(igp)};
            }
            else
            {
                vr.OptionalInputs.Any = new XmlElement[] { GetXmlElement<ClaimedIdentity>(identity),
                GetXmlElement<IgnoreGracePeriod>(igp)};
            }

            DocumentType doc = new DocumentType();
            doc.ID = "ID_DOCUMENTO";
            doc.Item = signature;
            vr.InputDocuments = new InputDocuments();
            vr.InputDocuments.Items = new object[] { doc };
            vr.SignatureObject = new SignatureObject();
            vr.SignatureObject.Item = new SignaturePtr()
            {
                WhichDocument = "ID_DOCUMENTO"
            };

            return vr;
        }


        private object GetSignatureObject(byte[] signature, SignatureFormat signatureFormat)
        {
            if (signatureFormat == SignatureFormat.XAdES)
            {
                return signature;
            }

            Base64Data b64Data = new Base64Data();
            if (signatureFormat == SignatureFormat.PAdES)
            {
                b64Data.MimeType = "application/pdf";
            }
            else
            {
                b64Data.MimeType = "application/octet-stream";
            }

            b64Data.Value = signature;

            return b64Data;
        }

        public AfirmaService(string url, Identity identity)
        {
            _baseUrlAfirma = url;
            _identity = identity;            
        }

        public AfirmaService(string url, Identity identity, X509Certificate2 serverCert) :
            this(url, identity)
        {
            _serverCert = serverCert;
        }

        public void VerifySignature(byte[] signature, SignatureFormat signatureFormat)
        {
            object signatureObject = GetSignatureObject(signature, signatureFormat);

            VerifyRequest request = BuildRequest(signatureObject, null);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<dssXML xmlns=\"\"></dssXML>");

            XmlNode dssXml = xmlDoc.SelectSingleNode("//dssXML");
            dssXml.InnerText = GetXmlElement<VerifyRequest>(request).OuterXml;

            DSSSignatureService ds = new DSSSignatureService(_baseUrlAfirma + "/DSSAfirmaVerify", _identity, _serverCert);

            string result = ds.verify(GetXmlElement<VerifyRequest>(request).OuterXml);

            VerifyResponse response = DeserializeXml<VerifyResponse>(result);

            if (response.Result.ResultMajor != "urn:afirma:dss:1.0:profile:XSS:resultmajor:ValidSignature")
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMessage.Value);
            }
        }

        public byte[] UpgradeSignature(byte[] signature, SignatureFormat signatureFormat, ReturnUpdatedSignatureType returnUpdateSignatureType)
        {
            object signatureObject = GetSignatureObject(signature, signatureFormat);

            VerifyRequest request = BuildRequest(signatureObject, returnUpdateSignatureType.ResourceName);

            DSSSignatureService ds = new DSSSignatureService(_baseUrlAfirma + "/DSSAfirmaVerify", _identity, _serverCert);

            string result = ds.verify(GetXmlElement<VerifyRequest>(request).OuterXml);

            VerifyResponse response = DeserializeXml<VerifyResponse>(result);

            if (response.Result.ResultMajor == "urn:oasis:names:tc:dss:1.0:resultmajor:Success")
            {
                XmlElement updatedSignatureXmlElement = response.OptionalOutputs.Any.Single(e => e.LocalName == "UpdatedSignature");
                UpdatedSignatureType updatedSignatureType = DeserializeXml<UpdatedSignatureType>(updatedSignatureXmlElement.OuterXml);

                if (updatedSignatureType.SignatureObject.Item.GetType() == typeof(SignaturePtr))
                {
                    SignaturePtr signaturePtr = updatedSignatureType.SignatureObject.Item as SignaturePtr;

                    DocumentWithSignature docWithSignature = null;
                    IEnumerable<XmlElement> documentWithSignatureXmlElements = response.OptionalOutputs.Any.Where(e => e.LocalName == "DocumentWithSignature");
                    foreach (var item in documentWithSignatureXmlElements)
                    {
                        docWithSignature = DeserializeXml<DocumentWithSignature>(item.OuterXml);

                        if (docWithSignature.Document.ID == signaturePtr.WhichDocument)
                        {
                            break;
                        }
                    }

                    if (docWithSignature == null)
                    {
                        throw new Exception("No se ha encontrado el documento de firma");
                    }
                    else
                    {
                        return docWithSignature.Document.Item as byte[];
                    }
                }
                else if (updatedSignatureType.SignatureObject.Item.GetType() == typeof(Base64Signature))
                {
                    Base64Signature b64Signature = updatedSignatureType.SignatureObject.Item as Base64Signature;

                    return b64Signature.Value;
                }
                else
                {
                    throw new Exception("Tipo de resultado no soportado");
                }
            }
            else
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMessage.Value);
            }
        }

        public mensajeSalidaRespuestaResultadoProcesamiento ValidarCertificado(X509Certificate2 certificado, string modoValidacion, bool obtenerInfo)
        {
            mensajeEntrada mensaje = new mensajeEntrada();
            mensaje.peticion = mensajeEntradaPeticion.ValidarCertificado;
            mensaje.versionMsg = "1.0";
            mensaje.parametros = new mensajeEntradaParametros();
            mensaje.parametros.idAplicacion = _identity.ApplicationId;
            mensaje.parametros.modoValidacion = modoValidacion;
            mensaje.parametros.certificado = certificado.GetRawCertData();
            mensaje.parametros.obtenerInfo = obtenerInfo;

            string peticion = GetXmlElement<mensajeEntrada>(mensaje).OuterXml;

            ValidarCertificadoService client = new ValidarCertificadoService(_baseUrlAfirma + "/ValidarCertificado", _identity, _serverCert);

            string result = client.ValidarCertificado(peticion);

            mensajeSalida salida = DeserializeXml<mensajeSalida>(result);

            if (salida.respuesta.Item is mensajeSalidaRespuestaResultadoProcesamiento)
            {
                return salida.respuesta.Item as mensajeSalidaRespuestaResultadoProcesamiento;
            }
            else
            {
                mensajeSalidaRespuestaExcepcion excepcion = salida.respuesta.Item as mensajeSalidaRespuestaExcepcion;

                throw new AfirmaResultException(excepcion.codigoError, excepcion.descripcion);
            }
        }
    }
}
