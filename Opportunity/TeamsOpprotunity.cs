//// <copyright file="TeamsOpprotunity.cs" company="PlaceholderCompany">
//// Copyright (c) PlaceholderCompany. All rights reserved.
//// </copyright>

//namespace CRM.AGS.Plugins.Opportunity
//{
//    using System;
//    using Microsoft.Xrm.Sdk;
//    using Microsoft.Xrm.Sdk.Query;

//    /// <summary>
//    /// TeamsOpprotunity.
//    /// </summary>
//    public class TeamsOpprotunity : IPlugin
//    {
//        /// <inheritdoc/>
//        public void Execute(IServiceProvider serviceProvider)
//        {
//            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
//            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
//            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
//            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
//            {
//                Entity entity = (Entity)context.InputParameters["Target"];
//                try
//                {
//                    Guid guidRoleId = new Guid();
//                    string teamName = string.Empty;
//                    if (entity.Attributes.Contains("cdi_type") != null)
//                    {
                        
                      
//                    }
//                }
//                catch (Exception ex)
//                {
//                    throw new InvalidPluginExecutionException(ex.Message);
//                }
//            }
//        }

//        public void GetTeams(IOrganizationService service)
//        {
//            QueryExpression query = new QueryExpression("team");
//            query.ColumnSet.AddColumns(new string[] { "name", "teamid" });
//            EntityCollection collection = service.RetrieveMultiple(query);
//            if (collection != null && collection.Entities != null && collection.Entities.Count > 0)
//            {
//                foreach (Entity teams in collection.Entities)
//                {
//                    // teamId = (teams.Attributes["teamid"] as EntityReference).Id;

//                    EntityReference team = (EntityReference)teams.Attributes["teamid"];
//                    Guid teamId = team.Id;
//                    string teamName = teams.Contains("name") ? Convert.ToString(teams.Attributes["name"]) : string.Empty;
//                    if (teamName == "Finance Team")
//                    {
//                        assignSecurityRole(service, teamId, teamName);
//                    }
//                    else if (teamName.ToUpperInvariant() == "Managerial Team")
//                    {
//                        assignSecurityRole(service, teamId, teamName);
//                    }
//                    else if (teamName.ToUpperInvariant() == "Operational Team")
//                    {
//                        assignSecurityRole(service, teamId, teamName);
//                    }

//                }

//            }
//        }

//        public string GetRoleName(IOrganizationService service)
//        {
//            string strRoleName = string.Empty;
//            QueryExpression query = new QueryExpression("role");
//            query.ColumnSet.AddColumns(new string[] { "name", "roleid"});
//            EntityCollection roleCollection = service.RetrieveMultiple(query);
//            if (roleCollection != null && roleCollection.Entities != null && roleCollection.Entities.Count > 0)
//            {
//                foreach (Entity roles in roleCollection.Entities)
//                {
//                    Guid roleId = (roles.Attributes["roleid"] as EntityReference).Id;
//                    strRoleName = roles.Contains("name") ? Convert.ToString(roles.Attributes["name"]) : string.Empty;
//                    if (strRoleName.ToUpperInvariant() == "Finance Manager")
//                    {
//                        return strRoleName;
//                    }
//                }
//            }
//        }

//        public string privilege(IOrganizationService service)
//        {
//            QueryExpression query = new QueryExpression("privilege");
//            query.ColumnSet.AddColumns(new string[] { "name", "roleid" });
//            var filter = new FilterExpression(LogicalOperator.Or);

//            foreach (var p in roleResponse.RolePrivileges)
//                filter.AddCondition("privilegeid", ConditionOperator.Equal, p.PrivilegeId);

//            privilegeQuery.Criteria = filter;

//            var privileges = service.RetrieveMultiple(privilegeQuery);
//        }
//    }
//}
