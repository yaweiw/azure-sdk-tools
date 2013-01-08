.".\Scaffolding.ps1"

function Get-Address
{
    param([array] $lines)
    Write-Log $lines
    foreach ($hardline in $lines)
    {
         Write-Log $hardline
        if ($hardline.indexOf('http:') -gt 0)
        {
            $outputlines = $hardline.split("`r`n")
            Write-Log $outputlines
            foreach($line in $outputlines)
            {
                Write-Log $line
                $start = $line.indexOf('http://')
                if ($start -gt 0)
                {
                   return ($line.substring($start))
                }
            }
        }
    }

    throw "Unable to determine service address"
}

function Test-NodeHelloInEmulator
{
    Stop-AzureEmulator
    $loc = Get-Location
    Create-NodeBaseService "myNodeService"
    try
    {
	    $info = Start-AzureEmulator
        Write-Log "Emulator output: $info"
        $address = Get-Address $info
        Write-Log "Using service address '$address'"
	    Dump-Document $address
    }
    finally
    {
	    Set-Location $loc
	    Stop-AzureEmulator
	    rm -Recurse ".\myNodeService"
    }
}

function Test-PHPHelloInEmulator
{
    Stop-AzureEmulator
    $loc = Get-Location
    Create-PHPWebService "myPHPService"
    try
    {
	    $info = Start-AzureEmulator
        Write-Log "Emulator output: $info"
        $address = Get-Address $info
        Write-Log "Using service address '$address'"
	    Dump-Document $address
    }
    finally
    {
	    Set-Location $loc
	    Stop-AzureEmulator
	    rm -Recurse ".\myPHPService"
    }
}

function Test-NodeServiceCreation
{
    $loc = Get-Location
    Create-NodeBaseService "myNodeService01"
	Set-Location $loc
    rm -Recurse ".\myNodeService01"
}

function Test-PHPServiceCreation
{
    $loc = Get-Location
    Create-PHPBaseService "myPHPService01"
	Set-Location $loc
	rm -Recurse ".\myPHPService01"
}