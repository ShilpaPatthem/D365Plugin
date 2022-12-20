// <copyright file="PostoperationCreateLead_EmailWithAttachment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.Lead
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// PostoperationCreateLead_EmailWithAttachment.
    /// </summary>
    public class PostoperationCreateLead_EmailWithAttachment : IPlugin
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
                    this.CreateEmail(service, entity, context.UserId);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        private void CreateEmail(IOrganizationService organizationService, Entity entity, Guid userId)
        {
            Entity email = new Entity("email");
            Entity fromActivityprty = new Entity("activityparty");
            Entity toActivityParty = new Entity("activityparty");
            fromActivityprty["partyid"] = new EntityReference("systemuser", userId);
            toActivityParty["partyid"] = new EntityReference("lead", entity.Id);
            email["from"] = new Entity[] { fromActivityprty };
            email["to"] = new Entity[] { toActivityParty };
            email["regardingobjectid"] = new EntityReference("lead", entity.Id);
            email["subject"] = "Creating Email.";
            email["description"] = "Creating Email";
            email["directioncode"] = true;
            Guid emailId = organizationService.Create(email);

            // 2. Create Attachment for email
            Entity linkedAttachment = new Entity("activitymimeattachment");
            linkedAttachment.Attributes["objectid"] = new EntityReference("email", emailId);
            linkedAttachment.Attributes["objecttypecode"] = "email";
            linkedAttachment.Attributes["filename"] = "DemoAttachment.pdf";
            linkedAttachment.Attributes["mimetype"] = "application/pdf";
            linkedAttachment.Attributes["body"] = "your attachment file stream in BASE64 format string";
            organizationService.Create(linkedAttachment);
        }
    }
}
