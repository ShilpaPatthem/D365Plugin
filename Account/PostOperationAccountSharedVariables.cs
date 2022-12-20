// <copyright file="PostOperationAccountSharedVariables.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.Account
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// PostOperationAccountSharedVariables.
    /// </summary>
    public class PostOperationAccountSharedVariables : IPlugin
    {
        /// <summary>
        /// Execution Method starts from here.
        /// </summary>
        /// <param name="serviceProvider">serviceProvider.</param>
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
                    if (entity.LogicalName == "account")
                    {
                        context.SharedVariables.Add("TaskDescription", "this is from shared variales");
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
