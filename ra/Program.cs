var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// 1. Register the standard ProblemDetails services
builder.Services.AddProblemDetails();

// 2. Register your custom global exception handler class
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


var app = builder.Build();
// 3. Enable the exception handler middleware early in the pipeline
//app.UseExceptionHandler();



// Configure the HTTP request pipeline. 
//if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// builder.Configuration["RoomAlert"];
//string RoomAlertPwd = builder.Configuration["RoomAlert"];

app.Run();
