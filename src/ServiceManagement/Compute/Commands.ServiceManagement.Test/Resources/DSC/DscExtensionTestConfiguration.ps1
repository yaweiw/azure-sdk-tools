configuration DscExtensionTestConfiguration
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
        [string] 
        $destinationPath
    )

    Node $AllNodes.Where{$_.Role -eq "TestNode"}.NodeName
    {
        File MyDirectory
        { 
            Type = 'Directory'
            DestinationPath = $destinationPath
            Ensure = "Present"
        }
    }
}
