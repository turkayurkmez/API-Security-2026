var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();


app.Use(async (context, next) =>
{
    //1. HSTS zorunlu:
    var headers = context.Response.Headers;
    headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");

    //2. Tarayýcý MIME türü algýlamasýný devre dýþý býrak:
    headers.Append("X-Content-Type-Options", "nosniff");

    //3. Clickjacking saldýrýlarýna karþý koruma:
    headers.Append("X-Frame-Options", "DENY");

    //4. XSS saldýrýlarýna karþý koruma - sadece kendi kaynaklarý script üretir:
    headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'");

    //5. Kritik alanlar cache'lenmesin:
    headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");

    headers.Remove("Server"); // Server bilgisini gizle
    headers.Remove("X-Powered-By"); // ASP.NET Core tarafýndan eklenen X-Powered-By baþlýðýný kaldýr

    await next();  



});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHsts(); // HSTS (HTTP Strict Transport Security) 'yi etkinleþtir
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
