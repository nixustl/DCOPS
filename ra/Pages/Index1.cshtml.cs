using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using static System.Net.WebRequestMethods;

namespace ra.Pages
{
    public class Index1Model : PageModel
    {
        public void OnGet()
        {
            //Task t = Task.Run(() => TeamsMessage());
            //t.Wait();
            Task t = Task.Run(() => SendEmailAlert());
            t.Wait();
        }

        static async Task TeamsMessage()
        {
            // webhook link
            // https://default79c742c4e61c4fa5be89a3cb566a80.d1.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/0f2c2d3c5a054ce4a3e77ebef4afed52/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=IxN6PQhr_PmqOK0otmHz1WiknGqQ8lv4opPef0AF9j8

            // https://teams.microsoft.com/l/channel/19%3Ae805dba1fce348e684560c79996b285b%40thread.tacv2/Logbook?groupId=b3ebb79d-34af-4ba4-b8b8-09d1a4fc8b2f&tenantId=79c742c4-e61c-4fa5-be89-a3cb566a80d1

            // Code snippets are only available for the latest version. Current version is 5.x

            // Dependencies
            var scopes = new[] { "User.Read", "Chat.ReadWrite", "Files.Read" };
            var tenantId = "79c742c4-e61c-4fa5-be89-a3cb566a80d1";
            var clientId = "appId";
            var options = new InteractiveBrowserCredentialOptions
            {
                TenantId = tenantId,
                ClientId = clientId,
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                RedirectUri = new Uri("http://localhost"),
            };
            var interactiveCredential = new InteractiveBrowserCredential(options);

            var requestBody = new ChatMessage
            {
                CreatedDateTime = DateTimeOffset.Parse("2019-02-04T19:58:15.511Z"),
                From = new ChatMessageFromIdentitySet
                {
                    User = new Identity
                    {
                        Id = "8ea0e38b-efb3-4757-924a-5f94061cf8c2",
                        DisplayName = "Terry Rosenbaum",
                        AdditionalData = new Dictionary<string, object>
                    {
                        {
                            "userIdentityType" , "aadUser"
                        },
                    },
                    },
                },
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "Hello World - testing...",
                },
            };
            var graphClient = new GraphServiceClient(interactiveCredential, scopes);
            // To initialize your graphClient, see https://learn.microsoft.com/en-us/graph/sdks/create-client?from=snippets&tabs=csharp
            var result = await graphClient.Chats["19%3Ae805dba1fce348e684560c79996b285b"].Messages.PostAsync(requestBody);
        }
        public async Task SendTeamsMessage()
        {
            var webhookUrl = "https://default79c742c4e61c4fa5be89a3cb566a80.d1.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/0f2c2d3c5a054ce4a3e77ebef4afed52/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=IxN6PQhr_PmqOK0otmHz1WiknGqQ8lv4opPef0AF9j8";

            var payload = "{ \"text\": \"Hello, this is a test message!\" }";

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(webhookUrl, content);
                response.EnsureSuccessStatusCode();
            }
        }
        public static void SendEmailAlert()
        {
            // RoomAlert - UITS: Data Center Operations <f68ca0f7.groups.uark.edu@amer.teams.ms>


            try
            {
                // SMTP server configuration
                string smtpServer = "smtp.office365.com"; // Change to your SMTP server
                int smtpPort = 587;                   // 587 for TLS, 465 for SSL
                string senderEmail = "ops-roomalert-noreply@uark.edu";
                string senderPassword = "yourpassword"; // Use app-specific password if required

                // Email details
                string recipientEmail = "f68ca0f7.groups.uark.edu@amer.teams.ms";
                string subject = "Test Email from C#";
                string body = "Hello! This is a test email sent from a C# application.";

                // Create the MailMessage object
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail);
                    mail.To.Add(recipientEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = false; // Set to true if sending HTML content

                    // Configure SMTP client
                    using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                        smtp.EnableSsl = true; // Enable SSL/TLS encryption
                        smtp.Send(mail);
                    }
                }

                Console.WriteLine("Email sent successfully!");
                }
                catch (SmtpException smtpEx)
                {
                    Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error: {ex.Message}");
                }
            }
        }
    }
