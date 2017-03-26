#r "System.Net"
using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using JWT;
using Microsoft.Owin.Security.DataHandler.Encoder;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    var request = new RestRequest("api/activity/check", Method.POST);
    var client = new RestClientBuilder()
        .WithValidAccessToken()
        .Build();

    client.Execute(request);
}

public class RestClientBuilder
{
    private string _authorizationHeader;

    public RestClient Build()
    {
        var client = new RestClient(ClientConfiguration.ServerUrl);
        if (_authorizationHeader != null)
        {
            client.AddDefaultHeader("Authorization", _authorizationHeader);
        }
        return client;
    }

    public RestClientBuilder WithMissingAccessToken()
    {
        _authorizationHeader = null;
        return this;
    }

    public RestClientBuilder WithInvalidAccessToken()
    {
        _authorizationHeader = "Bearer INVALID_TOKEN";
        return this;
    }

    public RestClientBuilder WithValidAccessToken()
    {
        _authorizationHeader = "Bearer " + CreateAccessToken();
        return this;
    }

    public RestClientBuilder WithExpiredAccessToken()
    {
        _authorizationHeader = "Bearer " + CreateAccessToken(true);
        return this;
    }

    private string CreateAccessToken(bool expired = false)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentUtc = expired ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow;
        var token = new Dictionary<string, object>();
        token["appid"] = ClientConfiguration.ClientId;
        token["aud"] = ClientConfiguration.ClientId;
        token["iss"] = ClientConfiguration.IssuerUri;
        token["nameid"] = "Google|100129284172502993075";
        token["unique_name"] = "Terry Adams";
        token["tid"] = ClientConfiguration.TenantId;
        token["oid"] = ClientConfiguration.UserId;
        token["nbf"] = (int)(currentUtc - unixEpoch).TotalSeconds;
        token["exp"] = (int)(currentUtc.AddMinutes(15) - unixEpoch).TotalSeconds;

        var accessToken = JsonWebToken.Encode(
            token,
            ClientConfiguration.ClientSymetricKey,
            JwtHashAlgorithm.HS256);

        return accessToken;
    }
}

public static class ClientConfiguration
{
    public static string ServerUrl => "https://app-example.com/availability/";

    public static Guid TenantId => new Guid("9936F133-DEA7-4C8A-9140-M3A9994C0268");

    public static Guid UserId => new Guid("39C00184-545E-6677-WW32-0C174656AC15");

    public static string IssuerUri => "https://app-example.com/auth".ToLower().TrimEnd('/');

    public static string ClientId => "ExampleWeb";

    public static byte[] ClientSymetricKey
    {
        get
        {
            var key = "IxrAjDka2FqElO3IhrSrUJELhUcKePEPVpaepLS_Wax";
            return TextEncodings.Base64Url.Decode(key);
        }
    }
}