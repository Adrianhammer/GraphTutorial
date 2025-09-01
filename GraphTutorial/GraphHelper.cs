using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Me.SendMail;

namespace GraphTutorial;

public class GraphHelper
{
    //Settings object
    private static Settings? _settings;
    //User auth token credential
    private static DeviceCodeCredential? _deviceCodeCredential;
    //Client configured with user authentication
    private static GraphServiceClient? _userClient;

    public static void InitializeGraphForUserAuth(Settings settings,
        Func<DeviceCodeInfo, CancellationToken, Task> deviceCodePrompt)
    {
        _settings = settings;

        var options = new DeviceCodeCredentialOptions
        {
            ClientId = settings.ClientId,
            TenantId = settings.TenantId,
            DeviceCodeCallback = deviceCodePrompt,
        };
        
        _deviceCodeCredential = new DeviceCodeCredential(options);
        
        _userClient = new GraphServiceClient(_deviceCodeCredential, settings.GraphUserScopes);
    }

    public static async Task<string> GetUserTokenAsync()
    {
        //Ensure credential isn`t null
        //_ is discard. Same as checking "if _deviceCodeCredential == null"
        _ = _deviceCodeCredential ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth.");
        
        //Ensure scopes isn`t null
        _ = _settings?.GraphUserScopes ?? throw new System.ArgumentException("Argument 'scopes' cannot be null.");
        
        //request token with given scopes
        var context = new TokenRequestContext(_settings.GraphUserScopes);
        var response = await _deviceCodeCredential.GetTokenAsync(context);
        return response.Token;
    }

    public static Task<User?> GetUserAsync()
    {
        //Ensure client is not null
        _ = _userClient ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");
        
        return _userClient.Me.GetAsync((config) =>
        {
            //Only request specific properties
            config.QueryParameters.Select = new[] { "displayName", "mail", "userPrincipalName" };
        });
    }

    public static Task<MessageCollectionResponse?> GetInboxAsync()
    {
        _ = _userClient ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");

        return _userClient.Me
            .MailFolders["Inbox"]
            .Messages
            .GetAsync((config) =>
            {
                config.QueryParameters.Select = new[] { "from", "isRead", "receivedDateTime", "subject" };
                config.QueryParameters.Top = 3;
                config.QueryParameters.Orderby = new[] { "receivedDateTime DESC" };
            });
    }

    public static async Task SendMailAsync(string subject, string body, string recipient)
    {
        _ = _userClient ??
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

        await _userClient.Me
            .SendMail
            .PostAsync(new SendMailPostRequestBody
            {
                Message = message,
            });
    }

    public static async Task DownloadProfilePictureAsync(string filePath)
    {
        _ = _userClient ??
            throw new NullReferenceException("Graph has now been initialized for user auth");

        var photo = await _userClient.Users["{a52499e1-4efd-478f-b74e-3af49fa79dde}"].Photo.Content.GetAsync();
        //var photo = await _userClient.Me.Photo.Content.GetAsync();

        try
        {
            _ = photo ??
                throw new NullReferenceException("No photo found");
            
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(fs);
            }

            Console.WriteLine("Photo downloaded");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}