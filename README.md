# Windows Azure PowerShell

This repo contains a set of PowerShell cmddlets for developers and administrators to deploy and manage 
Windows Azure applications. It includes the following:

* Cmdlets for developers to deploy both node.js and PHP applications.
* Cmdlets for IT Administrators to manage their Windows Azure environments.

For documentation on how to build and deploy applications to Windows Azure please see the [Windows Azure Developer Center](http://www.windowsazure.com/en-us/develop). For comprehensive documentation on the developer cmdlets see [How to use Windows Azure PowerShell](http://www.windowsazure.com/en-us/develop/nodejs/how-to-guides/powershell-cmdlets/). For comprehensive documentation on the full set of Windows Azure cmdlets see [Windows Azure Management Center](http://go.microsoft.com/fwlink/?linkID=254459&clcid=0x409).

# Cmdlets Features

*. Type ```help azure``` to get the full list of all the cmdlets.
*. Type ```help node-dev``` to get the list of all Node.js development related cmdlets.
*. Type ```help php-dev``` to get the list of all PHP development related cmdlets.

# Getting Started

## Install from Microsoft Web Platform Installer

1. Install [Microsoft Web Platform Installer](http://www.microsoft.com/web/downloads/platform.aspx).
2. Open Microsoft Web Platform Installer and search for __Windows Azure PowerShell__.
3. Install.

## Download Source Code

To get the source code of the SDK via git just type:
```git clone https://github.com/WindowsAzure/azure-sdk-tools.git<br/>cd ./azure-sdk-tools```

## Install Prerequisites

* [Windows Azure SDK](http://www.microsoft.com/windowsazure/sdk/)
* [Windows PowerShell 2.0](http://technet.microsoft.com/en-us/scriptcenter/dd742419)
* [WiX](http://wix.sourceforge.net/) (Only needed if you want to build the setup project)

### Node.js Prerequisites (developer only)

* [Node.js](http://nodejs.org/)
* [IISNode](https://github.com/tjanczuk/iisnode)

### PHP Prerequisites (developer only)

* [PHP](http://php.iis.net/)

## Configure PowerShell to automatically load commandlets

1. Create a folder inside your user's Documents folder and name it __WindowsPowerShell__

2. Inside that folder create a file called __Microsoft.PowerShell_profile.ps1__

3. Edit the file in a text editor and add the following contents

   ```Import-Module PATH_TO_AZURE-SDK-TOOLS_CLONE\Package\Release\Azure.psd1```

4. After you build the commandlets project, you can then open a PowerShell window and you should be able to use the commandlets. Please note that if you want to rebuild the project, you have close the PowerShell window, and then reopen it.

# Quick start

1. Create an Azure hosted service called HelloWorld by typing

   ```New-AzureServiceProject HelloWorld```

2. Inside the HelloWorld folder, add a new Web Role by typing

   ```Add-AzureNodeWebRole``` or ```Add-AzurePHPWebRole```

3. Test out the application in the local emulator by typing

   ```Start-AzureEmulator -Launch```

4. You are now ready to publish to the cloud service. Go ahead and register for a Windows Azure account and make sure you have your credentials handy.

5. Get your account's publish settings and save them to a file by typing

   ```Get-AzurePublishSettingsFile```

6. Now import the settings

   ```Import-AzurePublishSettingsFile PATH_TO_PUBLISH_SETTINGS_FILE```

7. You are now ready to publish to the cloud. Make sure you specify a unique name for your application to ensure there aren't any conflicts during the publish process. Then type

   ```Publish-AzureServiceProject -ServiceName UNIQUE_NAME -Launch```

# Need Help?

Be sure to check out the [Windows Azure Developer Forums on Stack Overflow](http://go.microsoft.com/fwlink/?LinkId=234489) if you have trouble with the provided code.

# Contribute Code or Provide Feedback

If you would like to become an active contributor to this project please follow the instructions provided in [Windows Azure Projects Contribution Guidelines](http://windowsazure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/WindowsAzure/azure-sdk-tools/issues) section of the project.

# Learn More

* [Windows Azure Developer Center](http://www.windowsazure.com/en-us/develop)