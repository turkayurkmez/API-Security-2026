using Microsoft.AspNetCore.Authorization;

namespace RBACandABAC.AuthPolicies
{
    public class  BusinessHoursRequirement : IAuthorizationRequirement
    {
        
    }
    public class BusinessHoursHandler : AuthorizationHandler<BusinessHoursRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, BusinessHoursRequirement requirement)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var startTime = new TimeSpan(9, 0, 0); // 9:00 AM
            var endTime = new TimeSpan(17, 0, 0); // 5:00 PM
            if (currentTime >= startTime && currentTime <= endTime)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            return Task.CompletedTask;
        }

    }
}
