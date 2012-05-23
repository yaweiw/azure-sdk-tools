$runtimeUrl = $args[0]
$overrideUrl = $args[1]
$current = [string] (Get-Location -PSProvider FileSystem)
$client = New-Object System.Net.WebClient

function downloadWithRetry {
	param([string]$url, [string]$dest, [int]$retry) 
	Write-Host
	Write-Host "Attempt: $retry\r\n"
    Write-Host
    trap {
    	Write-Host $_.Exception.ToString() + "\r\n"
	    if ($retry -lt 5) {
	    	$retry=$retry+1
	    	Write-Host
	    	Write-Host "Waiting 5 seconds and retrying\r\n"
	    	Write-Host
	    	Start-Sleep -s 5
	    	downloadWithRetry $url $dest $retry $client
	    }
	    else {
	    	Write-Host "Download failed \r\n"
	   		throw "Max number of retries downloading [5] exceeded\r\n" 	
	    }
    }
    $client.downloadfile($url, $dest)
}

function download($url, $dest) {
	Write-Host "Downloading $url \r\n"
	downloadWithRetry $url $dest 1
}

function verify($file) {
  return true
}

if ($overrideUrl) {
    Write-Host "Using override url: $overrideUrl \r\n"
	$url = $overrideUrl
}
else {
	$url = $runtimeUrl
}

$dest = $current + "\runtime.exe"
download $url $dest
verify $file
