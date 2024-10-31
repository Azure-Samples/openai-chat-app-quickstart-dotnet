using Microsoft.FluentUI.AspNetCore.Components;
using ChatAppUI.Components;
using Microsoft.Extensions.Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Identity.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

// OpenAI service
var endpoint = builder.Configuration.GetValue<string>("AZURE_OPENAI_ENDPOINT");
var modelName = builder.Configuration.GetValue<string>("AZURE_OPENAI_CHATGPT_DEPLOYMENT");

// Credentials
var userAssignedIdentityCredential = 
    new ManagedIdentityCredential(builder.Configuration.GetValue<string>("AZURE_CLIENT_ID"));
    
var azureDevCliCredential = new AzureDeveloperCliCredential(
    new AzureDeveloperCliCredentialOptions()
    { 
        TenantId = builder.Configuration.GetValue<string>("AZURE_TENANT_ID") 
    });

// Register service and credentials
builder.Services.AddAzureClients(
    clientBuilder => {
        clientBuilder.AddClient<AzureOpenAIClient, AzureOpenAIClientOptions>((options, _, _)
            => new AzureOpenAIClient(
                new Uri(endpoint),
                new ChainedTokenCredential(userAssignedIdentityCredential, azureDevCliCredential), options));
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

} else {
    
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
