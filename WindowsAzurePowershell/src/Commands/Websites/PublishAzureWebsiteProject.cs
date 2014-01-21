using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Web.Deployment;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Websites;
using Microsoft.WindowsAzure.Commands.Utilities.Websites.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Commands.Websites
{
    [Cmdlet(VerbsData.Publish, "AzureWebsiteProject")]
    public class PublishAzureWebsiteProject : WebsiteContextBaseCmdlet, IDynamicParameters
    {
        [Parameter(ParameterSetName = "ProjectFile", Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Visual Studio web application project to be published.")]
        [ValidateNotNullOrEmpty]
        public string ProjectFile { get; set; }

        [Parameter(ParameterSetName = "ProjectFile", Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The configuration used to build the Visual Studio web application project.")]
        [ValidateNotNullOrEmpty]
        public string Configuration { get; set; }

        [Parameter(ParameterSetName = "ProjectFile", Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The configuration used to build the Visual Studio web application project.")]
        [ValidateNotNullOrEmpty]
        public Hashtable BuildProperty { get; set; }

        [Parameter(ParameterSetName = "Package", Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The WebDeploy package folder for zip file of the Visual Studio web application project to be published.")]
        [ValidateNotNullOrEmpty]
        public string Package { get; set; }

        private string fullProjectFile;
        private string fullWebConfigFileWithConfiguration;
        private string fullWebConfigFile;
        private string fullPackage;
        private string configuration;

        private RuntimeDefinedParameterDictionary dynamicParameters;

        public override void ExecuteCmdlet()
        {
            DeploymentChangeSummary result = null;

            // If a project file is specified, use MSBuild to build the package zip file.
            if (String.Equals(ParameterSetName, "ProjectFile", StringComparison.OrdinalIgnoreCase))
            {
                fullPackage = this.BuildWebProject();
            }

            WriteObject(string.Format("[Start]    Publishing package {0}", fullPackage));

            // Resolve the full path of the package file or folder when the "Package" parameter set is used.
            fullPackage = string.IsNullOrEmpty(fullPackage) ? this.TryResolvePath(Package) : fullPackage;

            // Publish the package zip file or folder.
            result = File.GetAttributes(fullPackage).HasFlag(FileAttributes.Directory) ? this.PublishWebProjectFromPackagePath() : this.PublishWebProjectFromPackageFile();

            WriteObject(string.Format("{0} Publishing package {1}", (result.Errors == 0 ? "[Complete]" : "[Fail]    "), fullPackage));
        }

        /// <summary>
        /// Build a Visual Studio Web Project using MS build.
        /// </summary>
        /// <returns>The location of the WebDeploy package file.</returns>
        private string BuildWebProject()
        {
            if (File.Exists(fullWebConfigFile)) // Make sure the Web.config for the configuration exists.
            {
                ProjectCollection pc = new ProjectCollection();
                Project project = pc.LoadProject(fullProjectFile);

                // Use a file logger to store detailed build info.
                FileLogger fileLogger = new FileLogger();
                var logFile = Path.Combine(CurrentPath(), "build.log");
                fileLogger.Parameters = string.Format("logfile={0}", logFile);
                fileLogger.Verbosity = LoggerVerbosity.Diagnostic;

                // Set the configuration used by MSBuild.
                project.SetProperty("Configuration", configuration);

                // Set other MSBuild properties. Ignore "Configuration" since it should be specified by the -Configuration parameter directly.
                if (BuildProperty != null)
                {
                    foreach (DictionaryEntry property in BuildProperty)
                    {
                        if (!string.Equals("Configuration", property.Key.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            project.SetProperty(property.Key.ToString(), property.Value.ToString());
                        }

                    }
                }

                WriteObject(string.Format("[Start]    Building project {0}", fullProjectFile));
                // Build the project.
                var buildSucceed = project.Build("Package", new ILogger[] { fileLogger });

                if (buildSucceed)
                {
                    // If build succeeds, delete the build.log file since there is no use of it.
                    WriteObject(string.Format("[Complete] Building project {0}", fullProjectFile));
                    File.Delete(logFile);
                    return Path.Combine(Path.GetDirectoryName(fullProjectFile), "obj", configuration, "Package", Path.GetFileNameWithoutExtension(fullProjectFile) + ".zip");
                }
                else
                {
                    // If build fails, tell the user to look at the build.log file.
                    WriteObject(string.Format("[Fail]     Building project {0}", fullProjectFile));
                    throw new Exception(string.Format("Cannot build the project successfully. Please see logs in {0}.", logFile));
                }
            }
            else
            {
                throw new FileNotFoundException(string.Format("Cannot find file {0} for configuration {1}", fullProjectFile, configuration));
            }
        }

        /// <summary>
        /// Publish a Visual Studio Web Project to a website using the package folder.
        /// </summary>
        /// <returns>The change summary of the deployment.</returns>
        private DeploymentChangeSummary PublishWebProjectFromPackagePath()
        {
            DeploymentBaseOptions remoteBaseOptions = CreateRemoteDeploymentBaseOptions();
            DeploymentBaseOptions localBaseOptions = new DeploymentBaseOptions();

            using (var deploypment = DeploymentManager.CreateObject(DeploymentWellKnownProvider.ContentPath, fullPackage, localBaseOptions))
            {
                DeploymentSyncOptions syncOptions = new DeploymentSyncOptions();
                return deploypment.SyncTo(DeploymentWellKnownProvider.ContentPath, Name, remoteBaseOptions, syncOptions);
            }
        }

        /// <summary>
        /// Publish a Visual Studio Web Project to a website using the package zip file.
        /// </summary>
        /// <returns>The change summary of the deployment.</returns>
        private DeploymentChangeSummary PublishWebProjectFromPackageFile()
        {
            DeploymentBaseOptions remoteBaseOptions = CreateRemoteDeploymentBaseOptions();
            DeploymentBaseOptions localBaseOptions = new DeploymentBaseOptions();

            DeploymentProviderOptions remoteProviderOptions = new DeploymentProviderOptions(DeploymentWellKnownProvider.Auto);

            using (var deployment = DeploymentManager.CreateObject(DeploymentWellKnownProvider.Package, fullPackage, localBaseOptions))
            {
                DeploymentSyncParameter providerPathParameter = new DeploymentSyncParameter(
                    "Provider Path Parameter",
                    "Provider Path Parameter",
                    Name,
                    null);
                DeploymentSyncParameterEntry iisAppEntry = new DeploymentSyncParameterEntry(
                    DeploymentSyncParameterEntryKind.ProviderPath,
                    DeploymentWellKnownProvider.IisApp.ToString(),
                    ".*",
                    null);
                DeploymentSyncParameterEntry setAclEntry = new DeploymentSyncParameterEntry(
                    DeploymentSyncParameterEntryKind.ProviderPath,
                    DeploymentWellKnownProvider.SetAcl.ToString(),
                    ".*",
                    null);
                providerPathParameter.Add(iisAppEntry);
                providerPathParameter.Add(setAclEntry);
                deployment.SyncParameters.Add(providerPathParameter);

                // Replace the connection strings in Web.config with the ones user specifies from the cmdlet.
                foreach (var dp in dynamicParameters)
                {
                    if (MyInvocation.BoundParameters.ContainsKey(dp.Value.Name))
                    {
                        var deploymentSyncParameterName = string.Format("Connection String {0} Parameter", dp.Value.Name);
                        DeploymentSyncParameter connectionStringParameter = new DeploymentSyncParameter(
                            deploymentSyncParameterName,
                            deploymentSyncParameterName,
                            dp.Value.Value.ToString(),
                            null);
                        DeploymentSyncParameterEntry connectionStringEntry = new DeploymentSyncParameterEntry(
                            DeploymentSyncParameterEntryKind.XmlFile,
                            @"\\web.config$",
                            string.Format(@"//connectionStrings/add[@name='{0}']/@connectionString", dp.Value.Name),
                            null);
                        connectionStringParameter.Add(connectionStringEntry);
                        deployment.SyncParameters.Add(connectionStringParameter);
                    }
                }

                DeploymentSyncOptions syncOptions = new DeploymentSyncOptions();
                return deployment.SyncTo(remoteProviderOptions, remoteBaseOptions, syncOptions);
            }
        }

        /// <summary>
        /// Parse the Web.config files to get the connection string names.
        /// </summary>
        /// <returns>An array of connection string names</returns>
        private string[] ParseConnectionStringNamesFromWebConfig()
        {
            var names = new List<string>();
            var webConfigFiles = new string[] { fullWebConfigFile, fullWebConfigFileWithConfiguration };

            foreach (var file in webConfigFiles)
            {
                XDocument xdoc = XDocument.Load(file);
                names.AddRange(xdoc.Descendants("connectionStrings").SelectMany(css => css.Descendants("add")).Select(add => add.Attribute("name").Value));
            }

            return names.Distinct().ToArray<string>();
        }

        /// <summary>
        /// Generate dynamic parameters based on the connection strings in the Web.config.
        /// It will look at 2 Web.config files:
        /// 1. Web.config
        /// 2. Web.<configuration>.config (like Web.Release.config)
        /// </summary>
        /// <returns>The dynamic parameters.</returns>
        public override object GetDynamicParameters()
        {
            // Ge the 2 Web.config files.
            fullProjectFile = this.TryResolvePath(ProjectFile).Trim(new char[] { '"' });
            fullWebConfigFile = Path.Combine(Path.GetDirectoryName(fullProjectFile), "Web.config");
            configuration = string.IsNullOrEmpty(Configuration) ? "Release" : Configuration;
            fullWebConfigFileWithConfiguration = Path.Combine(Path.GetDirectoryName(fullProjectFile), string.Format("Web.{0}.config", configuration));

            dynamicParameters = new RuntimeDefinedParameterDictionary();
            if (string.Compare("ProjectFile", ParameterSetName) == 0)
            {
                // Parse the connection strings from the Web.config files.
                var names = ParseConnectionStringNamesFromWebConfig();

                // Create a dynmaic parameter for each connection string using the same name.
                foreach (var name in names)
                {
                    var parameter = new RuntimeDefinedParameter();
                    parameter.Name = name;
                    parameter.ParameterType = typeof(string);
                    parameter.Attributes.Add(new ParameterAttribute()
                        {
                            ParameterSetName = "ProjectFile",
                            Mandatory = false,
                            ValueFromPipelineByPropertyName = true,
                            HelpMessage = "Connection string from Web.config."
                        }
                    );
                    dynamicParameters.Add(name, parameter);
                }
            }
            return dynamicParameters;
        }

        /// <summary>
        /// Create remote deployment base options using the web site publish profile.
        /// </summary>
        /// <returns>The remote deployment base options.</returns>
        private DeploymentBaseOptions CreateRemoteDeploymentBaseOptions()
        {
            // Get the web site publish profile.
            var publishProfile = WebsitesClient.GetWebDeployPublishProfile(Name);

            DeploymentBaseOptions remoteBaseOptions = new DeploymentBaseOptions()
            {
                UserName = publishProfile.UserName,
                Password = publishProfile.UserPassword,
                ComputerName = string.Format("https://{0}/msdeploy.axd?site={1}", publishProfile.PublishUrl, Name),
                AuthenticationType = "Basic",
                TempAgent = false
            };

            return remoteBaseOptions;
        }
    }
}
