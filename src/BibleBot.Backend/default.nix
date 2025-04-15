{
  lib,
  stdenv,
  buildDotnetModule,
  fetchFromGitLab,
  dotnetCorePackages,
}:

buildDotnetModule (finalAttrs: {
  pname = "biblebot-backend";
  version = "9.5.0";

  src = fetchFromGitLab {
    owner = "kerygmadigital/biblebot";
    repo = "BibleBot";
    tag = "v${finalAttrs.version}";
    hash = "sha256-bxY190G12djkyfprrNt83+qzya44fnYV6Ij7D8SWelQ=";
  };

  projectFile = "src/BibleBot.Backend/BibleBot.Backend.csproj";
  nugetDeps = "src/BibleBot.Backend/deps.json";

  dotnet-sdk = dotnetCorePackages.sdk_9_0-bin;
  dotnet-runtime = dotnetCorePackages.aspnetcore_9_0-bin;

  buildType = "Release";

})
