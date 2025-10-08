using CommandApi.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

CommandControl? lastCommand = null;


app.MapPost("/command", (CommandControl command) =>
{
    try
    {

        if (command == null || string.IsNullOrWhiteSpace(command.Action))
        {
            return Results.BadRequest("Invalid command data.");
        }

        lastCommand = command;
        return Results.Ok("Command received.");


    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }


}
);

app.MapGet("/command",  (HttpContext context) =>
{
   
    if(lastCommand == null)
     return Results.NoContent();
      var cmd = lastCommand;
    lastCommand = null;
    return Results.Json(cmd);


});




app.Run();

