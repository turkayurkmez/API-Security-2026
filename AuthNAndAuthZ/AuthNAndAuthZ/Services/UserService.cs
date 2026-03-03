namespace AuthNAndAuthZ.Services;

/// <summary>
/// IUserService'in demo amacli somut implementasyonu.
///
/// DEMO / STUB UYARISI
/// Bu sinif yalnizca egitim amaclıdır. Gercek bir uygulamada:
///   - Kullanicilar ve hash'lenmis sifreleri bir veritabaninda saklanir.
///   - Sifreler PBKDF2, bcrypt veya Argon2 gibi algoritmalarla hash'lenir.
///   - Sabit zamanli karsilastirma (CryptographicOperations.FixedTimeEquals)
///     ile timing attack'lara karsi koruma saglanir.
///   - Bu sinif, bu sayimin hicbirini icermez.
/// </summary>
public class UserService : IUserService
{
    // DEMO: Kullanici listesi bellekte tutulmaktadir.
    //    Gercek uygulamada bu veriler veritabanindan, bir identity store'dan
    //    ya da harici bir kimlik saglayicidan (IdP) gelmelidir.
    //    Sifreler asla duz metin olarak saklanmamalidir.
    private static readonly Dictionary<string, string> _dummyUserStore = new()
    {
        // Format: { kullaniciAdi, duzMetinSifre }
        //
        // GERCEK UYGULAMADA ASLA KULLANMAYIN!
        //    Buradaki degerler "hash gibi gorunen ama aslinda duz metin" demektir.
        //    Gercekte bu alanin ici PBKDF2/bcrypt hash'i tasimalidir:
        //
        //    { "alice", "310000.<base64-salt>.<base64-hash>" }
        //
        //    ve dogrulama, BasicAuthenticationHandler'daki VerifyPassword() gibi
        //    bir metotla yapilmalidir.
        { "alice",   "AliceSecret" },
        { "bob",     "BobSecret"   }
    };

    /// <inheritdoc />
    public Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        // DEMO: Duz metin karsilastirmasi -- uretimde KULLANMAYIN!
        //    Gercek uygulamada hash dogrulamasi yapilir; ornek:
        //
        //    var storedHash = await _userRepository.GetPasswordHashAsync(username);
        //    return CryptographicOperations.FixedTimeEquals(
        //        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, algorithm, length),
        //        expectedHashBytes);
        //
        if (!_dummyUserStore.TryGetValue(username, out var storedPassword))
            return Task.FromResult(false);

        return Task.FromResult(storedPassword == password);
    }
}
