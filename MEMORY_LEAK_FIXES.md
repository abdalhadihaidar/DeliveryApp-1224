# Memory Leak Fixes for Delivery App

## Critical Issues Identified

### 1. HttpClient Memory Leaks
**Problem**: Creating new HttpClient instances in AuthService.cs and MobileAuthService.cs
**Impact**: Socket exhaustion, memory leaks, connection pool depletion

### 2. Entity Framework Memory Issues
**Problem**: Multiple ToList() calls on large datasets in SpecialOfferAppService.cs
**Impact**: Excessive memory consumption, GC pressure

### 3. Background Service Resource Management
**Problem**: Potential resource leaks in RecurringOffersBackgroundService
**Impact**: Accumulated memory usage over time

### 4. Missing Memory Configuration
**Problem**: No explicit memory management settings
**Impact**: Default limits too restrictive for application needs

## Immediate Actions Required

### 1. Fix HttpClient Usage
Replace direct HttpClient instantiation with HttpClientFactory pattern:

```csharp
// BEFORE (AuthService.cs line 1002)
using var httpClient = new HttpClient();

// AFTER
private readonly HttpClient _httpClient;
// Inject via constructor and reuse
```

### 2. Optimize Entity Framework Queries
Replace multiple ToList() calls with single query execution:

```csharp
// BEFORE
offers = offers.Where(o => o.IsValidAt(date)).ToList();
offers = offers.Where(o => o.Status.ToString() == input.Status).ToList();

// AFTER
var filteredOffers = offers.Where(o => o.IsValidAt(date) && 
    (string.IsNullOrEmpty(input.Status) || o.Status.ToString() == input.Status))
    .ToList();
```

### 3. Add Memory Configuration
Add to web.config:
```xml
<system.webServer>
  <aspNetCore processPath="dotnet" 
              arguments=".\DeliveryApp.HttpApi.Host.dll" 
              stdoutLogEnabled="false" 
              stdoutLogFile=".\logs\stdout" 
              hostingModel="inprocess">
    <environmentVariables>
      <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
      <environmentVariable name="DOTNET_gcServer" value="1" />
      <environmentVariable name="DOTNET_gcConcurrent" value="1" />
    </environmentVariables>
  </aspNetCore>
</system.webServer>
```

### 4. IIS Application Pool Settings
Configure application pool with:
- Private Memory Limit: 0 (unlimited) or increase to 2GB
- Virtual Memory Limit: 0 (unlimited) or increase to 4GB
- Rapid Fail Protection: Disable or increase failure count
- Recycling: Disable regular recycling

## Priority Implementation Order
1. Fix HttpClient usage (CRITICAL)
2. Optimize EF queries (HIGH)
3. Update IIS configuration (HIGH)
4. Add memory monitoring (MEDIUM)
5. Implement proper disposal patterns (MEDIUM)

