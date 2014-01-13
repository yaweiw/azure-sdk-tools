using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests
{
    [TestClass]
    public class StaticCATests:ServiceManagementTest
    {
        
        //Give  affintiy group name 
        private string affinityGroup = "WestUsAffinityGroup";
        private static string vNetName = "NewVNet1";
        private static List<string> localNets = new List<string>();
        private static List<string> virtualNets = new List<string>();
        private static HashSet<string> affinityGroups = new HashSet<string>();
        private static Dictionary<string, string> dns = new Dictionary<string, string>();
        static List<DnsServer> dnsServers = new List<DnsServer>();
        private static List<LocalNetworkSite> localNetworkSites = new List<LocalNetworkSite>();
        static string serviceName;
        const string StaticCASubnet = "Subnet1";
        const string IPUnavaialbleExceptionMessage = "Networking.DeploymentVNetAddressAllocationFailure";

        [ClassInitialize]
        public static void Intialize(TestContext context)
        {
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
            ReadVnetConfig();
            SetVNetForStaticCAtest();
        }

        [TestInitialize]
        public void TestIntialize()
        {
            serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
        }

        #region Test cases
        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void DeployVMWithStaticCATest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string vmName = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {
                //Test a static CA
                //Test-AzureStaticVNetIP-VNetName $vnet -IPAddress “10.0.0.5”
                string ipaddress = "10.0.0.5";
                CheckAvailabilityofIpAddress(vnet1, ipaddress);
           
                //Create an IaaS VM with a static CA.
                var vm = CreatIaasVMObject(vmName, ipaddress);

                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, vnet1, new DnsServer[1] { dnsServers[0] },
                    serviceName, "service for DeployVMWithStaticCATest", string.Empty, string.Empty, null, affinityGroup, null);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);

                //Verfications
                VerifyVmWithStaticCAIsReserved(vmName, serviceName, ipaddress);
                pass = true;
            }
            catch (Exception)
            {
                pass = false;
                throw;
            }

        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void AddVMWithStaticCATest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);
            string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {
                string ipaddress = "10.0.0.6";
                CheckAvailabilityofIpAddress(vnet1, ipaddress);
                 
                //Create an IaaS VM
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName1, serviceName, imageName, InstanceSize.Small, username, password, vNetName, new string[1] { StaticCASubnet }, affinityGroup);

                //Add an IaaS VM with a static CA
                var vm = CreatIaasVMObject(vmName2, ipaddress);
                // New-AzureVM
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, null); 
                
                //Verify that the DIP of the VM2 is reserved.
                VerifyVmWithStaticCAIsReserved(vmName2, serviceName, ipaddress);
                
                //Verify that the DIP of the VM1 is NOT reserved.
                VerfiyVmWithoutStaticCAIsNotReserved(vmName1, serviceName);
                pass = true;
                

            }
            catch (Exception)
            {
                pass = false;
                throw;
            }
        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void UpdateVMWithNewStaticCATest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {
                string NonStaticIpAddress = string.Empty;
                string ipaddress = "10.0.0.7";
                //Test a static CA
                Console.WriteLine("Checking if ipaddress {0} is available", ipaddress);
                CheckAvailabilityofIpAddress(vnet1, ipaddress);
                Console.WriteLine("ipaddress {0} is available", ipaddress);

                //Create an IaaS VM
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName1, serviceName, imageName, InstanceSize.Small, username, password, vNetName, new string[1] { StaticCASubnet }, affinityGroup);

                //Update the IaaS VM with a static CA
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName1, serviceName);
                NonStaticIpAddress = vmRoleContext.IpAddress;
                Console.WriteLine("Non static IpAddress of the vm {0} is {1}", vmName1, NonStaticIpAddress);
                var vm = vmPowershellCmdlets.SetAzureStaticVNetIP(ipaddress, vmRoleContext.VM);
                vmPowershellCmdlets.UpdateAzureVM(vmName1, serviceName, vm);

                //Verify that the DIP of the VM is matched with an input.
                VerifyVmWithStaticCAIsReserved(vmName1, serviceName, ipaddress);

                //Verify that the first DIP is released. 
                Console.WriteLine("Checking for the availability of non static IpAdress after giving a static CA to the VM");
                var availabilityContext = vmPowershellCmdlets.TestAzureStaticVNetIP(vnet1, NonStaticIpAddress);
                Assert.IsTrue(availabilityContext.IsAvailable,"Non static IpAddress {0} is not realesed.",NonStaticIpAddress);
                Utilities.PrintContext<VirtualNetworkStaticIPAvailabilityContext>(availabilityContext);
                pass = true;


            }
            catch (Exception)
            {
                pass = false;
                throw;
            }
        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void UpdateToStaticCATest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {
                string NonStaticIpAddress = string.Empty;
                string ipaddress = string.Empty;

                //Create an IaaS VM
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName1, serviceName, imageName, InstanceSize.Small, username, password, vNetName, new string[1] { StaticCASubnet }, affinityGroup);

                //Update the IaaS VM with a static CA
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName1, serviceName);
                ipaddress = vmRoleContext.IpAddress;
                Console.WriteLine("Non static IpAddress of the vm {0} is {1}", vmName1, ipaddress);
                PersistentVM vm = vmPowershellCmdlets.SetAzureStaticVNetIP(ipaddress, vmRoleContext.VM);
                vmPowershellCmdlets.UpdateAzureVM(vmName1, serviceName, vm);

                //Verify that the DIP of the VM is matched with an input.
                VerifyVmWithStaticCAIsReserved(vmName1, serviceName, ipaddress);

                pass = true;


            }
            catch (Exception)
            {
                pass = false;
                throw;
            }

        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void UnreserveStaticCATest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string vmName = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {
                //Test a static CA
                //Test-AzureStaticVNetIP-VNetName $vnet -IPAddress “10.0.0.5”
                string ipaddress = "10.0.0.9";
                CheckAvailabilityofIpAddress(vnet1, ipaddress);

                //Create an IaaS VM with a static CA.
                var vm = CreatIaasVMObject(vmName, ipaddress);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, vnet1, new DnsServer[1] { dnsServers[0] },
                    serviceName, "service for DeployVMWithStaticCATest", string.Empty, string.Empty, null, affinityGroup, null);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);

                //Verfications
                VerifyVmWithStaticCAIsReserved(vmName, serviceName, ipaddress);

                //Remove-AzureStaticIP
                Console.WriteLine("Removing Static CA for the VM {0}", vmName);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName,serviceName);
                vm = vmPowershellCmdlets.RemoveAzureStaticVNetIP(vmRoleContext.VM);
                vmPowershellCmdlets.UpdateAzureVM(vmName, serviceName, vm);
                Console.WriteLine("Static CA for the VM {0} removed", vmName);

                //Verify that VM doesnt have a static VNet IP address anymore.
                Console.WriteLine("Verifying that the DIP of the VM is not Static CA");
                vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
                Assert.IsNull(vmPowershellCmdlets.GetAzureStaticVNetIP(vmRoleContext.VM), "VM has Static Vnet IP Address after executing Remove-AzureStaticVNetIP command also.");
                Console.WriteLine("No static IP is assigned to the VM.");
                pass = true;
            }
            catch (Exception)
            {
                pass = false;
                throw;
            }
        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void TryToReserveExistingCATest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string serviceName2  = Utilities.GetUniqueShortName(serviceNamePrefix);
            string serviceName3  = Utilities.GetUniqueShortName(serviceNamePrefix);
            string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);
            string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
            string vmName3 = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {
                string nonStaticIpAddress = string.Empty;

                //Create an IaaS VM
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName1, serviceName, imageName, InstanceSize.Small, username, password, vNetName, new string[1] { StaticCASubnet }, affinityGroup);
                //Get the DIP of the VM (Get-AzureVM)
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName1, serviceName);
                nonStaticIpAddress = vmRoleContext.IpAddress;

                //Assert that the DIP is not available (Test-AzureStaticVNetIP)
                CheckAvailabilityOfIpAddressAndAssertFalse(vnet1, nonStaticIpAddress);

                //Try to deploy an IaaS VM with the same static CA (CreateDeployment) and Verify that the deployment failed
                //Add an IaaS VM with a static CA
                Console.WriteLine("Deploying an IaaS VM with the same static CA {0} (CreateDeployment)", nonStaticIpAddress);
                var vm = CreatIaasVMObject(vmName2, nonStaticIpAddress);
                //Verify that the deployment failed.
                Utilities.VerifyFailure(
                    () => vmPowershellCmdlets.NewAzureVM(serviceName2, new[] { vm }, vnet1, new DnsServer[1] { dnsServers[0] },
                        serviceName, "service for AddVMWithStaticCATest", string.Empty, string.Empty, null, affinityGroup, null),
                        IPUnavaialbleExceptionMessage);
                Console.WriteLine("Deployment with Static CA {0} failed as expectd", nonStaticIpAddress);

                //Try to deploy an IaaS VM with the same static CA (AddRole) and verify that the deployment fails
                //Add an IaaS VM with a static CA
                Console.WriteLine("Deploying an IaaS VM with the same static CA {0} (AddRole)", nonStaticIpAddress);
                vm = CreatIaasVMObject(vmName3, nonStaticIpAddress);
                //Verify that the deployment failed.
                Utilities.VerifyFailure(() => vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, null),IPUnavaialbleExceptionMessage);
                Console.WriteLine("Deployment with Static CA {0} failed as expectd", nonStaticIpAddress);

                //Reserve the DIP of the VM1
                vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName1,serviceName);
                vm = vmPowershellCmdlets.SetAzureStaticVNetIP(nonStaticIpAddress, vmRoleContext.VM);
                vmPowershellCmdlets.UpdateAzureVM(vmName1, serviceName, vm);

                //Verify that the DIP is reserved
                VerifyVmWithStaticCAIsReserved(vmName1, serviceName, nonStaticIpAddress);

                //Try to deploy an IaaS VM with the same static CA (CreateDeployment)
                Console.WriteLine("Deploying an IaaS VM with the same static CA {0} (CreateDeployment)", nonStaticIpAddress);
                vm = CreatIaasVMObject(vmName2, nonStaticIpAddress);
                Utilities.VerifyFailure(() => vmPowershellCmdlets.NewAzureVM(serviceName3, new[] { vm }, vnet1, new DnsServer[1] { dnsServers[0] },
                        serviceName, "service for AddVMWithStaticCATest", string.Empty, string.Empty, null, affinityGroup, null), IPUnavaialbleExceptionMessage);
                Console.WriteLine("Deployment with Static CA {0} failed as expectd", nonStaticIpAddress);
                
                //Add an IaaS VM with a static CA
                Console.WriteLine("Deploying an IaaS VM with the same static CA {0} (AddRole)", nonStaticIpAddress);
                vm = CreatIaasVMObject(vmName3, nonStaticIpAddress);
                Utilities.VerifyFailure(() => vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, null),IPUnavaialbleExceptionMessage);
                Console.WriteLine("Deployment with Static CA {0} failed as expectd", nonStaticIpAddress);
                pass = true;

            }
            catch (Exception)
            {
                pass = false;
                throw;
            }
            finally
            {
                CleanupService(serviceName);
                CleanupService(serviceName2);
                CleanupService(serviceName3);
            }
            
            
        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void StopStayProvisionedVMWithStaticCATest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string vmName = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {
                //Test a static CA
                //Test-AzureStaticVNetIP-VNetName $vnet -IPAddress “10.0.0.5”
                string ipaddress = "10.0.0.10";
                CheckAvailabilityofIpAddress(vnet1, ipaddress);

                //Create an IaaS VM with a static CA.
                PersistentVM vm = CreatIaasVMObject(vmName, ipaddress);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, vnet1, new DnsServer[1] { dnsServers[0] },
                    serviceName, "service for DeployVMWithStaticCATest", string.Empty, string.Empty, null, affinityGroup, null);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);

                //Verfications
                VerifyVmWithStaticCAIsReserved(vmName, serviceName, ipaddress);

                //StopStayProvisioned the VM (Stop-AzureVM –StayProvisioned)
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
                vmPowershellCmdlets.StopAzureVM(vmRoleContext.VM, serviceName, true, false);

                CheckAvailabilityOfIpAddressAndAssertFalse(vnet1, ipaddress);
                pass = true;
            }
            catch (Exception)
            {
                pass = false;
                throw;
            }

        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void StopDeallocateVMWithStaticCATest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string vmName = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {
                //Test a static CA
                //Test-AzureStaticVNetIP-VNetName $vnet -IPAddress “10.0.0.5”
                string ipaddress = "10.0.0.5";
                CheckAvailabilityofIpAddress(vnet1, ipaddress);
                

                //Create an IaaS VM with a static CA.
                PersistentVM vm = CreatIaasVMObject(vmName, ipaddress);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, vnet1, new DnsServer[1] { dnsServers[0] },
                    serviceName, "service for DeployVMWithStaticCATest", string.Empty, string.Empty, null, affinityGroup, null);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);

                //Verfications
                VerifyVmWithStaticCAIsReserved(vmName, serviceName, ipaddress);

                //StopDeallocate the VM 
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
                vmPowershellCmdlets.StopAzureVM(vmRoleContext.VM, serviceName, false, true);

                CheckAvailabilityOfIpAddressAndAssertFalse(vnet1, ipaddress);
                pass = true;
            }
            catch (Exception)
            {
                pass = false;
                throw;
            }

        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void UpdateVMWithStaticCA()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string vmName = Utilities.GetUniqueShortName(vmNamePrefix);

            try
            {
                //Test a static CA
                //Test-AzureStaticVNetIP-VNetName $vnet -IPAddress “10.0.0.5”
                string ipaddress = "10.0.0.10";
                CheckAvailabilityofIpAddress(vnet1, ipaddress);

                //Create an IaaS VM with a static CA.
                var vm = CreatIaasVMObject(vmName, ipaddress);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, vnet1, new DnsServer[1] { dnsServers[0] },
                    serviceName, "service for DeployVMWithStaticCATest", string.Empty, string.Empty, null, affinityGroup, null);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);

                //Verfications
                VerifyVmWithStaticCAIsReserved(vmName, serviceName, ipaddress);

                //Update the instance size of the VM (Get-AzureVM | Set-AzureVMSize | Update-AzureVM
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
                SetAzureVMSizeConfig vmSizeConfig = new SetAzureVMSizeConfig(InstanceSize.Medium);
                vmSizeConfig.Vm = vmRoleContext.VM;
                vmPowershellCmdlets.UpdateAzureVM(vmName, serviceName, vmRoleContext.VM);

                //Verify that the DIP of the VM is still reserved.
                VerifyVmWithStaticCAIsReserved(vmName, serviceName, ipaddress);
                pass = true;
            }
            catch (Exception)
            {
                pass = false;
                throw;
            }
        }

        [TestMethod(),Ignore(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove,Test)-AzureStaticVnetIP)")]
        public void StaticCAExhautionTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string vnet1 = virtualNets[0];
            string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);
            try
            {
                //Test a static CA
                //Test-AzureStaticVNetIP-VNetName $vnet -IPAddress “10.0.0.5”
                string ipaddress = "10.0.0.5";
                var availibiltyContext = vmPowershellCmdlets.TestAzureStaticVNetIP(vnet1, ipaddress);
                //Assert that it is available.
                Assert.IsTrue(availibiltyContext.IsAvailable);

                var vm = CreatIaasVMObject(vmName1, ipaddress);

                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, vnet1, new DnsServer[1] { dnsServers[0] },
                    serviceName, "service for DeployVMWithStaticCATest", string.Empty, string.Empty, null, affinityGroup, null);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);


                availibiltyContext = vmPowershellCmdlets.TestAzureStaticVNetIP(vnet1, ipaddress);
                int availableVIPsCount = availibiltyContext.AvailableAddresses.Count();
                Console.WriteLine(string.Format("AvailableAddresses:{0}{1}", Environment.NewLine, availibiltyContext.AvailableAddresses.Aggregate((current, next) => current + Environment.NewLine + next)));
                Console.WriteLine("VIPs avilable now:{0}", availableVIPsCount);
                int i = 0;
                foreach (string ip in availibiltyContext.AvailableAddresses)
                {
                    Console.WriteLine("Creating VM-{0} with IP: {1}", ++i,ip);
                    vm = CreatIaasVMObject(Utilities.GetUniqueShortName(vmNamePrefix), ip);
                    vmPowershellCmdlets.NewAzureVM(serviceName,new[] {vm},null);
                    Console.WriteLine("Created VM-{0} with IP: {1}", i,ip);
                }

                Console.WriteLine("Creating VM-{0}", ++i);
                vm = vmPowershellCmdlets.NewAzureVMConfig(new ConfigDataInfo.AzureVMConfigInfo(Utilities.GetUniqueShortName(vmNamePrefix), ConfigDataInfo.InstanceSize.Small, imageName));
                var azureProvisioningConfig1 = new AzureProvisioningConfigInfo(OS.Windows, username, password);
                azureProvisioningConfig1.Vm = vm;
                vm = vmPowershellCmdlets.AddAzureProvisioningConfig(azureProvisioningConfig1);
                vm = vmPowershellCmdlets.SetAzureSubnet(vm, new string[1] { StaticCASubnet });
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, null);
                var roleContext = vmPowershellCmdlets.GetAzureVM(vm.RoleName,serviceName);
                Console.WriteLine("Created VM-{0} with IP: {1}", i,roleContext.IpAddress);

                //try to create an vm and verify that it fails
                Console.WriteLine("Creating VM-{0}", ++i);
                vm = vmPowershellCmdlets.NewAzureVMConfig(new ConfigDataInfo.AzureVMConfigInfo(Utilities.GetUniqueShortName(vmNamePrefix), ConfigDataInfo.InstanceSize.Small, imageName));
                var azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password);
                azureProvisioningConfig.Vm = vm;
                vm = vmPowershellCmdlets.AddAzureProvisioningConfig(azureProvisioningConfig);
                vm = vmPowershellCmdlets.SetAzureSubnet(vm, new string[1] { StaticCASubnet });
                Utilities.VerifyFailure(() => vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, null),IPUnavaialbleExceptionMessage);
                Console.WriteLine("Creating VM-{0} failed as expected."); 
                pass = true;
            }
            catch (Exception)
            {
                pass = false;
                throw;
            }

        }
        #endregion Test cases

        #region Verifications Methods
        private void VerifyVmWithStaticCAIsReserved(string vmName,string serviceName,string inputDIP)
        {
            string confirguredVIP = string.Empty;

            
            //Get the DIP of the VM (Get-AzureVM) VirtualNetworkStaticIPContext ipContext
            PersistentVMRoleContext vm = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
            confirguredVIP = vm.IpAddress;
            
            if (!string.IsNullOrEmpty(confirguredVIP))
            {
                //Verify that the DIP of the VM is matched with an input DIP.
                Console.WriteLine("Verifying that the DIP of the VM {0} is matched with input DIP {1}.", inputDIP, confirguredVIP);
                Assert.AreEqual(inputDIP,confirguredVIP, string.Format("Static CA IpAddress {0} is not the same as the Input DIP {1}", confirguredVIP, inputDIP));
                Console.WriteLine("Verifyied that the DIP of the VM {0} is matched with input DIP {1}.", inputDIP, confirguredVIP);
                
                //Verify that the DIP is actually reserved.
                Console.WriteLine("Verifying that the DIP of the VM is actually reserved");
                var ipContext = vmPowershellCmdlets.GetAzureStaticVNetIP(vm.VM);
                Utilities.PrintContext<VirtualNetworkStaticIPContext>(ipContext);
                Assert.AreEqual(inputDIP,ipContext.IPAddress, string.Format("Reserved IPAddress {0}  is not equal to the input DIP {1}", ipContext.IPAddress, inputDIP));
                Console.WriteLine("Verifyied that the DIP of the VM is actually reserved");

                //Verify that the IP is not available (Test-AzureStaticVNetIP –VnetName $vnet –IPAddress “10.0.0.5”)
                Console.WriteLine("Verifing that the IP {0} is not available", inputDIP);
                VirtualNetworkStaticIPAvailabilityContext availibiltyContext = vmPowershellCmdlets.TestAzureStaticVNetIP(vNetName, inputDIP);
                Console.WriteLine(string.Format("IsAvailable:{0}", availibiltyContext.IsAvailable));
                Console.WriteLine(string.Format("AvailableAddresses:{0}{1}", Environment.NewLine, availibiltyContext.AvailableAddresses.Aggregate((current, next) => current + Environment.NewLine + next)));
                Assert.IsFalse(availibiltyContext.IsAvailable, string.Format("Test-AzureStaticVNetIP should return true as {0} is reserved", inputDIP,vm.Name));
                Assert.IsFalse(availibiltyContext.AvailableAddresses.Contains(inputDIP),string.Format("{0} is reserved for vm {1} and should not be in available addresses.",inputDIP,vmName));
                Console.WriteLine("Verified that the IP {0} is not available", inputDIP);
            }
            else
            {
                throw new Exception("Configured IPAddres value is null or empty");
            }
            
        }

        private void VerfiyVmWithoutStaticCAIsNotReserved(string vmName, string serviceName)
        {

            //Get the DIP of the VM (Get-AzureVM)
            Console.WriteLine("Getting the DIP of the VM");
            var vm = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
            Console.WriteLine(string.Format("IpAddress of the VM : {0}", vm.IpAddress));


            //Verify that the DIP is NOT reserved
            Console.WriteLine("Verifying that the DIP is NOT reserved");
            Assert.IsNull(vmPowershellCmdlets.GetAzureStaticVNetIP(vm.VM),"Reserved IPAddress should be null or empty for a VM without static CA");
            Console.WriteLine("Verified that the DIP {0} is NOT reserved", vm.IpAddress);

            //Verify that the IP is not available (Test-AzureStaticVNetIP –VnetName $vnet –IPAddress “10.0.0.6”)
            Console.WriteLine("Verifying that the IP is not available");
            var availibiltyContext = vmPowershellCmdlets.TestAzureStaticVNetIP(vNetName, vm.IpAddress);
            Console.WriteLine(string.Format("IsAvailable:{0}", availibiltyContext.IsAvailable));
            Console.WriteLine(string.Format("AvailableAddresses:{0}{1}", Environment.NewLine, availibiltyContext.AvailableAddresses.Aggregate((current, next) => current + Environment.NewLine + next)));
            Assert.IsFalse(availibiltyContext.IsAvailable, string.Format("Test-AzureStaticVNetIP should return true as {0} is reserved", vm.IpAddress, vm.Name));
            Assert.IsFalse(availibiltyContext.AvailableAddresses.Contains(vm.IpAddress), string.Format("{0} is reserved for vm {1} and should not be in available addresses.", vm.IpAddress, vmName));
            Console.WriteLine("Verified that IP {0} is not available", vm.IpAddress);
        }

        #endregion Verifications Methods

        [TestCleanup]
        public void TestCleanUp()
        {
            CleanupService(serviceName);
        }


        [ClassCleanup]
        public static void ClassCleanUp()
        {
            CleanUpVnetConfigForStaticCA();
        }

        private static void ReadVnetConfig()
        {
            // Read the vnetconfig file and get the names of local networks, virtual networks and affinity groups.
            XDocument vnetconfigxml = XDocument.Load(vnetConfigFilePath);
            
            AddressPrefixList prefixlist = null;

            foreach (XElement el in vnetconfigxml.Descendants())
            {
                switch (el.Name.LocalName)
                {
                    case "LocalNetworkSite":
                        {
                            localNets.Add(el.FirstAttribute.Value);
                            List<XElement> elements = el.Elements().ToList<XElement>();
                            prefixlist = new AddressPrefixList();
                            prefixlist.Add(elements[0].Elements().First().Value);
                            localNetworkSites.Add(new LocalNetworkSite()
                            {
                                Name = el.FirstAttribute.Value,
                                VpnGatewayAddress = elements[1].Value,
                                AddressSpace = new AddressSpace() { AddressPrefixes = prefixlist }
                            }
                            );
                        }
                        break;
                    case "VirtualNetworkSite":
                        virtualNets.Add(el.Attribute("name").Value);
                        affinityGroups.Add(el.Attribute("AffinityGroup").Value);
                        break;
                    case "DnsServer":
                        {
                            dnsServers.Add(new DnsServer() { Name = el.Attribute("name").Value, Address = el.Attribute("IPAddress").Value });
                            break;
                        }
                    default:
                        break;
                }
            }

            foreach (string aff in affinityGroups)
            {
                if (Utilities.CheckRemove(vmPowershellCmdlets.GetAzureAffinityGroup, aff))
                {
                    vmPowershellCmdlets.NewAzureAffinityGroup(aff, locationName, null, null);
                }
            }
        }

        private static void SetVNetForStaticCAtest()
        {
            vmPowershellCmdlets.SetAzureVNetConfig(Directory.GetCurrentDirectory() + "\\StaticCAvnetconfig.netcfg");
        }

        private static void CleanUpVnetConfigForStaticCA()
        {
            Utilities.RetryActionUntilSuccess(() => vmPowershellCmdlets.RemoveAzureVNetConfig(), "in use", 10, 30);
        }


        private void CheckAvailabilityofIpAddress(string vnetName, string ipaddress)
        {
            Console.WriteLine("Checking if VIP {0} is available and unreserved", ipaddress);
            VirtualNetworkStaticIPAvailabilityContext availibiltyContext = vmPowershellCmdlets.TestAzureStaticVNetIP(vnetName, ipaddress);
            Utilities.PrintContext<VirtualNetworkStaticIPAvailabilityContext>(availibiltyContext);
            //Assert that it is available.
            Assert.IsTrue(availibiltyContext.IsAvailable,"ipaddress {0} is expected to be available",ipaddress);
            Console.WriteLine("Ip address {0} is available", ipaddress);
        }

        private void CheckAvailabilityOfIpAddressAndAssertFalse(string vnetName, string ipaddress)
        {
            var availibiltyContext = vmPowershellCmdlets.TestAzureStaticVNetIP(vnetName, ipaddress);
            Utilities.PrintContext<VirtualNetworkStaticIPAvailabilityContext>(availibiltyContext);
            Console.WriteLine(string.Format("AvailableAddresses:{0}{1}", Environment.NewLine, availibiltyContext.AvailableAddresses.Aggregate((current, next) => current + Environment.NewLine + next)));
            Assert.IsFalse(availibiltyContext.IsAvailable, "Ipaddress {0} is avialable.", ipaddress);
        }
        private PersistentVM CreatIaasVMObject(string vmName, string ipaddress)
        {
            //Create an IaaS VM with a static CA.
            PersistentVM vm = vmPowershellCmdlets.NewAzureVMConfig(new ConfigDataInfo.AzureVMConfigInfo(vmName, ConfigDataInfo.InstanceSize.Small, imageName));
            //Add-AzureProvisioningConfig
            AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password);
            azureProvisioningConfig.Vm = vm;
            vm = vmPowershellCmdlets.AddAzureProvisioningConfig(azureProvisioningConfig);
            //Set-AzureSubnet
            vm = vmPowershellCmdlets.SetAzureSubnet(vm, new string[1] { StaticCASubnet });
            //Set-AzureStaticVNetIP
            vm = vmPowershellCmdlets.SetAzureStaticVNetIP(ipaddress, vm);
            return vm;
        }

    }
}
