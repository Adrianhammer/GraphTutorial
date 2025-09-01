using Microsoft.Graph.Models;
using Microsoft.Graph.Me.SendMail;

namespace GraphTutorial.Services;

public class MailService
{
    public static async Task SendMailAsync(string subject, string body, string recipient)
    {
        _ = GraphAuthService._userClient ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");

        var message = new Message
        {
            Subject = subject,
            Body = new ItemBody
            {
                Content = body,
                ContentType = BodyType.Text
            },
            ToRecipients = new List<Recipient>
            {
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = recipient
                    }
                }
            }
        };

        await GraphAuthService._userClient.Me
            .SendMail
            .PostAsync(new SendMailPostRequestBody
            {
                Message = message,
            });
    }
}