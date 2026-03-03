using AuthNAndAuthZ.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;

namespace AuthNAndAuthZ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    // GUVENLIK ACIGI: Kullanici adi ve sifre kaynak kodda hardcoded olarak tutulmaktadir.
    // Bu bilgiler kaynak kod yonetim sistemlerinde (Git, SVN vb.) acikca gorunur,
    // binary dosyalardan kolayca cikarilabilir ve degistirilemez (yeniden deploy gerektirir).
    private const string HardcodedUsername = "admin";
    private const string HardcodedPassword = "P@ssw0rd123!";

    /// <summary>
    /// Hesap bakiyesini doner.
    /// Basic Authentication ile korunmaktadir; ancak kimlik bilgileri kaynak kodda hardcoded'dir.
    /// </summary>
    [HttpGet("balance")]
    public IActionResult GetBalance()
    {
        // 1. Authorization header mevcut mu kontrol et
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"AccountAPI\"";
            return Unauthorized(new { message = "Authorization header eksik." });
        }

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(authHeaderValue!);

            // 2. Scheme "Basic" olmali
            if (!authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Sadece Basic authentication desteklenmektedir." });
            }

            // 3. Base64 decode -> "username:password"
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? "");
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

            if (credentials.Length != 2)
            {
                return Unauthorized(new { message = "Gecersiz credential formati." });
            }

            var username = credentials[0];
            var password = credentials[1];

            // 4. GUVENLIK ACIGI: Duz metin karsilastirma + hardcoded kimlik bilgileri.
            //    Timing attack'lara karsi savunmasizdir ve sifreler hash'lenmemistir.
            if (username != HardcodedUsername || password != HardcodedPassword)
            {
                return Unauthorized(new { message = "Kullanici adi veya sifre hatali." });
            }

            // 5. Kimlik dogrulama basarili -> bakiye dondur
            return Ok(new
            {
                accountHolder = username,
                balance = 15_750.00m,
                currency = "TRY",
                asOf = DateTime.UtcNow
            });
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Authorization header Base64 formatinda degil." });
        }
    }

    // -------------------------------------------------------------------------
    // IUserService ile Basic Authentication -- servis katmani devreye giriyor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Kullanici profil bilgisini doner.
    ///
    /// Credential dogrulamasi <see cref="IUserService.ValidateCredentialsAsync"/> araciligiyla
    /// servis katmanina devredilir. Controller kimlik dogrulama detaylarindan habersizdir;
    /// sadece sonucu (true/false) bilir.
    ///
    /// NOT: Bu ornekte UserService dummy hashing kullanmaktadir (demo amacli).
    /// Uretim kodunda PBKDF2/bcrypt/Argon2 ile hash dogrulamasi yapilmalidir.
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // 1. Authorization header kontrolu
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"AccountAPI\"";
            return Unauthorized(new { message = "Authorization header eksik." });
        }

        AuthenticationHeaderValue authHeader;
        try
        {
            authHeader = AuthenticationHeaderValue.Parse(authHeaderValue!);
        }
        catch
        {
            return Unauthorized(new { message = "Gecersiz Authorization header." });
        }

        if (!authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { message = "Sadece Basic authentication desteklenmektedir." });

        // 2. Base64 decode
        byte[] credentialBytes;
        try
        {
            credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? "");
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Authorization header Base64 formatinda degil." });
        }

        var parts = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
        if (parts.Length != 2)
            return Unauthorized(new { message = "Gecersiz credential formati." });

        var username = parts[0];
        var password = parts[1];

        // 3. Dogrulamayi servis katmanina devret -- controller sifre mantigini bilmez
        var isValid = await _userService.ValidateCredentialsAsync(username, password);
        if (!isValid)
            return Unauthorized(new { message = "Kullanici adi veya sifre hatali." });

        // 4. Kimlik dogrulama basarili -> profil bilgisi dondur
        return Ok(new
        {
            username,
            displayName = $"{char.ToUpper(username[0])}{username[1..]}",
            email = $"{username}@example.com",
            roles = new[] { "viewer" },
            authenticatedAt = DateTime.UtcNow
        });
    }
}