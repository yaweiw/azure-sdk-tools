<h1>Windows Azure PowerShell</h1>
<p>This repo contains a set of PowerShell commandlets for developers and administrators to deploy and manage Windows Azure applications. It includes the following:

<ul>
	<li>Cmdlets for developers to deploy both node.js and PHP applications.</li> 
	<li>Cmdlets for IT Administrators to manage their Windows Azure environments.</li>
</p>
For documentation on how to build and deploy applications to Windows Azure please see the 
<a href="http://www.windowsazure.com/en-us/develop">Windows Azure Developer Center</a>.</p>

For comprehensive documentation on the developer cmdlets see <a href="http://www.windowsazure.com/en-us/develop/nodejs/how-to-guides/powershell-cmdlets/">here</a>.

For comprehensive documentation on the full set of Windows Azure cmdlets see
<a href="http://go.microsoft.com/fwlink/?linkID=254459&clcid=0x409">Windows Azure Management Center</a>.</p> 

<h1>Developer Cmdlets</h1>
<p>Help</p>
<ul>
    <li><strong>help node-dev</strong> - List node.js development cmdlets</li>
    <li><strong>help php-dev</strong> - List PHP development cmdlets</li>
</ul>
<p>Configure machines for publishing to the cloud.</p>
<ul>
    <li><strong>Get-AzurePublishSettingsFile</strong> - Downloads a Windows Azure publish profile
    to your local computer </li>
    <li><strong>Import-AzurePublishSettingsFile</strong> - Imports the Publish Profile to enable
    publishing to Azure and managing hosted services</li>
</ul>
<p>Create services and roles that are preconfigured to use Node.js.</p>
<ul>
    <li><strong>New-AzureServiceProject</strong> - Creates scaffolding for a new service</li>
    <li><strong>Add-AzureNodeWebRole</strong> - Creates scaffolding for a Node.js application using web role which will be hosted in the cloud via IIS</li>
    <li><strong>Add-AzureNodeWorkerRole</strong> - Creates scaffolding for a Node.js application using worker role which will be hosted in the cloud via node.exe</li>
    <li><strong>Add-AzurePHPWebRole</strong> - Creates scaffolding for a PHP application using web role which will be hosted in the cloud via IIS</li>
    <li><strong>Add-AzurePHPWorkerRole</strong> - Creates scaffolding for a PHP application which will be hosted in the cloud via php.exe</li>

</ul>
<p>Debug your applications in the local compute and storage emulators</p>
<ul>
    <li><strong>Start-AzureEmulator</strong> - Starts both the compute and storage emulators
    and executes the service</li>
    <li><strong>Stop-AzureEmulator</strong> - Stops the compute emulator</li>
</ul>
<p>Publish your service to the cloud and configure publish settings</p>
<ul>
    <li><strong>Publish-AzureServiceProject</strong> - Publish the current service to the cloud</li>
    <li><strong>Set-AzureServiceProject</strong> - Configure service name, deployment location and storage account to use for publishing the service to the cloud</li>
    <li><strong>Set-AzureServiceProjectRole</strong> - Configure role instance count</li>
</ul>
<p>Manage your hosted services</p>
<ul>
    <li><strong>Start-AzureService</strong> - Starts a hosted service.</li>
    <li><strong>Stop-AzureService</strong> - Stops a hosted service</li>
    <li><strong>Remove-AzureService</strong> - Removes a hosted service</li>
    <li><strong>Get-AzureStorageAccount</strong> - Displays storage account details for the current Azure subscription.</li>
	<li><strong>Get-AzureStorageKey</strong> - Displays storage account keys for the specified storage account name. 
    <li><strong>Enable-AzureServiceProjectRemoteDesktop</strong> - Enables remote desktop access to a hosted service</li>
    <li><strong>Disable-AzureServiceProjectRemoteDesktop</strong> - Disables remote desktop access to a hosted service</li>
</ul>

<h1>Getting Started</h1>
<h2>Download Source Code</h2>
<p>To get the source code of the SDK via git just type:<br/>
<pre>git clone https://github.com/WindowsAzure/azure-sdk-tools.git<br/>cd ./azure-sdk-tools</pre>
</p>
<h2>General Install Prerequisites</h2>
<ul>
    <li><a href="http://www.microsoft.com/windowsazure/sdk/">Windows Azure SDK</a></li>
    <li><a href="http://technet.microsoft.com/en-us/scriptcenter/dd742419">Windows PowerShell 2.0</a></li>
    <li><a href="http://wix.sourceforge.net/">WiX</a> (Only needed if you want to build the setup project)</li>
</ul>
<h2>Node.js Prerequisites (developer only)</h2>
<ul>
    <li><a href="http://nodejs.org/">Node.js</a></li>
    <li><a href="https://github.com/tjanczuk/iisnode">IISNode</a></li>
</ul>
<h2>PHP Prerequisites (developer only)</h2>
<ul>
    <li><a href="http://php.iis.net/">PHP</a></li>
</ul>
</h2>

<h2>Configure PowerShell to automatically load commandlets</h2>
<ol>
    <li>Create a folder inside your user's Documents folder and name it <strong>WindowsPowerShell</strong></li>
    <li>Inside that folder create a file called <strong>Microsoft.PowerShell_profile.ps1</strong></li>
    <li>Edit the file in a text editor and add the following contents<br/>
    <pre>Import-Module<br/>PATH_TO_AZURE-SDK-TOOLS_CLONE\Package\Release\Microsoft.WindowsAzure.Management.psd1</pre></li>
    <li>After you build the commandlets project, you can then open a PowerShell window and you should be able to use the commandlets. Please note that if you want to rebuild the project, you have close the PowerShell window, and then reopen it.</li>
</ol>

<h1>Quick start</h1>
<ol>
    <li>First, create an Azure hosted service called HelloWorld by typing<br/>
    <pre>New-AzureServiceProject HelloWorld</pre></li>
    <li>Inside the HelloWorld folder, add a new Web Role by typing<br/>
    <pre>Add-AzureNodeWebRole or Add-AzurePHPWebRole</pre></li>
    <li>Test out the application in the local emulator by typing<br/>
    <pre>Start-AzureEmulator -Launch</pre></li>
    <li>You are now ready to publish to the cloud service. Go ahead and register
    for a Windows Azure account and make sure you have your credentials handy.</li>
    <li>Get your account's publish settings and save them to a file<br/>
    <pre>Get-AzurePublishSettingsFile</pre></li>
    <li>Now import the settings<br/>
    <pre>Import-AzurePublishSettingsFile PATH_TO_PUBLISH_SETTINGS_FILE</pre></li>
    <li>You are now ready to publish to the cloud. Make sure you specify a
    unique name for your application to ensure there aren't any conflicts during
    the publish process<br/>
    <pre>Publish-AzureService -ServiceName UNIQUE_NAME -Launch</pre></li>
</ol>

<h1>Need Help?</h1>
<p>Be sure to check out the Windows Azure <a href="http://go.microsoft.com/fwlink/?LinkId=234489">
Developer Forums on Stack Overflow</a> if you have trouble with the provided code.</p>

<h1>Contribute Code or Provide Feedback</h1>
<p>If you would like to become an active contributor to this project please follow the instructions provided in <a href="http://windowsazure.github.com/guidelines.html">Windows Azure Projects Contribution Guidelines</a>.</p>
<p>If you encounter any bugs with the library please file an issue in the <a href="https://github.com/WindowsAzure/azure-sdk-tools/issues">Issues</a> section of the project.</p>


<h1>Learn More</h1>
<ul>
    <li><a href="http://www.windowsazure.com/en-us/develop">Windows Azure
    Developer Center</a></li>
</ul>
