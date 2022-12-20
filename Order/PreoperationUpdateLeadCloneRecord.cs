// <copyright file="PreoperationUpdateLeadCloneRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.Order
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PreoperationUpdateLeadCloneRecord.
    /// </summary>
    public class PreoperationUpdateLeadCloneRecord : IPlugin
    {
        /// <inheritdoc/>
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                try
                {
                    Entity preImage = (Entity)context.PreEntityImages["PreImage"];
                    Entity oldOrder = new Entity("salesorder");
                    oldOrder = preImage;
                    oldOrder.Id = Guid.Empty;
                    oldOrder.Attributes.Remove("salesorderid"); // ordernumber
                    oldOrder.Attributes.Remove("ordernumber");
                    oldOrder.Attributes.Remove("createdon");
                    oldOrder.Attributes.Remove("modifiedon");
                    string originalState = oldOrder.Attributes.Contains("name") ? Convert.ToString(oldOrder.Attributes["name"]) : string.Empty;

                    Entity newOrder = oldOrder;
                    newOrder.Id = Guid.Empty;
                    newOrder.Attributes.Remove("salesorderid");
                    oldOrder.Attributes.Remove("ordernumber");
                    newOrder.Attributes.Remove("createdon");
                    newOrder.Attributes.Remove("modifiedon");
                    newOrder["name"] = "COPY" + originalState;
                    Guid newOrderId = service.Create(newOrder);

                    QueryByAttribute queryByAttribute = new QueryByAttribute("salesorderdetail");
                    queryByAttribute.ColumnSet = new ColumnSet(true);
                    queryByAttribute.Attributes.AddRange("salesorderid");
                    queryByAttribute.Values.AddRange(entity.Id);

                    EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
                    if (entityCollection != null && entityCollection.Entities.Count > 0)
                    {
                        foreach (var oldOrderProducts in entityCollection.Entities)
                        {
                            oldOrderProducts.Attributes.Remove("salesorderdetailid");
                            oldOrderProducts.Attributes.Remove("createdon");
                            oldOrderProducts.Attributes.Remove("modifiedon");

                            Entity newOrderProducts = oldOrderProducts;
                            newOrderProducts.Id = Guid.Empty;
                            newOrderProducts.Attributes.Remove("salesorderid");
                            newOrderProducts["salesorderid"] = new EntityReference("salesorder", newOrderId);
                            service.Create(newOrderProducts);
                        }
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
