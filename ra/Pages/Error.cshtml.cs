using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;


namespace ra.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet()
        {
            HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var exceptionHandlerFeature =  HttpContext.Features.Get<IExceptionHandlerFeature>();
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            var undhandledException = feature?.Error;
            IndexModel.errorNo = Request.Query["errorNo"];
            IndexModel.errorMessage = Request.Query["errorMessage"];


        }
    }

}
