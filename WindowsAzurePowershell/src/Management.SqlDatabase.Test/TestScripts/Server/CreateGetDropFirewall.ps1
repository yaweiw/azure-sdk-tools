# ----------------------------------------------------------------------------------
#
# Copyright 2011 Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

[CmdletBinding()]
Param
(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateNotNullOrEmpty()]
    [string]
    $subscriptionID, 
    [Parameter(Mandatory=$true, Position=1)]
    [ValidateNotNullOrEmpty()]
    [String]
    $SerializedCert,
    [Parameter(Mandatory=$true, Position=2)]
    [ValidateNotNullOrEmpty()]
    [String]
    $serverLocation
)
Write-Output "`$subscriptionID=$subscriptionID"
Write-Output "`$SerializedCert=$SerializedCert"
Write-Output "`$serverLocation=$serverLocation"

. .\CommonFunctions.ps1

Try
{
    Init-TestEnvironment
    Init-AzureSubscription -subscriptionID $subscriptionID -SerializedCert $SerializedCert
    $isTestPass = $False
    
    # Create Server
    $loginName="mylogin1"
    $loginPassword="Sql@zure1"
    Write-Output "Creating server"
    $server = New-AzureSqlDatabaseServer -AdministratorLogin $loginName -AdministratorLoginPassword $loginPassword -Location $serverLocation
    Assert {$server} "Server is not created"
    Write-Output "Server $($server.ServerName) created"
    
    # Create two Firewall rules
    $rule1Name="rule1"
    $rule1StartIP="1.0.0.0"
    $rule1EndIP="2.0.0.0"
    Write-Output "Creating Firewall rule $rule1Name ..."
    $rule = New-AzureSqlDatabaseServerFirewallRule -ServerName $server.ServerName -RuleName $rule1Name -StartIpAddress $rule1StartIP -EndIpAddress $rule1EndIP
    Validate-SqlDatabaseServerFirewallRuleContext -Actual $rule -ExpectedRuleName $rule1Name -ExpectedStartIpAddress $rule1StartIP -ExpectedEndIpAddress $rule1EndIP -ExpectedServerName $server.ServerName -ExpectedOperationDescription "New-AzureSqlDatabaseServerFirewallRule"
    Write-Output "created"
    
    $rule2Name="rule2"
    $rule2StartIP="2.3.4.5"
    $rule2EndIP="3.4.5.6"
    Write-Output "Creating Firewall rule $rule2Name ..."
    $rule = New-AzureSqlDatabaseServerFirewallRule -ServerName $server.ServerName -RuleName $rule2Name -StartIpAddress $rule2StartIP -EndIpAddress $rule2EndIP
    Write-Output "created"
    Validate-SqlDatabaseServerFirewallRuleContext -Actual $rule -ExpectedRuleName $rule2Name -ExpectedStartIpAddress $rule2StartIP -ExpectedEndIpAddress $rule2EndIP -ExpectedServerName $server.ServerName -ExpectedOperationDescription "New-AzureSqlDatabaseServerFirewallRule"
    
    # Get Firewall rules and validate
    Write-Output "Getting firewall rules..."
    $rules = Get-AzureSqlDatabaseServerFirewallRule -ServerName $server.ServerName
    Write-Output "Got firewall rules"
    Assert {$rules} "Get firewall rule didn't return any rule"
    Assert {$rules.Count -eq 2} "Get firewall rule didn't return expected number of rules 2. But returned $rule.Count"
    
    Write-Output "validating Firewall rule $rule1Name ..."
    $rule = $rules | Where-Object {$_.RuleName -eq $rule1Name}
    Validate-SqlDatabaseServerFirewallRuleContext -Actual $rule -ExpectedRuleName $rule1Name -ExpectedStartIpAddress $rule1StartIP -ExpectedEndIpAddress $rule1EndIP -ExpectedServerName $server.ServerName -ExpectedOperationDescription "Get-AzureSqlDatabaseServerFirewallRule"

    Write-Output "validating Firewall rule $rule2Name ..."
    $rule = $rules | Where-Object {$_.RuleName -eq $rule2Name}
    Validate-SqlDatabaseServerFirewallRuleContext -Actual $rule -ExpectedRuleName $rule2Name -ExpectedStartIpAddress $rule2StartIP -ExpectedEndIpAddress $rule2EndIP -ExpectedServerName $server.ServerName -ExpectedOperationDescription "Get-AzureSqlDatabaseServerFirewallRule"
    
    # Update a firewall rule and validate.
    $rule1StartIP = "255.255.255.254"
    $rule1EndIP="255.255.255.255"
    Write-Output "Updating Firewall rule $rule1Name ..."
    $rule = Set-AzureSqlDatabaseServerFirewallRule $server.ServerName -RuleName $rule1Name -StartIpAddress $rule1StartIP -EndIpAddress $rule1EndIP
    Validate-SqlDatabaseServerFirewallRuleContext -Actual $rule -ExpectedRuleName $rule1Name -ExpectedStartIpAddress $rule1StartIP -ExpectedEndIpAddress $rule1EndIP -ExpectedServerName $server.ServerName -ExpectedOperationDescription "Set-AzureSqlDatabaseServerFirewallRule"
    Write-Output "updated"

    Write-Output "Getting firewall rules..."
    $rules = Get-AzureSqlDatabaseServerFirewallRule -ServerName $server.ServerName
    Write-Output "Got firewall rules"
    Write-Output "validating Firewall rule $rule1Name ..."
    $rule = $rules | Where-Object {$_.RuleName -eq $rule1Name}
    Validate-SqlDatabaseServerFirewallRuleContext -Actual $rule -ExpectedRuleName $rule1Name -ExpectedStartIpAddress $rule1StartIP -ExpectedEndIpAddress $rule1EndIP -ExpectedServerName $server.ServerName -ExpectedOperationDescription "Get-AzureSqlDatabaseServerFirewallRule"
    
    # Delete a Firewall rules
    Write-Output "Deleting firewall rule $rule1Name ..."
    Remove-AzureSqlDatabaseServerFirewallRule -ServerName $server.ServerName -RuleName $rule1Name -Force
    Write-Output "Deleted"
    $rules = Get-AzureSqlDatabaseServerFirewallRule -ServerName $server.ServerName | Where-Object {$_.RuleName -eq $rule1Name}
    Assert {$rules -eq $null} "Firewall rule $rule1Name is not dropped"
    
    $isTestPass = $True
}
Finally
{
    Drop-Server $server
    Write-TestResult $IsTestPass
}

