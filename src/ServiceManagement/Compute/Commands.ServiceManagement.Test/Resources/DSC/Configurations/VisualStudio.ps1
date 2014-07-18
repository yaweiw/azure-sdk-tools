Configuration VisualStudio
{
    Import-DscResource -Module xPSDesiredStateConfiguration
    Node $nodeName {
        xPackage VS
        {
            Ensure="Present"
            Name = "Microsoft Visual Studio Ultimate 2013"
            Path = "\\products\public\PRODUCTS\Developers\Visual Studio 2013\ultimate\vs_ultimate.exe"
            Arguments = "/quiet /noweb /Log c:\temp\vc.log"
            RunAsCredential = New-Object System.Management.Automation.PSCredential -ArgumentList $user, (ConvertTo-SecureString -String $password -AsPlainText -Force)
            Credential = New-Object System.Management.Automation.PSCredential -ArgumentList $user, (ConvertTo-SecureString -String $password -AsPlainText -Force)
            # we check VS existense by regkey presents, but we also can use ProductId for that
            ProductId = ""
            InstalledCheckRegKey = "SOFTWARE\Microsoft\DevDiv\winexpress\Servicing\12.0\coremsi"
            InstalledCheckRegValueName = "Install"
            InstalledCheckRegValueData = "1"    
        }
    }
}