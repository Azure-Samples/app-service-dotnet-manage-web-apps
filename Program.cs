// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ManageWebAppBasic
{
    public class Program
    {
        /**
         * Azure App Service basic sample for managing web apps.
         *  - Create 3 web apps under the same new app service plan:
         *    - 1, 2 are in the same resource group, 3 in a different one
         *    - Stop and start 1, restart 2
         *    - Add Java support to app 3
         *  - List web apps
         *  - Delete a web app
         */

        public static async Task RunSample(ArmClient client)
        {
            AzureLocation region = AzureLocation.EastUS;
            string app1Name = Utilities.CreateRandomName("webapp1-");
            string app2Name = Utilities.CreateRandomName("webapp2-");
            string app3Name = Utilities.CreateRandomName("webapp3-");
            string rg1Name = Utilities.CreateRandomName("rg1NEMV_");
            string rg2Name = Utilities.CreateRandomName("rg2NEMV_");
            var lro1 = await client.GetDefaultSubscription().GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, rg1Name, new ResourceGroupData(AzureLocation.EastUS));
            var resourceGroup1 = lro1.Value;
            var lro2 = await client.GetDefaultSubscription().GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, rg2Name, new ResourceGroupData(AzureLocation.EastUS));
            var resourceGroup2 = lro2.Value;

            try
            {
                //============================================================
                // Create a web app with a new app service plan

                Utilities.Log("Creating web app " + app1Name + " in resource group " + rg1Name + "...");

                var webSite1Collection = resourceGroup1.GetWebSites();
                var webSite1Data = new WebSiteData(region)
                {
                    SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                    {
                        WindowsFxVersion = "PricingTier.StandardS1",
                        NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                        PhpVersion = "PhpVersion.V5_6",
                    },

                };
                var webSite1_lro = await webSite1Collection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, app1Name, webSite1Data);
                var webSite1 = webSite1_lro.Value;

                Utilities.Log("Created web app " + webSite1.Data.Name);
                Utilities.Print(webSite1);

                //============================================================
                // Create a second web app with the same app service plan

                Utilities.Log("Creating another web app " + app2Name + " in resource group " + rg1Name + "...");
                var planId = webSite1.Data.AppServicePlanId;
                var webSite2Collection = resourceGroup2.GetWebSites();
                var webSite2Data = new WebSiteData(region)
                {
                    SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                    {
                        WindowsFxVersion = "PricingTier.StandardS1",
                        NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                    },
                    AppServicePlanId = planId,
                };
                var webSite2_lro = await webSite2Collection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, app2Name, webSite2Data);
                var webSite2 = webSite2_lro.Value;

                Utilities.Log("Created web app " + webSite2.Data.Name);
                Utilities.Print(webSite2);

                //============================================================
                // Create a third web app with the same app service plan, but
                // in a different resource group

                Utilities.Log("Creating another web app " + app3Name + " in resource group " + rg2Name + "...");
                var webSite3Collection = resourceGroup2.GetWebSites();
                var webSite3Data = new WebSiteData(region)
                {
                    SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                    {
                        WindowsFxVersion = "PricingTier.StandardS1",
                        NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                    },
                    AppServicePlanId = planId,
                };
                var webSite3_lro = await webSite3Collection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, app3Name, webSite3Data);
                var webSite3 = webSite3_lro.Value;

                Utilities.Log("Created web app " + webSite3.Data.Name);
                Utilities.Print(webSite3);

                //============================================================
                // stop and start app1, restart app 2
                Utilities.Log("Stopping web app " + webSite1.Data.Name);
                await webSite1.StopAsync();
                Utilities.Log("Stopped web app " + webSite1.Data.Name);
                Utilities.Print(webSite1);
                Utilities.Log("Starting web app " + webSite1.Data.Name);
                await webSite1.StartAsync();
                Utilities.Log("Started web app " + webSite1.Data.Name);
                Utilities.Print(webSite1);
                Utilities.Log("Restarting web app " + webSite2.Data.Name);
                await webSite2.RestartAsync();
                Utilities.Log("Restarted web app " + webSite2.Data.Name);
                Utilities.Print(webSite2);

                //============================================================
                // Configure app 3 to have Java 8 enabled
                Utilities.Log("Adding Java support to web app " + app3Name + "...");
                await webSite3.UpdateAsync(new SitePatchInfo()
                {
                    SiteConfig = new SiteConfigProperties()
                    {
                        JavaVersion = "V8Newest",
                        JavaContainer = "Tomcat8_0Newest"
                    }
                });
                Utilities.Log("Java supported on web app " + app3Name + "...");

                //=============================================================
                // List web apps

                Utilities.Log("Printing list of web apps in resource group " + rg1Name + "...");

                await foreach (var webApp in webSite1Collection.GetAllAsync())
                {
                    Utilities.Print(webApp);
                }

                Utilities.Log("Printing list of web apps in resource group " + rg2Name + "...");

                await foreach (var webApp in webSite2Collection.GetAllAsync())
                {
                    Utilities.Print(webApp);
                }

                //=============================================================
                // Delete a web app

                Utilities.Log("Deleting web app " + app1Name + "...");
                await webSite1.DeleteAsync(WaitUntil.Completed);
                Utilities.Log("Deleted web app " + app1Name + "...");

                Utilities.Log("Printing list of web apps in resource group " + rg1Name + " again...");
                await foreach (var webApp in webSite1Collection.GetAllAsync())
                {
                    Utilities.Print(webApp);
                }
            }
            finally
            {
                try
                {
                    Utilities.Log("Deleting Resource Group: " + rg2Name);
                    await resourceGroup2.DeleteAsync(WaitUntil.Completed);
                    Utilities.Log("Deleted Resource Group: " + rg2Name);
                    Utilities.Log("Deleting Resource Group: " + rg1Name);
                    await resourceGroup1.DeleteAsync(WaitUntil.Completed);
                    Utilities.Log("Deleted Resource Group: " + rg1Name);
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                // Print selected subscription
                Utilities.Log("Selected subscription: " + client.GetSubscriptions().Id);

                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}