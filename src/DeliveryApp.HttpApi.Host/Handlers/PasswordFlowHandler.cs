using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using static OpenIddict.Server.OpenIddictServerEvents;
using AbpIdentityUser = Volo.Abp.Identity.IdentityUser;

namespace DeliveryApp.HttpApi.Host.Handlers
{
    public class PasswordFlowHandler : IOpenIddictServerHandler<HandleTokenRequestContext>, ITransientDependency
    {
        private readonly IdentityUserManager _userManager;
        private readonly ILogger<PasswordFlowHandler> _logger;

        public PasswordFlowHandler(
            IdentityUserManager userManager,
            ILogger<PasswordFlowHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async ValueTask HandleAsync(HandleTokenRequestContext context)
        {
            // Only handle password grant type
            if (!context.Request.IsPasswordGrantType())
            {
                return;
            }

            try
            {
                _logger.LogInformation("Processing password flow authentication for username: {Username}", context.Request.Username);
                
                // Find user by username/email
                var user = await _userManager.FindByEmailAsync(context.Request.Username);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", context.Request.Username);
                    context.Reject(
                        error: OpenIddictConstants.Errors.InvalidGrant,
                        description: "The username/password couple is invalid.");
                    return;
                }

                _logger.LogInformation("User found: {UserId}, Email: {Email}, UserName: {UserName}", 
                    user.Id, user.Email, user.UserName);

                // Validate password using ABP IdentityUserManager
                var isValidPassword = await _userManager.CheckPasswordAsync(user, context.Request.Password);
                if (!isValidPassword)
                {
                    _logger.LogWarning("Invalid password for user: {Email}", user.Email);
                    context.Reject(
                        error: OpenIddictConstants.Errors.InvalidGrant,
                        description: "The username/password couple is invalid.");
                    return;
                }

                _logger.LogInformation("Password validation successful for user: {Email}", user.Email);

                // Validate user properties
                if (user.Id == Guid.Empty)
                {
                    _logger.LogError("User has invalid ID: {UserId}", user.Id);
                    context.Reject(
                        error: OpenIddictConstants.Errors.ServerError,
                        description: "Invalid user data.");
                    return;
                }

                // Create principal with proper claims for OpenIddict
                var identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()),
                        new Claim(OpenIddictConstants.Claims.Name, user.UserName ?? user.Email ?? ""),
                        new Claim(OpenIddictConstants.Claims.Email, user.Email ?? ""),
                        new Claim(OpenIddictConstants.Claims.EmailVerified, user.EmailConfirmed.ToString().ToLowerInvariant())
                    },
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    OpenIddictConstants.Claims.Name,
                    OpenIddictConstants.Claims.Role);

                var principal = new ClaimsPrincipal(identity);

                // Add roles
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role));
                }

                // Validate principal before signing in
                if (principal == null || principal.Identity == null)
                {
                    _logger.LogError("Failed to create valid ClaimsPrincipal for user {UserId}", user.Id);
                    context.Reject(
                        error: OpenIddictConstants.Errors.ServerError,
                        description: "Failed to create authentication principal.");
                    return;
                }

                _logger.LogInformation("Successfully created principal for user {UserId} with {RoleCount} roles", 
                    user.Id, roles.Count);
                
                // Log principal details for debugging
                _logger.LogDebug("Principal identity name: {IdentityName}, is authenticated: {IsAuthenticated}", 
                    principal.Identity?.Name, principal.Identity?.IsAuthenticated);
                
                // Set the principal and mark as handled
                try
                {
                    context.SignIn(principal);
                    _logger.LogInformation("Successfully signed in principal for user: {UserId}", user.Id);
                }
                catch (Exception signInEx)
                {
                    _logger.LogError(signInEx, "Error during context.SignIn for user: {UserId}", user.Id);
                    context.Reject(
                        error: OpenIddictConstants.Errors.ServerError,
                        description: "An error occurred during authentication.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password flow authentication");
                context.Reject(
                    error: OpenIddictConstants.Errors.ServerError,
                    description: "An error occurred while processing the request.");
            }
        }
    }
}
