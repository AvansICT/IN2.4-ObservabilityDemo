# Traffic Generation Script for ObservabilityDemo
# This script sends continuous HTTP requests to the API endpoint

param(
    [string]$Url = "http://localhost:5000",
    [int]$RequestsPerSecond = 2,
    [int]$DurationSeconds = 60,
    [switch]$Infinite
)

Write-Host "Starting traffic generation..." -ForegroundColor Green
Write-Host "Target URL: $Url" -ForegroundColor Cyan
Write-Host "Requests per second: $RequestsPerSecond" -ForegroundColor Cyan

if ($Infinite) {
    Write-Host "Duration: Infinite (press Ctrl+C to stop)" -ForegroundColor Cyan
} else {
    Write-Host "Duration: $DurationSeconds seconds" -ForegroundColor Cyan
}

$delayMs = [int](1000 / $RequestsPerSecond)
$startTime = Get-Date
$requestCount = 0
$successCount = 0
$errorCount = 0

function Send-Request {
    param([string]$url)
    
    try {
        $response = Invoke-WebRequest -Uri $url -Method Get -UseBasicParsing -TimeoutSec 5
        $global:successCount++
        
        $content = $response.Content | ConvertFrom-Json
        $roll = $content.Roll
        
        Write-Host "[$($global:requestCount)] Success ($($response.StatusCode)): Rolled $roll (Delay: $($content.DelayMs)ms)" -ForegroundColor Green
    }
    catch {
        $global:errorCount++
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode) {
            Write-Host "[$($global:requestCount)] Error ($statusCode): $($_.Exception.Message)" -ForegroundColor Red
        } else {
            Write-Host "[$($global:requestCount)] Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`nSending requests...`n" -ForegroundColor Yellow

try {
    while ($true) {
        $requestCount++
        Send-Request -url $Url
        
        # Check if we should stop
        if (-not $Infinite) {
            $elapsed = (Get-Date) - $startTime
            if ($elapsed.TotalSeconds -ge $DurationSeconds) {
                break
            }
        }
        
        Start-Sleep -Milliseconds $delayMs
    }
}
catch [System.Management.Automation.PipelineStoppedException] {
    Write-Host "`nTraffic generation stopped by user." -ForegroundColor Yellow
}
finally {
    Write-Host "`n--- Summary ---" -ForegroundColor Cyan
    Write-Host "Total Requests: $requestCount" -ForegroundColor White
    Write-Host "Successful: $successCount" -ForegroundColor Green
    Write-Host "Errors: $errorCount" -ForegroundColor Red
    
    if ($requestCount -gt 0) {
        $successRate = [math]::Round(($successCount / $requestCount) * 100, 2)
        Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -ge 95) { "Green" } else { "Yellow" })
    }
}
