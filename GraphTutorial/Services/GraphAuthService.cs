using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;

namespace GraphTutorial.Services;

public class GraphAuthService
{
    //Settings object
    private static Settings? _settings;
    //User auth token credential
    private static DeviceCodeCredential? _deviceCodeCredential;
    //Client configured with user authentication
    public static GraphServiceClient? _userClient;

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
}