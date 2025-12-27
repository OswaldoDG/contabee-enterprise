using MudBlazor.Services;
using staterkit.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using ApexCharts;
using BlazorColorPicker;
using Blazored.SessionStorage;

namespace staterkit
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddColorPicker();
            builder.Services.AddMudServices();
            builder.Services.AddScoped<StateService>();
            builder.Services.AddSingleton<AppState>();
            builder.Services.AddScoped<SessionService>();
            builder.Services.AddScoped<IActionService, ActionService>();
            builder.Services.AddSession();
            builder.Services.AddScoped<NavScrollService>();
            builder.Services.AddWMBOS();
            builder.Services.AddScoped<MenuDataService>();
            builder.Services.AddBlazoredSessionStorage();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddTransient<TokenHandler>();

            builder.Services.AddHttpClient("ContabeeAPI", client =>
            {
                client.BaseAddress = new Uri("https://api.contabee.mx");
            }).AddHttpMessageHandler<TokenHandler>();

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ContabeeAPI"));

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://api.contabee.mx")
            });

            // Add session services
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust timeout as needed
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Register HttpClient service
            builder.Services.AddHttpClient();

            builder.Services.AddHttpContextAccessor();

            // Add services to the container.
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.Configuration.GetSection<OIDCSettings>("OIDCSettings");

            app.UseHttpsRedirection();

            app.UseSession();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
            app.Run();
        }
    }
}
