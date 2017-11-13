using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.Exceptions
{
    public class AfirmaResultException : Exception
    {
        private string _majorCode;
        private string _minorCode;

        public string MajorCode
        {
            get
            {
                return _majorCode;
            }
        }

        public string MinorCode
        {
            get
            {
                return _minorCode;
            }
        }

        public AfirmaResultException(string majorCode, string message): base(message)
        {
            _majorCode = majorCode;
        }

        public AfirmaResultException(string majorCode, string minorCode, string message)
            : this(majorCode, message)
        {
            _minorCode = minorCode;
        }

    }
}
