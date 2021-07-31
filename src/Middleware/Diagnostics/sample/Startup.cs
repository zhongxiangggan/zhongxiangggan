using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseRouting();
app.UseExceptionHandler("/error");

app.MapGet("/error", () => "oh no!");
app.MapGet("/", () => "Hello World!");
app.MapGet("/throw", IResult () =>
{
    throw new Exception("bad bad");
});

app.Run();
