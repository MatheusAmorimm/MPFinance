using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MPFinance.Domain.Interfaces;

namespace MPFinance.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _senderEmail;
    private readonly string _appPassword;

    public SmtpEmailService()
    {
        _host        = Environment.GetEnvironmentVariable("SMTP_HOST")         ?? "smtp.gmail.com";
        _port        = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        _senderEmail = Environment.GetEnvironmentVariable("SMTP_EMAIL")        ?? throw new InvalidOperationException("SMTP_EMAIL não configurado.");
        _appPassword = Environment.GetEnvironmentVariable("SMTP_APP_PASSWORD") ?? throw new InvalidOperationException("SMTP_APP_PASSWORD não configurado.");
    }

    public async Task SendVerificationEmailAsync(string toEmail, string userName, string code)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("MPFinance", _senderEmail));
        message.To.Add(new MailboxAddress(userName, toEmail));
        message.Subject = "Seu código de verificação — MPFinance";

        message.Body = new TextPart("html") { Text = BuildEmailBody(userName, code) };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_senderEmail, _appPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private static string BuildEmailBody(string userName, string code) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <body style="margin:0;padding:0;background-color:#18181B;font-family:'Segoe UI',sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr>
              <td align="center" style="padding:2rem 1rem;">
                <table width="500" cellpadding="0" cellspacing="0"
                  style="background:#27272A;border-radius:1rem;border:1px solid #4C1D95;overflow:hidden;">

                  <tr>
                    <td style="padding:2rem;text-align:center;border-bottom:1px solid #3F3F46;">
                      <h1 style="margin:0;font-size:1.75rem;font-weight:800;color:#D9F99D;">
                        MP<span style="color:#FFFBEB;">Finance</span>
                      </h1>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding:2rem;">
                      <p style="margin:0 0 1rem;color:#FFFBEB;font-size:1rem;">
                        Olá, <strong>{userName}</strong>!
                      </p>
                      <p style="margin:0 0 2rem;color:#A1A1AA;font-size:0.9rem;line-height:1.6;">
                        Use o código abaixo para verificar seu e-mail.
                        Ele é válido por <strong style="color:#FFFBEB;">15 minutos</strong>.
                      </p>

                      <div style="text-align:center;margin:0 0 2rem;">
                        <span style="display:inline-block;font-size:2.5rem;font-weight:800;
                          letter-spacing:0.6rem;color:#18181B;background:#D9F99D;
                          padding:1rem 2rem;border-radius:0.75rem;">
                          {code}
                        </span>
                      </div>

                      <p style="margin:0;color:#52525B;font-size:0.75rem;text-align:center;">
                        Se você não criou uma conta no MPFinance, ignore este e-mail.
                      </p>
                    </td>
                  </tr>

                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
}
