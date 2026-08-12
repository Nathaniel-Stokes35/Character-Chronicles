using CharacterChronicles.Components;
using CharacterChronicles.Data;
using CharacterChronicles.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;

Env.Load();

static string ConvertPostgresUrl(
    string value)
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

    var uri =
        new Uri(value);

    var userInfo =
        uri.UserInfo.Split(':', 2);

    var connectionBuilder =
        new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host =
                uri.Host,

            Port =
                uri.IsDefaultPort
                    ? 5432
                    : uri.Port,

            Username =
                Uri.UnescapeDataString(
                    userInfo[0]),

            Password =
                userInfo.Length > 1
                    ? Uri.UnescapeDataString(
                        userInfo[1])
                    : string.Empty,

            Database =
                uri.AbsolutePath.TrimStart('/'),

            SslMode =
                Npgsql.SslMode.Require,

            Pooling =
                true,

            MinPoolSize =
                0,

            MaxPoolSize =
                100
        };

    return connectionBuilder.ConnectionString;
}


static string SafeReturnUrl(
    string? value,
    string fallback = "/account")
{
    if (string.IsNullOrWhiteSpace(value) ||
        !value.StartsWith('/') ||
        value.StartsWith(
            "//",
            StringComparison.Ordinal))
    {
        return fallback;
    }

    return value;
}


static string BuildAuthErrorUrl(
    string page,
    string message,
    string? returnUrl = null)
{
    var url =
        $"{page}?error={Uri.EscapeDataString(message)}";

    if (!string.IsNullOrWhiteSpace(
            returnUrl))
    {
        url +=
            $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
    }

    return url;
}


static string BuildAccountMessageUrl(
    string parameter,
    string message)
{
    return
        $"/account?{parameter}=" +
        Uri.EscapeDataString(message);
}


static ClaimsPrincipal CreatePrincipal(
    User user)
{
    var claims =
        new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id),

            new Claim(
                ClaimTypes.Name,
                user.Email),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                "display_name",
                user.DisplayName)
        };

    var identity =
        new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults
                .AuthenticationScheme);

    return
        new ClaimsPrincipal(identity);
}


static string CreateFriendCode()
{
    const string alphabet =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    const int length =
        10;

    var result =
        new char[length];

    for (var i = 0;
         i < result.Length;
         i++)
    {
        var index =
            RandomNumberGenerator.GetInt32(
                alphabet.Length);

        result[i] =
            alphabet[index];
    }

    return new string(result);
}


static async Task<string>
    CreateUniqueFriendCodeAsync(
        CharacterChroniclesDbContext db)
{
    while (true)
    {
        var code =
            CreateFriendCode();

        var exists =
            await db.Users
                .AsNoTracking()
                .AnyAsync(
                    user =>
                        user.FriendCode ==
                        code);

        if (!exists)
        {
            return code;
        }
    }
}


var builder =
    WebApplication.CreateBuilder(args);


var databaseUrl =
    Environment.GetEnvironmentVariable(
        "DATABASE_URL")
    ?? throw new InvalidOperationException(
        "DATABASE_URL environment variable is not configured.");


var connectionString =
    ConvertPostgresUrl(
        databaseUrl);


builder.Services
    .AddPooledDbContextFactory<
        CharacterChroniclesDbContext>(
        options =>
        {
            options.UseNpgsql(
                connectionString);
        },
        poolSize: 64);


builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults
            .AuthenticationScheme)
    .AddCookie(
        options =>
        {
            options.Cookie.Name =
                "CharacterChronicles.Auth";

            options.Cookie.HttpOnly =
                true;

            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy
                    .SameAsRequest;

            options.LoginPath =
                "/login";

            options.AccessDeniedPath =
                "/login";

            options.ExpireTimeSpan =
                TimeSpan.FromDays(14);

            options.SlidingExpiration =
                true;
        });


builder.Services.AddAuthorization();

builder.Services
    .AddCascadingAuthenticationState();


builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();


var app =
    builder.Build();


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


await using (
    var scope =
        app.Services.CreateAsyncScope())
{
    var factory =
        scope.ServiceProvider
            .GetRequiredService<
                IDbContextFactory<
                    CharacterChroniclesDbContext>>();

    await using var db =
        await factory.CreateDbContextAsync();


    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS "Users" (
            "Id" character varying(64) NOT NULL,
            "DisplayName" character varying(80) NOT NULL,
            "Email" character varying(320) NOT NULL,
            "NormalizedEmail" character varying(320) NOT NULL,
            "PasswordHash" text NOT NULL,
            "FriendCode" character varying(12),
            "CreatedAt" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Users"
                PRIMARY KEY ("Id")
        );

        ALTER TABLE "Users"
        ADD COLUMN IF NOT EXISTS
            "FriendCode" character varying(12);

        CREATE UNIQUE INDEX IF NOT EXISTS
            "IX_Users_NormalizedEmail"
        ON "Users" ("NormalizedEmail");

        CREATE TABLE IF NOT EXISTS "Friendships" (
            "UserId" character varying(64) NOT NULL,
            "FriendUserId" character varying(64) NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_Friendships"
                PRIMARY KEY (
                    "UserId",
                    "FriendUserId"
                )
        );

        CREATE INDEX IF NOT EXISTS
            "IX_Friendships_UserId"
        ON "Friendships" ("UserId");

        CREATE INDEX IF NOT EXISTS
            "IX_Friendships_FriendUserId"
        ON "Friendships" ("FriendUserId");

        ALTER TABLE "Campaigns"
        ADD COLUMN IF NOT EXISTS
            "Setting" character varying(150)
            NOT NULL DEFAULT '';

        ALTER TABLE "Campaigns"
        ADD COLUMN IF NOT EXISTS
            "GameSystem" character varying(80)
            NOT NULL DEFAULT 'D&D 5e';

        ALTER TABLE "Campaigns"
        ADD COLUMN IF NOT EXISTS
            "SessionSchedule" character varying(200)
            NOT NULL DEFAULT '';

        ALTER TABLE "Campaigns"
        ADD COLUMN IF NOT EXISTS
            "Status" character varying(40)
            NOT NULL DEFAULT 'Active';

        ALTER TABLE "Campaigns"
        ADD COLUMN IF NOT EXISTS
            "DmNotes" text
            NOT NULL DEFAULT '';

        CREATE INDEX IF NOT EXISTS
            "IX_Campaigns_UserId"
        ON "Campaigns" ("UserId");

        CREATE INDEX IF NOT EXISTS
            "IX_Characters_UserId"
        ON "Characters" ("UserId");

        CREATE INDEX IF NOT EXISTS
            "IX_Characters_CampaignId"
        ON "Characters" ("CampaignId");

        CREATE INDEX IF NOT EXISTS
            "IX_Characters_UserId_CampaignId"
        ON "Characters" ("UserId", "CampaignId");
        """
    );


    var missingFriendCodeIds =
        new List<string>();


    var connection =
        db.Database.GetDbConnection();


    var shouldClose =
        connection.State !=
        ConnectionState.Open;


    if (shouldClose)
    {
        await connection.OpenAsync();
    }


    try
    {
        await using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            SELECT "Id"
            FROM "Users"
            WHERE "FriendCode" IS NULL
               OR "FriendCode" = '';
            """;


        await using var reader =
            await command.ExecuteReaderAsync();


        while (await reader.ReadAsync())
        {
            missingFriendCodeIds.Add(
                reader.GetString(0));
        }
    }
    finally
    {
        if (shouldClose)
        {
            await connection.CloseAsync();
        }
    }


    foreach (
        var userId
        in missingFriendCodeIds)
    {
        var friendCode =
            await CreateUniqueFriendCodeAsync(
                db);

        await db.Database
            .ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE "Users"
                SET "FriendCode" = {friendCode}
                WHERE "Id" = {userId};
                """);
    }


    await db.Database.ExecuteSqlRawAsync(
        """
        ALTER TABLE "Users"
        ALTER COLUMN "FriendCode"
        SET NOT NULL;

        CREATE UNIQUE INDEX IF NOT EXISTS
            "IX_Users_FriendCode"
        ON "Users" ("FriendCode");

        CREATE TABLE IF NOT EXISTS "CampaignMembers" (
            "CampaignId" integer NOT NULL,
            "UserId" character varying(64) NOT NULL,
            "JoinedAt" timestamp with time zone NOT NULL,

            CONSTRAINT "PK_CampaignMembers"
                PRIMARY KEY (
                    "CampaignId",
                    "UserId"
                ),

            CONSTRAINT "FK_CampaignMembers_Campaigns"
                FOREIGN KEY ("CampaignId")
                REFERENCES "Campaigns" ("Id")
                ON DELETE CASCADE,

            CONSTRAINT "FK_CampaignMembers_Users"
                FOREIGN KEY ("UserId")
                REFERENCES "Users" ("Id")
                ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS
            "IX_CampaignMembers_UserId"
        ON "CampaignMembers" ("UserId");

        CREATE INDEX IF NOT EXISTS
            "IX_CampaignMembers_CampaignId"
        ON "CampaignMembers" ("CampaignId");
        """
    );
}


app.MapPost(
    "/account/register",
    async (
        HttpContext httpContext,
        IAntiforgery antiforgery,
        IDbContextFactory<
            CharacterChroniclesDbContext>
            dbFactory) =>
    {
        await antiforgery
            .ValidateRequestAsync(
                httpContext);


        var form =
            await httpContext.Request
                .ReadFormAsync();


        var displayName =
            form["displayName"]
                .ToString()
                .Trim();


        var email =
            form["email"]
                .ToString()
                .Trim();


        var password =
            form["password"]
                .ToString();


        var acceptedTerms =
            string.Equals(
                form["terms"].ToString(),
                "on",
                StringComparison.OrdinalIgnoreCase);


        var returnUrl =
            SafeReturnUrl(
                form["returnUrl"].ToString(),
                "/account");


        if (string.IsNullOrWhiteSpace(
                displayName) ||
            string.IsNullOrWhiteSpace(
                email) ||
            string.IsNullOrWhiteSpace(
                password))
        {
            return Results.Redirect(
                BuildAuthErrorUrl(
                    "/register",
                    "Display name, email, and password are required.",
                    returnUrl));
        }


        if (password.Length < 8)
        {
            return Results.Redirect(
                BuildAuthErrorUrl(
                    "/register",
                    "Password must be at least 8 characters.",
                    returnUrl));
        }


        if (!acceptedTerms)
        {
            return Results.Redirect(
                BuildAuthErrorUrl(
                    "/register",
                    "Please accept the terms before creating your account.",
                    returnUrl));
        }


        var normalizedEmail =
            email.ToUpperInvariant();


        await using var db =
            await dbFactory.CreateDbContextAsync();


        var exists =
            await db.Users
                .AsNoTracking()
                .AnyAsync(
                    user =>
                        user.NormalizedEmail ==
                        normalizedEmail);


        if (exists)
        {
            return Results.Redirect(
                BuildAuthErrorUrl(
                    "/register",
                    "An account already exists for that email address.",
                    returnUrl));
        }


        var friendCode =
            await CreateUniqueFriendCodeAsync(
                db);


        var user =
            new User
            {
                Id =
                    Guid.NewGuid()
                        .ToString("N"),

                DisplayName =
                    displayName,

                Email =
                    email,

                NormalizedEmail =
                    normalizedEmail,

                PasswordHash =
                    PasswordService.Hash(
                        password),

                FriendCode =
                    friendCode,

                CreatedAt =
                    DateTime.UtcNow
            };


        db.Users.Add(user);

        await db.SaveChangesAsync();


        await httpContext.SignInAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme,
            CreatePrincipal(user),
            new AuthenticationProperties
            {
                IsPersistent = false
            });


        return Results.Redirect(
            returnUrl);
    });


app.MapPost(
    "/account/login",
    async (
        HttpContext httpContext,
        IAntiforgery antiforgery,
        IDbContextFactory<
            CharacterChroniclesDbContext>
            dbFactory) =>
    {
        await antiforgery
            .ValidateRequestAsync(
                httpContext);


        var form =
            await httpContext.Request
                .ReadFormAsync();


        var email =
            form["email"]
                .ToString()
                .Trim();


        var password =
            form["password"]
                .ToString();


        var rememberMe =
            string.Equals(
                form["remember"].ToString(),
                "on",
                StringComparison.OrdinalIgnoreCase);


        var returnUrl =
            SafeReturnUrl(
                form["returnUrl"].ToString(),
                "/account");


        var normalizedEmail =
            email.ToUpperInvariant();


        await using var db =
            await dbFactory.CreateDbContextAsync();


        var user =
            await db.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    user =>
                        user.NormalizedEmail ==
                        normalizedEmail);


        if (user is null ||
            !PasswordService.Verify(
                password,
                user.PasswordHash))
        {
            return Results.Redirect(
                BuildAuthErrorUrl(
                    "/login",
                    "The email address or password is incorrect.",
                    returnUrl));
        }


        await httpContext.SignInAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme,
            CreatePrincipal(user),
            new AuthenticationProperties
            {
                IsPersistent =
                    rememberMe,

                ExpiresUtc =
                    rememberMe
                        ? DateTimeOffset.UtcNow
                            .AddDays(14)
                        : null
            });


        return Results.Redirect(
            returnUrl);
    });


app.MapPost(
    "/account/profile",
    async (
        HttpContext httpContext,
        IAntiforgery antiforgery,
        IDbContextFactory<
            CharacterChroniclesDbContext>
            dbFactory) =>
    {
        await antiforgery
            .ValidateRequestAsync(
                httpContext);


        var userId =
            httpContext.User
                .FindFirstValue(
                    ClaimTypes.NameIdentifier);


        if (string.IsNullOrWhiteSpace(
                userId))
        {
            return Results.Redirect(
                "/login?returnUrl=%2Faccount");
        }


        var form =
            await httpContext.Request
                .ReadFormAsync();


        var displayName =
            form["displayName"]
                .ToString()
                .Trim();


        var email =
            form["email"]
                .ToString()
                .Trim();


        var newPassword =
            form["newPassword"]
                .ToString();


        if (string.IsNullOrWhiteSpace(
                displayName) ||
            string.IsNullOrWhiteSpace(
                email))
        {
            return Results.Redirect(
                BuildAccountMessageUrl(
                    "error",
                    "Display name and email are required."));
        }


        if (!string.IsNullOrWhiteSpace(
                newPassword) &&
            newPassword.Length < 8)
        {
            return Results.Redirect(
                BuildAccountMessageUrl(
                    "error",
                    "New passwords must be at least 8 characters."));
        }


        var normalizedEmail =
            email.ToUpperInvariant();


        await using var db =
            await dbFactory.CreateDbContextAsync();


        var user =
            await db.Users.SingleOrDefaultAsync(
                user =>
                    user.Id == userId);


        if (user is null)
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            return Results.Redirect(
                "/login");
        }


        var emailUsed =
            await db.Users
                .AsNoTracking()
                .AnyAsync(
                    other =>
                        other.Id != userId &&
                        other.NormalizedEmail ==
                            normalizedEmail);


        if (emailUsed)
        {
            return Results.Redirect(
                BuildAccountMessageUrl(
                    "error",
                    "That email address is already being used by another account."));
        }


        user.DisplayName =
            displayName;

        user.Email =
            email;

        user.NormalizedEmail =
            normalizedEmail;


        if (!string.IsNullOrWhiteSpace(
                newPassword))
        {
            user.PasswordHash =
                PasswordService.Hash(
                    newPassword);
        }


        await db.SaveChangesAsync();


        var existingAuthentication =
            await httpContext.AuthenticateAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);


        await httpContext.SignInAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme,
            CreatePrincipal(user),
            existingAuthentication.Properties ??
                new AuthenticationProperties());


        return Results.Redirect(
            BuildAccountMessageUrl(
                "success",
                "Your account details have been updated."));
    });


app.MapPost(
    "/account/friends/add",
    async (
        HttpContext httpContext,
        IAntiforgery antiforgery,
        IDbContextFactory<
            CharacterChroniclesDbContext>
            dbFactory) =>
    {
        await antiforgery
            .ValidateRequestAsync(
                httpContext);


        var userId =
            httpContext.User.FindFirstValue(
                ClaimTypes.NameIdentifier);


        if (string.IsNullOrWhiteSpace(
                userId))
        {
            return Results.Redirect(
                "/login?returnUrl=%2Faccount");
        }


        var form =
            await httpContext.Request
                .ReadFormAsync();


        var friendCode =
            form["friendCode"]
                .ToString()
                .Trim()
                .ToUpperInvariant();


        if (string.IsNullOrWhiteSpace(
                friendCode))
        {
            return Results.Redirect(
                BuildAccountMessageUrl(
                    "friendError",
                    "Enter a friend code."));
        }


        await using var db =
            await dbFactory.CreateDbContextAsync();


        var friend =
            await db.Users.SingleOrDefaultAsync(
                user =>
                    user.FriendCode ==
                    friendCode);


        if (friend is null)
        {
            return Results.Redirect(
                BuildAccountMessageUrl(
                    "friendError",
                    "No user was found with that friend code."));
        }


        if (friend.Id == userId)
        {
            return Results.Redirect(
                BuildAccountMessageUrl(
                    "friendError",
                    "You cannot add yourself as a friend."));
        }


        var alreadyFriends =
            await db.Friendships
                .AsNoTracking()
                .AnyAsync(
                    friendship =>
                        friendship.UserId ==
                            userId &&
                        friendship.FriendUserId ==
                            friend.Id);


        if (alreadyFriends)
        {
            return Results.Redirect(
                BuildAccountMessageUrl(
                    "friendError",
                    $"{friend.DisplayName} is already in your friends list."));
        }


        var createdAt =
            DateTime.UtcNow;


        db.Friendships.Add(
            new Friendship
            {
                UserId =
                    userId,

                FriendUserId =
                    friend.Id,

                CreatedAt =
                    createdAt
            });


        db.Friendships.Add(
            new Friendship
            {
                UserId =
                    friend.Id,

                FriendUserId =
                    userId,

                CreatedAt =
                    createdAt
            });


        await db.SaveChangesAsync();


        return Results.Redirect(
            BuildAccountMessageUrl(
                "friendSuccess",
                $"{friend.DisplayName} has been added to your friends."));
    });


app.MapPost(
    "/account/logout",
    async (
        HttpContext httpContext,
        IAntiforgery antiforgery) =>
    {
        await antiforgery
            .ValidateRequestAsync(
                httpContext);


        await httpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);


        return Results.Redirect("/");
    });


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();