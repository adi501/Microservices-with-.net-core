using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Extensions;
using Mango.Services.ShoppingCartAPI.Service;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Mango.Services.ShoppingCartAPI.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//S- Connection string
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//E- Connection string

//S- Auto Mapping
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
//E- Auto Mapping



builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BackendApiAuthenticationHttpClientHandler>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IMessageBus, MessageBus>();






//S-It will add HttpClient for Product API
builder.Services.AddHttpClient("Product", u => u.BaseAddress =
new Uri(builder.Configuration["ServiceUrls:ProductAPI"])).AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();
//E-It will add HttpClient for Product API

//S-It will add HttpClient for Coupon API
builder.Services.AddHttpClient("Coupon", u => u.BaseAddress =
new Uri(builder.Configuration["ServiceUrls:CouponAPI"])).AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();
//E-It will add HttpClient for Coupon API

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// it's default swagger 
builder.Services.AddSwaggerGen();

//S- apply authentication process to swagger here
//builder.Services.AddSwaggerGen(options =>
//{
//    options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });
//    options.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference=new OpenApiReference
//                {
//                    Type=ReferenceType.SecurityScheme,
//                    Id=JwtBearerDefaults.AuthenticationScheme
//                }

//            },new string[]{ }
//        }
//    });
//});
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

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}
//E-Method to -Auto migration when application start if you have any peding updates