using OrderLagerSystem.Client.Components;
using OrderLagerSystem.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registrera AuthService som Singleton för att behålla state
builder.Services.AddSingleton<AuthService>();

// Registrera ArticleService för artikel-operationer
builder.Services.AddScoped<IArticleService, ArticleService>();

// Konfigurera HttpClient för API-kommunikation
builder.Services.AddHttpClient("OrderLagerSystemApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5265");
});

// Registrera en default HttpClient för komponenter
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("OrderLagerSystemApi");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
