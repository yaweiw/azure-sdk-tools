param([string] $VMName, [string] $VMConfigDataPath, [string] $MofDestinationPath)

Configuration DomainController
{
    Import-DscResource -Name MSFT_xComputer, MSFT_xADDomain, MSFT_xADUser

    Node $AllNodes.Where({$_.MachineName -eq $VMName}).NodeName
    {   
        xComputer MachineName
        {
            Name     = $Node.MachineName
        }

        WindowsFeature ADDS
        {
            Ensure     = "Present"
            Name       = "AD-Domain-Services"
            DependsOn  = "[xComputer]MachineName"
        }

        xADDomain Forest
        {
            DomainName = $Node.DomainName
            DomainAdministratorCredential = (Import-Clixml $Node.DomainCredFile)
            SafemodeAdministratorPassword = (Import-Clixml $Node.DomainCredFile)
            DependsOn               = "[WindowsFeature]ADDS"
        }  
                
        foreach($User in $Node.Users)
        {
            xADUser $User.UserName
            {
                Ensure = "Present"
                UserName = $User.UserName
                Password = (Import-Clixml $User.UserCredFile)
                DomainName = $Node.DomainName
                DomainAdministratorCredential = (Import-Clixml $Node.DomainCredFile)
                DependsOn = "[DNSTransferZone]Setting"
            }
        }
    }
}
