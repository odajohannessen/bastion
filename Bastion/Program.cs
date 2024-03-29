using Bastion.Core; 
using Bastion.Core.Domain.Encryption.Services;
using Bastion.Core.Domain.Decryption.Services;
using Bastion.Managers;
using MediatR;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Rewrite;
using BootstrapBlazor.Components;

var builder = WebApplication.CreateBuilder(args);

var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd")) 
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
.AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddMicrosoftIdentityConsentHandler();
builder.Services.AddTransient<IEncryptionService, EncryptionService>();
builder.Services.AddTransient<IDecryptionService, DecryptionService>();
builder.Services.AddTransient<IStorageService, StorageService>();
builder.Services.AddTransient<IDeletionService, DeletionService>();
builder.Services.AddTransient<LoggingManager>();
builder.Services.AddScoped<CopyToClipboardManager>();
builder.Services.AddBootstrapBlazor();

// Default login after startup before reaching index page
//builder.Services.AddAuthorization(options =>
//{
//    // By default, all incoming requests will be authorized according to the default policy
//    options.FallbackPolicy = options.DefaultPolicy;
//});

builder.Services.AddAuthorization(); 

// Prompt user to select Microsoft account when logging in
builder.Services.Configure<OpenIdConnectOptions>(options =>
{
    options.Events.OnRedirectToIdentityProvider = context =>
    {
        context.ProtocolMessage.SetParameter("prompt", "select_account");
        return Task.FromResult(0);
    };
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

var app = builder.Build();

// Redirect to index page after log out
app.UseRewriter(
    new RewriteOptions().Add(
        context => {
            if (context.HttpContext.Request.Path == "/MicrosoftIdentity/Account/SignedOut")
            { context.HttpContext.Response.Redirect("/"); }
        })
);

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapBlazorHub();

app.MapFallbackToPage("/_Host");

app.Run();
