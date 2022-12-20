// <copyright file="CreateContact_Emailattachments.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.Contact
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Permissions;
    using System.Text;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// CreateContact_Emailattachments.
    /// </summary>
    public class CreateContact_Emailattachments : IPlugin
    {
        /// <inheritdoc/>
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                try
                {
                    this.CreateEmailWithAttachment(service, entity, context, tracingService);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        private void CreateEmailWithAttachment(IOrganizationService service, Entity entity, IPluginExecutionContext context, ITracingService tracing)
        {
            // 1. Create the email record.

            Entity email = new Entity("email");
            Entity fromActivityprty = new Entity("activityparty");
            Entity toActivityParty = new Entity("activityparty");
            fromActivityprty["partyid"] = new EntityReference("systemuser", context.UserId);
            toActivityParty["partyid"] = new EntityReference(entity.LogicalName, entity.Id);
            email["from"] = new Entity[] { fromActivityprty };
            email["to"] = new Entity[] { toActivityParty };
            email["regardingobjectid"] = new EntityReference(entity.LogicalName, entity.Id);
            email["subject"] = "subject!";
            email["description"] = "Email is created from contact plugin for CRM .";
            email["directioncode"] = true;
            Guid targetEmailId = service.Create(email);
            QueryExpression _QueryNotes = new QueryExpression("annotation");
            _QueryNotes.ColumnSet = new ColumnSet(new string[] { "subject", "mimetype", "filename", "documentbody" });
            _QueryNotes.Criteria = new FilterExpression();
            _QueryNotes.Criteria.FilterOperator = LogicalOperator.And;
            _QueryNotes.Criteria.AddCondition(new ConditionExpression("objectid", ConditionOperator.Equal, new Guid("7da38ae0-4d0e-ea11-a813-000d3a1bbd52")));
            EntityCollection mimeCollection = service.RetrieveMultiple(_QueryNotes);
            if (mimeCollection.Entities.Count > 0)
            {
                Entity notesAttachment = mimeCollection.Entities.First();

                // Create email attachment
                Entity emailAttachment = new Entity("activitymimeattachment");

                if (notesAttachment.Contains("subject"))
                {
                    emailAttachment["subject"] = notesAttachment.GetAttributeValue<string>("subject");

                    emailAttachment["objectid"] = new EntityReference("email", targetEmailId);

                    emailAttachment["objecttypecode"] = "email";
                }

                if (notesAttachment.Contains("filename"))
                {
                    emailAttachment["filename"] = notesAttachment.GetAttributeValue<string>("filename");
                }

                if (notesAttachment.Contains("documentbody"))
                {
                    emailAttachment["body"] = notesAttachment.GetAttributeValue<string>("documentbody");
                }

                if (notesAttachment.Contains("mimetype"))
                {
                    emailAttachment["mimetype"] = notesAttachment.GetAttributeValue<string>("mimetype");
                }

                service.Create(emailAttachment);
            }
        }
    }
}
