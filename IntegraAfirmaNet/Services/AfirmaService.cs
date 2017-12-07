using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using IntegraAfirmaNet.Authentication;
using IntegraAfirmaNet.Exceptions;
using IntegraAfirmaNet.Schemas;
using IntegraAfirmaNet.SignatureFramework;

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

        public AfirmaService(Identity identity)
            : base(identity)
        {

        }

        public AfirmaService(Identity identity, X509Certificate2 serverCert) :
            base(identity, serverCert)
        {

        }

        public VerifyResponse VerifySignature(byte[] signature, SignatureFormat signatureFormat, bool includeDetails,
            IEnumerable<DocumentBaseType> otherInputDocuments = null)
        {
            if (signature == null)
            {
                throw new ArgumentNullException("signature", "El valor no puede ser nulo.");
            }            
            
            object document = GetDocument(signature, signatureFormat);

            DocumentType doc = new DocumentType();
            doc.ID = "ID_DOCUMENTO";
            doc.Item = document;

            SignatureObject signatureObject = new SignatureObject();
            signatureObject.Item = new SignaturePtr()
            {
                WhichDocument = "ID_DOCUMENTO"
            };

            List<DocumentBaseType> documents = new List<DocumentBaseType>();
            documents.Add(doc);

            if (otherInputDocuments != null)
            {
                foreach (var inputDocument in otherInputDocuments)
                {
                    documents.Add(inputDocument);
                }
            }

            IgnoreGracePeriod igp = new IgnoreGracePeriod();

            ReturnVerificationReport verificationReport = new ReturnVerificationReport();
            verificationReport.ReportOptions = new ReportOptionsType();
            if (includeDetails)
            {
                verificationReport.ReportOptions.ReportDetailLevel = "urn:oasis:names:tc:dss:1.0:reportdetail:allDetails";
            }
            else
            {
                verificationReport.ReportOptions.ReportDetailLevel = "urn:oasis:names:tc:dss:1.0:reportdetail:noDetails";
            }

            VerifyRequest request = BuildRequest(documents, signatureObject, new XmlElement[] { GetXmlElement(igp), GetXmlElement(verificationReport) });

            DSSAfirmaVerifyService ds = new DSSAfirmaVerifyService(_identity, _serverCert);

            string result = ds.verify(GetXmlElement(request).OuterXml);

            VerifyResponse response = DeserializeXml<VerifyResponse>(result);

            if (ResultType.RequesterError.Equals(response.Result.ResultMajor) ||
                ResultType.ResponderError.Equals(response.Result.ResultMajor))
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMinor, response.Result.ResultMessage.Value);
            }

            return response;
        }

        public byte[] UpgradeSignature(byte[] signature, SignatureFormat signatureFormat, ReturnUpdatedSignatureType returnUpdateSignatureType,
            byte[] targetSignerCert = null, IEnumerable<DocumentBaseType> otherInputDocuments = null)
        {
            if (signature == null)
            {
                throw new ArgumentNullException("signature", "El valor no puede ser nulo.");
            }

            if (returnUpdateSignatureType == null)
            {
                throw new ArgumentNullException("returnUpdateSignatureType", "El valor no puede ser nulo.");
            } 
            
            object document = GetDocument(signature, signatureFormat);

            ReturnUpdatedSignature returnUpdated = new ReturnUpdatedSignature();
            returnUpdated.Type = returnUpdateSignatureType.ResourceName;

            IgnoreGracePeriod igp = new IgnoreGracePeriod();

            DocumentType doc = new DocumentType();
            doc.ID = "ID_DOCUMENTO";
            doc.Item = document;

            SignatureObject signatureObject = new SignatureObject();
            signatureObject.Item = new SignaturePtr()
            {
                WhichDocument = "ID_DOCUMENTO"
            };

            List<DocumentBaseType> documents = new List<DocumentBaseType>();
            documents.Add(doc);

            if (otherInputDocuments != null)
            {
                foreach (var inputDocument in otherInputDocuments)
                {
                    documents.Add(inputDocument);
                }
            }

            List<XmlElement> optionalInputs = new List<XmlElement>();
            optionalInputs.Add(GetXmlElement(igp));
            optionalInputs.Add(GetXmlElement(returnUpdated));

            if (targetSignerCert != null)
            {
                TargetSigner targetSigner = new TargetSigner();
                targetSigner.Value = targetSignerCert;

                optionalInputs.Add(GetXmlElement(targetSigner));
            }

            VerifyRequest request = BuildRequest(documents, signatureObject, optionalInputs);

            DSSAfirmaVerifyService ds = new DSSAfirmaVerifyService(_identity, _serverCert);

            string result = ds.verify(GetXmlElement(request).OuterXml);

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
                            return docWithSignature.Document.Item as byte[];
                        }
                    }

                    throw new Exception("No se ha encontrado el documento de firma");
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

        public VerifyResponse ValidateCertificate(X509Certificate2 certificate, bool includeDetails, bool returnReadableCertificateInfo)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "El valor no puede ser nulo.");
            }             
            
            List<XmlElement> optionalInputs = new List<XmlElement>();
            
            ReturnVerificationReport verificationReport = new ReturnVerificationReport();
            verificationReport.CheckOptions = new CheckOptionsType();
            verificationReport.CheckOptions.CheckCertificateStatus = true;
            verificationReport.ReportOptions = new ReportOptionsType();
            if (includeDetails)
            {
                verificationReport.ReportOptions.ReportDetailLevel = "urn:oasis:names:tc:dss:1.0:reportdetail:allDetails";
            }
            else
            {
                verificationReport.ReportOptions.ReportDetailLevel = "urn:oasis:names:tc:dss:1.0:reportdetail:noDetails";
            }

            optionalInputs.Add(GetXmlElement(verificationReport));

            if (returnReadableCertificateInfo)
            {
                optionalInputs.Add(GetXmlElement("<afxp:ReturnReadableCertificateInfo xmlns:afxp=\"urn:afirma:dss:1.0:profile:XSS:schema\"/>"));
            }
                      
            X509DataType x509Data = new X509DataType();
            x509Data.Items = new object[] { new X509Cert(certificate.GetRawCertData()) };
            x509Data.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.X509Certificate };
            
            SignatureObject signatureObject = new SignatureObject();
            signatureObject.Item = new AnyType() { Any = new XmlElement[] { GetXmlElement(x509Data) } };

            VerifyRequest request = BuildRequest(null, signatureObject, optionalInputs.ToArray());

            DSSAfirmaVerifyCertificateService ds = new DSSAfirmaVerifyCertificateService(_identity, _serverCert);

            string result = ds.verify(GetXmlElement(request).OuterXml);

            VerifyResponse response = DeserializeXml<VerifyResponse>(result);

            if (!ResultType.Success.Equals(response.Result.ResultMajor))
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMinor, response.Result.ResultMessage.Value);
            }

            return response;
        }

        private VerifyRequest BuildRequest(IEnumerable<object> inputDocuments, SignatureObject signatureObject,
            IEnumerable<XmlElement> optionalInputs)
        {
            VerifyRequest vr = new VerifyRequest();

            ClaimedIdentity identity = new ClaimedIdentity();
            identity.Name = new NameIdentifierType() { Value = _identity.ApplicationId };

            List<XmlElement> optionalInputsList = new List<XmlElement>();
            optionalInputsList.Add(GetXmlElement(identity));

            foreach (var optionalInput in optionalInputs)
            {
                optionalInputsList.Add(optionalInput);
            }

            vr.OptionalInputs = new AnyType();
            vr.OptionalInputs.Any = optionalInputsList.ToArray();

            if (inputDocuments != null)
            {
                vr.InputDocuments = new InputDocuments();
                vr.InputDocuments.Items = inputDocuments.ToArray();
            }
            vr.SignatureObject = signatureObject;

            return vr;
        }

        private object GetDocument(byte[] signature, SignatureFormat signatureFormat)
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

    }
}
