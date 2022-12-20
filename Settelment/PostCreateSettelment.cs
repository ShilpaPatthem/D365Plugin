// <copyright file="PostCreateSettelment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CDI.AGS.Plugins.Settelment
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// public class PostCreateSettelment.
    /// </summary>
    public class PostCreateSettelment : IPlugin
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
                   this.CreateSettlementparty(service, entity);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        private void CreateSettlementparty(IOrganizationService organizationService, Entity entity)
        {
            string name = entity.Attributes.Contains("cdi_name") ? Convert.ToString(entity["cdi_name"]) : string.Empty;
            var roleType = (OptionSetValue)entity.Attributes["cdi_role"]; // 765680000

            // var name = entity.FormattedValues["cdi_role"].ToString();
            if (roleType != null)
            {
                Entity settlementparty = new Entity("cdi_settlementpartyentity");
                settlementparty["cdi_name"] = name;
                settlementparty["cdi_roletype"] = roleType;
                settlementparty["cdi_settlementid"] = new EntityReference(entity.LogicalName, entity.Id);
                organizationService.Create(settlementparty); // cdi_settlement
            }
        }
    }
}
