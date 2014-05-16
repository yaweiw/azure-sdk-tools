The following changes were made to the 4/24/2011 version of the ServiceManagement library: 

http://code.msdn.microsoft.com/Windows-Azure-CSManage-e3f1882c (Last Updated 4/24/2011)

- The "Additions" folder was added to hold new wrappers for some Affinity Groups and Storage Accounts Windows Azure REST services
- The following constant was added in the Constants.cs file to use the new version of the Windows Azure REST services
        public const string VersionHeaderContent20110601 = "2011-06-01";
- ServiceManagementHelper.cs was modified to use the new constant VersionHeaderContent20110601
- New files in the Additions folder were added in the project file ServiceManagement.csproj 
- A reference to System.Core was added in the project file (ServiceManagement.csproj) to solve a problem that ocurred when building the project with msbuild

