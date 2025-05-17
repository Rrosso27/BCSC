public interface IServicioEmail
{
    List<string> GetCodigosPosibles();
    string SendVerificationEmail(string toEmail);
}