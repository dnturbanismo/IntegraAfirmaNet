using System;
using System.Collections;
using System.Security.Cryptography;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Xml.XPath;
using System.IO;
using System.Collections.Generic;


namespace IntegraAfirmaNet.SignatureFramework
{

    public class SignedXml
    {
        private SignedXmlSignature signature;

        public SignedXml()
        {
            signature = new SignedXmlSignature();
            signature.SignedInfo = new SignedInfo();
        }

        private AsymmetricAlgorithm key;
        private string keyName;
        private XmlDocument envdoc;

        public SignedXml(XmlDocument document)
            : this()
        {
            envdoc = document;
        }

        public SignedXml(XmlElement elem)
            : this()
        {
            if (elem == null)
                throw new ArgumentNullException("elem");
        }

        public KeyInfo KeyInfo
        {
            get { return signature.KeyInfo; }
            set { signature.KeyInfo = value; }
        }

        public SignedXmlSignature Signature
        {
            get { return signature; }
        }

        public string SignatureLength
        {
            get { return signature.SignedInfo.SignatureLength; }
        }

        /// <summary>
        /// Método de firma. Por defecto se coge el asociado al par de claves.
        /// </summary>
        public string SignatureMethod
        {
            get { return signature.SignedInfo.SignatureMethod; }
            set { signature.SignedInfo.SignatureMethod = value; }
        }

        public byte[] SignatureValue
        {
            get { return signature.SignatureValue; }
        }

        /// <summary>
        /// Objeto SignedInfo, este objeto ha sido completamente reformado para incluir los prefijos, 
        /// ya que se observaban ciertas anomalías en la generación de firmas 
        /// </summary>
        public SignedInfo SignedInfo
        {
            get { return signature.SignedInfo; }
            set { signature.SignedInfo = value; }
        }

        /// <summary>
        /// Par de claves de firma electrónica
        /// </summary>
        public AsymmetricAlgorithm SigningKey
        {
            get { return key; }
            set { key = value; }
        }

        public string SigningKeyName
        {
            get { return keyName; }
            set { keyName = value; }
        }

        public void AddObject(DataObject dataObject)
        {
            signature.AddObject(dataObject);
        }

        /// <summary>
        /// Añade un objeto de tipo <code>Reference</code>
        /// </summary>
        /// <param name="reference"></param>
        public void AddReference(System.Security.Cryptography.Xml.Reference reference)
        {
            signature.SignedInfo.AddReference(reference);
        }

        private Stream ApplyTransform(System.Security.Cryptography.Xml.Transform t, XmlDocument doc)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(doc.OuterXml);

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                t.LoadInput(ms);
                return (MemoryStream)t.GetOutput(typeof(Stream));
            }
        }

        private Stream ApplyTransform(System.Security.Cryptography.Xml.Transform t, Stream s)
        {
            try
            {
                t.LoadInput(s);
                s = (Stream)t.GetOutput();
            }
            catch (Exception e)
            {
                string temp = e.ToString();
            }
            return s;
        }

        private byte[] GetReferenceHash(System.Security.Cryptography.Xml.Reference r)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            if (r.Uri == "")
                doc = envdoc;
            else
            {
                XmlElement element = GetIdElement(envdoc, r.Uri.Remove(0, 1));
                doc.LoadXml(element.OuterXml);

                if (r.TransformChain.Count > 0)
                {
                    var namespaces = GetAllNamespaces(element);

                    foreach (var item in namespaces)
                    {
                        AddNameSpace(doc, doc.DocumentElement, item.Name, item.Value);
                    }
                }
            }

            Stream s = null;
            if (r.TransformChain.Count > 0)
            {
                foreach (System.Security.Cryptography.Xml.Transform t in r.TransformChain)
                {
                    if (s == null)
                        s = ApplyTransform(t, doc);
                    else
                        s = ApplyTransform(t, s);
                }
            }
            else
                s = ApplyTransform(new System.Security.Cryptography.Xml.XmlDsigC14NTransform(), doc);

            HashAlgorithm hash = (HashAlgorithm)CryptoConfig.CreateFromName(r.DigestMethod);
            return hash.ComputeHash(s);
        }

        private void DigestReferences()
        {
            foreach (System.Security.Cryptography.Xml.Reference r in signature.SignedInfo.References)
            {
                if (r.DigestMethod == null)
                    r.DigestMethod = XmlSignatureConstants.XmlDsigSHA1Url;
                r.DigestValue = GetReferenceHash(r);
            }
        }

        /// <remarks>Sólo se implementan las transformadas de canonicalización</remarks>

        private Stream SignedInfoTransformed()
        {
            System.Security.Cryptography.Xml.Transform t = null;
            switch (signature.SignedInfo.CanonicalizationMethod)
            {
                case XmlSignatureConstants.XmlDsigC14NTransformUrl:
                    t = new System.Security.Cryptography.Xml.XmlDsigC14NTransform();
                    break;
                case XmlSignatureConstants.XmlDsigC14NWithCommentsTransformUrl:
                    t = new System.Security.Cryptography.Xml.XmlDsigC14NWithCommentsTransform();
                    break;
                case XmlSignatureConstants.XmlDsigExcC14NTransformUrl:
                    t = new System.Security.Cryptography.Xml.XmlDsigExcC14NTransform(signature.SignedInfo.InclusiveNamespaces);
                    break;
                case XmlSignatureConstants.XmlDsigExcC14NWithCommentsTransformUrl:
                    t = new System.Security.Cryptography.Xml.XmlDsigExcC14NWithCommentsTransform();
                    break;
                default:
                    t = null;
                    break;
            }
            if (t == null)
                return null;

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;

            XmlElement signatureNode = (XmlElement)envdoc.SelectSingleNode("//*[@Id='" + signature.Id + "']");

            if (signatureNode != null)
            {
                XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(envdoc.NameTable);
                xmlnsManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

                XmlNode signedInfoNode = signatureNode.SelectSingleNode("ds:SignedInfo", xmlnsManager);

                doc.LoadXml(signedInfoNode.OuterXml);
            }
            else
            {
                doc.LoadXml(signature.SignedInfo.GetXml().OuterXml);
            }

            var namespaces = GetAllNamespaces(signatureNode);

            foreach (var item in namespaces)
            {
                AddNameSpace(doc, doc.DocumentElement, item.Name, item.Value);
            }

            return ApplyTransform(t, doc);
        }

        private void AddNameSpace(XmlDocument doc, XmlNode node, string name, string uri)
        {
            XmlAttribute newAttr = doc.CreateAttribute(name);
            newAttr.Value = uri;

            node.Attributes.Append(newAttr);
        }


        private List<XmlAttribute> GetAllNamespaces(XmlElement fromElement)
        {
            List<XmlAttribute> namespaces = new List<XmlAttribute>();

            if (fromElement != null &&
                fromElement.ParentNode.NodeType == XmlNodeType.Document)
            {
                foreach (XmlAttribute attr in fromElement.Attributes)
                {
                    if (attr.Name.StartsWith("xmlns") && !namespaces.Exists(f => f.Name == attr.Name))
                    {
                        namespaces.Add(attr);
                    }
                }

                return namespaces;
            }

            XmlNode currentNode = fromElement;

            while (currentNode != null && currentNode.NodeType != XmlNodeType.Document)
            {
                foreach (XmlAttribute attr in currentNode.Attributes)
                {
                    if (attr.Name.StartsWith("xmlns") && !namespaces.Exists(f => f.Name == attr.Name))
                    {
                        namespaces.Add(attr);
                    }
                }

                currentNode = currentNode.ParentNode;
            }

            return namespaces;
        }

        private byte[] Hash(string hashAlgorithm)
        {
            HashAlgorithm hash = HashAlgorithm.Create(hashAlgorithm);
            System.IO.BinaryReader reader = new BinaryReader(SignedInfoTransformed());
            byte[] bytes = reader.ReadBytes((int)reader.BaseStream.Length);
            return hash.ComputeHash(bytes);
        }

        /// <summary>
        /// Validación de la firma electrónica contenida
        /// </summary>
        /// <returns>true si se ha validado correctamente</returns>
        public virtual bool CheckSignature()
        {
            if (key == null)
                key = GetPublicKey();
            return CheckSignature(key);
        }

        private bool CheckReferenceIntegrity()
        {
            foreach (System.Security.Cryptography.Xml.Reference r in signature.SignedInfo.References)
            {
                if (!Compare(r.DigestValue, GetReferenceHash(r)))
                    return false;
            }
            return true;
        }

        private SignatureDescription GetSignatureDescription(string signatureMethod)
        {
            SignatureDescription sd = (SignatureDescription)CryptoConfig.CreateFromName(signature.SignedInfo.SignatureMethod);

            if (sd == null)
            {
                if (signature.SignedInfo.SignatureMethod == "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")
                {
                    CryptoConfig.AddAlgorithm(typeof(Microsoft.Xades.RSAPKCS1SHA256SignatureDescription), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
                }
                else if (this.SignedInfo.SignatureMethod == "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512")
                {
                    CryptoConfig.AddAlgorithm(typeof(Microsoft.Xades.RSAPKCS1SHA512SignatureDescription), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512");
                }

                sd = CryptoConfig.CreateFromName(this.SignedInfo.SignatureMethod) as SignatureDescription;
            }

            return sd;
        }

        /// <summary>
        /// Valida la firma electronica utilizando una clave externa
        /// </summary>
        /// <param name="key">Clave publica asociada a la firma electrónica</param>
        /// <returns>true si la firma se valido correctamente</returns>
        public bool CheckSignature(AsymmetricAlgorithm key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            bool result = CheckReferenceIntegrity();
            if (result)
            {
                SignatureDescription sd = GetSignatureDescription(signature.SignedInfo.SignatureMethod);

                byte[] hash = Hash(sd.DigestAlgorithm);
                AsymmetricSignatureDeformatter verifier = sd.CreateDeformatter(key); //(AsymmetricSignatureDeformatter)CryptoConfig.CreateFromName(sd.DeformatterAlgorithm);
                if (verifier != null)
                {
                    //verifier.SetHashAlgorithm(sd.DigestAlgorithm);
                    //verifier.SetKey(key);
                    result = verifier.VerifySignature(hash, signature.SignatureValue);
                }
                else
                    result = false;
            }

            return result;
        }

        private bool Compare(byte[] expected, byte[] actual)
        {
            bool result = ((expected != null) && (actual != null));
            if (result)
            {
                int l = expected.Length;
                result = (l == actual.Length);
                if (result)
                {
                    for (int i = 0; i < l; i++)
                    {
                        if (expected[i] != actual[i])
                            return false;
                    }
                }
            }
            return result;
        }

        public bool CheckSignatureReturningKey(out AsymmetricAlgorithm signingKey)
        {
            if (key == null)
                key = GetPublicKey();
            signingKey = key;
            return CheckSignature(key);
        }

        public void ComputeSignature()
        {
            if (key != null)
            {
                // required before hashing
                DigestReferences();

                SignatureDescription sd = GetSignatureDescription(signature.SignedInfo.SignatureMethod);

                // the hard part - C14Ning the KeyInfo
                byte[] hash = Hash(sd.DigestAlgorithm);
                AsymmetricSignatureFormatter signer = null;

                // in need for a CryptoConfig factory
                if (key is DSA)
                    signer = new DSASignatureFormatter(key);
                else if (key is RSA)
                    signer = sd.CreateFormatter(key); //new RSAPKCS1SignatureFormatter(key);

                /*if (signer != null)
                {
                    if (sd.DigestAlgorithm == typeof(SHA1CryptoServiceProvider).FullName || sd.DigestAlgorithm == typeof(SHA1Cng).FullName)
                    {
                        signer.SetHashAlgorithm("SHA1");
                    }
                    else if (sd.DigestAlgorithm == typeof(SHA256Managed).FullName || sd.DigestAlgorithm == typeof(SHA256Cng).FullName)
                    {
                        signer.SetHashAlgorithm("SHA256");
                    }
                    else if (sd.DigestAlgorithm == typeof(SHA512Managed).FullName || sd.DigestAlgorithm == typeof(SHA512Cng).FullName)
                    {
                        signer.SetHashAlgorithm("SHA512");
                    }

                    signature.SignatureValue = signer.CreateSignature(hash);
                }*/

                signature.SignatureValue = signer.CreateSignature(hash);
            }
        }

        public virtual XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            if (idValue != null && idValue.Length > 0)
            {
                XmlElement result = document.SelectSingleNode("//*[@" + XmlSignatureConstants.AttributeNames.Id + "='" + idValue + "']") as XmlElement;

                if (result == null)
                {
                    XmlNamespaceManager manager = new XmlNamespaceManager(document.NameTable);
                    if (!"".Equals(manager.LookupPrefix("wsu")))
                        manager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                    result = document.SelectSingleNode("//*[@" + "wsu:" + XmlSignatureConstants.AttributeNames.Id + "='" + idValue + "']", manager) as XmlElement;
                }
                return result;
            }
            else
            {
                return document.DocumentElement;
            }
        }

        protected virtual AsymmetricAlgorithm GetPublicKey()
        {
            AsymmetricAlgorithm key = null;
            if (signature.KeyInfo != null)
            {
                foreach (KeyInfoClause kic in signature.KeyInfo)
                {
                    if (kic is DSAKeyValue)
                        key = DSA.Create();
                    else if (kic is RSAKeyValue)
                        key = RSA.Create();

                    if (key != null)
                    {
                        key.FromXmlString(kic.GetXml().InnerXml);
                        break;
                    }
                }
            }
            return key;
        }

        public XmlElement GetXml()
        {
            return signature.GetXml();
        }

        public void LoadXml(XmlElement value)
        {
            signature.LoadXml(value);
        }

        public virtual XmlElement GetXml(XmlDocument document)
        {
            return null;
        }
        private XmlResolver xmlResolver;

        XmlResolver Resolver
        {
            set { xmlResolver = value; }
        }
    }
}
