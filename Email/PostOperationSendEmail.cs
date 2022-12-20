//// <copyright file="PostOperationSendEmail.cs" company="PlaceholderCompany">
//// Copyright (c) PlaceholderCompany. All rights reserved.
//// </copyright>

//namespace CRM.AGS.Plugins.Email
//{
//    using System;
//    using System.Text;
//    using Microsoft.Xrm.Sdk;
//    using Microsoft.Xrm.Sdk.Query;

//    /// <summary>
//    /// PostOperationSendEmail.
//    /// </summary>

//    public class PostOperationSendEmail : IPlugin
//    {
//        /// <inheritdoc/>
//        public void Execute(IServiceProvider serviceProvider)
//        {
//            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
//            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
//            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
//            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
//            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
//            {
//                Entity entity = (Entity)context.InputParameters["Target"];

//                try
//                {

//                }
//                catch (Exception ex)
//                {
//                    throw new InvalidPluginExecutionException(ex.Message);
//                }
//            }
//        }

//        public void CreateEmail(IOrganizationService service)

//        {

//            // Fetch a user/contact to send an email to (To: field)

//            Entity emailUser = new Entity();

//            emailUser.LogicalName = "systemuser";

//            ColumnSet cset = new ColumnSet();

//            // check if account already exists

//            cset = new ColumnSet(new string[] { "internalemailaddress" });

//            QueryExpression userquery = new QueryExpression();

//            userquery.EntityName = "systemuser";

//            userquery.ColumnSet = cset;

//            userquery.Criteria = new FilterExpression();

//            EntityCollection ToUserResult = new EntityCollection();

//            userquery.Criteria.AddCondition("internalemailaddress", ConditionOperator.Equal, "touser@email.com");

//            ToUser = service.RetrieveMultiple(userquery);

//            Guid ToUserGuid;

//            if (ToUserResult.Entities.Count > 0)

//            {

//                ToUserGuid = ToUserResult.Entities[0].Id;

//                Entity fromParty = new Entity("activityparty");

//                Entity ToParty = new Entity("activityparty");

//                fromParty["partyid"] = new EntityReference("systemuser", ToUserGuid);

//                ToParty["partyid"] = new EntityReference("systemuser", ToUserGuid);

//                Entity email = new Entity();

//                email.LogicalName = "email";

//                email["from"] = new Entity[] { fromParty };

//                email["to"] = new Entity[] { ToParty };

//                email["subject"] = "Test email from Workflow";

//                email["description"] = "This email has been sent from Test workflow";

//                email["directioncode"] = true;

//                EmailId = service.Create(email);

//            }

//        }

//        public void CreateAttachment(IOrganizationService service, Guid EmailId)

//        {

//            Entity EmailAttachment = new Entity();

//            EmailAttachment.LogicalName = "activitymimeattachment";

//            EmailAttachment["objectid"] = new EntityReference("email", EmailId);

//            EmailAttachment["objecttypecode"] = "email";

//            EmailAttachment["subject"] = "Test Attachment";

//            EmailAttachment["body"] = System.Convert.ToBase64String(new ASCIIEncoding().GetBytes("This is a test content for attachment"));

//            EmailAttachment["filename"] = "TestAttachment.txt";

//            Guid attachmentId = service.Create(EmailAttachment);

//            Console.WriteLine("attachment created");

//        }

//        public void SendEmail(IOrganizationService service)

//        {

//            SendEmailRequest sendEmailRequest = new SendEmailRequest();

//            sendEmailRequest.EmailId = EmailId;

//            sendEmailRequest.TrackingToken = "";

//            sendEmailRequest.IssueSend = true;

//            SendEmailResponse sendEmailResponse = (SendEmailResponse)service.Execute(sendEmailRequest);

//            Console.WriteLine("email sent");

//        }
//    }
//}
