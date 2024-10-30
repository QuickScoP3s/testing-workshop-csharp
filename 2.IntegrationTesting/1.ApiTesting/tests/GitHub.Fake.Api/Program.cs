
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

var settings = new WireMockServerSettings
{
    Urls = new[] { "https://localhost:9095/" },
    StartAdminInterface = true,
    ProxyAndRecordSettings = new ProxyAndRecordSettings
    {
        Url = "https://api.github.com",
        SaveMapping = true,
        SaveMappingToFile = true,
        SaveMappingForStatusCodePattern = "2xx"
    }
};
var wiremockServer =  WireMockServer.Start(settings);

// wiremockServer
//     .Given(Request.Create().WithPath("/users/nickchapsas").UsingGet())
//     .RespondWith(Response.Create().WithStatusCode(200)
//         .WithHeader("Content-Type", "application/json; charset=utf-8")
//         .WithBody("""
//         {
//             "name": "Nick Chapsas"
//         }
//         """
// )
//     );

Console.WriteLine($"Server started at: {wiremockServer.Url}");

Console.ReadKey();

wiremockServer.Dispose();
