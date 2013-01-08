.".\Common.ps1"

$CloudConfig=".\ServiceConfiguration.Cloud.cscfg"
$LocalConfig=".\ServiceConfiguration.Local.cscfg"
$ServiceDefinition=".\ServiceDefinition.csdef"
$DeploymentSettings=".\DeploymentSettings.json"

function Create-Service
{
    [CmdletBinding()]
    param([Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)] [string] $ServiceName)
	PROCESS
	{
	    New-AzureServiceProject -ServiceName $ServiceName
		Validate-ServiceScaffolding $ServiceName
	}

}

function Validate-ServiceScaffolding
{
    [CmdletBinding()]
    param([Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)] [string] $ServiceName)
	PROCESS
	{
	    Assert-Exists $ServiceDefinition
		Assert-Exists $DeploymentSettings
		Assert-Exists $LocalConfig
		Assert-Exists $CloudConfig
	}
}

function Create-NodeBaseService
{
    [CmdletBinding()]
    param([Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)] [string] $ServiceName)
	PROCESS
	{
	    Create-Service $ServiceName
		Add-AzureNodeWebRole
		Add-AzureNodeWorkerRole
		Dump-Contents "." -recurse
	}

}

function Create-PHPBaseService
{
    [CmdletBinding()]
    param([Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)] [string] $ServiceName)
	PROCESS
	{
	    Create-Service $ServiceName
		Add-AzurePHPWebRole
		Add-AzurePHPWorkerRole
		Dump-Contents "." -recurse
	}
}

function Create-PHPWebService
{
    [CmdletBinding()]
    param([Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)] [string] $ServiceName)
	PROCESS
	{
	    Create-Service $ServiceName
		Add-AzurePHPWebRole
		Dump-Contents "." -recurse
	}
}
function Dump-Document
{
   param([string]$uri)
   $request = [System.Net.WebRequest]::Create($uri)
   $response = $request.GetResponse();
   Write-Log ("GET URI $uri Status code: "+ $response.StatusCode)
   Assert-True {$response.StatusCode -eq [System.Net.HttpStatusCode]::OK} "[Dump-Documet]: Failure, received bad status code"
   $reader = New-Object -TypeName System.IO.StreamReader(($response.GetResponseStream()))
   Write-Log ("Content: " + ($reader.ReadToEnd()))
}