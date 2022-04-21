﻿// See https://aka.ms/new-console-template for more information
using Demo;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World!");
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory());
builder.AddJsonFile("appsettings.json", true, true);
builder.AddJsonFile($"appsettings.{environment}.json");
builder.AddEnvironmentVariables();
var configRoot = builder.Build();

var author = configRoot.GetSection(nameof(Author)).Get<Author>();
Console.WriteLine(author.Description);