using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace RBACandABAC.AuthPolicies
{
    public class WithinLimitAuthorityRequirement : IAuthorizationRequirement
    {
        public decimal MaxLimit { get; }
        public WithinLimitAuthorityRequirement(decimal maxLimit)
        {
            MaxLimit = maxLimit;
        }
    }
    public class WithinLimitAuthorityHandler : AuthorizationHandler<WithinLimitAuthorityRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WithinLimitAuthorityHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, WithinLimitAuthorityRequirement requirement)
        {
            // Burada, yöneticinin yetkisinin belirli bir kredi limiti içinde olup olmadığını kontrol etmek için gerekli mantığı ekleyebilirsiniz.
            // Örneğin, yöneticinin mevcut kredi limitini bir veri kaynağından alabilir ve requirement.MaxLimit ile karşılaştırabilirsiniz.
            // Eğer yöneticinin yetkisi requirement.MaxLimit içinde ise, context.Succeed(requirement) çağırarak yetkiyi başarılı kılabilirsiniz.
            // Aksi takdirde, context.Fail() çağırarak yetkiyi başarısız kılabilirsiniz.
            // Örnek olarak, burada basit bir kontrol yapıyoruz:
            //decimal userCreditLimit = 500000; // Bu değeri gerçek uygulamada veri kaynağından almanız
            //
            // gerekir.


            var httpContext = _httpContextAccessor.HttpContext!;
            httpContext.Request.EnableBuffering();
            var body = await JsonSerializer.DeserializeAsync<LimitRequest>(httpContext.Request.Body);

            httpContext.Request.Body.Position = 0; // Stream'i başa sararak diğer middleware'lerin de okuyabilmesini sağlıyoruz.

            if (body?.NewLimit <= requirement.MaxLimit)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }



        }
    }
}
