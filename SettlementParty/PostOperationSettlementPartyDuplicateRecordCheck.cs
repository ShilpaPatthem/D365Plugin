// <copyright file="PostOperationSettlementPartyDuplicateRecordCheck.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// <author></author>
// <date>20/10/2022 1:09:36 PM</date>
// <summary>Implements the Post Operation SettlementParty Create DuplicateReocrd Check Plugin.</summary>
namespace CRM.AGS.Plugins.SettlementParty
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Post Operation SettlementParty Create DuplicateReocrd Check.
    /// </summary>
    public class PostOperationSettlementPartyDuplicateRecordCheck : IPlugin
    {
        /// <inheritdoc/>
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    Entity postImage = (Entity)context.PostEntityImages["PostImage"];
                    if (postImage.Contains("cdi_settlement"))
                    {
                        Guid id = ((EntityReference)postImage.Attributes["cdi_settlement"]).Id;
                        this.DuplicateSettlementPartyRecordCheck(service, entity, id);
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in Post Operation SettlementParty Create Duplicate Reocrd.", ex);
                }
            }
        }

        private void DuplicateSettlementPartyRecordCheck(IOrganizationService service, Entity entity, Guid id)
        {
            int roleType = ((OptionSetValue)entity.Attributes["cdi_roletype"]).Value;
            QueryExpression queryExpression = new QueryExpression(entity.LogicalName);
            queryExpression.ColumnSet.AddColumns(new string[] { "cdi_name", "cdi_roletype", "cdi_settlement" });
            queryExpression.Criteria.AddCondition("cdi_roletype", ConditionOperator.Equal, roleType);
            queryExpression.Criteria.AddCondition("cdi_settlement", ConditionOperator.Equal, id);

            EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
            if (entityCollection.Entities.Count > 1)
            {
                throw new InvalidPluginExecutionException("Duplicate Parties are not allowed.");
            }
            else
            {
                Entity settlement = new Entity("cdi_settlement");
                settlement.Id = id;
                settlement["cdi_allsettlementpartiessigned"] = false;
                settlement["statuscode"] = new OptionSetValue(1);
                service.Update(settlement);
            }
        }
    }
}
