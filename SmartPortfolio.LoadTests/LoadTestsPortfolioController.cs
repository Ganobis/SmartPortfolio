using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Text.Json;

var portfolioId = "1a9451b5-2f11-45a4-9696-4fbe823f523c";
var baseUrl = "http://localhost:5000/";

using var httpClient = new HttpClient();

var depositScenario = Scenario.Create("deposit_funds", async context =>
{
    var json = JsonSerializer.Serialize(new
    {
        Amount = 1000,
        Currency = "USD"
    });
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    var request = Http.CreateRequest("POST", $"{baseUrl}api/portfolios/{portfolioId}/deposit")
        .WithBody(content);
    var response = await Http.Send(httpClient, request);

    return response;
}).WithWarmUpDuration(TimeSpan.FromSeconds(10))
  .WithLoadSimulations(
      Simulation.Inject(rate: 50,
                        interval: TimeSpan.FromSeconds(1),
                        during: TimeSpan.FromSeconds(10))
  );

NBomberRunner
    .RegisterScenarios(depositScenario)
    .WithReportFormats()
    .Run();