using IntegraAfirmaNet.Authentication;
using IntegraAfirmaNet.Exceptions;
using IntegraAfirmaNet.Schemas;
using IntegraAfirmaNet.Soap.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IntegraAfirmaNet.Services
{
    public class TsaService : BaseService
    {
        public TsaService(string url, Identity identity)
            : base(url, identity)
        {
        }

        public TsaService(string url, Identity identity, X509Certificate2 serverCert)
            : base(url, identity, serverCert)
        {

        }

        private SignRequest BuildRequest<T>(RequestSignatureType signatureType, T document, Timestamp previousTimestamp) where T : DocumentBaseType
        {
            SignRequest sr = new SignRequest();

            ClaimedIdentity identity = new ClaimedIdentity();
            identity.idAplicacion = _identity.ApplicationId;

            XmlElement signatureTypeElement = GetXmlElement(string.Format("<SignatureType>{0}</SignatureType>", signatureType.Uri));

            sr.OptionalInputs = new AnyType();

            if (previousTimestamp != null)
            {
                string renewXml = "<RenewTimestamp><PreviousTimestamp>{0}</PreviousTimestamp></RenewTimestamp>";

                XmlElement previousTimestampElement = GetXmlElement(string.Format(renewXml, GetXmlElement<Timestamp>(previousTimestamp).OuterXml));

                sr.OptionalInputs.Any = new XmlElement[] { signatureTypeElement, GetXmlElement<ClaimedIdentity>(identity), previousTimestampElement };
            }
            else
            {
                sr.OptionalInputs.Any = new XmlElement[] { signatureTypeElement, GetXmlElement<ClaimedIdentity>(identity) };
            }

            sr.InputDocuments = new InputDocuments();
            sr.InputDocuments.Items = new object[] { document };

            return sr;
        }

        private VerifyRequest BuildVerifyRequest<T>(T document, Timestamp timeStamp) where T : DocumentBaseType
        {
            VerifyRequest vr = new VerifyRequest();

            ClaimedIdentity identity = new ClaimedIdentity();
            identity.idAplicacion = _identity.ApplicationId;

            vr.OptionalInputs = new AnyType();
            vr.OptionalInputs.Any = new XmlElement[] { GetXmlElement<ClaimedIdentity>(identity) };

            vr.SignatureObject = new SignatureObject();
            vr.SignatureObject.Item = timeStamp;
            vr.InputDocuments = new InputDocuments();
            vr.InputDocuments.Items = new object[] { document };

            return vr;
        }

        public Timestamp CreateTimeStamp<T>(RequestSignatureType signatureType, T document) where T : DocumentBaseType
        {
            SignRequest request = BuildRequest<T>(signatureType, document, null);

            CreateTimeSoapClient tsaSoapClient = new CreateTimeSoapClient(_baseUrl + "/CreateTimeStampWS", _identity, _serverCert);

            SignResponse response = tsaSoapClient.createTimeStamp(request);

            if (ResultType.Success.Equals(response.Result.ResultMajor))
            {
                return response.SignatureObject.Item as Timestamp;
            }
            else
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMessage.Value);
            }
        }

        public Timestamp RenewTimeStamp<T>(RequestSignatureType signatureType, Timestamp previousTimestamp, T document) where T : DocumentBaseType
        {
            SignRequest request = BuildRequest<T>(signatureType, document, previousTimestamp);

            RenewTimeSoapClient tsaSoapClient = new RenewTimeSoapClient(_baseUrl + "/RenewTimeStampWS", _identity, _serverCert);

            SignResponse response = tsaSoapClient.renewTimeStamp(request);

            if (ResultType.Success.Equals(response.Result.ResultMajor))
            {
                return response.SignatureObject.Item as Timestamp;
            }
            else
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMessage.Value);
            }
        }

        public void VerifyTimestamp<T>(T document, Timestamp timeStamp) where T : DocumentBaseType
        {
            VerifyRequest request = BuildVerifyRequest<T>(document, timeStamp);

            VerifyTimeSoapClient tsaSoapClient = new VerifyTimeSoapClient(_baseUrl + "/VerifyTimeStampWS", _identity, _serverCert);

            VerifyResponse response = tsaSoapClient.verifyTimeStamp(request);

            if (!ResultType.Success.Equals(response.Result.ResultMajor))
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMessage.Value);
            }
        }
    }
}
