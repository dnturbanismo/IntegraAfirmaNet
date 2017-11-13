using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.Schemas
{
    public class RequestSignatureType
    {
        private string _uri;

        public string Uri
        {
            get
            {
                return _uri;
            }
        }

        public static RequestSignatureType XML = new RequestSignatureType("urn:oasis:names:tc:dss:1.0:core:schema:XMLTimeStampToken");
        public static RequestSignatureType ASN1 = new RequestSignatureType("urn:ietf:rfc:3161");

        private RequestSignatureType(string uri)
        {
            _uri = uri;
        }
    }
}
