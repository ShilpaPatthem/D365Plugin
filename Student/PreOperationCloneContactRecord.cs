// <copyright file="PreOperationCloneContactRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace CRM.AGS.Plugins.Student
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PreOperationCloneContactRecord.
    /// </summary>
    public class PreOperationCloneContactRecord : IPlugin
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
                    Entity preImage = (Entity)context.PreEntityImages["preImage"];
                    Entity oldStudent = new Entity("cdi_student");
                    oldStudent = preImage;
                    oldStudent.Id = Guid.Empty;
                    oldStudent.Attributes.Remove("cdi_studentid");
                    oldStudent.Attributes.Remove("createdon");
                    oldStudent.Attributes.Remove("modifiedon");
                    string originalStudent = oldStudent.Attributes.Contains("cdi_name") ? Convert.ToString(oldStudent.Attributes["cdi_name"]) : string.Empty;

                    Entity newStudent = oldStudent;
                    newStudent.Id = Guid.Empty;
                    newStudent.Attributes.Remove("cdi_studentid");
                    newStudent.Attributes.Remove("createdon");
                    newStudent.Attributes.Remove("modifiedon");
                    newStudent["cdi_name"] = "COPY" + originalStudent;
                    Guid newStudentId = service.Create(newStudent);

                    QueryByAttribute queryByAttribute = new QueryByAttribute("cdi_courseregistration");
                    queryByAttribute.ColumnSet = new ColumnSet(true);
                    queryByAttribute.Attributes.AddRange("cdi_student");
                    queryByAttribute.Values.AddRange(entity.Id);

                    EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
                    if (entityCollection != null && entityCollection.Entities.Count > 0)
                    {
                        foreach (var oldCourses in entityCollection.Entities)
                        {
                            oldCourses.Attributes.Remove("cdi_courseregistrationid");
                            oldCourses.Attributes.Remove("createdon");
                            oldCourses.Attributes.Remove("modifiedon");

                            Entity newCourses = oldCourses;
                            newCourses.Id = Guid.Empty;
                            newCourses.Attributes.Remove("cdi_student");
                            newCourses["cdi_student"] = new EntityReference("cdi_student", newStudentId); // child lookup
                            service.Create(newCourses);
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
