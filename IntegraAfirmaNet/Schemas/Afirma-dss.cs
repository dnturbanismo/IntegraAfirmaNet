using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.Schemas
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "urn:afirma:dss:1.0:profile:XSS:schema")]
    public partial class SignedDataRefsType
    {

        private SignedDataRefType[] signedDataRefField;

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("SignedDataRef")]
        public SignedDataRefType[] SignedDataRef
        {
            get
            {
                return this.signedDataRefField;
            }
            set
            {
                this.signedDataRefField = value;
            }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "urn:afirma:dss:1.0:profile:XSS:schema")]
    public partial class SignedDataRefType
    {

        private string xPathField;

        private string referenceTypeField;

        private string mimeTypeField;

        private string encodingField;

        private string hashAlgorithmField;

        /// <comentarios/>
        public string XPath
        {
            get
            {
                return this.xPathField;
            }
            set
            {
                this.xPathField = value;
            }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string ReferenceType
        {
            get
            {
                return this.referenceTypeField;
            }
            set
            {
                this.referenceTypeField = value;
            }
        }

        /// <comentarios/>
        public string MimeType
        {
            get
            {
                return this.mimeTypeField;
            }
            set
            {
                this.mimeTypeField = value;
            }
        }

        /// <comentarios/>
        public string Encoding
        {
            get
            {
                return this.encodingField;
            }
            set
            {
                this.encodingField = value;
            }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string HashAlgorithm
        {
            get
            {
                return this.hashAlgorithmField;
            }
            set
            {
                this.hashAlgorithmField = value;
            }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "urn:afirma:dss:1.0:profile:XSS:schema")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:afirma:dss:1.0:profile:XSS:schema")]
    public partial class TargetSigner
    {
        private byte[] valueField;

        /// <comentarios/>
        [System.Xml.Serialization.XmlText(DataType = "base64Binary")]
        public byte[] Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "urn:afirma:dss:1.0:profile:XSS:schema")]
    public partial class ContentDataType
    {

        private byte[] binaryValueField;

        private string mimeTypeField;

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] BinaryValue
        {
            get
            {
                return this.binaryValueField;
            }
            set
            {
                this.binaryValueField = value;
            }
        }

        /// <comentarios/>
        public string MimeType
        {
            get
            {
                return this.mimeTypeField;
            }
            set
            {
                this.mimeTypeField = value;
            }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "urn:afirma:dss:1.0:profile:XSS:schema")]
    public partial class DataInfoType
    {

        private object itemField;

        private string idField;

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("ContentData", typeof(ContentDataType))]
        [System.Xml.Serialization.XmlElementAttribute("SignedDataRefs", typeof(SignedDataRefsType))]
        [System.Xml.Serialization.XmlElementAttribute("DocumentHash", typeof(DocumentHash), Namespace = "urn:oasis:names:tc:dss:1.0:core:schema")]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }
}
