using Microsoft.VisualBasic;
using System;

namespace CoreDll.Utils
{
    public delegate void ExibirAlerta(string mensagem);

    public static class CertificadoHelper
    {
        public static DateTime? DataDeExpiracao(System.Security.Cryptography.X509Certificates.X509Certificate2 certificado)
        {
            DateTime? data = null;

            try
            {
                if (certificado == null)
                    return null;

                data = DateTime.Parse(certificado.GetExpirationDateString(), System.Globalization.CultureInfo.CurrentCulture);
            }
            finally { }

            return data;
        }



        public static bool VerificarVencimento(System.Security.Cryptography.X509Certificates.X509Certificate2 certificado, ExibirAlerta alerta = null)
        {
            //Verificando a validade do certificado
            //-------------------------------------
            DateTime? DtCertificado = CertificadoHelper.DataDeExpiracao(certificado);

            if (!DtCertificado.HasValue)
            {
                if (alerta != null)
                {
                    //Interaction.MsgBox("Certificado inválido!", MsgBoxStyle.Critical, "Certificado");
                    alerta("Certificado inválido!");
                }
                return false;
            }

            int tempoRestante = (int)DateAndTime.DateDiff(DateInterval.Day, DateTime.Now, DtCertificado.Value);


            if (tempoRestante < 15)
            {
                if (tempoRestante < 0)
                {
                    if (alerta != null)
                    {
                        //Interaction.MsgBox("Certificado digital expirou." + Environment.NewLine  + Environment.NewLine  + "Adiquira um novo certificado digital!", MsgBoxStyle.Critical, "Certificado");
                        alerta("Certificado digital expirou." +
                                Environment.NewLine + Environment.NewLine +
                                "Adiquira um novo certificado digital!");
                    }
                    return true;
                }
                else
                {

                    if (alerta != null)
                    {
                        //Interaction.MsgBox("**** ATENÇÃO **** " + Environment.NewLine  + Environment.NewLine  + "Faltam " + tempoRestante + " dias para expiração do certificado digital!" + Environment.NewLine  + Environment.NewLine  + "(Lembramos que o sistema emissor de NFe da GData Sistemas aceita apenas certificados do tipo A1!)", MsgBoxStyle.Critical, "Certificado");
                        alerta("**** ATENÇÃO **** " +
                                Environment.NewLine + Environment.NewLine +
                                "Faltam " + tempoRestante + " dias para expiração do certificado digital!" +
                                Environment.NewLine + Environment.NewLine +
                                "(Lembramos que o sistema emissor de NFe da GData Sistemas aceita apenas certificados do tipo A1!)");
                    }
                }
            }

            return false;
        }
    }
}