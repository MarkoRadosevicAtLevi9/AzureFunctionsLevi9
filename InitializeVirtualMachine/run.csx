#load "..\Shared\Encryption.cs"
#r "Newtonsoft.Json"
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;

public class VirtualMachine
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualMachineName { get; set; }
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log, IAsyncCollector<string> outputQueueItem)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    // parse query parameter
    string userName = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "userName", true) == 0)
        .Value;

    string encryptedPassword = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "password", true) == 0)
        .Value;

    string virtualMachineName = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "virtualMachineName", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set arguments to query string or body data
    userName = userName ?? data?.userName;
    encryptedPassword = encryptedPassword ?? data?.password;
    virtualMachineName = virtualMachineName ?? data?.virtualMachineName;

    if(userName==null || encryptedPassword == null || virtualMachineName == null)
    {
        req.CreateResponse(HttpStatusCode.BadRequest, "One or more parameters are not valid or not passed");
    }
  

    var encryption = new Encryption();

    var password = encryption.DecryptString(encryptedPassword);

    var virtualMachine = new VirtualMachine
    {
         UserName = userName,
         Password = password,
         VirtualMachineName = virtualMachineName
     };

    await outputQueueItem.AddAsync(JsonConvert.SerializeObject(virtualMachine));

    return req.CreateResponse(HttpStatusCode.OK, new
    {
        message = $"Virtual Machine creation in progress!"
    });
}