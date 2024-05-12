namespace Babatoobin_II.Services
{
    public interface IMailService
    {
        Task<string> SendEmailAsync(MailRequest mailRequest);
    }
}
