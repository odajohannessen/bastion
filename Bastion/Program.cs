using Bastion.Core; 
using Bastion.Core.Domain.Encryption.Services;
using Bastion.Core.Domain.Decryption.Services;
using MediatR;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Azure.Identity;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddTransient<IEncryptionService, EncryptionService>();
builder.Services.AddTransient<IDecryptionService, DecryptionService>();
builder.Services.AddTransient<IStorageService, StorageService>();
builder.Services.AddTransient<IDeletionService, DeletionService>();
builder.Services.AddMediatR(typeof(Program));
//builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

var app = builder.Build();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
