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

namespace IntegraAfirmaNet.Services
{
    public enum SignatureFormat
    {
        CAdES,
        XAdES,
        PAdES
    }

    public class AfirmaService : BaseService
    {
        private static ResultType _validSignature = new ResultType("urn:afirma:dss:1.0:profile:XSS:resultmajor:ValidSignature");
        
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

        public AfirmaService(string url, Identity identity) : base  (url, identity)
        {
        }

        public AfirmaService(string url, Identity identity, X509Certificate2 serverCert) :
            base(url, identity, serverCert)
        {
            _serverCert = serverCert;
        }

        public void VerifySignature(byte[] signature, SignatureFormat signatureFormat)
        {
            object signatureObject = GetSignatureObject(signature, signatureFormat);

            VerifyRequest request = BuildRequest(signatureObject, null);

            DSSSignatureService ds = new DSSSignatureService(_baseUrl + "/DSSAfirmaVerify", _identity, _serverCert);

            string result = ds.verify(GetXmlElement<VerifyRequest>(request).OuterXml);

            VerifyResponse response = DeserializeXml<VerifyResponse>(result);

            if (!_validSignature.Equals(response.Result.ResultMajor))
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMinor, response.Result.ResultMessage.Value);
            }
        }

        public byte[] UpgradeSignature(byte[] signature, SignatureFormat signatureFormat, ReturnUpdatedSignatureType returnUpdateSignatureType)
        {
            object signatureObject = GetSignatureObject(signature, signatureFormat);

            VerifyRequest request = BuildRequest(signatureObject, returnUpdateSignatureType.ResourceName);

            DSSSignatureService ds = new DSSSignatureService(_baseUrl + "/DSSAfirmaVerify", _identity, _serverCert);

            string result = ds.verify(GetXmlElement<VerifyRequest>(request).OuterXml);

            VerifyResponse response = DeserializeXml<VerifyResponse>(result);

            if (ResultType.Success.Equals(response.Result.ResultMajor))
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
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMinor, response.Result.ResultMessage.Value);
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

            ValidarCertificadoService client = new ValidarCertificadoService(_baseUrl + "/ValidarCertificado", _identity, _serverCert);

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
