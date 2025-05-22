using EF.Blockchain.Domain;
using Flurl.Http;

// Base URL of your blockchain server
const string BLOCKCHAIN_SERVER = "http://localhost:3000";

// Uses Flurl.Http to send an HTTP GET request to /blocks/next and prints the response.
try
{
    // Sends a GET request to /blocks/next and automatically parses the JSON response
    var blockInfo = await $"{BLOCKCHAIN_SERVER}/blocks/next"
        .GetJsonAsync<BlockInfo>();

    Console.WriteLine("Next Block Info:");
    Console.WriteLine($"Index: {blockInfo.Index}");
    Console.WriteLine($"PreviousHash: {blockInfo.PreviousHash}");
    Console.WriteLine($"Difficulty: {blockInfo.Difficulty}");
    Console.WriteLine($"MaxDifficulty: {blockInfo.MaxDifficulty}");
    Console.WriteLine($"FeePerTx: {blockInfo.FeePerTx}");
    Console.WriteLine($"Data: {blockInfo.Data}");
}
catch (FlurlHttpException ex)
{
    Console.WriteLine($"Request failed: {ex.Message}");
    if (ex.Call.Response != null)
        Console.WriteLine(await ex.GetResponseStringAsync());
}
