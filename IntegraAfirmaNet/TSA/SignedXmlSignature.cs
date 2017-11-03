using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace IntegraAfirmaNet.TSA
{
///<summary>
///Clase de acceso a la firma electronica XML. Permite acceso a los elementos <code>SignedInfo</code>,<code>KeyInfo</code>, <code>Objects</code>, etc
///</summary>

    public class SignedXmlSignature
    {
        private ArrayList list;
        private SignedInfo info;
        private KeyInfo key;
        private string id;
        private byte[] signature;

        public SignedXmlSignature()
        {
            list = new ArrayList();
        }

        /// <summary>
        /// Acceso al atributo Id del elemento <code>Signature</code>.
        /// </summary>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Acceso al elemento <code>KeyInfo</code>.
        /// </summary>
        /// <remarks>Este elemento no ha sido modificado respecto a la distribución proporcionada por el Framework .NET 2.0</remarks>
        /// <see cref="KeyInfo"/>
        public KeyInfo KeyInfo
        {
            get { return key; }
            set { key = value; }
        }
        /// <summary>
        /// Acceso a los elemento <code>Object</code>.
        /// </summary>
        public IList ObjectList
        {
            get { return list; }
            set { list = ArrayList.Adapter(value); }
        }

        /// <summary>
        /// Acceso al contenido del elemento <code>SignatureValue</code>.
        /// </summary>
        /// <remarks>Se ha incluido el prefijo en la serialización.</remarks>
        public byte[] SignatureValue
        {
            get { return signature; }
            set { signature = value; }
        }

        /// <summary>
        /// Acceso al elemento <code>SignedInfo</code>. Este elemento ha sido modificado completamente, implementándose dentro del SignatureFramework.
        /// </summary>
        /// <remarks>Se ha incluido el prefijo en la serialización.</remarks>
        /// <see cref="SignedInfo"/>
        public SignedInfo SignedInfo
        {
            get { return info; }
            set { info = value; }
        }

        /// <summary>
        /// Añade un elemento <code>Object</code> directamente y lo inserta en la lista de objetos.
        /// </summary>
        /// <param name="dataObject">Objecto a insertar</param>
        public void AddObject(DataObject dataObject)
        {
            list.Add(dataObject);
        }

        /// <summary>
        /// Calcula los valores necesarios para el elemento <code>ds:Signature</code>
        /// </summary>
        /// <returns>Elemento ds:Signature asociado al objeto</returns>
        public XmlElement GetXml()
        {
            if (info == null)
                throw new CryptographicException("SignedInfo");
            if (signature == null)
                throw new CryptographicException("SignatureValue");

            XmlDocument document = new XmlDocument();
            XmlElement xel = document.CreateElement(XmlSignatureConstants.Prefix+":"+XmlSignatureConstants.ElementNames.Signature, XmlSignatureConstants.NamespaceURI);
            if (id != null)
                xel.SetAttribute(XmlSignatureConstants.AttributeNames.Id, id);

            XmlNode xn = info.GetXml();
            XmlNode newNode = document.ImportNode(xn, true);
            xel.AppendChild(newNode);

            if (signature != null)
            {
                XmlElement sv = document.CreateElement(XmlSignatureConstants.Prefix,XmlSignatureConstants.ElementNames.SignatureValue, XmlSignatureConstants.NamespaceURI);
                sv.InnerText = Convert.ToBase64String(signature);
                xel.AppendChild(sv);
            }

            if (key != null)
            {
                xn = key.GetXml();
                newNode = document.ImportNode(xn, true);
                xel.AppendChild(newNode);
            }

            if (list.Count > 0)
            {
                foreach (DataObject obj in list)
                {
                    xn = obj.GetXml();
                    newNode = document.ImportNode(xn, true);
                    xel.AppendChild(newNode);
                }
            }

            return xel;
        }

        private string GetAttribute(XmlElement xel, string attribute)
        {
            XmlAttribute xa = xel.Attributes[attribute];
            return ((xa != null) ? xa.InnerText : null);
        }

        /// <summary>
        /// Carga un elemento <code>ds:Signature</code>
        /// </summary>
        public void LoadXml(XmlElement value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if ((value.LocalName == XmlSignatureConstants.ElementNames.Signature) && (value.NamespaceURI == XmlSignatureConstants.NamespaceURI))
            {
                id = GetAttribute(value, XmlSignatureConstants.AttributeNames.Id);

                XmlNodeList xnl = value.GetElementsByTagName(XmlSignatureConstants.ElementNames.SignedInfo);
                if ((xnl != null) && (xnl.Count == 1))
                {
                    info = new SignedInfo();
                    info.LoadXml((XmlElement)xnl[0]);
                }

                xnl = value.GetElementsByTagName(XmlSignatureConstants.ElementNames.SignatureValue);
                if ((xnl != null) && (xnl.Count == 1))
                {
                    signature = Convert.FromBase64String(xnl[0].InnerText);
                }

                xnl = value.GetElementsByTagName(XmlSignatureConstants.ElementNames.KeyInfo);
                if ((xnl != null) && (xnl.Count == 1))
                {
                    key = new KeyInfo();
                    key.LoadXml((XmlElement)xnl[0]);
                }

                xnl = value.GetElementsByTagName(XmlSignatureConstants.ElementNames.Object);
                if ((xnl != null) && (xnl.Count > 0))
                {
                    foreach (XmlNode xn in xnl)
                    {
                        DataObject obj = new DataObject();
                        obj.LoadXml((XmlElement)xn);
                        AddObject(obj);
                    }
                }
            }
            if (info == null)
                throw new CryptographicException("SignedInfo");
            if (signature == null)
                throw new CryptographicException("SignatureValue");
        }
    }
}
