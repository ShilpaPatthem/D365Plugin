// <copyright file="PostAccountUpdatePrimaryContactUpdateNotes.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CRM.AGS.Plugins.Account
{
    using System;
    using System.Globalization;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PostAccountUpdatePrimaryContactUpdateNotes.   
    /// </summary>
    public class PostAccountUpdatePrimaryContactUpdateNotes : IPlugin
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
                Entity preImage = (Entity)context.PreEntityImages["preImage"];
                try
                {
                    EntityReference prePrimarycontactid = preImage.Attributes.Contains("primarycontactid") ? preImage.Attributes["primarycontactid"] as EntityReference : null;
                   // EntityReference contact = new EntityReference("primarycontactid");
                    Entity user = service.Retrieve("systemuser", context.UserId, new ColumnSet("fullname"));
                    string ownerName = user["fullname"].ToString();
                    string name = entity.Contains("name") ? Convert.ToString(entity.Attributes["name"]) : string.Empty;
                    bool rolename = this.Roles(service, entity);
                    if (prePrimarycontactid != null && rolename == true)
                    {
                        this.NotesRecords(service, entity, name, ownerName, prePrimarycontactid);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        public void NotesRecords(IOrganizationService service, Entity entity, string name, string ownerName,EntityReference prePrimarycontactid)
        {
            DateTime today = DateTime.Now;
            QueryExpression queryExpression = new QueryExpression("annotation");
            queryExpression.ColumnSet.AddColumns(new string[] { "createdon", "objectid", "subject", "notetext" });
            queryExpression.Criteria.AddCondition("createdon", ConditionOperator.On, today);
            queryExpression.Criteria.AddCondition("objectid", ConditionOperator.Equal, entity.Id);
            EntityCollection collection = service.RetrieveMultiple(queryExpression);
            if (collection != null && collection.Entities != null && collection.Entities.Count > 0)
            {
                foreach (var records in collection.Entities)
                {
                    var sub = records.Contains("subject") ? records.Attributes["subject"] : string.Empty;
                    var description = records.Contains("notetext") ? records.Attributes["notetext"] : string.Empty;

                    if (sub != null && description != null)
                    {
                        Entity note = new Entity("annotation");
                        note.Id = records.Id;
                        note["subject"] = ownerName + " " + "updatedid" + name + "on" + DateTime.Now;
                        note["notetext"] = "primary contact is updated at" + DateTime.Now + " old value " +" "+prePrimarycontactid.Name;
                        service.Update(note);
                    }
                }
            }
            else
            {
                Entity notes = new Entity("annotation");
                notes["notetext"] = "new Description";
                notes["objectid"] = new EntityReference("account", entity.Id);
                service.Create(notes);
            }
        }

        public bool Roles(IOrganizationService service, Entity entity)
        {
            bool isRoleExist = false;
            QueryExpression queryExpression = new QueryExpression("role");
            queryExpression.ColumnSet.AddColumn("name");
            queryExpression.Criteria.AddCondition("roleid", ConditionOperator.NotNull);

            LinkEntity linkEntity = new LinkEntity();
            linkEntity.LinkFromEntityName = "role";
            linkEntity.LinkToEntityName = "systemuserroles";
            linkEntity.LinkFromAttributeName = "roleid";
            linkEntity.LinkToAttributeName = "roleid";

            LinkEntity linkEntity1 = new LinkEntity();
            linkEntity1.LinkFromEntityName = "systemuserroles";
            linkEntity1.LinkToEntityName = "systemuser";
            linkEntity1.LinkFromAttributeName = "systemuserid";
            linkEntity1.LinkToAttributeName = "systemuserid";
            linkEntity1.EntityAlias = "Shilpa";
            linkEntity1.Columns = new ColumnSet("fullname");
            linkEntity1.LinkCriteria.AddCondition("isdisabled", ConditionOperator.Equal, false);

            linkEntity.LinkEntities.Add(linkEntity1);
            queryExpression.LinkEntities.Add(linkEntity);

            EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);

            if (entityCollection != null && entityCollection.Entities != null && entityCollection.Entities.Count > 0)
            {
                foreach (var roles in entityCollection.Entities)
                {
                    string roleName = roles.Contains("name") ? Convert.ToString(roles["name"], CultureInfo.InvariantCulture) : string.Empty;
                    if (roleName.ToUpperInvariant() == "DEMO ROLE")
                    {
                        isRoleExist = true;
                        break;
                    }
                }
            }

            return isRoleExist;
        }
    }
}
