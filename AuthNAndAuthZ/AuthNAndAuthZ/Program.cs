using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Servis kayitlari
// -----------------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Servis katmani DI kaydi
builder.Services.AddScoped<AuthNAndAuthZ.Services.IUserService, AuthNAndAuthZ.Services.UserService>();

// -----------------------------------------------------------------------
// JWT Bearer Authentication
// Dogrulama kurallari:
//   - IssuerSigningKey : appsettings'ten gelen simetrik anahtar ile imza dogrulamasi yapilir.
//   - ValidateIssuer   : token'in "iss" (Issuer) alani beklenen degerle eslesmelidir.
//   - ValidateAudience : token'in "aud" (Audience) alani beklenen degerle eslesmelidir.
//   - ValidateLifetime : "exp" (Expiration) alani kontrol edilir; suresi dolmus token reddedilir.
//   - ValidateIssuerSigningKey : imza algoritmasi ve anahtar dogrulanir ("alg:none" gecmez).
// -----------------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey  = jwtSection["SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey appsettings.json icerisinde tanimli olmalidir.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Imza dogrulamasi -- "alg:none" saldirilarini engeller
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

            // Issuer dogrulamasi
            ValidateIssuer = true,
            ValidIssuer    = jwtSection["Issuer"],

            // Audience dogrulamasi
            ValidateAudience = true,
            ValidAudience    = jwtSection["Audience"],

            // Suresi dolmus token'lara izin verilmez
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// -----------------------------------------------------------------------
// Pipeline
// -----------------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

// Sira onemli: Authentication once, Authorization sonra
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
