// <copyright file="PostOperationContacUpdateEstimatedRevenue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace CRM.AGS.Plugins.Contact
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PostOperationContacUpdateEstimatedRevenue.
    /// </summary>
    public class PostOperationContacUpdateEstimatedRevenue : IPlugin
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
                    if (context.Depth > 1)
                    {
                        return;
                    }

                    this.UpdateEstimatedRevenue(service, entity);
                    this.UpdatePriorityEstimatedRevenue(service, entity);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        private void UpdateEstimatedRevenue(IOrganizationService organizationService, Entity entity)
        {
            Entity entityObj = null;

            decimal revenue = entity.Attributes.Contains("tex_revenue") ? (entity.Attributes["tex_revenue"] as Money).Value : 0;
            Guid parentcustomerId = (entity.Attributes["parentcustomerid"] as EntityReference).Id; // {87f665cb-6c53-ed11-bba3-000d3a339364}
            if (parentcustomerId != null)
            {
                entityObj = organizationService.Retrieve("account", parentcustomerId, new ColumnSet("tex_revenuepercentage"));
            }

            int revenuePercentage = Convert.ToInt32(entityObj.Attributes["tex_revenuepercentage"]);
            entity["tex_estimatedrevenue"] = revenue * Convert.ToDecimal(Convert.ToDecimal(revenuePercentage) / 100);
            organizationService.Update(entity);
        }

        private void UpdatePriorityEstimatedRevenue(IOrganizationService organizationService, Entity entity)
        {
            Entity entityObj = null;
            decimal priorityrevenue = entity.Attributes.Contains("tex_priorityrevenue") ? (entity.Attributes["tex_priorityrevenue"] as Money).Value : 0;
            int prioroty = entity.Attributes.Contains("tex_priority") ? ((OptionSetValue)entity.Attributes["tex_priority"]).Value : 0;
            Guid parentcustomerId = (entity.Attributes["parentcustomerid"] as EntityReference).Id; // {87f665cb-6c53-ed11-bba3-000d3a339364}
            if (parentcustomerId != null)
            {
                entityObj = organizationService.Retrieve("account", parentcustomerId, new ColumnSet("tex_majorpercentage", "tex_mediumpercentage", "tex_minorpercentage"));

                int majorrevenuePercentage = Convert.ToInt32(entityObj.Attributes["tex_majorpercentage"]);
                int mediumrevenuePercentage = Convert.ToInt32(entityObj.Attributes["tex_mediumpercentage"]);
                int minorrevenuePercentage = Convert.ToInt32(entityObj.Attributes["tex_minorpercentage"]);

                this.PriorotySwitchCase(organizationService, entity, prioroty, priorityrevenue, majorrevenuePercentage, mediumrevenuePercentage, minorrevenuePercentage);
            }
        }

        private void PriorotySwitchCase(IOrganizationService organizationService, Entity entity, int prioroty, decimal priorityrevenue, int majorrevenuePercentage, int mediumrevenuePercentage, int minorrevenuePercentage)
        {
            switch (prioroty)
            {
                case 282650000:
                    entity["tex_priorityestimatedrevenue"] = priorityrevenue * Convert.ToDecimal(Convert.ToDecimal(majorrevenuePercentage) / 100);
                    organizationService.Update(entity);
                    break;
                case 282650001:
                    entity["tex_priorityestimatedrevenue"] = priorityrevenue * Convert.ToDecimal(Convert.ToDecimal(mediumrevenuePercentage) / 100);
                    organizationService.Update(entity);
                    break;
                case 282650002:
                    entity["tex_priorityestimatedrevenue"] = priorityrevenue * Convert.ToDecimal(Convert.ToDecimal(minorrevenuePercentage) / 100);
                    organizationService.Update(entity);
                    break;
            }
        }
    }
}
