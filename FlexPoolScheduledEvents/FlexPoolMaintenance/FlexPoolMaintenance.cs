using System;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using System.Net;
using MailKit.Net.Smtp;
using MimeKit;

//// https://aws.amazon.com/premiumsupport/knowledge-center/build-lambda-deployment-dotnet/ 
//// $> dotnet lambda deploy-function --region us-east-2 --function-name flexpoolmaintenance
////
//// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace FlexPoolMaintenance
{
    public class FlexPoolMaintenance
    {
        public void StartMaintenance()
        {
            Console.WriteLine("Testing 123.");
            MimeMessage message = new MimeMessage();
            MailboxAddress from = new MailboxAddress("noreply", "ussflexpool@gmail.com");
            message.From.Add(from);

            MailboxAddress to = new MailboxAddress("Scott Smalley", "scottsmalley90@gmail.com");
            message.To.Add(to);
            message.Subject = "Test email from Flexpool.";
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = "Hello! \n I'm the automated maintenance tester. \n I run every night at 1 AM MST. \n" +
                "My goal is to generate weekly statistics for HR, delete 90-day-or-older messages, and potentially notify " +
                "shift managers and employees of 24 / 48 hr reminders of their shifts, if something changes in their signed up shift," +
                "and/or available shifts that said employee qualifies for." +
                "\n Thank you," +
                "\n noreply." +
                "\n\n Side Note: This email is not monitored for responses. Please contact your manager or HR for assistance. Thank you.";

            message.Body = bodyBuilder.ToMessageBody();
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, ch, e) => true;
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("ussflexpool@gmail.com", "Fl3xPo0lDB!");

                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
                Console.WriteLine("Done");
            }
        }
    }
}
