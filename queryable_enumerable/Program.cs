// See https://aka.ms/new-console-template for more information
using queryable_enumerable;
using System.Text.Json;

Console.WriteLine("Hello, World!");

MyContext myContext = new();

//await myContext.Users.AddAsync(new User { Name = "sherman" });
//await myContext.SaveChangesAsync();

var users = myContext.Users.AsEnumerable().Where(p => p.Id > 0);

Console.WriteLine(JsonSerializer.Serialize(users));

var querable = myContext.Users.Where(p => p.Id > 0);  

Console.WriteLine(JsonSerializer.Serialize(querable));

myContext.Dispose();