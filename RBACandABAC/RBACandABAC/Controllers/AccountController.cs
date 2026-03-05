using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RBACandABAC.Settings;
using System.Security.Claims;

namespace RBACandABAC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(AccountService accountService) : ControllerBase
    {
        [HttpGet("{accountId}/statement")]
        [Authorize(Roles = $"{Roles.Customer},{Roles.Auditor}, {Roles.BranchManager}")]
        public IActionResult GetAccountStatement(string accountId)
        {

            /*
             * RBAC bizi, sisteme girişten korudu
             * fakat, kullanıcıların hangi hesaplara erişebileceği konusunda yeterli kontrol sağlamadı.
             * Bu, RBAC'ın sorumluluğu değil.
             * */

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Kullanıcının kimliğini alıyoruz

            //IDOR (Insecure Direct Object Reference) açığını önlemek için, kullanıcıların sadece kendi hesaplarına erişebilmesini sağlıyoruz.

            if (User.IsInRole(Roles.Customer))
            {
                var isOwner = accountService.IsAccountOwner(accountId, currentUserId);
                if (!isOwner)
                {
                    return NotFound(); // 403 Forbidden yerine NotFound döndürüyoruz, böylece kötü niyetli kullanıcılar var olmayan bir hesapla mı yoksa erişim izni olmayan bir hesapla mı karşılaştıklarını anlayamazlar.
                }
            }

            var statement = accountService.GetAccountStatement(accountId);
            return Ok(statement);

        }

        [HttpGet("{accountId}/statement-for-auditor")]
        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult GetAccountStatementForAuditor(string accountId)
        {
            // Denetçi rolüne sahip kullanıcıların tüm hesaplara erişmesine izin veriyoruz.
            var statement = accountService.GetAccountStatement(accountId);
            return Ok(statement);
        }

        [HttpPost("{accountId}/increase-limit")]
        [Authorize(Policy = Policies.CanIncreaseCreditLimit)]
        public async Task<IActionResult> IncreaseLimit() { 

                // Sadece belirli bir politika ile yetkilendirilmiş kullanıcıların kredi limitini artırmasına izin veriyoruz.
                // Bu, RBAC'ın ötesinde, kullanıcıların belirli eylemleri gerçekleştirme yeteneklerini kontrol etmek için ABAC'ı kullanır.
                // Politika, kullanıcının rolüne, departmanına veya diğer özelliklerine göre tanımlanabilir.
    
                // Kredi limitini artırma işlemi burada gerçekleştirilecek
                return Ok("Kredi limiti artırıldı.");

        }
           
    }
}
