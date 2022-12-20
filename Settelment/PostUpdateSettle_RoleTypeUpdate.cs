// <copyright file="PostUpdateSettle_RoleTypeUpdate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace CDI.AGS.Plugins.Settelment
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PostUpdateSettle_RoleTypeUpdate.
    /// </summary>
    public class PostUpdateSettle_RoleTypeUpdate : IPlugin
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
                    Entity postImage = (Entity)context.PostEntityImages["PostImage"];
                    if (postImage.Contains("cdi_name"))
                    {
                        string name = postImage.Contains("cdi_name") ? Convert.ToString(postImage["cdi_name"]) : string.Empty;
                        bool retrieve = this.RetriveSettleparty(service, entity, name);
                        if (retrieve == false)
                        {
                            this.CreatePartyRecord(service, entity, name);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("Duplicate Parties are not allowed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        private bool RetriveSettleparty(IOrganizationService organizationService, Entity entity, string name)
        {
            bool records = false;
            int roleType = ((OptionSetValue)entity["cdi_role"]).Value;

            // string name = entity.Attributes.Contains("cdi_name") ? Convert.ToString(entity.Attributes["cdi_name"]) : string.Empty;
            QueryExpression queryExpression = new QueryExpression("cdi_settlementpartyentity");
            queryExpression.ColumnSet.AddColumns(new string[] { "cdi_roletype", "cdi_settlementid" });
            queryExpression.Criteria.AddCondition("cdi_roletype", ConditionOperator.Equal, roleType);
            queryExpression.Criteria.AddCondition("cdi_settlementid", ConditionOperator.Equal, entity.Id);
            EntityCollection entityCollection = organizationService.RetrieveMultiple(queryExpression);
            if (entityCollection != null && entityCollection.Entities != null && entityCollection.Entities.Count > 0)
            {
                records = true;
            }

            return records;
        }

        private void CreatePartyRecord(IOrganizationService organizationService, Entity entity, string name)
        {
            // Entity SettlepartyName = organizationService.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("cdi_name"));
            // var name = SettlepartyName.Attributes.Contains("cdi_name") ? Convert.ToString(SettlepartyName["cdi_name"]) : string.Empty;
            OptionSetValue roleType = (OptionSetValue)entity.Attributes["cdi_role"];

            // var name = entity.FormattedValues["cdi_role"].ToString();
            if (roleType != null)
            {
                Entity settleparty = new Entity("cdi_settlementpartyentity");
                settleparty["cdi_name"] = name;
                settleparty["cdi_roletype"] = roleType;
                settleparty["cdi_settlementid"] = new EntityReference(entity.LogicalName, entity.Id);
                organizationService.Create(settleparty);
            }
        }
    }
}
