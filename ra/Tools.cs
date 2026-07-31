using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph.Models.Security;
using MimeKit;
using MimeKit.Text;
using Npgsql;
using System.Net;
using System.Net.Mail;

namespace ra
{
    public class Tools
    {


        public static class ApplicationConfiguration
        {
            private static IConfigurationRoot _configuration;
            static ApplicationConfiguration()
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                _configuration = builder.Build();
            }
            public static string GetSetting(string key)
            {
                return _configuration[key];
            }
        }


        public static float ToFarenheit(float celVal)
        {
            return (float) Math.Round((celVal * 1.8) + 32, 2);
        }


        public static void sendMail()
        {
            try
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("from name", "trosenbaum@uark.edu"));
                mailMessage.To.Add(new MailboxAddress("to name", "trosenbaum@uark.edu"));
                mailMessage.Subject = "subject";
                mailMessage.Body = new TextPart("plain")
                {
                    Text = "Hello"
                };

                using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
                {
                    smtpClient.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    smtpClient.Authenticate("roomalertsvc@uark.edu", "(bP&t\\o0?ti.'}H`6KRf");
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        // PublishData posts HTML sensor data to Team RoomAlert feed
        public async static Task<DCOPS_Message> PublishData(string body, string runStamp, DCOPS_Message returnMessage)
        {            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("DC OPS", "ops-roomalert-noreply@uark.edu"));
            message.To.Add(new MailboxAddress("Teams RoomAlert", "f68ca0f7.groups.uark.edu@amer.teams.ms"));
            message.Subject = "DC Ops Sensor Data";
            message.Body = new TextPart(TextFormat.Html) { Text = body };
            
            using var client = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("roomalertsvc@uark.edu", ApplicationConfiguration.GetSetting("RoomAlertPWD"));
                await client.SendAsync(message);
                return returnMessage;
            }
            catch (Exception ex)
            {
                returnMessage.isError = true;
                returnMessage.errorNumber = ex.HResult;
                returnMessage.msg = ex.Message;
                return returnMessage;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
        public static string GetHistory(string targetDate)
        {
            string connectionString = ApplicationConfiguration.GetSetting("ConnectionStrings:Postgres");
            using var dataSource = NpgsqlDataSource.Create(connectionString);
            DCOPS_Message DCOPS_return_message = new DCOPS_Message();
            using var command = dataSource.CreateCommand("select html from \"sensorHistory\" where run_dt = '" + targetDate + "'");
            using var reader =  command.ExecuteReader();
            List<string> sDates = new List<string>();
            string returnHtml = string.Empty;
            reader.ReadAsync();
            if(reader.HasRows)
            {
                returnHtml = reader.GetFieldValue<string>(0);
            } else
            {
                returnHtml = "<span style='font-size:24px;'> No records found.";
            }
             
            return returnHtml;
        }
        public async static Task<List<string>> PopulateDates()
        {
            string connectionString = ApplicationConfiguration.GetSetting("ConnectionStrings:Postgres");

            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            //DCOPS_Message DCOPS_return_message = new DCOPS_Message();
   
            await using var command = dataSource.CreateCommand("select run_dt from \"sensorHistory\" order by run_dt desc;");

            await using var reader = await command.ExecuteReaderAsync();
            List<string> sDates = new List<string>();
            while (await reader.ReadAsync())
            {
                sDates.Add(reader.GetFieldValue<DateTime>(0).ToString());
            }

            return sDates;
        }
        public async static Task<DCOPS_Message> SaveToDB(string html, DateTime runDate)
        {
            string connectionString = ApplicationConfiguration.GetSetting("ConnectionStrings:Postgres");

            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            DCOPS_Message DCOPS_return_message = new DCOPS_Message();

            await using var command = dataSource.CreateCommand("INSERT INTO  \"sensorHistory\" (run_dt , html) VALUES ($1, $2);");
            command.Parameters.AddWithValue(runDate);
            command.Parameters.AddWithValue(html);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return DCOPS_return_message;

        }
        public async static Task<DCOPS_Message> DBFetch(DateTime startDate, DateTime endDate)
        {
            string connectionString = ApplicationConfiguration.GetSetting("ConnectionStrings:Postgres");

            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            DCOPS_Message DCOPS_return_message = new DCOPS_Message();

            await using var command = dataSource.CreateCommand("SELECT getsensordata($1,$2);");
            command.Parameters.AddWithValue(startDate);
            command.Parameters.AddWithValue(endDate);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string username = reader.GetString(0);
                string email = reader.GetString(1);
            }


            return DCOPS_return_message;

        }

    }
}
