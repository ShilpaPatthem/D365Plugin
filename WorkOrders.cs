// <copyright file="WorkOrders.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins
{
    using System;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// WorkOrders.
    /// </summary>
    public class WorkOrders : IPlugin
    {
        /// <inheritdoc/>
        public void Execute(IServiceProvider serviceProvider)

        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                try
                {
                    Entity opportunity = null;
                    int workOrderValue = entity.Attributes.Contains("cdi_workordervalue") ? Convert.ToInt32(entity.Attributes["cdi_workordervalue"]) : 0;
                    Guid oppid = (entity.Attributes["cdi_opportunity"] as EntityReference).Id;
                    opportunity = service.Retrieve("opportunity", oppid, new ColumnSet("cdi_opportunityvalue", "cdi_totalworkorder"));
                    int oppValue = opportunity.Attributes.Contains("cdi_opportunityvalue") ? Convert.ToInt32(opportunity.Attributes["cdi_opportunityvalue"]) : 0;
                    int totalWorkorder = opportunity.Attributes.Contains("cdi_totalworkorder") ? Convert.ToInt32(opportunity.Attributes["cdi_totalworkorder"]) : 0;

                    if (oppValue < totalWorkorder || oppValue < workOrderValue)
                    {
                        throw new InvalidPluginExecutionException("You cannot create WorkOrder Items");
                    }
                    else
                    {
                        CalculateRollupFieldRequest crfr = new CalculateRollupFieldRequest // to calucate rollup fields. 
                        {
                            Target = new EntityReference(opportunity.LogicalName, oppid),
                            FieldName = "cdi_totalworkorder",
                        };
                        CalculateRollupFieldResponse responseRequested = (CalculateRollupFieldResponse)service.Execute(crfr);
                        opportunity = responseRequested.Entity;
                        totalWorkorder = opportunity.GetAttributeValue<int>("cdi_totalworkorder");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }
    }
}
