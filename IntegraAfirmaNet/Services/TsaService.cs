﻿using IntegraAfirmaNet.Authentication;
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
        public TsaService(Identity identity)
            : base(identity)
        {
        }

        public TsaService(Identity identity, X509Certificate2 serverCert)
            : base(identity, serverCert)
        {

        }

        private SignRequest BuildRequest(RequestSignatureType signatureType, DocumentBaseType document, Timestamp previousTimestamp)
        {
            SignRequest sr = new SignRequest();

            ClaimedIdentity identity = new ClaimedIdentity();
            identity.idAplicacion = _identity.ApplicationId;

            TimestampSignatureType signatureTypeElement = new TimestampSignatureType() { Value = signatureType.Uri };

            sr.OptionalInputs = new AnyType();

            if (previousTimestamp != null)
            {
                string renewXml = "<dst:RenewTimestamp xmlns:dst=\"urn:oasis:names:tc:dss:1.0:profiles:TimeStamp:schema#\"><dst:PreviousTimestamp>{0}</dst:PreviousTimestamp></dst:RenewTimestamp>";

                XmlElement previousTimestampElement = GetXmlElement(string.Format(renewXml, GetXmlElement(previousTimestamp).OuterXml));

                sr.OptionalInputs.Any = new XmlElement[] { GetXmlElement(signatureTypeElement), GetXmlElement(identity), previousTimestampElement };
            }
            else
            {
                sr.OptionalInputs.Any = new XmlElement[] { GetXmlElement(signatureTypeElement), GetXmlElement(identity) };
            }

            sr.InputDocuments = new InputDocuments();
            sr.InputDocuments.Items = new object[] { document };

            return sr;
        }

        private VerifyRequest BuildVerifyRequest(DocumentBaseType document, Timestamp timeStamp, bool returnProcessingDetails)
        {
            VerifyRequest vr = new VerifyRequest();

            IntegraAfirmaNet.Schemas.ClaimedIdentity identity = new IntegraAfirmaNet.Schemas.ClaimedIdentity();
            identity.idAplicacion = _identity.ApplicationId;

            vr.OptionalInputs = new AnyType();

            if (returnProcessingDetails)
            {
                vr.OptionalInputs.Any = new XmlElement[] { GetXmlElement(identity), GetXmlElement("<dss:ReturnProcessingDetails xmlns:dss=\"urn:oasis:names:tc:dss:1.0:core:schema\"/>") };
            }
            else
            {
                vr.OptionalInputs.Any = new XmlElement[] { GetXmlElement(identity) };
            }
            
            vr.SignatureObject = new SignatureObject();
            vr.SignatureObject.Item = timeStamp;
            vr.InputDocuments = new InputDocuments();
            vr.InputDocuments.Items = new object[] { document };

            return vr;
        }

        public Timestamp CreateTimeStamp(RequestSignatureType signatureType, DocumentBaseType document)
        {
            if (signatureType == null)
            {
                throw new ArgumentNullException("signatureType", "El valor no puede ser nulo.");
            }

            if (document == null)
            {
                throw new ArgumentNullException("document", "El valor no puede ser nulo.");
            } 
            
            SignRequest request = BuildRequest(signatureType, document, null);

            CreateTimeSoapClient tsaSoapClient = new CreateTimeSoapClient(_identity, _serverCert);

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

        public Timestamp RenewTimeStamp(Timestamp previousTimestamp, DocumentBaseType document)
        {
            if (previousTimestamp == null)
            {
                throw new ArgumentNullException("previousTimestamp", "El valor no puede ser nulo.");
            } 

            if (document == null)
            {
                throw new ArgumentNullException("document", "El valor no puede ser nulo.");
            } 
            
            SignRequest request = BuildRequest(RequestSignatureType.XML, document, previousTimestamp);

            RenewTimeSoapClient tsaSoapClient = new RenewTimeSoapClient(_identity, _serverCert);

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

        public VerifyResponse VerifyTimestamp(DocumentBaseType document, Timestamp timeStamp, bool returnProcessingDetails = false)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document", "El valor no puede ser nulo.");
            }

            if (timeStamp == null)
            {
                throw new ArgumentNullException("timeStamp", "El valor no puede ser nulo.");
            }
            
            VerifyRequest request = BuildVerifyRequest(document, timeStamp, returnProcessingDetails);

            VerifyTimeSoapClient tsaSoapClient = new VerifyTimeSoapClient(_identity, _serverCert);

            VerifyResponse response = tsaSoapClient.verifyTimeStamp(request);

            if (ResultType.RequesterError.Equals(response.Result.ResultMajor) ||
                ResultType.ResponderError.Equals(response.Result.ResultMajor))
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMinor, response.Result.ResultMessage.Value);
            }

            return response;
        }
    }
}
