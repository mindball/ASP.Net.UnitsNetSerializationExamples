using ASP.Net.UnitsNetSerializationExamples.Extensions;
using UnitsNet.Serialization.JsonNet;
using JsonConverter = Newtonsoft.Json.JsonConverter;

var builder = WebApplication.CreateBuilder(args);

var serializationOptions = builder.Configuration.GetSection("JsonConverter").Get<SerializationOptions>();
switch (serializationOptions.Serializer)
{
    case SerializerType.NewtonsoftJson:
    {
        builder.Services.AddControllersWithNewtonsoftConverter(serializationOptions.Schema);
        break;
    }
    //case SerializerType.SystemTextJson: not yet implemented
    default:
        throw new ArgumentOutOfRangeException();
}

builder.Services.AddCustomSwagger(serializationOptions.Schema);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();


