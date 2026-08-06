using CharacterChronicles.Components;
using CharacterChronicles.Data;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

Env.Load();

static string ConvertPostgresUrl(string value)
{
    if (!value.StartsWith(
            "postgres://",
            StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith(
            "postgresql://",
            StringComparison.OrdinalIgnoreCase))
    {
        return value;
    }

    var uri = new Uri(value);

    var userInfo =
        uri.UserInfo.Split(':', 2);

    var username =
        Uri.UnescapeDataString(userInfo[0]);

    var password =
        userInfo.Length > 1
            ? Uri.UnescapeDataString(userInfo[1])
            : string.Empty;

    var database =
        uri.AbsolutePath.TrimStart('/');

    var builder =
        new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Username = username,
            Password = password,
            Database = database,
            SslMode = Npgsql.SslMode.Require
        };

    return builder.ConnectionString;
}

var builder = WebApplication.CreateBuilder(args);

var databaseUrl =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException(
        "DATABASE_URL environment variable is not configured.");

/* ==========================================================
 * Database
 * ========================================================== */

var connectionString =
    ConvertPostgresUrl(databaseUrl);

builder.Services
    .AddDbContextFactory<CharacterChroniclesDbContext>(
        options =>
            options.UseNpgsql(connectionString));


/* ==========================================================
 * Identity / Authentication
 * ========================================================== */

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            IdentityConstants.ApplicationScheme;

        options.DefaultChallengeScheme =
            IdentityConstants.ApplicationScheme;

        options.DefaultSignInScheme =
            IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services
    .AddIdentityCore<IdentityUser>(
        options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
    .AddEntityFrameworkStores<CharacterChroniclesDbContext>()
    .AddSignInManager();

/* ==========================================================
 * Authorization
 * ========================================================== */

builder.Services.AddAuthorization();

/* ==========================================================
 * Razor / Blazor
 * ========================================================== */

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

/* ==========================================================
 * HTTP Pipeline
 * ========================================================== */

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();