using Babatoobin_II.Settings;
using MailKit.Net.Smtp;
using MimeKit;


namespace Babatoobin_II.Services
{
    public class MailService : IMailService
    {
        private MailSettings? MailSettings;
        public MailService(IConfiguration config)
        {

            MailSettings = new MailSettings
            {
                Mail = config["MailSettings:Mail"],
                DisplayName = config["MailSettings:DisplayName"],
                Password = config["MailSettings:Password"],
                Host = config["MailSettings:Host"],
                Port = int.Parse(config["MailSettings:Port"]!),

            };
        }

        public async Task<string> SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();

            try
            {
                email.From.Add(MailboxAddress.Parse(MailSettings?.Mail));
                email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
                if (mailRequest.CcEmails != null)
                { 
                    mailRequest.CcEmails!.ForEach(x => { email.Cc.Add(MailboxAddress.Parse(x)); });
                }

                email.Subject = mailRequest.Subject;

                var builder = new BodyBuilder();
                if (mailRequest.Attachments != null)
                {
                    byte[] fileBytes;
                    foreach (var file in mailRequest.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                            builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                        }
                    }
                }
                builder.HtmlBody = mailRequest.Body;
                email.Body = builder.ToMessageBody();

            }
            catch (Exception e)
            {
                ;
                Console.WriteLine(e.Message);
                return "Incorrect email recipient";// e.Message;
            }



            var message = "";

            //With the new C# 8 using declaration, the code with the using statement can be simplified. 
            //Curly brackets are no longer needed. At the end of the scope of the variable r
            //(which is here the end of the method), the Dispose method is invoked.9 Apr 2019
            using var smtp = new SmtpClient();

            smtp.MessageSent += async (sender, args) =>
            {
                var myTask = await Task.Run(() => message = args.Response);

            };

            try
            {
                smtp.Connect(MailSettings?.Host, MailSettings!.Port, false);
                smtp.Authenticate(MailSettings.Mail, MailSettings.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return e.Message;
            }
            finally
            {
                smtp.Disconnect(true);
            }

            return message;

        }
    }
}
