#r "System.Net"
using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using JWT;
using Microsoft.Owin.Security.DataHandler.Encoder;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    var request = new RestRequest("posts?userId=1", Method.GET);
    var client = new RestClient("https://jsonplaceholder.typicode.com/");

    client.Execute(request);
}
