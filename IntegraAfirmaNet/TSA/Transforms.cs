using System;
using System.Collections;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Xml;

namespace IntegraAfirmaNet.TSA
{
    /// <summary>
    /// Transformada de canonicalización exclusiva sin comentarios
    /// </summary>
    public class XmlDsigExcC14NTransform : Transform
    {

        private Type[] input;
        private Type[] output;
        private XmlCanonicalizer canonicalizer;
        private Stream s;
        private string prefixList;
        private bool comments;

        public XmlDsigExcC14NTransform()
            : this(false)
        {
        }

        protected XmlDsigExcC14NTransform(bool includeComments)
        {
            comments = includeComments;
            canonicalizer = new XmlCanonicalizer(includeComments, true);
            Algorithm = XmlSignatureConstants.XmlDsigExcC14NTransformUrl;
        }

        public XmlDsigExcC14NTransform(string inclusiveNamespacesPrefixList)
            : this(false, inclusiveNamespacesPrefixList)
        {
        }

        protected XmlDsigExcC14NTransform(bool includeComments, string inclusiveNamespacesPrefixList)
            : this(includeComments)
        {
            prefixList = inclusiveNamespacesPrefixList;
        }

        public string InclusiveNamespacesPrefixList
        {
            get { return prefixList; }
            set { prefixList = value; }
        }

        public override Type[] InputTypes
        {
            get
            {
                if (input == null)
                {
                    lock (this)
                    {
                        input = new Type[3];
                        input[0] = typeof(System.IO.Stream);
                        input[1] = typeof(System.Xml.XmlDocument);
                        input[2] = typeof(System.Xml.XmlNodeList);
                    }
                }
                return input;
            }
        }

        public override Type[] OutputTypes
        {
            get
            {
                if (output == null)
                {
                    lock (this)
                    {
                        output = new Type[1];
                        output[0] = typeof(System.IO.Stream);
                    }
                }
                return output;
            }
        }
        protected override XmlNodeList GetInnerXml()
        {
            return null;
        }

        public override object GetOutput()
        {
            return (object)s;
        }

        public override object GetOutput(Type type)
        {
            if (type != typeof(Stream))
                throw new ArgumentException("type");
            return GetOutput();
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {

        }

        public override void LoadInput(object obj)
        {
            Reset();
            if (obj is Stream)
            {
                s = (obj as Stream);
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(obj as Stream);
                s = canonicalizer.Canonicalize(doc);
            }
            else if (obj is XmlDocument)
                s = canonicalizer.Canonicalize((obj as XmlDocument));
            else if (obj is XmlNodeList)
                s = canonicalizer.Canonicalize((obj as XmlNodeList));
        }

        public override void Reset()
        {
            canonicalizer.Reset();
        }
    }

    /// <summary>
    /// Transformada de canonicalización inclusiva con comentarios
    /// </summary>
    public class XmlDsigExcC14NWithCommentsTransform : XmlDsigExcC14NTransform
    {

        public XmlDsigExcC14NWithCommentsTransform()
            : base(true)
        {
            Algorithm = XmlSignatureConstants.XmlDsigExcC14NWithCommentsTransformUrl;
        }

        public XmlDsigExcC14NWithCommentsTransform(string inclusiveNamespacesPrefixList)
            : base(true, inclusiveNamespacesPrefixList)
        {
            Algorithm = XmlSignatureConstants.XmlDsigC14NWithCommentsTransformUrl;
        }
    }

    /// <summary>
    /// Transformada de canonicalización inclusiva sin comentarios
    /// </summary>
    public class XmlDsigC14NTransform : Transform
    {

        private Type[] input;
        private Type[] output;
        private XmlCanonicalizer canonicalizer;
        private Stream s;
        private string prefixList;
        private bool comments;

        public XmlDsigC14NTransform()
            : this(false)
        {
        }

        public XmlDsigC14NTransform(bool includeComments)
        {
            comments = includeComments;
            canonicalizer = new XmlCanonicalizer(includeComments, false);
            Algorithm = XmlSignatureConstants.XmlDsigC14NTransformUrl;
        }

        public XmlDsigC14NTransform(string inclusiveNamespacesPrefixList)
            : this(false, inclusiveNamespacesPrefixList)
        {
        }

        public XmlDsigC14NTransform(bool includeComments, string inclusiveNamespacesPrefixList)
            : this(includeComments)
        {
            prefixList = inclusiveNamespacesPrefixList;
        }

        public string InclusiveNamespacesPrefixList
        {
            get { return prefixList; }
            set { prefixList = value; }
        }

        public override Type[] InputTypes
        {
            get
            {
                if (input == null)
                {
                    lock (this)
                    {
                        input = new Type[3];
                        input[0] = typeof(System.IO.Stream);
                        input[1] = typeof(System.Xml.XmlDocument);
                        input[2] = typeof(System.Xml.XmlNodeList);
                    }
                }
                return input;
            }
        }

        public override Type[] OutputTypes
        {
            get
            {
                if (output == null)
                {
                    lock (this)
                    {
                        output = new Type[1];
                        output[0] = typeof(System.IO.Stream);
                    }
                }
                return output;
            }
        }
        protected override XmlNodeList GetInnerXml()
        {
            return null;
        }

        public override object GetOutput()
        {
            return (object)s;
        }

        public override object GetOutput(Type type)
        {
            if (type != typeof(Stream))
                throw new ArgumentException("type");
            return GetOutput();
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
            // documented as not changing the state of the transform
        }

        public override void LoadInput(object obj)
        {
            Reset();
            if (obj is Stream)
            {
                s = (obj as Stream);
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(obj as Stream);
                s = canonicalizer.Canonicalize(doc);
            }
            else if (obj is XmlDocument)
                s = canonicalizer.Canonicalize((obj as XmlDocument));
            else if (obj is XmlNodeList)
                s = canonicalizer.Canonicalize((obj as XmlNodeList));
        }

        public override void Reset()
        {
            canonicalizer.Reset();
        }
    }

    /// <summary>
    /// Transformada de canonicalización inclusiva con comentarios
    /// </summary>
    public class XmlDsigC14NWithCommentsTransform : XmlDsigC14NTransform
    {

        public XmlDsigC14NWithCommentsTransform()
            : base(true)
        {
            Algorithm = XmlSignatureConstants.XmlDsigC14NWithCommentsTransformUrl;
        }

        public XmlDsigC14NWithCommentsTransform(string inclusiveNamespacesPrefixList)
            : base(true, inclusiveNamespacesPrefixList)
        {
            Algorithm = XmlSignatureConstants.XmlDsigC14NWithCommentsTransformUrl;
        }
    }


    /// <summary>
    /// Clase abstracta para la implementación de transformadas. 
    /// </summary>
    /// <remarks>
    /// Inicialmente sólo se han implementado las transformadas de canonicalización, ya que no son necesarias otras para 
    /// la interacción con @firma5
    /// </remarks>
    public abstract class Transform
    {

        private string algo;
        private XmlResolver xmlResolver;

		protected Transform ()
        {
            if (SecurityManager.SecurityEnabled)
            {
                xmlResolver = new XmlSecureResolver(new XmlUrlResolver(), (Evidence)new Evidence());
            }
            else
            {
                xmlResolver = new XmlUrlResolver();
            }
        }

        /// <summary>
        /// Algoritmo de canonicalizacion.
        /// </summary>
        public string Algorithm
        {
            get { return algo; }
            set { algo = value; }
        }

        public abstract Type[] InputTypes
        {
            get;
        }

        public abstract Type[] OutputTypes
        {
            get;
        }

		public virtual byte[] GetDigestedOutput (HashAlgorithm hash)
		{
			return hash.ComputeHash ((Stream) GetOutput (typeof (Stream)));
		}

        protected abstract XmlNodeList GetInnerXml();

        public abstract object GetOutput();

        public abstract object GetOutput(Type type);

        /// <summary>
        /// Proporciona el elemento asociado a la transformación implementada. Se ha mejorado la serialización.
        /// </summary>
        /// <returns>Elemento totalmente qualificado.</returns>
        public XmlElement GetXml()
        {
            XmlDocument document = new XmlDocument();
            document.XmlResolver = GetResolver();
            XmlElement xel = document.CreateElement(XmlSignatureConstants.Prefix,XmlSignatureConstants.ElementNames.Transform, XmlSignatureConstants.NamespaceURI);
            xel.SetAttribute(XmlSignatureConstants.AttributeNames.Algorithm, algo);
            XmlNodeList xnl = this.GetInnerXml();
            if (xnl != null)
            {
                foreach (XmlNode xn in xnl)
                {
                    XmlNode importedNode = document.ImportNode(xn, true);
                    xel.AppendChild(importedNode);
                }
            }
            return xel;
        }

        /// <summary>
        /// Carga el objeto a canonicalizar
        /// </summary>
        /// <param name="obj">De forma genérica se admiten: Stream, XmlElement y XmlNodeList</param>
        public abstract void LoadInput(object obj);

        internal XmlResolver GetResolver()
        {
            return xmlResolver;
        }

        /// <summary>
        /// Resetea el motor de canonicalización XML
        /// </summary>
        public abstract void Reset();

        public abstract void LoadInnerXml(XmlNodeList nodeList);
    }

    

    /// <summary>
    /// Cadena de transformadas.
    /// </summary>
    public class TransformChain
    {

        private ArrayList chain;

        public TransformChain()
        {
            chain = new ArrayList();
        }

        public int Count
        {
            get { return chain.Count; }
        }

        public Transform this[int index]
        {
            get { return (Transform)chain[index]; }
        }

        public void Add(Transform transform)
        {
            chain.Add(transform);
        }

        public IEnumerator GetEnumerator()
        {
            return chain.GetEnumerator();
        }
    }

}
