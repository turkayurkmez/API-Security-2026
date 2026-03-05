

namespace RBACandABAC.Controllers
{
    public class AccountService
    {
        public dynamic GetAccountStatement(string accountId)
        {
            // Hesap durumu verilerini döndürmek için bir örnek nesne oluşturuyoruz.
            return new
            {
                AccountId = accountId,
                Balance = 1000,
                Transactions = new[]
                {
                    new { Date = DateTime.Now.AddDays(-1), Amount = -100, Description = "ATM Withdrawal" },
                    new { Date = DateTime.Now.AddDays(-2), Amount = 200, Description = "Deposit" }
                }
            };
        }

        public bool IsAccountOwner(string accountId, string? currentUserId)
        {
            // Bu metot, kullanıcının belirtilen hesaba sahip olup olmadığını kontrol eder.
            // Gerçek uygulamada, bu kontrol veritabanı sorgusu veya başka bir veri kaynağı üzerinden yapılır.
            // Burada basit bir örnek olarak, kullanıcı ID'sinin "user123" olduğunu varsayıyoruz ve sadece bu kullanıcıya erişim izni veriyoruz.
            if (currentUserId == accountId)
            {
                return true; // Kullanıcı hesabın sahibi
            }
            return false; // Kullanıcı hesabın sahibi değil

        }
    }
}