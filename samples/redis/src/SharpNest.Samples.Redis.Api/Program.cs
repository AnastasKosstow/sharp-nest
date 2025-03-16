using Microsoft.AspNetCore.Mvc;
using SharpNest.Redis;
using SharpNest.Samples.Redis.Api.Models;
using SharpNest.Samples.Redis.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRedis(builder.Configuration, config =>
{
    config.AddRedisCache();
    config.AddRedisStreaming();
});

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/post", async (IUserService userService, [FromBody] User user, CancellationToken cancellationToken) =>
{
    await userService.SetUserAsync(user, cancellationToken);
    return Results.Created($"/users/{user.Id}", user);
});

app.MapGet("/get/{id}", async (IUserService userService, int id, CancellationToken cancellationToken) =>
{
    var user = await userService.GetUserAsync(id, cancellationToken);
    return user != null 
        ? Results.Ok(user) 
        : Results.NotFound();
});

app.MapGet("/tryGet/{id}", async (IUserService userService, int id, CancellationToken cancellationToken) =>
{
    var user = await userService.TryGetUserAsync(id, cancellationToken);
    return user != null
        ? Results.Ok(user)
        : Results.NotFound();
});

app.MapDelete("/remove/{id}", async (IUserService userService, int id, CancellationToken cancellationToken) =>
{
    await userService.RemoveUserAsync(id, cancellationToken);
    return Results.NoContent();
});

app.Run();
