using Microsoft.Graph.Models;

namespace GraphTutorial.Services;

public class UserService
{
    public static Task<User?> GetUserAsync()
    {
        
        return GraphAuthService.UserClient.Me.GetAsync((config) =>
        {
            //Only request specific properties
            config.QueryParameters.Select = new[] { "displayName", "mail", "userPrincipalName" };
        });
    }

    public static Task<MessageCollectionResponse?> GetInboxAsync()
    {
        _ = GraphAuthService.UserClient ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");

        return GraphAuthService.UserClient.Me
            .MailFolders["Inbox"]
            .Messages
            .GetAsync((config) =>
            {
                config.QueryParameters.Select = new[] { "from", "isRead", "receivedDateTime", "subject" };
                config.QueryParameters.Top = 3;
                config.QueryParameters.Orderby = new[] { "receivedDateTime DESC" };
            });
    }
    
    public static async Task DownloadProfilePictureAsync(string filePath)
    {
        _ = GraphAuthService.UserClient ??
            throw new NullReferenceException("Graph has now been initialized for user auth");

        var photo = await GraphAuthService.UserClient.Users["{a52499e1-4efd-478f-b74e-3af49fa79dde}"].Photo.Content.GetAsync();
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

    public static async Task SearchOneDrive(string word)
    {
        var result = await GraphAuthService.UserClient.Me.Drive.Root.SearchWithQ($"{word}").GetAsSearchWithQGetResponseAsync();

        if (result?.Value == null || !result.Value.Any())
        {
            Console.WriteLine($"Found nothing containing {word}");
            return;
        }

        foreach (var items in result.Value)
        {
            Console.WriteLine($"Name: {items.Name}");
            Console.WriteLine($"    Search Result: {items.SearchResult}");
        }
    }
}