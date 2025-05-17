using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

public class ServicioEmail : IServicioEmail
{

    private readonly string _host;
    private readonly int _puerto;
    private readonly string _email;
    private readonly string _password;

    public ServicioEmail(IConfiguration config)
    {
        var emailConfig = config.GetSection("CONFIGURACIONES_EMAIL");
        _host = emailConfig["HOST"];
        _puerto = int.Parse(emailConfig["PUERTO"]);
        _email = emailConfig["EMAIL"];
        _password = emailConfig["PASSWORD"];
    }

    public string SendVerificationEmail(string toEmail)
    {
        var random = new Random();
        var code = CodigosPosibles[random.Next(CodigosPosibles.Count)];

        try
        {
            var smtpClient = new SmtpClient(_host)
            {
                Port = _puerto,
                Credentials = new NetworkCredential(_email, _password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_email),
                Subject = "Código de verificación",
                Body = $"Tu código de verificación es: {code}",
                IsBodyHtml = false,
            };
            mailMessage.To.Add(toEmail);

            smtpClient.Send(mailMessage);

            return code;
        }
        catch (Exception ex)
        {
            throw new Exception("Error enviando correo: " + ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : ""), ex);
        }
    }

    public List<string> GetCodigosPosibles()
    {
        return CodigosPosibles;
    }

    public static readonly List<string> CodigosPosibles = new()
    {
        "47392", "82910", "15437", "90218", "67123", "38495", "21098", "56743", "43210", "87654",
        "13579", "24680", "97531", "86420", "11223", "33445", "55667", "77889", "99001", "22334",
        "31045", "72859", "64312", "50783", "19246", "85037", "41926", "73698", "28514", "96420",
        "53179", "41758", "30261", "68924", "75310", "82463", "19578", "63041", "27835", "94016",
        "12754", "23867", "34980", "45019", "56132", "67245", "78358", "89461", "90574", "01687",
        "12903", "23814", "34725", "45636", "56547", "67458", "78369", "89270", "90181", "01292"
    };

}