using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetScheduler.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.ConfigureServices();
builder.Services.AddControllers(config =>
{
    config.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
