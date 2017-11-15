using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.Schemas
{
    public class ResultType
    {
        private string _uri;

        public string Uri
        {
            get
            {
                return _uri;
            }
        }

        public static ResultType Success = new ResultType("urn:oasis:names:tc:dss:1.0:resultmajor:Success");
        public static ResultType RequesterError = new ResultType("urn:oasis:names:tc:dss:1.0:resultmajor:RequesterError");
        public static ResultType ResponderError = new ResultType("urn:oasis:names:tc:dss:1.0:resultmajor:ResponderError");
        public static ResultType Warning = new ResultType("urn:oasis:names:tc:dss:1.0:resultmajor:Warning");

        // Resultados de afirma
        public static ResultType ValidSignature = new ResultType("urn:afirma:dss:1.0:profile:XSS:resultmajor:ValidSignature");
        public static ResultType InvalidSignature = new ResultType("urn:afirma:dss:1.0:profile:XSS:resultmajor:InvalidSignature");

        internal ResultType(string uri)
        {
            _uri = uri;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return obj.Equals(_uri);
            }
            
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
