// <copyright file="PreOperationSharedVariablesAccount.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.Account
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// PreOperationSharedVariablesAccount.
    /// </summary>
    public class PreOperationSharedVariablesAccount : IPlugin
    {
        /// <summary>
        /// Main Execution Starts From Here.
        /// </summary>
        /// <param name="serviceProvider">serviceProvider.</param>
        /// <exception cref="NotImplementedException">Exception.</exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                try
                {
                    Entity task = new Entity("task");
                    task["subject"] = Convert.ToString(context.SharedVariables["Shared Variables"]);
                    task["description"] = Convert.ToString(context.SharedVariables["TaskDescription"]);
                    task["regardingobjectid"] = entity.ToEntityReference();
                    service.Create(task);
                    tracingService.Trace($" Post  Contact creation plug in execution end time{DateTime.Now} ");
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }
    }
}
