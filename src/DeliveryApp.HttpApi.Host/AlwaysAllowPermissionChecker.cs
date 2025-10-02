using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace DeliveryApp.HttpApi.Host;

/// <summary>
/// Permission checker that always allows access, effectively disabling ABP authorization
/// </summary>
public class AlwaysAllowPermissionChecker : IPermissionChecker
{
    public Task<bool> IsGrantedAsync(string name)
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsGrantedAsync(object user, string name)
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsGrantedAsync(object user, string name, string providerName)
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsGrantedAsync(object user, string name, string providerName, string providerKey)
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsGrantedAsync(ClaimsPrincipal? claimsPrincipal, string name)
    {
        return Task.FromResult(true);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(string[] names)
    {
        var result = new MultiplePermissionGrantResult();
        foreach (var name in names)
        {
            result.Result.Add(name, PermissionGrantResult.Granted);
        }
        return Task.FromResult(result);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(object user, string[] names)
    {
        var result = new MultiplePermissionGrantResult();
        foreach (var name in names)
        {
            result.Result.Add(name, PermissionGrantResult.Granted);
        }
        return Task.FromResult(result);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(object user, string[] names, string providerName)
    {
        var result = new MultiplePermissionGrantResult();
        foreach (var name in names)
        {
            result.Result.Add(name, PermissionGrantResult.Granted);
        }
        return Task.FromResult(result);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(object user, string[] names, string providerName, string providerKey)
    {
        var result = new MultiplePermissionGrantResult();
        foreach (var name in names)
        {
            result.Result.Add(name, PermissionGrantResult.Granted);
        }
        return Task.FromResult(result);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(ClaimsPrincipal? claimsPrincipal, string[] names)
    {
        var result = new MultiplePermissionGrantResult();
        foreach (var name in names)
        {
            result.Result.Add(name, PermissionGrantResult.Granted);
        }
        return Task.FromResult(result);
    }
}
