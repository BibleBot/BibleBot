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