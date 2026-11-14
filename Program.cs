// Many OpenAPI documents, each with many endpoints.
// Build-time generation (dotnet build) runs all documents in one dotnet-getdocument process,
// which is killed after a hardcoded 2 minutes.
const int Documents = 20;
const int EndpointsPerDocument = 1500;

var builder = WebApplication.CreateBuilder(args);
for (var d = 0; d < Documents; d++)
{
    builder.Services.AddOpenApi($"doc{d}");
}

var app = builder.Build();
for (var d = 0; d < Documents; d++)
{
    var group = app.MapGroup($"/doc{d}").WithGroupName($"doc{d}");
    for (var e = 0; e < EndpointsPerDocument; e++)
    {
        group.MapGet($"/items{e}/{{id:int}}", (int id, string? filter) => new Item(id, filter ?? "", []));
        group.MapPost($"/items{e}", (Item item) => TypedResults.Created($"/items/{item.Id}", item));
    }
}
app.MapOpenApi();
app.Run();

record Item(int Id, string Name, List<Child> Children);
record Child(int Id, string Name, DateTimeOffset At, Dictionary<string, string> Tags);
