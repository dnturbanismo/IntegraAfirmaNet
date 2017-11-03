using System;
using System.Security.Cryptography;
using System.Xml;

namespace IntegraAfirmaNet.TSA
{


        // http://www.w3.org/TR/2002/REC-xmldsig-core-20020212/Overview.html#sec-Reference
        public class Reference
        {
            private TransformChain chain;
            private string digestMethod;
            private byte[] digestValue;
            private string id;
            private string uri;
            private string type;
            private HashAlgorithm hash;

            public Reference()
            {
                chain = new TransformChain();
                digestMethod = XmlSignatureConstants.NamespaceURI + "sha1";
            }

            public Reference(string uri)
                : this()
            {
                this.uri = uri;
            }

            public string DigestMethod
            {
                get { return digestMethod; }
                set { digestMethod = value; }
            }

            public byte[] DigestValue
            {
                get { return digestValue; }
                set { digestValue = value; }
            }

            public string Id
            {
                get { return id; }
                set { id = value; }
            }

            public TransformChain TransformChain
            {
                get { return chain; }
            }

            public string Type
            {
                get { return type; }
                set { type = value; }
            }

            public string Uri
            {
                get { return uri; }
                set { uri = value; }
            }

            public void AddTransform(Transform transform)
            {
                chain.Add(transform);
            }

            public XmlElement GetXml()
            {
                if (digestMethod == null)
                    throw new CryptographicException("DigestMethod");
                if (digestValue == null)
                    throw new NullReferenceException("DigestValue");

                XmlDocument document = new XmlDocument();
                XmlElement xel = document.CreateElement(XmlSignatureConstants.Prefix+":"+XmlSignatureConstants.ElementNames.Reference, XmlSignatureConstants.NamespaceURI);
                if (id != null)
                    xel.SetAttribute(XmlSignatureConstants.AttributeNames.Id, id);
                if (uri != null)
                    xel.SetAttribute(XmlSignatureConstants.AttributeNames.URI, uri);
                if (type != null)
                    xel.SetAttribute(XmlSignatureConstants.AttributeNames.Type, type);

                if (chain.Count > 0)
                {
                    XmlElement ts = document.CreateElement(XmlSignatureConstants.Prefix+":"+XmlSignatureConstants.ElementNames.Transforms, XmlSignatureConstants.NamespaceURI);
                    foreach (Transform t in chain)
                    {
                        XmlNode xn = t.GetXml();
                        ts.AppendChild(document.ImportNode(xn, true));
                    }
                    xel.AppendChild(ts);
                }

                XmlElement dm = document.CreateElement(XmlSignatureConstants.Prefix+":"+XmlSignatureConstants.ElementNames.DigestMethod, XmlSignatureConstants.NamespaceURI);
                dm.SetAttribute(XmlSignatureConstants.AttributeNames.Algorithm, digestMethod);
                xel.AppendChild(dm);

                XmlElement dv = document.CreateElement(XmlSignatureConstants.Prefix+":"+XmlSignatureConstants.ElementNames.DigestValue, XmlSignatureConstants.NamespaceURI);
                dv.InnerText = Convert.ToBase64String(digestValue);
                xel.AppendChild(dv);

                return xel;
            }

            private string GetAttributeFromElement(XmlElement xel, string attribute, string element)
            {
                string result = null;
                XmlNodeList xnl = xel.GetElementsByTagName(element);
                if ((xnl != null) && (xnl.Count > 0))
                {
                    XmlAttribute xa = xnl[0].Attributes[attribute];
                    if (xa != null)
                        result = xa.InnerText;
                }
                return result;
            }

            private string GetAttribute(XmlElement xel, string attribute)
            {
                XmlAttribute xa = xel.Attributes[attribute];
                return ((xa != null) ? xa.InnerText : null);
            }

            public void LoadXml(XmlElement value)
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if ((value.LocalName != XmlSignatureConstants.ElementNames.Reference) || (value.NamespaceURI != XmlSignatureConstants.NamespaceURI))
                    throw new CryptographicException();

                id = GetAttribute(value, XmlSignatureConstants.AttributeNames.Id);
                uri = GetAttribute(value, XmlSignatureConstants.AttributeNames.URI);
                type = GetAttribute(value, XmlSignatureConstants.AttributeNames.Type);
                // Note: order is important for validations
                XmlNodeList xnl = value.GetElementsByTagName(XmlSignatureConstants.ElementNames.Transform);
                if ((xnl != null) && (xnl.Count > 0))
                {
                    Transform t = null;
                    foreach (XmlNode xn in xnl)
                    {
                        string a = GetAttribute((XmlElement)xn, XmlSignatureConstants.AttributeNames.Algorithm);
                        switch (a)
                        {
                            //case "http://www.w3.org/2000/09/xmldsig#base64":
                            //    t = new XmlDsigBase64Transform();
                            //    break;
                            case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315":
                                t = new XmlDsigC14NTransform();
                                break;
                            case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments":
                                t = new XmlDsigC14NWithCommentsTransform();
                                break;
                            case "http://www.w3.org/2001/10/xml-exc-c14n#":
                                t = new XmlDsigExcC14NTransform();
                                break;
                            case "http://www.w3.org/2001/10/xml-exc-c14n#WithComments":
                                t = new XmlDsigExcC14NWithCommentsTransform();
                                break;
                            //case "http://www.w3.org/2000/09/xmldsig#enveloped-signature":
                            //    t = new XmlDsigEnvelopedSignatureTransform();
                            //    break;
                            //case "http://www.w3.org/TR/1999/REC-xpath-19991116":
                            //    t = new XmlDsigXPathTransform();
                            //    break;
                            //case "http://www.w3.org/TR/1999/REC-xslt-19991116":
                            //    t = new XmlDsigXsltTransform();
                            //    break;
                            default:
                                throw new NotSupportedException();
                        }
                        AddTransform(t);
                    }
                }
                // get DigestMethod
                DigestMethod = GetAttributeFromElement(value, XmlSignatureConstants.AttributeNames.Algorithm, XmlSignatureConstants.ElementNames.DigestMethod);
                // get DigestValue
                xnl = value.GetElementsByTagName(XmlSignatureConstants.ElementNames.DigestValue);
                if ((xnl != null) && (xnl.Count > 0))
                {
                    DigestValue = Convert.FromBase64String(xnl[0].InnerText);
                }
            }
        }
    }