namespace CRM.AGS.Plugins.Account
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// EnvUpdateAccount.
    /// </summary>
    public class EnvUpdateAccount : IPlugin
    {
        static IReadOnlyDictionary<string, string> EnvVariables;

        public void Execute(IServiceProvider serviceProvider)

        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    Entity account = (Entity)context.InputParameters["Target"];
                    if (EnvVariables == null)
                    {
                        var envVariables = new Dictionary<string, string>();
                        var query = new QueryExpression("environmentvariabledefinition")
                        {
                            ColumnSet = new ColumnSet("statecode", "defaultvalue", "valueschema",
                              "schemaname", "environmentvariabledefinitionid", "type"),
                            LinkEntities =
                        {
                            new LinkEntity
                            {
                                JoinOperator = JoinOperator.LeftOuter,
                                LinkFromEntityName = "environmentvariabledefinition",
                                LinkFromAttributeName = "environmentvariabledefinitionid",
                                LinkToEntityName = "environmentvariablevalue",
                                LinkToAttributeName = "environmentvariabledefinitionid",
                                Columns = new ColumnSet("statecode", "value", "environmentvariablevalueid"),
                                EntityAlias = "v",
                            },
                        },
                        };
                        EntityCollection results = service.RetrieveMultiple(query);
                        if (results?.Entities.Count > 0)
                        {
                            foreach (Entity entity in results.Entities)
                            {
                                var schemaName = entity.GetAttributeValue<string>("schemaname");
                                var value = entity.GetAttributeValue<AliasedValue>("v.value")?.Value?.ToString();
                                var defaultValue = entity.GetAttributeValue<string>("defaultvalue");
                                if (schemaName != null && !envVariables.ContainsKey(schemaName))
                                {
                                    envVariables.Add(schemaName, string.IsNullOrEmpty(value) ? defaultValue : value);
                                }
                            }
                        }

                        EnvVariables = envVariables;
                        string EnvVal = EnvVariables["cdi_EnvVariable"];
                        Entity LeadRec = new Entity("account");
                        LeadRec.Id = account.Id;
                        LeadRec["description"] = "KEY: " + "cdi_EnvVariable" + "\n" + "VAL: " + EnvVal.ToString();
                        service.Update(LeadRec);
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
