using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TestValetax.DB;
using TestValetax.DB.Repositories;
using TestValetax.DB.Repositories.Interface;
using TestValetax.Middleware;
using TestValetax.Middleware.TestValetax.Middleware;
using TestValetax.Services;
using TestValetax.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IJournalRepository, JournalRepository>();
builder.Services.AddScoped<ITreeNodeRepository, TreeNodeRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();


// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITreeService, TreeService>();

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

app.UseHttpsRedirection();

app.UseMiddleware<TokenValidationMiddleware>();

app.MapControllers();

app.Run();
