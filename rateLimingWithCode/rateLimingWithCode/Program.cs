using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddRateLimiter(options =>
{
    //Tüm istekler için 1 saniyede 5 istek sýnýrý
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {

        //Önce kimliði doðrulanmýþ kullanýcýya bak Yoksa IP adresine göre sýnýrlama yap
        //asla IP'ye tek baþýna güvenme.

        var partitionKey = httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.Identity.Name ?? "anonymous"
            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter<string>(partitionKey: partitionKey, factory: partition =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 2,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromMinutes(5);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    options.AddSlidingWindowLimiter("sliding", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromMinutes(1);
        options.SegmentsPerWindow = 6;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    options.AddTokenBucketLimiter("token-bucket", options =>
    {
        options.TokenLimit = 45; // kovadaki maksimum 45 jeton olabilir
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; 
        options.QueueLimit = 0;
        options.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        options.TokensPerPeriod = 5; // Her dakika 5 token eklenir
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Çok fazla istek gönderdiniz. Lütfen daha sonra tekrar deneyin.", cancellationToken);
    };


});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();
