// <copyright file="PreoperationUpdateQuoteCloneRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.Quote
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PreoperationUpdateQuoteCloneRecord.
    /// </summary>
    public class PreoperationUpdateQuoteCloneRecord : IPlugin
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
                    if (context.Depth > 0)
                    {
                        return;
                    }

                    Entity preImage = (Entity)context.PreEntityImages["PreImage"];
                    Entity oldOrder = new Entity("quote");
                    oldOrder = preImage;
                    oldOrder.Id = Guid.Empty;
                    oldOrder.Attributes.Remove("quoteid"); // ordernumber
                    oldOrder.Attributes.Remove("quotenumber");
                    oldOrder.Attributes.Remove("createdon");
                    oldOrder.Attributes.Remove("modifiedon");
                    string originalState = oldOrder.Attributes.Contains("name") ? Convert.ToString(oldOrder.Attributes["name"]) : string.Empty;

                    Entity newOrder = oldOrder;
                    newOrder.Id = Guid.Empty;
                    newOrder.Attributes.Remove("quoteid");
                    oldOrder.Attributes.Remove("quotenumber");
                    newOrder.Attributes.Remove("createdon");
                    newOrder.Attributes.Remove("modifiedon");
                    newOrder["name"] = "COPY" + originalState;
                    Guid newOrderId = service.Create(newOrder);

                    QueryByAttribute queryByAttribute = new QueryByAttribute("quotedetail");
                    queryByAttribute.ColumnSet = new ColumnSet(true);
                    queryByAttribute.Attributes.AddRange("quoteid");
                    queryByAttribute.Values.AddRange(entity.Id);

                    EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
                    if (entityCollection != null && entityCollection.Entities.Count > 0)
                    {
                        foreach (var oldOrderProducts in entityCollection.Entities)
                        {
                            oldOrderProducts.Attributes.Remove("quotedetailid");
                            oldOrderProducts.Attributes.Remove("createdon");
                            oldOrderProducts.Attributes.Remove("modifiedon");

                            Entity newquoteProducts = oldOrderProducts;
                            newquoteProducts.Id = Guid.Empty;
                            newquoteProducts.Attributes.Remove("quoteid");
                            newquoteProducts["quoteid"] = new EntityReference("quote", newOrderId);
                            service.Create(newquoteProducts);
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
