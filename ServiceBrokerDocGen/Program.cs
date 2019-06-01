using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using System.Text;

namespace ServiceBrokerDocGen
{
    class Program
    {
        static void Main(string[] args)
        {
            LoadJsonTemplate();
        }
        static string path = "";
        static void LoadJsonTemplate()
        {
            Console.WriteLine(" ");
            Console.WriteLine("Welcome.");
            Console.WriteLine("This program automatically creates the Service Broker doc Readme.md from the Cloudformation Template provided.");            
            Console.WriteLine("==========================================================================================================");
            Console.WriteLine("IMPORTANT: Ensure the Template contains the 'AWS::ServiceBroker::Specification' section before proceeding");
            Console.WriteLine(" ");
            Console.WriteLine("Enter in path to '.json' cloudformation template:");
            string input = Console.ReadLine();
            path = input;

            try
            {
                string json = "";
                using (StreamReader r = new StreamReader(input))
                {
                    json = r.ReadToEnd();
                }
                
                TraverseJson(json);     
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Console.WriteLine(" ");
                Console.WriteLine("Press Key to retry");
                Console.ReadKey();
                Console.Clear();
                LoadJsonTemplate();
            }
        }

        static void TraverseJson(string json)
        {
            JObject rss = JObject.Parse(json);
            AWSServiceBrokerSpecification AWSSvBSpec = new AWSServiceBrokerSpecification();
            try
            {
                AWSSvBSpec.Name = (string)rss["Metadata"]["AWS::ServiceBroker::Specification"]["Name"] ?? "Short Product Name Goes Here";
                AWSSvBSpec.DisplayName = (string)rss["Metadata"]["AWS::ServiceBroker::Specification"]["DisplayName"] ?? "Display Product Name Goes Here";
                AWSSvBSpec.LongDescription = (string)rss["Metadata"]["AWS::ServiceBroker::Specification"]["LongDescription"] ?? "Long Description Goes Here";
                AWSSvBSpec.ImageUrl = (string)rss["Metadata"]["AWS::ServiceBroker::Specification"]["ImageUrl"] ?? "Image Url Goes Here";
                AWSSvBSpec.DocumentationUrl = (string)rss["Metadata"]["AWS::ServiceBroker::Specification"]["DocumentationUrl"] ?? "Documentation Url Goes Here";
                AWSSvBSpec.Cost = (string)rss["Metadata"]["AWS::ServiceBroker::Specification"]["ServicePlans"]["production"]["Cost"] ?? "Cost Url Goes Here";
            }
            catch (Exception ex)
            {
                Console.Write("Error: Ensure the file contains the AWS::ServiceBroker::Specification section before attempting to create the Readme.md");
                throw ex;
            }

            var Params = new List<Parameter>();
            foreach (JProperty item in rss["Parameters"])
            {
                Parameter param = new Parameter();
                param.Name = item.Name;
                param.Default = (string)item.Value["Default"];
                param.Description = (string)item.Value["Description"];
                param.Type = (string)item.Value["Type"];
                param.AllowedValues = (JArray)item.Value["AllowedValues"];
                Params.Add(param);
            }
            CreateMarkDown(AWSSvBSpec, Params);
        }

        static void CreateMarkDown(AWSServiceBrokerSpecification AWSSvBSpec, List<Parameter> Params)
        {
            //Symbols
            var CarriageRtn = "\n\r";
            var NewLine = "\r";

            //Skeleton
            var Content = "";
            Content += "# AWS Service Broker - " + AWSSvBSpec.DisplayName + CarriageRtn;

            Content += @"<img align=""left"" src=""https://s3.amazonaws.com/awsservicebroker/icons/aws-service-broker.png"" width=""120""><img align=""right"" src=" + AWSSvBSpec.ImageUrl + @" width=""108"">";
            Content += @"<p align=""center"">" + AWSSvBSpec.LongDescription + "</p>" + "&nbsp;" + CarriageRtn;

            Content += "Table of contents" + NewLine;
            Content += "=================" + CarriageRtn;

            Content += "* [Parameters](#parameters)" + NewLine;
            Content += "  * [production](#param-production)" + NewLine;
            Content += "  * [dev](#param-dev)" + NewLine;
            Content += "  * [custom](#param-custom)" + NewLine;
            Content += "* [Bind Credentials](#bind-credentials)" + NewLine;
            Content += "* [Examples](#kubernetes-openshift-examples)" + NewLine;
            Content += "  * [production](#example-production)" + NewLine;
            Content += "  * [dev](#example-dev)" + NewLine;
            Content += "  * [custom](#example-custom)" + CarriageRtn;

            Content += @"<a id=""parameters""></a>" + CarriageRtn;

            Content += "# Parameters" + CarriageRtn;

            Content += @"<a id = ""param-production""></a>" + CarriageRtn;

            Content += "Creates an " + AWSSvBSpec.DisplayName + " optimised for production use.  " + NewLine;
            Content += "Pricing: " + AWSSvBSpec.Cost + CarriageRtn;

            Content += @"### Required" + CarriageRtn;
            Content += "At a minimum these parameters must be declared when provisioning an instance of this service" + CarriageRtn;

            var TableA = @"Name           | Description     | Accepted Values" + NewLine;
            TableA += "-------------- | --------------- | ---------------" + NewLine;
            foreach (var item in Params)
            {
                TableA += item.Name + "|" + item.Description + "|" + item.Default + NewLine;
            }

            Content += TableA + CarriageRtn;

            Content += "### Optional" + CarriageRtn;

            Content += "These parameters can optionally be declared when provisioning" + CarriageRtn;

            var TableMain = @"Name            | Description     | Default         | Accepted Values" + NewLine;
            TableMain += "--------------- | --------------- | --------------- | ---------------" + NewLine;
            foreach (var item in Params)
            {
                if (item.AllowedValues != null)
                {
                    string allowedValues = String.Join(", ", item.AllowedValues);
                    TableMain += item.Name + "|" + item.Description + "|" + item.Default + "|" + allowedValues + NewLine;
                }
            }

            Content += TableMain + CarriageRtn;

            Content += @"### Generic" + CarriageRtn;

            Content += "These parameters are required, but generic or require privileged access to the underlying AWS account, we recommend they are configured with a broker secret, see [broker documentation](/docs/) for details." + CarriageRtn;

            Content += TableMain + CarriageRtn;

            Content += @"### Prescribed" + CarriageRtn;

            Content += "These are parameters that are prescribed by the plan and are not configurable, should adjusting any of these be required please choose a plan that makes them available." + CarriageRtn;

            Content += TableA + CarriageRtn;

            Content += @"<a id=""bind - credentials""></a>" + CarriageRtn;

            Content += "# Bind Credentials" + CarriageRtn;
            Content += "These are the environment variables that are available to an application on bind." + CarriageRtn;

            var TableB = @"Name           | Description" + NewLine;
            TableB += "-------------- | ---------------" + NewLine;

            Content += TableB + CarriageRtn;
            //==================================================
            //End of Skeleton

            //Start of K8 section Skeleton
            //==================================================

            //Get Parameters to add into K8 setion
            string K8Params = "";

            foreach (var item in Params)
            {
                string ParamValue = item.Default ?? "[VALUE]";               
                K8Params += "".PadRight(4) + item.Name + ": " + ParamValue + NewLine;               
            }
            
            //==================================================

            Content += @"# Kubernetes/Openshift Examples" + CarriageRtn;

            Content += @"***Note:*** Examples do not include generic parameters, if you have not setup defaults for these you will need to add them as additional parameters" + CarriageRtn;

            //Production
            Content += @"<a id =""example-production""></a>" + CarriageRtn;

            Content += "## production" + CarriageRtn;

            Content += "### Minimal" + NewLine;
            Content += "```yaml" + NewLine;
            Content += "apiVersion: servicecatalog.k8s.io / v1beta1" + NewLine;
            Content += "kind: ServiceInstance" + NewLine;
            Content += "metadata: " + NewLine;
            Content += "  name: " + AWSSvBSpec.Name+"-production-complete-example" + NewLine;
            Content += "spec: " + NewLine;
            Content += "  clusterServiceClassExternalName: " + AWSSvBSpec.Name + NewLine;
            Content += "  clusterServicePlanExternalName: production" + NewLine;
            Content += "  parameters: " + NewLine;
            Content += K8Params;            
            Content += "```" + CarriageRtn + NewLine;

            Content += "### Complete" + NewLine;
            Content += "```yaml" + NewLine;
            Content += "apiVersion: servicecatalog.k8s.io / v1beta1" + NewLine;
            Content += "kind: ServiceInstance" + NewLine;
            Content += "metadata: " + NewLine;
            Content += "  name: " + AWSSvBSpec.Name + "-production-complete-example" + NewLine;
            Content += "spec: " + NewLine;
            Content += "  clusterServiceClassExternalName: " + AWSSvBSpec.Name + NewLine;
            Content += "  clusterServicePlanExternalName: production" + NewLine;
            Content += "  parameters: " + NewLine;
            Content += K8Params;
            Content += "```" + CarriageRtn + NewLine;

            //Development
            Content += @"<a id=""example-dev""></a>" + CarriageRtn;

            Content += @"## dev" + CarriageRtn;

            Content += "### Minimal" + NewLine;
            Content += "```yaml" + NewLine;
            Content += "apiVersion: servicecatalog.k8s.io / v1beta1" + NewLine;
            Content += "kind: ServiceInstance" + NewLine;
            Content += "metadata: " + NewLine;
            Content += "  name: " + AWSSvBSpec.Name + "-dev-complete-example" + NewLine;
            Content += "spec: " + NewLine;
            Content += "  clusterServiceClassExternalName: " + AWSSvBSpec.Name + NewLine;
            Content += "  clusterServicePlanExternalName: dev" + NewLine;
            Content += "  parameters: " + NewLine;
            Content += K8Params;
            Content += "```" + CarriageRtn + NewLine;

            Content += "### Complete" + NewLine;
            Content += "```yaml" + NewLine;
            Content += "apiVersion: servicecatalog.k8s.io / v1beta1" + NewLine;
            Content += "kind: ServiceInstance" + NewLine;
            Content += "metadata: " + NewLine;
            Content += "  name: " + AWSSvBSpec.Name + "-dev-complete-example" + NewLine;
            Content += "spec: " + NewLine;
            Content += "  clusterServiceClassExternalName: " + AWSSvBSpec.Name + NewLine;
            Content += "  clusterServicePlanExternalName: production" + NewLine;
            Content += "  parameters: " + NewLine;
            Content += K8Params;
            Content += "```" + CarriageRtn + NewLine;

            //Custom
            Content += @"<a id = ""example-custom""></a>" + CarriageRtn;

            Content += @"## custom" + CarriageRtn;

            Content += "### Minimal" + NewLine;
            Content += "```yaml" + NewLine;
            Content += "apiVersion: servicecatalog.k8s.io / v1beta1" + NewLine;
            Content += "kind: ServiceInstance" + NewLine;
            Content += "metadata: " + NewLine;
            Content += "  name: " + AWSSvBSpec.Name + "-custom-complete-example" + NewLine;
            Content += "spec: " + NewLine;
            Content += "  clusterServiceClassExternalName: " + AWSSvBSpec.Name + NewLine;
            Content += "  clusterServicePlanExternalName: custom" + NewLine;
            Content += "  parameters: " + NewLine;
            Content += K8Params;
            Content += "```" + CarriageRtn + NewLine;

            Content += "### Complete" + NewLine;
            Content += "```yaml" + NewLine;
            Content += "apiVersion: servicecatalog.k8s.io / v1beta1" + NewLine;
            Content += "kind: ServiceInstance" + NewLine;
            Content += "metadata: " + NewLine;
            Content += "  name: " + AWSSvBSpec.Name + "-custom-complete-example" + NewLine;
            Content += "spec: " + NewLine;
            Content += "  clusterServiceClassExternalName: " + AWSSvBSpec.Name + NewLine;
            Content += "  clusterServicePlanExternalName: custom" + NewLine;
            Content += "  parameters: " + NewLine;
            Content += K8Params;
            Content += "```" + CarriageRtn + NewLine;

            WriteMarkDownToFile(Content);
        }

        static void WriteMarkDownToFile(string output)
        {
            path = path.Substring(0, path.LastIndexOf("\\"));            
            StreamWriter sw = new StreamWriter(path+@"\readme.md");
            sw.Write(output);
            sw.Close();
            Console.WriteLine(" ");
            Console.WriteLine("============================================================================================");
            Console.WriteLine("Generation of Readme.md completed, and located in the same directory as the template path.");
            Console.WriteLine("Press a Key to Exit");
            Console.ReadKey();
        }

        class AWSServiceBrokerSpecification
        {
            public string Name = "";
            public string DisplayName = "";
            public string LongDescription = "";
            public string ImageUrl = "";
            public string DocumentationUrl = "";
            public string ProviderDisplayName = "Amazon Web Services";
            public string Cost = "";
        }

        class Parameter
        {
            public string Name = "";
            public string Description = "";
            public string Type = "";
            public string Default = "";
            public JArray AllowedValues;
        }
    }
}
