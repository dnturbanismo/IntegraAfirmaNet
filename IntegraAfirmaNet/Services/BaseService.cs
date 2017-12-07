using IntegraAfirmaNet.Authentication;
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
    public abstract class BaseService
    {
        protected Identity _identity = null;
        protected X509Certificate2 _serverCert = null;

        protected XmlElement GetXmlElement(object source)
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

        protected XmlElement GetXmlElement(string xml)
        {
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;

            document.LoadXml(xml);

            return document.DocumentElement;
        }

        protected T DeserializeXml<T>(string xml)
        {
            using (MemoryStream ms = new MemoryStream(UTF8Encoding.UTF8.GetBytes(xml)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T result = (T)serializer.Deserialize(ms);

                return result;
            }
        }

        public BaseService(Identity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity", "El valor no puede ser nulo.");
            }
            
            _identity = identity;            
        }

        public BaseService(Identity identity, X509Certificate2 serverCert) :
            this(identity)
        {
            if (serverCert == null)
            {
                throw new ArgumentNullException("serverCert", "El valor no puede ser nulo.");
            }
            
            _serverCert = serverCert;
        }

    }
}
