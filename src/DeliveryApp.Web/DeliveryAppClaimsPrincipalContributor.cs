using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;
using OpenIddict.Abstractions;
using System.Linq;

namespace DeliveryApp.Web
{
    [Dependency(ReplaceServices = true)]
    public class DeliveryAppClaimsPrincipalContributor : IAbpClaimsPrincipalContributor, ITransientDependency
    {
        public Task ContributeAsync(AbpClaimsPrincipalContributorContext context)
        {
            // Add safety checks to prevent null reference exceptions
            if (context?.ClaimsPrincipal == null || context.ClaimsPrincipal.Identity == null)
            {
                return Task.CompletedTask;
            }

            var identity = context.ClaimsPrincipal.Identity as ClaimsIdentity;
            if (identity != null && identity.IsAuthenticated)
            {
                // Only add audience claim if it doesn't already exist
                if (!context.ClaimsPrincipal.HasClaim(OpenIddictConstants.Claims.Audience, "DeliveryApp"))
                {
                    identity.AddClaim(new Claim(OpenIddictConstants.Claims.Audience, "DeliveryApp"));
                }
                
                // Map custom JWT claims to ABP claims if they don't already exist
                var userId = context.ClaimsPrincipal.FindFirst("user_id")?.Value;
                if (!string.IsNullOrEmpty(userId) && !context.ClaimsPrincipal.HasClaim(AbpClaimTypes.UserId, userId))
                {
                    identity.AddClaim(new Claim(AbpClaimTypes.UserId, userId));
                }
                
                var email = context.ClaimsPrincipal.FindFirst("email")?.Value;
                if (!string.IsNullOrEmpty(email) && !context.ClaimsPrincipal.HasClaim(AbpClaimTypes.Email, email))
                {
                    identity.AddClaim(new Claim(AbpClaimTypes.Email, email));
                }
                
                var role = context.ClaimsPrincipal.FindFirst("role")?.Value;
                if (!string.IsNullOrEmpty(role) && !context.ClaimsPrincipal.HasClaim(AbpClaimTypes.Role, role))
                {
                    identity.AddClaim(new Claim(AbpClaimTypes.Role, role));
                }
                
                var name = context.ClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(name) && !context.ClaimsPrincipal.HasClaim(AbpClaimTypes.Name, name))
                {
                    identity.AddClaim(new Claim(AbpClaimTypes.Name, name));
                }
            }

            return Task.CompletedTask;
        }
    }
} 
