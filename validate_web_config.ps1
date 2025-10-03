# Web.config Validation Script
# This script validates web.config for common IIS configuration issues

param(
    [string]$WebConfigPath = "C:\inetpub\wwwroot\WASEL\web.config"
)

Write-Host "=== Web.config Validation ===" -ForegroundColor Green

# Check if web.config exists
if (-not (Test-Path $WebConfigPath)) {
    Write-Host "Error: web.config not found at: $WebConfigPath" -ForegroundColor Red
    exit 1
}

Write-Host "Validating web.config at: $WebConfigPath" -ForegroundColor Green

# Read the web.config content
$webConfigContent = Get-Content $WebConfigPath -Raw
$validationErrors = @()
$warnings = @()

# Test 1: Check if XML is well-formed
Write-Host "1. Checking XML syntax..." -ForegroundColor Yellow
try {
    [xml]$xml = $webConfigContent
    Write-Host "   ✓ XML syntax is valid" -ForegroundColor Green
} catch {
    $validationErrors += "XML syntax error: $($_.Exception.Message)"
    Write-Host "   ✗ XML syntax error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Check for duplicate MIME type entries
Write-Host "2. Checking for duplicate MIME type entries..." -ForegroundColor Yellow
$mimeMapNodes = $xml.configuration.'system.webServer'.staticContent.mimeMap
if ($mimeMapNodes) {
    $fileExtensions = @()
    foreach ($node in $mimeMapNodes) {
        if ($fileExtensions -contains $node.fileExtension) {
            $validationErrors += "Duplicate MIME type entry for: $($node.fileExtension)"
            Write-Host "   ✗ Duplicate MIME type entry for: $($node.fileExtension)" -ForegroundColor Red
        } else {
            $fileExtensions += $node.fileExtension
        }
    }
    if ($validationErrors.Count -eq 0) {
        Write-Host "   ✓ No duplicate MIME type entries found" -ForegroundColor Green
    }
}

# Test 3: Check for duplicate file extension restrictions
Write-Host "3. Checking for duplicate file extension restrictions..." -ForegroundColor Yellow
$fileExtensionNodes = $xml.configuration.'system.webServer'.security.requestFiltering.fileExtensions.add
if ($fileExtensionNodes) {
    $restrictedExtensions = @()
    foreach ($node in $fileExtensionNodes) {
        if ($restrictedExtensions -contains $node.fileExtension) {
            $validationErrors += "Duplicate file extension restriction for: $($node.fileExtension)"
            Write-Host "   ✗ Duplicate file extension restriction for: $($node.fileExtension)" -ForegroundColor Red
        } else {
            $restrictedExtensions += $node.fileExtension
        }
    }
    if ($validationErrors.Count -eq 0) {
        Write-Host "   ✓ No duplicate file extension restrictions found" -ForegroundColor Green
    }
}

# Test 4: Check ASP.NET Core configuration
Write-Host "4. Checking ASP.NET Core configuration..." -ForegroundColor Yellow
$aspNetCore = $xml.configuration.'system.webServer'.aspNetCore
if ($aspNetCore) {
    if ($aspNetCore.processPath -eq "dotnet") {
        Write-Host "   ✓ Process path is correctly set to 'dotnet'" -ForegroundColor Green
    } else {
        $warnings += "Process path is not set to 'dotnet': $($aspNetCore.processPath)"
        Write-Host "   ⚠ Process path is not set to 'dotnet': $($aspNetCore.processPath)" -ForegroundColor Yellow
    }
    
    if ($aspNetCore.arguments -like "*DeliveryApp.Web.dll*") {
        Write-Host "   ✓ Arguments correctly reference DeliveryApp.Web.dll" -ForegroundColor Green
    } else {
        $warnings += "Arguments do not reference DeliveryApp.Web.dll: $($aspNetCore.arguments)"
        Write-Host "   ⚠ Arguments do not reference DeliveryApp.Web.dll: $($aspNetCore.arguments)" -ForegroundColor Yellow
    }
    
    if ($aspNetCore.hostingModel -eq "inprocess") {
        Write-Host "   ✓ Hosting model is set to 'inprocess'" -ForegroundColor Green
    } else {
        $warnings += "Hosting model is not set to 'inprocess': $($aspNetCore.hostingModel)"
        Write-Host "   ⚠ Hosting model is not set to 'inprocess': $($aspNetCore.hostingModel)" -ForegroundColor Yellow
    }
} else {
    $validationErrors += "ASP.NET Core configuration not found"
    Write-Host "   ✗ ASP.NET Core configuration not found" -ForegroundColor Red
}

# Test 5: Check environment variables
Write-Host "5. Checking environment variables..." -ForegroundColor Yellow
$envVars = $aspNetCore.environmentVariables.environmentVariable
if ($envVars) {
    $hasProductionEnv = $false
    $hasUrls = $false
    
    foreach ($envVar in $envVars) {
        if ($envVar.name -eq "ASPNETCORE_ENVIRONMENT" -and $envVar.value -eq "Production") {
            $hasProductionEnv = $true
            Write-Host "   ✓ ASPNETCORE_ENVIRONMENT is set to Production" -ForegroundColor Green
        }
        if ($envVar.name -eq "ASPNETCORE_URLS") {
            $hasUrls = $true
            Write-Host "   ✓ ASPNETCORE_URLS is configured" -ForegroundColor Green
        }
    }
    
    if (-not $hasProductionEnv) {
        $warnings += "ASPNETCORE_ENVIRONMENT is not set to Production"
        Write-Host "   ⚠ ASPNETCORE_ENVIRONMENT is not set to Production" -ForegroundColor Yellow
    }
    
    if (-not $hasUrls) {
        $warnings += "ASPNETCORE_URLS is not configured"
        Write-Host "   ⚠ ASPNETCORE_URLS is not configured" -ForegroundColor Yellow
    }
} else {
    $validationErrors += "No environment variables configured"
    Write-Host "   ✗ No environment variables configured" -ForegroundColor Red
}

# Test 6: Check URL rewrite rules
Write-Host "6. Checking URL rewrite configuration..." -ForegroundColor Yellow
$rewriteRules = $xml.configuration.'system.webServer'.rewrite.rules.rule
if ($rewriteRules) {
    $hasApiRule = $false
    $hasSpaRule = $false
    
    foreach ($rule in $rewriteRules) {
        if ($rule.name -eq "API Routes") {
            $hasApiRule = $true
            Write-Host "   ✓ API Routes rule found" -ForegroundColor Green
        }
        if ($rule.name -eq "SPA Routes") {
            $hasSpaRule = $true
            Write-Host "   ✓ SPA Routes rule found" -ForegroundColor Green
        }
    }
    
    if (-not $hasApiRule) {
        $warnings += "API Routes rewrite rule not found"
        Write-Host "   ⚠ API Routes rewrite rule not found" -ForegroundColor Yellow
    }
    
    if (-not $hasSpaRule) {
        $warnings += "SPA Routes rewrite rule not found"
        Write-Host "   ⚠ SPA Routes rewrite rule not found" -ForegroundColor Yellow
    }
} else {
    $warnings += "No URL rewrite rules configured"
    Write-Host "   ⚠ No URL rewrite rules configured" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "=== Validation Summary ===" -ForegroundColor Green

if ($validationErrors.Count -eq 0) {
    Write-Host "✓ No critical errors found" -ForegroundColor Green
} else {
    Write-Host "✗ Critical errors found:" -ForegroundColor Red
    foreach ($error in $validationErrors) {
        Write-Host "  - $error" -ForegroundColor Red
    }
}

if ($warnings.Count -eq 0) {
    Write-Host "✓ No warnings" -ForegroundColor Green
} else {
    Write-Host "⚠ Warnings:" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "  - $warning" -ForegroundColor Yellow
    }
}

Write-Host ""
if ($validationErrors.Count -eq 0) {
    Write-Host "🎉 Web.config validation passed! Your configuration should work correctly." -ForegroundColor Green
} else {
    Write-Host "❌ Web.config has critical errors that need to be fixed." -ForegroundColor Red
    Write-Host "Please fix the errors above before deploying." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Total Issues Found:" -ForegroundColor Cyan
Write-Host "  - Critical Errors: $($validationErrors.Count)" -ForegroundColor $(if ($validationErrors.Count -eq 0) { "Green" } else { "Red" })
Write-Host "  - Warnings: $($warnings.Count)" -ForegroundColor $(if ($warnings.Count -eq 0) { "Green" } else { "Yellow" })









