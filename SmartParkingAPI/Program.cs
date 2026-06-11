using Microsoft.EntityFrameworkCore;
using SmartParkingAPI; 

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ParkingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("LejoTeGjithe", policy =>
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

app.UseHttpsRedirection();


app.UseCors("LejoTeGjithe");

app.UseAuthorization();

app.MapControllers();

app.Run();