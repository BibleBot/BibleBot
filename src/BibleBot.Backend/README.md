<p align="center">
    <a alt="Backend" href="https://biblebot.xyz">
        <img alt="Backend" width="400px" src="https://i.imgur.com/JVBY24z.png">
    </a>
</p>
<p align="center">
    The Backend API for BibleBot.
</p>

## Internal Organization

This project derives from the ASP.NET Web API template, so you'll find some of its boilerplate (particularly in `Program.cs` and `Startup.cs`). Otherwise, the structure is straightforward from there if you're familiar with the MVC model.

- `Controllers`, well, contains controllers.
- `Data` contains any data files that the backend relies upon that isn't worthy of being put into the database, so things like book names and USX files reside here.
- `Properties` contains files needed for the `dotnet` CLI and configuration files for the backend itself.
- `Services` is where all the heavy lifting is done for each facet of the backend - preferences, message parsing, name fetching, etc. Inside is a `Providers` directory that links to sources like BibleGateway, API.Bible, etc.

On the side, `.vscode` was generated likely by the C# extension for VSCode and is probably useful for anyone else using the editor. Nothing in there is custom, so it's presumed that it's useful.

## Prerequisites

- .NET Core 5.0 (SDK/Runtime)
- ASP.NET Core 5.0 (Runtime)
- Docker

## Self-Host Setup
This is from the old Golang backend which we'll derive from later. **This does not work.**

```bash
git clone https://github.com/BibleBot/backend && cd backend
cp config.example.yml && $EDITOR config.yml

# build production container
# the build-arg is optional if you're wanting localhost *without* HTTPS
docker build --build-arg DOMAIN=<domain> -t backend .
docker run -dp 443:443 backend
```