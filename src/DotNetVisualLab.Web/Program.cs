using DotNetVisualLab.Web.Components;
using DotNetVisualLab.Web.Labs.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// Lab 01: three registrations, three different lifetime contracts.
builder.Services.AddTransient<ITransientProbe, TransientProbe>();
builder.Services.AddScoped<IScopedProbe, ScopedProbe>();
builder.Services.AddSingleton<ISingletonProbe, SingletonProbe>();

// The runner is scoped to the current Blazor circuit. Each Run() still creates
// a fresh IServiceScope so request-boundary behavior remains real and isolated.
builder.Services.AddScoped<LifetimeExperimentRunner>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
