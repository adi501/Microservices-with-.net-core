using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Extension;
using Mango.Services.RewardAPI.Messaging;
using Mango.Services.RewardAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//S- Connection string
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//E- Connection string

var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddSingleton(new RewardService(optionBuilder.Options));


builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();


builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

//S- callig Apply Migration
ApplyMigration();
//E- callig Apply Migration

// S- it's a extention method we are adding in pipline
app.UseAzureServiceBusConsumer();
// E- it's a extention method we are adding in pipline

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
