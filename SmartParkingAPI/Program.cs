using Microsoft.EntityFrameworkCore;
using SmartParkingAPI; // Sigurohu që emri i namespace përputhet me projektin tënd

var builder = WebApplication.CreateBuilder(args);

// >>> SHTESA E RE: Lidhja me Databazën Supabase (PostgreSQL) <<<
builder.Services.AddDbContext<ParkingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// >>> 1. RREGULLIMI I CORS (Lejon faqen web të lexojë të dhënat) <<<
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// >>> 2. AKTIVIZIMI I RREGULLIT TË CORS <<<
app.UseCors("LejoTeGjithe");

app.UseAuthorization();

app.MapControllers();

app.Run();