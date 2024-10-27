using AutoMapper;
using Mango.Services.CouponAPI;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection.Metadata;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//DB connection

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//S- Auto Mapping
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//E- Auto Mapping



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// it's default swagger 
//builder.Services.AddSwaggerGen();

//S- apply authentication process to swagger here
builder.Services.AddSwaggerGen(options=> 
{
    options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name="Authorization",
        Description="Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In=ParameterLocation.Header,
        Type=SecuritySchemeType.ApiKey,
        Scheme="Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference=new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id=JwtBearerDefaults.AuthenticationScheme
                }

            },new string[]{ }
        }
    });
});
//E- apply authentication process to swagger here



//S- adding authentication & Authorization 
builder.AddAppAuthentication();

builder.Services.AddAuthorization();
//E- adding authentication & Authorization 


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
Stripe.StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseHttpsRedirection();

//S- adding authentication & Authorization 
app.UseAuthentication();
app.UseAuthorization();
//E- adding authentication & Authorization 

app.MapControllers();

//S- callig Apply Migration
ApplyMigration();
//E- callig Apply Migration

app.Run();

//S-Method to -Auto migration when application start if you have any peding updates
void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if(_db.Database.GetPendingMigrations().Count()>0)
        {
            _db.Database.Migrate();
        }
    }
}
//E-Method to -Auto migration when application start if you have any peding updates