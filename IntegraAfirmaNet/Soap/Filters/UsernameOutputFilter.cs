using IntegraAfirmaNet.Soap.Assertions;
using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraAfirmaNet.Soap.Filters
{
    internal class UsernameOutputFilter : SendSecurityFilter
    {
        private UsernameTokenSoapAssertion parentAssertion;
        private UsernameToken token;

        public UsernameOutputFilter(UsernameTokenSoapAssertion parent)
            : base("", true)
        {
            parentAssertion = parent;
            token = parent.Token;
        }

        public override void SecureMessage(SoapEnvelope envelope, Security security)
        {
            security.Tokens.Add(parentAssertion.Token);
            security.MustUnderstand = false;
        }
    }
}
