using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.Exceptions
{
    public class AfirmaResultException : Exception
    {
        private string _codigo;

        public string Codigo
        {
            get
            {
                return _codigo;
            }
        }

        public AfirmaResultException(string codigo, string message): base(message)
        {
            _codigo = codigo;
        }
    }
}
