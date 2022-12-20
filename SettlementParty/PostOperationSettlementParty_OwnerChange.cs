// <copyright file="PostOperationSettlementParty_OwnerChange.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.SettlementParty
{
    using System;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PostOperationSettlementParty_OwnerChange.
    /// </summary>
    public class PostOperationSettlementParty_OwnerChange : IPlugin
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
                   this.UpdateSettlementPrtyOwner(service, entity);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        private void UpdateSettlementPrtyOwner(IOrganizationService service, Entity entity)
        {
            int value = ((OptionSetValue)entity["cdi_roletype"]).Value;
            if (value == 765680000)
            {
                this.AssignUser(service, entity, "Arbitrator");
            }
            else if (value == 765680001)
            {
                this.AssignUser(service, entity, "Commissioner");
            }
            else if (value == 765680008)
            {
                this.AssignUser(service, entity, "Respondent");
            }
            else if (value == 765680007)
            {
                this.AssignUser(service, entity, "Petitioner");
            }
        }

        private void AssignUser(IOrganizationService service, Entity entity, string name)
        {
            QueryExpression queryExpression = new QueryExpression("ags_settings");
            queryExpression.ColumnSet.AddColumns(new string[] { "ags_value", "ags_settingsid" });
            queryExpression.Criteria.AddCondition("ags_name", ConditionOperator.Equal, name);
            EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);

            if (entityCollection != null && entityCollection.Entities != null && entityCollection.Entities.Count > 0)
            {
                string id = entityCollection.Entities[0].Attributes.Contains("ags_value") ? Convert.ToString(entityCollection.Entities[0].Attributes["ags_value"]) : string.Empty;

                AssignRequest assign = new AssignRequest
                {
                    Assignee = new EntityReference("systemuser", new Guid(id)),
                    Target = new EntityReference(entity.LogicalName, entity.Id),
                };
                service.Execute(assign);
            }
        }
    }
}
