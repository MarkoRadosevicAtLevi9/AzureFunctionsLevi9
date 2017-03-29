using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;

public class AvailabilityEntity : TableEntity
{
    public AvailabilityEntity()
    {
        this.PartitionKey = Guid.NewGuid().ToString();
        this.RowKey = Guid.NewGuid().ToString();
    }

    public string StatusCode { get; set; }
}

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

    var request = new RestRequest("posts?userId=1", Method.GET);
    var client = new RestClient("https://jsonplaceholder.typicode.com/");

    var response = client.Execute(request);

    // Retrieve the storage account from the connection string.
    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
        CloudConfigurationManager.GetSetting("testwebtorage_STORAGE"));

    // Create the table client.
    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

    // Retrieve a reference to the table.
    CloudTable table = tableClient.GetTableReference("Availability");

    // Create the table if it doesn't exist.
    table.CreateIfNotExists();

    var dateTime = DateTime.UtcNow.ToString();

    var statusCode = response.StatusCode.ToString();

    // Create a new customer entity.
    AvailabilityEntity availabilityEntity = new AvailabilityEntity();
    availabilityEntity.StatusCode = statusCode;

    // Create the TableOperation object that inserts the customer entity.
    TableOperation insertOperation = TableOperation.Insert(availabilityEntity);

    // Execute the insert operation.
    table.Execute(insertOperation);

    log.Info($"C# Timer trigger function done at: {DateTime.Now}");
}