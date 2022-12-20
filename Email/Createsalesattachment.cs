// <copyright file="Createsalesattachment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace CRM.AGS.Plugins.Email
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Createsalesattachment.
    /// </summary>
    public class Createsalesattachment : IPlugin
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
                    this.AddSalesAttchments(service, context, entity);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        private void AddSalesAttchments(IOrganizationService organizationService, IPluginExecutionContext context, Entity entity)
        {
            Entity email = new Entity("email");
            Entity fromActivityprty = new Entity("activityparty");
            Entity toActivityParty = new Entity("activityparty");
            fromActivityprty["partyid"] = new EntityReference("systemuser", context.UserId);
            toActivityParty["partyid"] = new EntityReference("account", entity.Id);
            email["from"] = new Entity[] { fromActivityprty };
            email["to"] = new Entity[] { toActivityParty };

            // email["regardingobjectid"] = new EntityReference(logicalname, regardingobjectid);
            email["subject"] = "Creating Email.";
            email["description"] = "Creating Email";
            email["directioncode"] = true;
            Guid emailId = organizationService.Create(email);

            QueryExpression queryExpression = new QueryExpression("cdi_salesattchment");
            queryExpression.ColumnSet = new ColumnSet(true);

            EntityCollection attachmentsCollection = organizationService.RetrieveMultiple(queryExpression);
            if (attachmentsCollection != null && attachmentsCollection.Entities.Count > 0)
            {
                foreach (var salesAttachements in attachmentsCollection.Entities)
                {
                    QueryExpression annotations = new QueryExpression("annotation");
                    annotations.ColumnSet = new ColumnSet(true);
                    annotations.Criteria.AddCondition("objectid", ConditionOperator.Equal, salesAttachements.Id);

                    EntityCollection annotationCollection = organizationService.RetrieveMultiple(annotations);
                    if (annotationCollection != null && annotationCollection.Entities.Count > 0)
                    {
                        foreach (var oldNotes in annotationCollection.Entities)
                        {
                            Entity attachment = new Entity("activitymimeattachment");
                            attachment["subject"] = oldNotes.Contains("subject") ? Convert.ToString(oldNotes["subject"]) : string.Empty;
                            if (oldNotes.Contains("isdocument") && (Convert.ToBoolean(oldNotes.Attributes["isdocument"]) == true))
                            {
                                attachment["body"] = oldNotes["documentbody"];
                                attachment["filename"] = oldNotes.Contains("filename") ? Convert.ToString(oldNotes["filename"]) : string.Empty;
                                attachment["mimetype"] = oldNotes["mimetype"];
                            }

                            attachment["objectid"] = new EntityReference("email", emailId);
                            attachment["attachmentnumber"] = 1;
                            attachment["objecttypecode"] = "email";
                            organizationService.Create(attachment);
                        }
                    }
                }
            }
        }
    }
}
