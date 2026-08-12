# Database-backed authentication

Authentication now uses the PostgreSQL database configured by `DATABASE_URL` in `.env` directly through the existing EF Core `CharacterChroniclesDbContext`.

It does **not** use ASP.NET Identity/UserManager/SignInManager. The app has its own `Users` table with `Id`, `DisplayName`, `Email`, `NormalizedEmail`, `PasswordHash`, and `CreatedAt`.

Passwords are never stored as plain text. They are hashed with PBKDF2-SHA256 using a random salt and 210,000 iterations. A normal ASP.NET Core encrypted authentication cookie only keeps the login session after the database credentials have been checked.

On registration:
1. The app checks `Users` in the PostgreSQL database for the email.
2. It creates a custom user row in that database.
3. It signs the browser in with a cookie containing that user's database ID.

On login:
1. The app queries `Users` in the PostgreSQL database.
2. It verifies the supplied password against the stored hash.
3. It signs the browser in and exposes the database user ID as `ClaimTypes.NameIdentifier`.

Characters use that database user ID in `Character.UserId`. `/characters` filters by it and new characters store it automatically, so each user only sees and changes their own characters.

The `AddApplicationUsers` migration creates the custom `Users` table. `Program.cs` applies pending EF migrations at startup to the database configured by `.env`.

Older migrations may still contain the original `AspNet*` tables because they are historical migrations. The current authentication code does not read from or write to those tables.
