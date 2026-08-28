using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using ra.Pages;
using System.Reflection.Metadata;
using System.Web;
//using static System.ClientModel.Primitives.JsonPatch;

namespace ra
{
    public class raData
    {
    }
}

public class Rootobject
{
    public General general { get; set; }
    public Debug debug { get; set; }
    public Digitalsensor[] digitalSensors { get; set; }
    public Switchsensor[] switchSensors { get; set; }
    public Analogsensor[] analogSensors { get; set; }
    public Externalrelay[] externalRelays { get; set; }
    public Internalrelay[] internalRelays { get; set; }
    public Powersensor powerSensor { get; set; }
}

public class General
{
    public string schemaVersion { get; set; }
    public string environment { get; set; }
    public string deviceName { get; set; }
    public int deviceTimestamp { get; set; }
    public string temperatureScale { get; set; }
    public string unitOfSpeed { get; set; }
    public string macAddress { get; set; }
    public string uniqueId { get; set; }
    public string chipType { get; set; }
    public int deviceType { get; set; }
    public int interfaceRefreshRate { get; set; }
    public string firmwareVersion { get; set; }
    public int httpPort { get; set; }
    public bool https { get; set; }
    public int httpsCertExpiration { get; set; }
    public string ipv4Address { get; set; }
    public string serialNumber { get; set; }
    public int raaPushInterval { get; set; }
    public int raaPushTimeout { get; set; }
    public bool raaPushEnabled { get; set; }
    public int customPushInterval { get; set; }
    public int customPushTimeout { get; set; }
    public bool customPushEnabled { get; set; }
    public int bid { get; set; }
}

public class Debug
{
    public int sensorPicVersion { get; set; }
    public bool sensorPicActive { get; set; }
    public bool resetButtonState { get; set; }
    public int uptime { get; set; }
    public Timeconfiguration timeConfiguration { get; set; }
    public Ethernetconfiguration ethernetConfiguration { get; set; }
    public int lastRaaTransmission { get; set; }
    public int lastCustomTransmission { get; set; }
}

public class Timeconfiguration
{
    public int timezone { get; set; }
    public bool displayAmPm { get; set; }
    public bool displayMonthFirst { get; set; }
    public bool daylightSavingsEnabled { get; set; }
}

public class Ethernetconfiguration
{
    public bool autoNegotiateEnabled { get; set; }
}

public class Powersensor
{
    public bool connected { get; set; }
    public string label { get; set; }
    public bool state { get; set; }
    public bool alarmEnabled { get; set; }
    public bool alarmState { get; set; }
}

public class Digitalsensor
{
    public int id { get; set; }
    public int type { get; set; }
    public bool connected { get; set; }
    public string label { get; set; }
    public bool alarmEnabled { get; set; }
    public bool alarmState { get; set; }
    public float temperature { get; set; }
    public float temperatureHigh { get; set; }
    public float temperatureLow { get; set; }
    public float humidity { get; set; }
    public float humidityHigh { get; set; }
    public float humidityLow { get; set; }
    public float heatIndex { get; set; }
    public float heatIndexHigh { get; set; }
    public float heatIndexLow { get; set; }
    public float dewPoint { get; set; }
    public float dewPointHigh { get; set; }
    public float dewPointLow { get; set; }
}

public class Switchsensor
{
    public int id { get; set; }
    public string label { get; set; }
    public bool enabled { get; set; }
    public bool alarmState { get; set; }
    public bool state { get; set; }
}

public class Analogsensor
{
    public string label { get; set; }
    public int id { get; set; }
    public int type { get; set; }
    public bool enabled { get; set; }
}

public class Externalrelay
{
    public int id { get; set; }
    public bool connected { get; set; }
    public int type { get; set; }
    public string label { get; set; }
    public Relay[] relays { get; set; }
}

public class Relay
{
    public int id { get; set; }
    public bool state { get; set; }
}

public class Internalrelay
{
    public int id { get; set; }
    public string label { get; set; }
    public Relay1 relay { get; set; }
}

public class Relay1
{
    public int id { get; set; }
    public bool state { get; set; }
}

public class DCOPS_Message
{
    public bool isError { get; set; }
    public int errorNumber { get; set; } = 0;
    public string? msg { get; set; }
    public string? stacktrace { get; set; }
    public const int SuccessCode = 0;
}

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    // Inject the built-in logger via constructor
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Log the internal exception safely on the server
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // 2. Map different exceptions to distinct HTTP Status Codes
        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        // 3. Create a standardized Problem Details payload
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message, // Consider masking this detail in Production envs
            Instance = httpContext.Request.Path
        };

        // 4. Configure the HTTP context response
        httpContext.Response.StatusCode = statusCode;

        //return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        //{
        //    HttpContext = httpContext,
        //    ProblemDetails = problemDetails
        //});


        // 5. Serialize and write the response back to the client

        //await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        //httpContext.Response.Redirect("/Index?errorNo="+statusCode+"&errorMessage="+exception.Message);

        //string targetUrl = QueryHelpers.AddQueryString("/dashboard", exception.Message);


        //string encodedValue = HttpUtility.UrlEncode(exception.Message);
        //string key = "msg";
        //string targetPage = "/Index";
        //string redirectUrl = $"{targetPage}?{key}={exception.Message}";
        //httpContext.Response.Redirect(redirectUrl);



        //httpContext.Response.Redirect("/Index?msg="+exception.Message);
        // Return true to signal that this exception has been successfully handled
        return true;
    }
}