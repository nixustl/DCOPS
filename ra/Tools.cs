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
        public async static Task<List<string>> PopulateDates(DateTime startDate, DateTime endDate)
        {
            string connectionString = ApplicationConfiguration.GetSetting("ConnectionStrings:Postgres");
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            await using var command = dataSource.CreateCommand("select run_dt from \"sensorHistory\" where CAST(run_dt AS date) >= $1 and CAST(run_dt AS date) <= $2 order by run_dt desc");
            command.Parameters.AddWithValue(startDate);
            command.Parameters.AddWithValue(endDate);
            await using var reader = await command.ExecuteReaderAsync();
            List<string> DateList = new List<string>();
            while (await reader.ReadAsync())
            {
                DateList.Add(reader.GetFieldValue<DateTime>(0).ToString());
            }
            return DateList;
        }
        public async static Task<DCOPS_Message> SaveToDB(string html, DateTime runDate, DCOPS_Message dcm)
        {
            DCOPS_Message DCOPS_return_message = new DCOPS_Message();
            try
            {
                string connectionString = ApplicationConfiguration.GetSetting("ConnectionStrings:Postgres");
                await using var dataSource = NpgsqlDataSource.Create(connectionString);
                await using var command = dataSource.CreateCommand("INSERT INTO  \"sensorHistory\" (run_dt , html) VALUES ($1, $2);");
                command.Parameters.AddWithValue(runDate);
                command.Parameters.AddWithValue(html);
                int rowsAffected = await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                dcm.isError = true;
                dcm.errorNumber = ex.HResult;
                dcm.msg = ex.Message;
                dcm.stacktrace = ex.StackTrace;
            }
            return dcm;
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
