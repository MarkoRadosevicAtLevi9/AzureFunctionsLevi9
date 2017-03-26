using System.Net;
using System.Security.Cryptography;


public class VirtualMachine
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualMachineName { get; set; }
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log, IAsyncCollector<VirtualMachine> outputQueueItem)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    // parse query parameter
    string username = req.GetQueryNameValuePairs()
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
    userName = username ?? data?.username;
    password = password ?? data?.password;
    virtualMachineName = virtualMachineName ?? data?.virtualMachineName;

    return userName == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a user name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + userName);

    return password == null
    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a password on the query string or in the request body")
    : req.CreateResponse(HttpStatusCode.OK, "Hello " + password);

    return virtualMachineName == null
    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a virtual machine name on the query string or in the request body")
    : req.CreateResponse(HttpStatusCode.OK, "Hello " + virtualMachineName);

    var encryption = new Encryption();

    var password = encryption.EncryptToString(encryptedPassword);

    var virtualMachine = new VirtualMachine
    {
         UserName = userName,
         Password = password,
         VirtualMachineName = virtualMachineName
     };

    await outputQueueItem.AdAsync(virtualMachine);

    return req.CreateResponse(HttpStatusCode.OK, new
    {
        message = $"Virtual Machine creation in progress!"
    });
}