using AuthNAndAuthZ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace AuthNAndAuthZ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController(IUserService userService) : ControllerBase
{
    // GUVENLIK ACIGI: "Algorithm Confusion / Algorithm None" saldirisina acik JWT dogrulama.
    //
    // CVE benzeri senaryo:
    //   Saldirgan, header'daki "alg" alanini "none" olarak degistirir ve imza bolumunu siler.
    //   Bu uygulama imzayi dogrulamadigindan sahte token'i gecerli kabul eder.
    //
    // Gercek uygulamalarda bu acik soyle istismar edilir:
    //   1. Ele gecirilmis ya da gozlemlenmis gecerli bir JWT alinir.
    //   2. Header decode edilip "alg":"none" yapilir, payload istenilen sekilde duzenlenir.
    //   3. Imza bolumu bos birakilir: "<header>.<payload>." seklinde token olusturulur.
    //   4. Uygulama algoritmay kontrol etmediginden token gecerli sayilir.

    /// <summary>
    /// Hesap bilgisini doner.
    ///
    /// GUVENSIIZ: JWT dogrulama kasitli olarak hatali uygulanmistir:
    ///   - Imza dogrulamasi tamamen devre disi birakilmistir.
    ///   - "alg:none" iceren token'lar kabul edilir.
    ///   - Payload yalnizca Base64 decode edilerek okunur; butunluk kontrolu yoktur.
    ///   - Saldirgan, istedigi claim degerlerini (or. rol, kullanici adi) token'a yerlestirebilir.
    /// </summary>
    [HttpGet("account")]
    public IActionResult GetAccount()
    {
        // 1. Authorization header kontrolu
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader)
            || !authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = "Bearer token gereklidir." });
        }

        var token = authHeader.ToString()["Bearer ".Length..].Trim();

        // 2. GUVENLIK ACIGI: JwtSecurityTokenHandler dogrudan TokenValidationParameters
        //    ile kullanilmiyor; bunun yerine token sadece "okunuyor" (imza kontrolu YOK).
        JwtSecurityToken parsedToken;
        try
        {
            var handler = new JwtSecurityTokenHandler();

            // ReadJwtToken sadece parse eder, DOGRULAMAZ.
            // ValidateToken() cagrilmadigi icin:
            //   - Imza kontrol edilmez.
            //   - "alg" degeri denetlenmez.
            //   - Expiration (exp) kontrolu yapilmaz.
            //   - Issuer / Audience dogrulanmaz.
            parsedToken = handler.ReadJwtToken(token);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Token parse edilemedi.", detail = ex.Message });
        }

        // 3. ACIK: Herhangi bir algoritma (dahil "none") kabul ediliyor.
        //    Saldirgan alg=none ile imzasiz token gonderip gecebilir.
        var algorithm = parsedToken.Header.Alg;

        // 4. Payload'dan claim'leri oku -- bunlar sahte olabilir!
        var username = parsedToken.Claims
            .FirstOrDefault(c => c.Type is "sub" or JwtRegisteredClaimNames.Sub)?.Value
            ?? "unknown";

        var role = parsedToken.Claims
            .FirstOrDefault(c => c.Type is "role" or "roles")?.Value
            ?? "none";

        // 5. SONUC: Imzasiz ya da manipule edilmis token basariyla islem yapiyor.
        return Ok(new
        {
            warning = "Bu endpoint guvensizdir! Imza dogrulamasi yapilmamaktadir.",
            algorithmFromToken = algorithm,
            username,
            role,
            claims = parsedToken.Claims.Select(c => new { c.Type, c.Value }),
            tokenHeader = new
            {
                alg = parsedToken.Header.Alg,
                typ = parsedToken.Header.Typ,
                kid = parsedToken.Header.Kid
            }
        });
    }

    // -------------------------------------------------------------------------
    // Guvenli JWT dogrulama -- Program.cs'deki middleware tarafindan yapilir
    // -------------------------------------------------------------------------

    /// <summary>
    /// Hesap bilgisini guvenli sekilde doner.
    ///
    /// JWT dogrulamasi tamamen Program.cs'deki AddJwtBearer() middleware'i tarafindan yapilir:
    ///   - Imza HMAC-SHA256 ile dogrulanir; "alg:none" token'lari reddedilir.
    ///   - Issuer, appsettings.json/Jwt:Issuer ile karsilastirilir.
    ///   - Audience, appsettings.json/Jwt:Audience ile karsilastirilir.
    ///   - Suresi dolmus token'lar (exp) otomatik olarak reddedilir.
    /// Bu action, gecersiz token ile hic cagirilmaz; istek middleware katmaninda durur.
    /// </summary>
    [HttpGet("secure-account")]
    [Authorize(Roles ="admin")]//iþi olmayan giremez:
    public IActionResult SecureGetAccount()
    {
        // Bu noktaya yalnizca gecerli, imzali ve suresi dolmamis token ile ulasilabilir.
        // Kullanici bilgileri User.Claims'ten guvenle okunur.
        var username  = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? "unknown";

        var role      = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                     ?? User.FindFirst("role")?.Value
                     ?? "none";

        var issuer    = User.FindFirst("iss")?.Value;
        var audience  = User.FindFirst("aud")?.Value;

        return Ok(new
        {
            message  = "Kimlik dogrulama basarili. Token gecerli, imzali ve suresi dolmamis.",
            username,
            role,
            issuer,
            audience,
            claims   = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    [HttpPost("login")]
    // Bu endpoint, demo amacli olarak JWT olusturur; gercek uygulamalarda kullanici dogrulama ve token olusturma
    // islemleri daha guvenli sekilde yapilmalidir.

        public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        // Demo: Sabit bir JWT token donduruyoruz; gercek uygulamada kullanici dogrulama yapilmali ve
        // token dinamik olarak olusturulmalidir.

        var validUser = await userService.ValidateCredentialsAsync(loginRequest.Username, loginRequest.Password);
        if (!validUser)
        {
            return Unauthorized(new { message = "Gecersiz kullanici adi veya sifre." });
        }



        var demoToken = new JwtSecurityToken(
            issuer: "AuthNAndAuthZ.Api",
            audience: "AuthNAndAuthZ.Client",
            claims: new[]
            {
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, "demo-user"),
                new System.Security.Claims.Claim("role", "admin")
            },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("bu-anahtar-uretim-ortaminda-environment-variable-veya-key-vault-uzerinden-gelmeli!")),
                SecurityAlgorithms.HmacSha256)
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(demoToken);
        return Ok(new
        {
            token = tokenString,
            message = "Bu token demo amacli olusturulmustur. Gercek uygulamalarda kullanici dogrulama yapilmali ve token dinamik olarak olusturulmalidir."
        });
    }

    public record LoginRequest(string Username, string Password);
}
