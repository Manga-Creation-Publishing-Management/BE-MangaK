using MailKit;
using Manga.Api.extensions;
using Manga.Middlewares;
using Manga.Repository.Data;
using Microsoft.EntityFrameworkCore;

using CloudinaryService = Manga.Service.CloudinaryService;
using MediaService = Manga.Service.MediaService;
using ChapterService = Manga.Service.Chapter;
using SeriesService = Manga.Service.Series;
using JwtService = Manga.Service.JwtService;
using AuthService = Manga.Service.Auth;
using MailService = Manga.Service.MailService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//Cái này để FE gửi string thì các kiểu dữ liệu cấu trúc sẽ gửi xuống DB là int
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddHttpContextAccessor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<MediaService.IService, CloudinaryService.Service>();
builder.Services.AddScoped<ChapterService.IService, ChapterService.Service>();
builder.Services.AddScoped<SeriesService.IService, SeriesService.Service>();
builder.Services.AddScoped<MediaService.IService, CloudinaryService.Service>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddJwtServices(builder.Configuration);
builder.Services.AddSwaggerServices();

builder.Services.AddScoped<AuthService.IService, AuthService.Service>();
builder.Services.AddScoped<JwtService.IService, JwtService.Service>();
builder.Services.AddScoped<MailService.IService, MailService.Service>();
// ─── Middleware ────────────────────────────────────────────────────────────────
builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
// ─── SeedData ────────────────────────────────────────────────────────────────

//AI
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});

//AI


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await AppDbContextSeed.SeedAsync(db);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//AI
app.UseCors("AllowFrontend");
//AI

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();