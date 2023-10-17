using MyExampleProject.S3.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddFileStorageByS3Config();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

static partial class Program
{
    public static IServiceCollection AddFileStorageByS3Config(this IServiceCollection services)
    {
        var accesskey = Environment.GetEnvironmentVariable("SELECTEL_FILESTORAGE_ACCESSKEY") ?? "";
        var secretKey = Environment.GetEnvironmentVariable("SELECTEL_FILESTORAGE_SECRETKEY") ?? "";
        var isActiveString = Environment.GetEnvironmentVariable("SELECTEL_FILESTORAGE_ISACTIVE") ?? "true";
        var bucket = Environment.GetEnvironmentVariable("SELECTEL_FILESTORAGE_BUCKET") ?? "";

        services.Configure<FileStorageByS3Config>(opt =>
        {
            var successfully = bool.TryParse(isActiveString, out bool isActive);
            // Logger successfully
            opt.IsActive = isActive;
            opt.AccessKey = accesskey;
            opt.SecretKey = secretKey;
            opt.Bucket = bucket;
        });

        return services;
    }
}
