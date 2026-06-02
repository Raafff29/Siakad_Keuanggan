using Microsoft.EntityFrameworkCore;
using SiakadKeuanganAPI.Data;
using SiakadKeuanganAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("MahasiswaAPI", client =>
{
    var baseUrl = builder.Configuration["MahasiswaAPI:BaseUrl"]
                  ?? "https://mahasiswa-api-psi.vercel.app";
    var timeout = int.Parse(
        builder.Configuration["MahasiswaAPI:TimeoutSeconds"] ?? "30");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(timeout);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<MahasiswaSyncService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "SIAKAD API Keuangan",
        Version = "v1",
        Description = "API Keuangan SIAKAD dengan sinkronisasi data mahasiswa"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIAKAD Keuangan API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();