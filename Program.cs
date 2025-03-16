using ASP.Net.UnitsNetSerializationExamples.Extensions;
using UnitsNet.Serialization.JsonNet;
using JsonConverter = Newtonsoft.Json.JsonConverter;

var builder = WebApplication.CreateBuilder(args);

var converterType = builder.Configuration.GetSection("JsonConverter:Type").Value;
var converterTypeName = Type.GetType(converterType)?.FullName;
JsonConverter globalJsonConverter = converterTypeName == typeof(AbbreviatedUnitsConverter).FullName
    ? new AbbreviatedUnitsConverter()
    : new UnitsNetIQuantityJsonConverter();

builder.Services.AddCustomJson(globalJsonConverter);
builder.Services.AddCustomSwagger(globalJsonConverter);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();


