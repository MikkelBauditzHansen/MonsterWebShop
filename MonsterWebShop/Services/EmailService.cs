using System.Net;
using System.Net.Mail;

namespace MonsterWebShop.Services
{
    public class EmailService
    {
        private readonly IConfiguration configuration;

        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public bool SendPasswordResetEmail(
            string email,
            string resetLink)
        {
            try
            {
                string senderEmail =
                    configuration["EmailSettings:Email"]!;

                string senderPassword =
                    configuration["EmailSettings:Password"]!;

                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(senderEmail);
                mail.To.Add(email);

                mail.Subject = "Nulstil din adgangskode";

                mail.Body =
                    "Klik på linket for at nulstille din adgangskode:\n\n"
                    + resetLink;

                SmtpClient smtpClient =
                    new SmtpClient("smtp.gmail.com", 587);

                smtpClient.EnableSsl = true;

                smtpClient.Credentials =
                    new NetworkCredential(
                        senderEmail,
                        senderPassword
                    );

                smtpClient.Send(mail);

                Console.WriteLine("MAIL SENDT");
                Console.WriteLine("Til: " + email);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("MAIL FEJL:");
                Console.WriteLine(ex.Message);

                return false;
            }
        }
    }
}