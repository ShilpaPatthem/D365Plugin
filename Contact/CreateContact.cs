using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace CRM.AGS.Plugins.Contact
{
    public class CreateContact : IPlugin      
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));           
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];

                    Contact contact =new Contact();
                    contact.LastName =entity.Attributes["name"].ToString();

                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Contact));
                    MemoryStream memoryStream = new MemoryStream();
                    serializer.WriteObject(memoryStream, contact);
                    var jsonObject = Encoding.Default.GetString(memoryStream.ToArray());

                    var webClient = new WebClient();
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    var serviceUrl = "https://shilpafunctionapp.azurewebsites.net/api/CreateContact?code=IWw6bA/DrKKe7P04dQW7MgrsYEwA28Q7zoVgTuC32TkDBUUaVTCBaQ==";

                    // upload the data using Post mehtod
                    string response = webClient.UploadString(serviceUrl, jsonObject);
                    Entity Con=new Entity("contact");
                    Con["jobtitle"] = response;
                    service.Create(Con);

                }
            } 
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        public class Contact
        {
            public string LastName { get; set; }
        }
    }
}
