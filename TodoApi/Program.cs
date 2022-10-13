using Microsoft.EntityFrameworkCore;
using TodoApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// builder.Services.AddDbContext<TodoContext>(opt =>
// opt.UseInMemoryDatabase("TodoList"));
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new() { Title = "TodoApi", Version = "v1" });
//});
builder.Services.AddDbContext<TodoApi.Data.todoDBContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("TODO")));

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (builder.Environment.IsDevelopment())
//{

//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();