namespace AuthNAndAuthZ.Services;

/// <summary>
/// Kullanıcı kimlik doğrulama işlemlerini soyutlayan servis arayüzü.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Verilen kullanıcı adı ve şifrenin geçerli olup olmadığını doğrular.
    /// </summary>
    /// <param name="username">Doğrulanacak kullanıcı adı.</param>
    /// <param name="password">Doğrulanacak düz metin şifre.</param>
    /// <returns>Kimlik bilgileri geçerliyse <c>true</c>, aksi hâlde <c>false</c>.</returns>
    Task<bool> ValidateCredentialsAsync(string username, string password);
}
