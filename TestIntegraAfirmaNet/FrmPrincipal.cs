// --------------------------------------------------------------------------------------------------------------------
// FrmPrincipal.cs
//
// Demostración de uso de los servicios de firma digital de @firma
// Copyright (C) 2016 Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
//
// E-Mail: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using IntegraAfirmaNet.Authentication;
using IntegraAfirmaNet.Services;
using IntegraAfirmaNet.SignatureFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace DSSAfirmaVerifySampleTest
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();
        }


        private X509Certificate2 SeleccionarCertificado()
        {
            X509Certificate2 cert = null;

            try
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, "Seleccionar certificado", "Certificado para firmar la solicitud", X509SelectionFlag.SingleSelection);

                if (scollection != null && scollection.Count == 1)
                {
                    cert = scollection[0];

                    if (cert.HasPrivateKey == false)
                    {
                        throw new Exception("El certificado no tiene asociada una clave privada.");
                    }
                }

                store.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("No se ha podido obtener la clave privada.", ex);
            }

            return cert;
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFicheroFirma.Text = openFileDialog1.FileName;
            }
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            cmbTipo.SelectedIndex = 0;
        }

        private void btnEnviarSolicitud_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFicheroFirma.Text))
            {
                MessageBox.Show("Debe seleccionar un fichero de firma");
                return;
            }

            if (string.IsNullOrEmpty(txtIdentificador.Text))
            {
                MessageBox.Show("Debe introducir el identificador de aplicación de @firma");
                return;
            }

            /* Selecionar el certificado para firmar la solicitud. El certificado deberá ser el mismo 
             * que el empleado durante el registro en @firma */
            X509Certificate2 cert = SeleccionarCertificado();

            if (cert == null)
            {
                MessageBox.Show("Debe seleccionar un certificado para firmar la solicitud.");
                return;
            }

            try
            {
                byte[] contenidoFirma = File.ReadAllBytes(txtFicheroFirma.Text);
                SignatureFormat formatoFirma;

                if (Path.GetExtension(txtFicheroFirma.Text).ToUpper() == ".PDF")
                {

                    if (cmbTipo.SelectedIndex != cmbTipo.Items.Count - 1)
                    {
                        MessageBox.Show("Debe especificar un formato de firma correcto.");
                    }

                    formatoFirma = SignatureFormat.PAdES;
                }
                else if (Path.GetExtension(txtFicheroFirma.Text).ToUpper() == ".XML" ||
                    Path.GetExtension(txtFicheroFirma.Text).ToUpper() == ".XSIG")
                {
                    formatoFirma = SignatureFormat.XAdES;
                }
                else
                {
                    formatoFirma = SignatureFormat.CAdES;
                }

                Identity identity = new Identity(cert, txtIdentificador.Text);

                // Certificado que firma las respuestas del servidor
                X509Certificate2 serverCert = null; // new X509Certificate2(File.ReadAllBytes("SGAD_PRO.cer"));

                AfirmaService afirmaService = new AfirmaService(identity, serverCert);

                byte[] resultado = afirmaService.UpgradeSignature(contenidoFirma, formatoFirma, ReturnUpdatedSignatureType.GetReturnUpdatedSignatureType(cmbTipo.Text));

                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(saveFileDialog1.FileName, resultado);
                }

                MessageBox.Show("Proceso completado correctamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ha ocurrido procesando la solicitud");
            }
        }
    }
}
