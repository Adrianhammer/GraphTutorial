// See https://aka.ms/new-console-template for more information

using GraphTutorial;

Console.WriteLine(".NET Graph Tutorial\n");

var settings = Settings.LoadSettings();

//Initialize Graph
InitializeGraph(settings);

await GreetUserAsync();

int choice = -1;

while (choice != 0)
{
    Console.WriteLine("Please choose on of the following options: ");
    Console.WriteLine("0. Exit");
    Console.WriteLine("1. Display access token");
    Console.WriteLine("2. List my inbox");
    Console.WriteLine("3. Send mail");
    Console.WriteLine("4. Make a graph call");

    try
    {
        choice = int.Parse(Console.ReadLine() ?? string.Empty);
    }
    catch (FormatException)
    {
        //Set to invalid value
        choice = -1;
    }
    
    switch (choice)
    {
        case 0:
            //exit program
            Console.WriteLine("Goodbye...");
            break;
        case 1:
            //display access token
            await DisplayAccessTokenAsync();
            break;
        case 2:
            //List emails from user's inbox
            await ListInboxAsync();
            break;
        case 3:
            //Send an email message
            await SendMailAsync();
            break;
        case 4:
            //Run any graph code
            //await MakeGraphCallAsync();
            break;
        default:
            Console.WriteLine("Invalid choice! Please, try again.");
            break;
    }
}


void InitializeGraph(Settings settings)
{
    GraphHelper.InitializeGraphForUserAuth(settings, (info, cancel) =>
    {
        //Display the device code message to the user
        //This tells them where to go to sign in and provides the code to use
        Console.WriteLine(info.Message);
        return Task.FromResult(0);
    });
}

async Task GreetUserAsync()
{
    try
    {
        var user = await GraphHelper.GetUserAsync();
        Console.WriteLine($@"Hello, {user?.DisplayName}!");
        //Work/school accounts - email is Mail property
        //Personal accounts - email is in UserPrincipalName
        Console.WriteLine($"Email: {user?.Mail ?? user?.UserPrincipalName ?? ""}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"error getting user: {ex.Message}");
        throw;
    }    
}

async Task DisplayAccessTokenAsync()
{
    try
    {
        var userToken = await GraphHelper.GetUserTokenAsync();
        Console.WriteLine($"User token: {userToken}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"error getting user access token: {ex.Message}");
        throw;
    }
}

async Task ListInboxAsync()
{
    try
    {
        var messagePage = await GraphHelper.GetInboxAsync();

        if (messagePage?.Value == null)
        {
            Console.WriteLine("No results returned.");
            return;
        }

        foreach (var message in messagePage.Value)
        {
            Console.WriteLine($"Message: {message.Subject ?? "NO SUBJECT"}");
            Console.WriteLine($"    From: {message.From?.EmailAddress?.Name}");
            Console.WriteLine($"    Status: {(message.IsRead!.Value ? "Read" : "Unread")}");
            Console.WriteLine($"    Received: {message.ReceivedDateTime?.ToLocalTime().ToString()}");
        }

        var moreAvailable = !string.IsNullOrEmpty(messagePage.OdataNextLink);

        Console.WriteLine($"\nMore messages available? {moreAvailable}");

    }
    catch (Exception ex)
    {
        Console.WriteLine($"error getting inbox: {ex.Message}");

    }    
}

async Task SendMailAsync()
{
    try
    {
        //Send mail to the signed-in user
        //Get the user for their email address
        var user = await GraphHelper.GetUserAsync();
        
        var userEmail = user?.Mail ?? user?.UserPrincipalName;

        if (string.IsNullOrEmpty(userEmail))
        {
            Console.WriteLine("Couldn't get your email address, canceling...");
            return;
        }
        
        await GraphHelper.SendMailAsync("Testing Microsoft Graph",
            "Hello World", userEmail);

        Console.WriteLine("Mail sent!");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error sending mail: {e.Message}");
        throw;
    }
}

//async Task MakeGraphCallAsync()
{
    // TODO
}