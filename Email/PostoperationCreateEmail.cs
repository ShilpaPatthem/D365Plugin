// <copyright file="PostoperationCreateEmail.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.Email
{
    using System;
    using System.Text;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PostoperationCreateEmail.
    /// </summary>
    public class PostoperationCreateEmail : IPlugin
    {
        /// <inheritdoc/>
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];

                try
                {
                    tracingService.Trace($"post Operation email creation execution started :{DateTime.Now}");
                    if (entity.Contains("regardingobjectid") && (entity.Attributes["regardingobjectid"] as EntityReference).LogicalName == "contact")
                    {
                        Entity contact = service.Retrieve("contact", (entity.Attributes["regardingobjectid"] as EntityReference).Id, new ColumnSet("description"));
                        if (contact != null && contact.Contains("description"))
                        {
                            string description = contact.Attributes.Contains("description") ? Convert.ToString(contact.Attributes["description"]) : string.Empty;
                            Entity attachment = new Entity("activitymimeattachment");
                            attachment["subject"] = "Attachment";
                            string filename = "attachmentfile.txt";
                            attachment["filename"] = filename;
                            byte[] fileStream = Encoding.ASCII.GetBytes(description);

                            attachment["subject"] = Convert.ToBase64String(fileStream);
                            attachment["mimetype"] = "text/plain";
                            attachment["attachmentnumber"] = 1;
                            attachment["objectid"] = new EntityReference("email", entity.Id);
                            attachment["objecttypecode"] = "email";
                            service.Create(attachment);
                        }
                    }

                    tracingService.Trace($"post Operation email creation execution ended :{DateTime.Now}");
                }
                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
