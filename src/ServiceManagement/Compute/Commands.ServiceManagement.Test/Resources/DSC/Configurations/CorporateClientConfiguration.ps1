param([string] $VMName, [string] $VMConfigDataPath, [string] $MofDestinationPath)

Configuration CorpClientVMConfiguration
{ 
    Node $AllNodes.Where{$_.Role -eq "CorpClient"}.NodeName
    {     	
       Import-DscResource -Name MSFT_xComputer

        xComputer NameAndDomain
        {
            Name     = $Node.MachineName
            DomainName = $Node.DomainName
            Credential = (Import-CliXML $Node.DomainCredFile)
        }

        Group RemoteDesktop
        {
            Ensure     = "Present"
            GroupName  = "Remote Desktop Users"
            Members    = @("Corporate\User1","Corporate\PAPA","Corporate\DeptHead")
            Credential = (Import-CliXML $Node.DomainCredFile)
            DependsOn  = "[xComputer]NameAndDomain"
        }
        
        Group Administrator
        {
            Ensure           = "Present"
            GroupName        = "Administrators"
            MembersToInclude = @("Corporate\PAPA","Corporate\DeptHead")
            Credential       = (Import-CliXML $Node.DomainCredFile)
            DependsOn        = "[xComputer]NameAndDomain"
        }
    }
}

# Generate mof
$scriptLocation = $PSScriptRoot
CorpClientVMConfiguration -OutputPath .
