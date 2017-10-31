using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.SignatureFramework
{
    public class ReturnUpdatedSignatureType
    {
        private string _resourceName;

        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
        }

        public static ReturnUpdatedSignatureType AdES_T = new ReturnUpdatedSignatureType("urn:oasis:names:tc:dss:1.0:profiles:AdES:forms:ES-T");
        public static ReturnUpdatedSignatureType AdES_XL = new ReturnUpdatedSignatureType("urn:oasis:names:tc:dss:1.0:profiles:AdES:forms:ES-X-L");
        public static ReturnUpdatedSignatureType AdES_A = new ReturnUpdatedSignatureType("urn:oasis:names:tc:dss:1.0:profiles:AdES:forms:ES-A");
        public static ReturnUpdatedSignatureType PAdES_LTV = new ReturnUpdatedSignatureType("urn:afirma:dss:1.0:profile:XSS:PAdES:1.1.2:forms:LTV");

        // Nuevo reglamento eIDAS
        public static ReturnUpdatedSignatureType AdES_T_Level = new ReturnUpdatedSignatureType("urn:afirma:dss:1.0:profile:XSS:AdES:forms:T-Level");
        public static ReturnUpdatedSignatureType AdES_LT_Level = new ReturnUpdatedSignatureType("urn:afirma:dss:1.0:profile:XSS:AdES:forms:LT-Level");
        public static ReturnUpdatedSignatureType AdES_LTA_Level = new ReturnUpdatedSignatureType("urn:afirma:dss:1.0:profile:XSS:AdES:forms:LTA-Level");


        private ReturnUpdatedSignatureType(string resourceName)
        {
            _resourceName = resourceName;
        }

        public static ReturnUpdatedSignatureType GetReturnUpdatedSignatureType(string urn)
        {
            if (urn == AdES_T.ResourceName)
            {
                return AdES_T;
            }
            else if (urn == AdES_XL.ResourceName)
            {
                return AdES_XL;
            }
            else if (urn == AdES_A.ResourceName)
            {
                return AdES_A;
            }
            else if (urn == PAdES_LTV.ResourceName)
            {
                return PAdES_LTV;
            }
            else
            {
                throw new Exception("URN desconocido");
            }
        }
    }
}
