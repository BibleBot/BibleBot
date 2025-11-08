## BibleBot — Copilot / AI agent instructions

Short, focused guidance for AI contributors working in this repository. Use this as a quick reference to be productive without guessing project conventions.

-   Repository layout: a monorepo under `src/` with the main components:

    -   `BibleBot.Backend` — ASP.NET Core Web API (primary logic, controllers, DI, providers).
    -   `BibleBot.AutomaticServices` — trimmed-down backend that runs scheduled/background tasks.
    -   `BibleBot.Frontend` — Python Discord bot (disnake) that talks to the backend.
    -   `BibleBot.Models` / `BibleBot.DataFormats` — shared .NET model libraries.

-   Quick build/run hints (dev):

    -   Backend: use `dotnet build` or `dotnet run` for `src/BibleBot.Backend`. Tasks exist in workspace for `build-backend` and `watch-backend`.
    -   AutomaticServices: same as backend but uses `src/BibleBot.AutomaticServices` (see `watch-autoserv` task).
    -   Frontend bot: Python, requirements in `src/BibleBot.Frontend/requirements.txt`. The bot is `application.py`.
    -   Tests: look in `test/` (e.g. `test/BibleBot.Tests.Backend`) and run `dotnet test` at the solution level.

-   Important runtime/config conventions:

    -   appsettings.json and appsettings.Development.json live alongside each .NET project (`src/*/appsettings*.json`). Prefer environment variables for secrets.
    -   Redis default in code: `127.0.0.1:6379` (see `Startup.cs`); production may override via environment or infrastructure.
    -   Database: Postgres connection is read from `POSTGRES_CONN` env var in `Startup.cs`.
    -   Sentry: `SENTRY_DSN` environment variable is used in `Program.cs`.
    -   Services expect Mongo and Postgres availability; running locally often requires those services or test stubs.

-   Patterns and conventions to follow when editing C# code:

    -   Dependency registration happens in `src/BibleBot.Backend/Startup.cs` (AddSingleton/AddDbContextPool/etc.). When adding a provider/service:
        1. Implement the service class under `Services/` (or `Services/Providers` for content/metadata providers).
        2. Register it in `Startup.ConfigureServices` and, if appropriate, add it to the `List<IContentProvider>` registration.
        3. Inject via constructor where needed (controllers, other services).
    -   Controllers live in `Controllers/` and use typical ASP.NET routing. Add unit tests under `test/` when behavior is non-trivial.
    -   Localization: resources live under `Resources/` and cultures are configured in `Startup.cs` (see `RequestLocalizationOptions`).
    -   Background/hosted services: conditional registration (e.g., `SystemdWatchdogService`) is used for platform-specific behavior.

-   Observability and telemetry:

    -   OpenTelemetry is configured in `Startup.cs` (tracing + metrics + Prometheus exporter).
    -   Serilog is configured in `Program.cs` and used for request logging.

-   Examples to cite when making changes:

    -   Adding a content provider: inspect `BibleBot.Backend/Services/Providers/*` and follow their registration in `Startup.cs`.
    -   Fetching metadata/version: see `MetadataFetchingService` and `VersionService` usage in `Startup.Configure`.

-   What not to change without explicit verification:

    -   Global registration ordering in `Startup.ConfigureServices` and `Configure` (middleware ordering matters).
    -   Hard-coded Redis/Postgres settings unless you confirm local dev overrides or CI configs.

-   Files to inspect for clarifying intent or examples:
    -   `src/BibleBot.Backend/Startup.cs` (DI, middleware, telemetry, localization)
    -   `src/BibleBot.Backend/Program.cs` (logging, Sentry, host configuration)
    -   `src/BibleBot.Backend/Services/` and `Services/Providers/` (how providers are implemented)
    -   `src/BibleBot.Frontend/application.py` (how the bot calls backend endpoints)
    -   `test/` (unit/integration test patterns)

If anything below is unclear or you want conventions extended (commit message style, branching, PR checklists), tell me which area and I will update this file with concrete examples.
