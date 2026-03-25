// <copyright file="Program.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using Microsoft.OpenApi.Models;
using MyNUnit.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyNUnit API", Version = "v1" });
});

builder.Services.AddScoped<IFileStorage, FileStorageService>();
builder.Services.AddScoped<ITestRunService, TestRunService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();