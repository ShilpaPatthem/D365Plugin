// <copyright file="PostOperationSettlementPartyOnChangeOfDateSign.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.SettlementParty
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PostOperationSettlementPartyOnChangeOfDateSign.
    /// </summary>
    public class PostOperationSettlementPartyOnChangeOfDateSign : IPlugin
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
                    Entity postImage = (Entity)context.PostEntityImages["PostUpdateImage"];
                    if (postImage.Contains("cdi_settlement"))
                    {
                        Guid settlementId = ((EntityReference)postImage.Attributes["cdi_settlement"]).Id;
                        this.UpdateSettlementParties(service, settlementId, entity);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        private void UpdateSettlementParties(IOrganizationService organizationService, Guid settlementId, Entity entity)
        {
            QueryExpression query = new QueryExpression(entity.LogicalName);
            query.ColumnSet.AddColumns(new string[] { "cdi_settlementid", "cdi_datesigned", "cdi_emailaddressfields" });
            query.Criteria.AddCondition("cdi_settlementid", ConditionOperator.Equal, settlementId);
            EntityCollection entityCollection = organizationService.RetrieveMultiple(query);
            bool isDataExists = false;
            if (entityCollection != null && entityCollection.Entities != null && entityCollection.Entities.Count > 0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    if (!item.Contains("cdi_emailaddressfields") || !item.Contains("cdi_datesigned"))
                    {
                        isDataExists = true;
                    }
                }
            }

            if (isDataExists == true)
            {
                Entity settlement = new Entity("cdi_settlement");
                settlement.Id = settlementId;
                settlement["cdi_allsettlementpartiessigned"] = true;
                settlement["statuscode"] = new OptionSetValue(765680001);
                organizationService.Update(settlement);
            }
            else
            {
                throw new InvalidPluginExecutionException("Email & Date field should not be null.");
            }
        }
    }
}
