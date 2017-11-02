using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Web.Services3.Design;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.Services3.Security.Tokens;

namespace IntegraAfirmaNet.SignatureFramework
{
    /// <summary>
    /// Clase de herramientas para el uso del SignatureFramework
    /// </summary>
    class Tools
    {
        /// <summary>
        /// Clase para crear un TokenProvider desde un almacén de claves externo
        /// </summary>
        public class ExternalX509TokenProvider : X509TokenProvider
        {
            X509Certificate2 certificate = null;

            /// <summary>
            /// Crea un TokenProvider a partir de un fichero de claves y su password
            /// </summary>
            /// <param name="keystorePath">Ruta del almacén de claves</param>
            /// <param name="keystorePassword">Password que protege el almacén</param>
            public ExternalX509TokenProvider(String keystorePath, String keystorePassword)
            {
                certificate = new X509Certificate2(keystorePath, keystorePassword);
            }

            /// <summary>
            /// Proporciona el X509SecurityToken asociado al almacen identificado.
            /// </summary>
            /// <returns>null si ha ocurrido algún error en el acceso al almacén</returns>
            public override X509SecurityToken GetToken()
            {
                if (certificate == null)
                {
                    return null;
                }
                else
                {
                    return new X509SecurityToken(certificate);
                }
            }
        }

    }
}
