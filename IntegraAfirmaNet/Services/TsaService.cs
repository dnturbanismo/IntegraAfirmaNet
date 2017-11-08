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
    public class TsaService: BaseService
    {       
        public TsaService(string url, Identity identity)
            :base(url, identity)
        {
        }

        public TsaService(string url, Identity identity, X509Certificate2 serverCert) 
            :base(url, identity, serverCert)
        {

        }

        private SignRequest BuildRequest(byte[] hashValue, string algorithm)
        {
            SignRequest sr = new SignRequest();

            ClaimedIdentity identity = new ClaimedIdentity();
            identity.idAplicacion = _identity.ApplicationId;
         
            sr.OptionalInputs = new AnyType();
            sr.OptionalInputs.Any = new XmlElement[] { GetXmlElement<ClaimedIdentity>(identity) };

            DocumentHash documentHash = new DocumentHash();
            documentHash.DigestMethod = new DigestMethodType();
            documentHash.DigestMethod.Algorithm = algorithm;
            documentHash.DigestValue = hashValue;
            sr.InputDocuments = new InputDocuments();
            sr.InputDocuments.Items = new object[] { documentHash };

            return sr;
        }


        public Timestamp CreateTimeStamp(byte[] hashValue, string algorithm)
        {
            SignRequest request = BuildRequest(hashValue, algorithm);

            TsaSoapClient tsaSoapClient = new TsaSoapClient(_baseUrl + "/CreateTimeStampWS", _identity, _serverCert);

            SignResponse response = tsaSoapClient.createTimeStamp(request);

            if (response.Result.ResultMajor == "urn:oasis:names:tc:dss:1.0:resultmajor:Success")
            {
                return response.SignatureObject.Item as Timestamp;
            }
            else
            {
                throw new AfirmaResultException(response.Result.ResultMajor, response.Result.ResultMessage.Value);
            }
        }
    }
}
