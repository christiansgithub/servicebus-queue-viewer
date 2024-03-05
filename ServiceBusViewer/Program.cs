using Microsoft.Extensions.Azure;
using ServiceBusViewer.Pages.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAzureClients(clientsBuilder =>
{
    var connectionString = builder.Configuration["AzureServiceBusConnectionString"];
    
    clientsBuilder.AddServiceBusClient(connectionString);
    clientsBuilder.AddServiceAdministrationBusClient(connectionString);
});

builder.Services.AddSingleton<ServiceBus>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();