# Changelog

All notable changes to this project will be documented in this file.

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [12.0.0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/compare/v11.2.0..v12.0.0) - 2025-07-26

### 🚀 Features

- Support common dash variants in references by normalizing them ([1411463](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/141146351e1c02f44abbea3726b0bdecf8abb441))
- Function for converting Hebrew-numbered Psalm references to the Septuagint-numbered equivalent ([b583ff0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b583ff0d1f2bf3d9fb381a93f2ec7c27d19a919b))
- Establish indexes for the larger-used collections. ([8e4485b](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8e4485b8c4a92ca85e4062c2cffae337ff4eba58))
- Add healthcheck endpoint ([fe6b0a0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/fe6b0a0468c5e80e39f37f55af644caab4c1741d))
- Record status codes for daily verses ([8e21c37](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8e21c37f40ec5d1da23dce350d3a3290d4e9379b))
- *(autoserv)* Add failed webhook calls to a queue to be retried ([b3f37a8](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b3f37a8af39c45b231a9f43c5c92d0c822355f85))
- *(autoserv)* Run daily verses in parallel ([d2eff90](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/d2eff90805fcdcafb1bcf5389f486aee29d130e3))
- *(autoserv)* Remove daily verse preferences when webhook returns 404 ([bb93321](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/bb93321c5ce33716408cea16e19a4923db2852d9))
- Add /cleardailyverserole ([e263a6e](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/e263a6e27aba348cd7d1a806043c2ccdc5e58512))

### 🐛 Bug Fixes

- Use database count for GetCount() implementations, make sure user prefs aren't cached by autoserv ([4f754b1](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/4f754b185b84b459b905a188d3bb17423f22e155))
- ArgumentNullException for some search queries ([c0f9671](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/c0f96713529f5baaebc4ef5997092667e3c6d2f2))
- NullReferenceException when an invalid version is passed to /search ([8e4736f](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8e4736f4624346c6b7f8df086c0cbe2e06eeee51))
- ArgumentOutOfRangeException when a comma is on a reference that isn't looking for other verses ([b161177](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b161177475b637c85308de4f47a427922ae58858))
- Duplicate of LJE in default_names causing invalid parsing for some DEU book names ([3dbfce1](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/3dbfce13a51bf736a168dc089934cfaec37f776c))
- NullReferenceException caused by fallback version in /search not applying correctly ([1d020a9](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/1d020a9d187a1e0b9edf9242e8cbe437ee0602c3))
- Allow variant dashes in frontend ([3de7725](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/3de77254de2c375a4d4650270cf3cb77865d6eac))
- *(preferences)* Refactor preference cache/db logic, increase timeout and pool size for db ([8b6e0ed](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8b6e0ed77c40ebcd5e1027d2e2e256e260d984dc))
- *(autoserv)* Save VerseResult to avoid unnecessary outbound connections ([bf2f01e](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/bf2f01ee53181f7445be56decaca8d81a047d919))
- *(resources)* ArgumentOutOfRangeException when giving a non-existent paragraph number to a resource ([7a9db53](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/7a9db531d0b268e528aad41663effb9190c09511))
- *(resources)* Some formatting in creeds ([941deba](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/941deba0d716503c594c6897dc971f08fbca2700))
- [**breaking**] Remove OptOutUser in favor of UserPreference flag, rework MongoService.Get() ([b64e845](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b64e8455de0ed060d572b2cbbb8f7216c1c97bfb))
- Cache for null values ([c4d67d6](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/c4d67d60e7362b4271459b6fed0f2fbaf6e8c3d4))
- NullReferenceException for optoutuser check ([2a2f93e](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/2a2f93e2d3d8601c3fd6b24d9a13d998f173ddf7))
- *(autoserv)* Avoid requeuing failed daily verse sends ([ef27206](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/ef272068a7429ccd4f09c74afbfcde55bc2c04db))
- *(autoserv)* Log when failed webhook works on resend. ([039b87a](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/039b87ab7dfe46642c15a6868c3fdf3de99e6d72))
- *(autoserv)* Reposition log for failure queue addition ([1a23e86](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/1a23e86e9704a90cea0626bad8f38834e6e50ae9))
- *(autoserv)* Return count of passed daily verses ([9e3a8c2](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/9e3a8c288beb1b737eece0a896a38da715719808))

### 🚜 Refactor

- Add more sentry context ([b0f87ed](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b0f87eda4f86a6a55d75b2496d6b3f24de4c045b))
- Optimize verse generation code ([3dcb01a](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/3dcb01a15b26c37a328ccf52b3793e3f34953ea6))
- More optimizations ([0a768d8](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/0a768d897afe5729a5a88a1e48cb45c936d14b4d))

## [11.2.0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/compare/v11.1.0..v11.2.0) - 2025-05-31

### 🚀 Features

- Force pull guilds in autoserv ([c5bfb96](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/c5bfb969cc520b03c48a0939d4a8be7eb79d48f2))
- Foundation work on metrics ([a7ac2de](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/a7ac2dee8363088427bfb117fc92f55785ff56de))
- Sentry for error tracking ([040c510](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/040c5101fd1c400beb82c077621b4ba33e316c5a))

### 🐛 Bug Fixes

- Typecasting filtered versions ([0accf93](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/0accf937d6122d37784dea6949d93845b1b38288))
- Handle API.Bible's book name sensitivity ([b580b1d](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b580b1d060d52932b42b0b1d8ed4811b8fa9b086))
- *(metadata)* Use fallback book name if preferred name is an abbreviation ([525ed29](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/525ed298c9d8683990a4bae703c66044a4594d90))
- *(ab)* Return proper reference based on book data ([94c9d33](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/94c9d330c0bef393ec0618478995f62166e3123a))
- *(metadata)* Compare lowercase version names because NRSVue exists... ([b8a8343](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b8a83435693c66c440285a3ff5281c6d4da99957))
- *(truerandom)* Abbreviated book names not processing ([8fedb77](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8fedb77ebc356fe9c062183b9304303ba62ee717))
- *(guild)* Don't cache guilds when autoserv fetches all guilds ([9fd7946](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/9fd7946deb4eac03cc48c46aa97c7fd0404b0ccf))
- *(ab)* Handle InvalidOperationException for special verses ([1861ce6](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/1861ce6df525524cce8ce2ddb54edfc1907b95ac))
- *(ab)* Resolve import collisions ([5c56376](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/5c5637689c2d335c67f2807b0e3249c528a825eb))
- Use user display style in /truerandom ([a211354](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/a2113540470e3802cca94d3945a55cfaa962c35d))
- Move redirect for NRSV to NRSVA into the reference generation ([47b4f06](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/47b4f06bb03e6f31df2274da483317187286ab98))
- Catch various unknown exceptions with sentry ([28ed240](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/28ed240b74db385776c117e86917565a6459c533))
- Don't let dangling commas in verse reference prevent parsing otherwise ([0482aae](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/0482aaed3ea0529204489365c6253e08376253f9))

### 🚜 Refactor

- Remove useless code ([d357eac](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/d357eac6b899f6a9b1a2882a5c3f744cad6ba70e))
- Yes ([49de6d5](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/49de6d57b944cedcc946f86d4d5bd9e70b960ba7))
- Use correct keyword ([253487c](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/253487c77c3593ac535893f30de07f00df1688c7))
- Clarify error messages ([8dc2c66](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8dc2c66b44fb52d2eafd62918dfa7b3ed4480eb8))
- Light formatting ([00abd64](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/00abd64bf6fd6ac39a23ad718b2c5d35f9533f66))
- Leverage more sentry options ([fc67091](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/fc67091e1f3fba785a0cba5016e06190a4a77e65))
- Use proper version info for sentry where possible ([f93af69](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/f93af69769db2654978edab66313d4c7bcada372))

## [11.1.0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/compare/v11.0.1..v11.1.0) - 2025-05-18

### 🚀 Features

- Pull DB data on startup, use in MetadataFetchingService ([42dc57d](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/42dc57ddb5b95f4c5617d76d15f1e7c7bb2407de))
- Use redis cache for DB information ([1797042](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/1797042514e6d624c696ac88a3025ca422c1972a))

### 🐛 Bug Fixes

- Parameters for MetadataFetchingService in AutoServ's startup sequence ([8ba2ba1](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8ba2ba1ed6523a441afe9b14c14f8822322b7057))
- Don't use redis for version caching ([6e980ac](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/6e980ac97ba5d5073f03caa25833229e0bd0a69b))
- Versionservice construction ([c3d451f](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/c3d451f9b3304dd4847742459d81a2baef1c1e0f))

### 🚜 Refactor

- Fetch automatic daily verse reference before iterating through guilds ([b0d4b68](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b0d4b685d7fbf5db113af4456da7e3e9c92e0623))
- Remove unused variable ([4f68c78](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/4f68c783f3090aabfab768d9462d1160bdd8f418))
- Restore version cache prefetch ([fb7e95c](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/fb7e95c2a4d35b1e0e5489241e0ecf30009aeb7c))

## [11.0.1](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/compare/v11.0.0..v11.0.1) - 2025-05-17

### 🐛 Bug Fixes

- Handle InformationCommandGroup calls properly ([ce7ae05](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/ce7ae05c2a20534cf52844907df7822742eb522a))
- Return null if entry in DB doesn't truly exist ([71bed44](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/71bed4467340e6ca31816fc2b66707828ba880ed))
- Force pull guilds for Automatic Daily Verses ([36b058a](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/36b058ac19124577e11d99d3b7c9b78217944e7b))
- Allow lowercase abbreviations for version references, refactor slightly ([847bef1](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/847bef11963b5506f0fbe0f13a96ef6b41d5dc17))
- Server's display style in /formatting accidentally used user's display style ([91bffb3](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/91bffb3d5b7ac234e114cf1f64c93dfd775182f3))
- Missing localizations ([c2aea5d](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/c2aea5d502498d7f857e465918b8e2b76588d136))
- Optimize DB updating functions ([8b83c05](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8b83c056851f275a0639fca40caa84cba2f028d0))
- Avoid issues with guild enumerable in AutomaticDailyVerseService, handle non-JSON responses in JSON caching client ([381a04f](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/381a04f3bfc71ccc76210f143c82801bcfbe8a84))
- Add John to overlapping book names list ([9944680](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/994468042b75b1663dabfe677ad9f2c63a1553fd))
- Truncate verse titles when length exceeds limit ([e12a05d](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/e12a05d7d2f14594a0ab08e31e2e8febbba993e9))

### 🚜 Refactor

- Optimize version search in reference ([609d151](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/609d151236c4279718bf4ad7944dc9baf848f4d1))
- Cache DB tables locally, fetch when necessary ([8cb2d36](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8cb2d36371ac6d0863cee549e60e9dac4efec5db))

## [11.0.0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/compare/v10.0.0..v11.0.0) - 2025-05-16

### 🚀 Features

- Fetch section title data from API.Bible ([04ca6cb](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/04ca6cb77a03019999134d66a25f1d51a9cf34d0))
- [**breaking**] Use API.Bible data names as default internal names. ([5c20755](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/5c20755f7385129f4fe944b6ea857f23f4d9b384))
- Add locale field to versions ([9663575](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/966357596afcc6f5d4ec168606a120a5844cb1b3))

### 🐛 Bug Fixes

- Use internal book information for finding preferred book name of an AB verse ([1fab847](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/1fab8479bcdcfdc36a6fbf65dbd35f4051bb5b35))
- Remove redundant fetch for internal book information ([b81ede8](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b81ede8ef0df75f49ce49c58398e599d0954ad8f))

## [10.0.0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/compare/v9.5.0..v10.0.0) - 2025-05-03

### 🚀 Features

- Log when at various points of automatic daily verse sending ([5071c32](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/5071c323d53b88b17d27c16ed710253e0ff08f41))
- Store book names for API.Bible versions ([f3e412b](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/f3e412b736cc39b1c2a492a7963671d5365fde3f))
- Store the source's internal name for a book in version data, rename some variables ([4eb3f32](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/4eb3f3290191383036045d15c01cab49f9f14443))
- Use stored API.Bible book names where applicable ([e2413ca](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/e2413ca031442a50c18fbc1b75df7e6fe523b319))
- Pre-pad chapters in books in API.Bible versions ([e1ffe38](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/e1ffe38e2ea89c76dff2eebfd29957772fe52283))
- [**breaking**] Rename Version.ApiBibleId to Version.InternalId, add an Active flag to versions to be used later, import BG metadata on first run instead of always fetching on boot ([d8a75c6](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/d8a75c6bae0e62ca551f8984b97a0f7772ff889b))

### 🐛 Bug Fixes

- Use GitInfo data for version number reporting ([67db5d4](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/67db5d42673ca93020880ebee83b1d38f35b9684))
- Remove the stupid semicolon from the version number ([c6e0eac](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/c6e0eac26ec8ae938057c07fe117cb4202207604))
- Use GitInfo version in backend ([3f1d0ea](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/3f1d0ea5c5224a25523eae0eb5288147eaae4d03))
- Add fallback for missing GitInfo ([1b3198e](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/1b3198e933a5ccca166e83118ab75a05e6ed51f2))
- Reference inconsistencies on verse output ([a760594](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/a760594402b0ea72f3d0375982a3e90d9f275a45))
- *(frontend)* Parse newlines from backend properly ([06412d1](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/06412d18193a7440b9f98831e762091332898f19))
- *(frontend)* Make /booklist acronym parameter optional ([cd33f03](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/cd33f037d779d744fc5f0364df9a01d4f0a89524))
- *(bg)* Check for null before trying to fix verse number ([71f4e80](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/71f4e809de5361de98f6ac254d78a3c1e627a482))
- [**breaking**] Rename Models.Verse to Models.VerseResult to avoid conflicts ([6c09895](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/6c09895a09a2420029605ed5d86ef12369864bcc))

### 🚜 Refactor

- Remove redundant Linux check for watchdog ([8be7ce9](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/8be7ce9056d23a6556d3871434c2b20f38fbfd03))
- Begin reorganizing commands to reflect frontend structure ([7d6a265](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/7d6a265b4943e4ccd7507fbdfe3fd5980d4eb9ca))
- Remove isISV check for NLT provider's PurifyText(), add test for NT book in OT-only version ([e153024](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/e153024e8e7f8a0d50ba35e2c481ba9b3ee507c4))
- Remove unused configuration variables, clean up command controller ([3796105](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/379610599bfad8d1fe05e2ab9bd30ef01fdb5a18))
- Alias the various dictionaries used in metadata fetching for code readability ([c7b7664](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/c7b7664452cdf919eb22b71fb2ee647219ea6abe))
- Change how metadata dictionaries are aliased, restructure providers to allow for metadata providers ([870ceec](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/870ceecd5f62e6df24cf08ca44a5307165388eaa))

## [9.5.0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/compare/v9.4.0..v9.5.0) - 2025-03-22

### 🚀 Features

- Support @everyone for setdailyverserole, add confirmation prompt when doing so ([2214153](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/221415300e0703ecc83a00262c8d6c7241b9c80a))

### 🐛 Bug Fixes

- Get the correct version information when running development versions ([b95c347](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/b95c347c80a8820c0556bf9d689e2917923dcd10))

## [9.4.0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/compare/v9.3.0..v9.4.0) - 2025-03-05

### 🚀 Features

- *(verses)* Implement NLT API provider ([ac61ffb](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/ac61ffb3e8631372f38a370162542e761380af9f))
- Register support for Polish ([49b9166](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/49b9166d1300d650bb8635a8b99e82c178c42bf4))

### 🐛 Bug Fixes

- Add en-GB to /setlanguage options ([3ca7a0e](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/3ca7a0ec990a9645ca52d84bf4f5672538486205))
- Resolve broken typecasting in MongoService.Search(), fetch book names from API.Bible versions we have ([a4f1ddb](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/a4f1ddb52c3d88167aeb2d23b461f99fed9848f6))
- *(ab)* Sometimes data-sid is not used for verse numbers ([9aa7b8d](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/9aa7b8d9748dbccee2113f626126a4510b6d0c09))
- Avoid errors because of duplicate versions in DB ([c8b8fde](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/c8b8fde234b75ce7f796004c5ea9ca1ed87aa8e6))
- Patch content error in Galatians 3:24 NLD1939 ([75bd00a](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/75bd00aeec944238046da42c7705faa9af2cca3f))
- Patch previous patch ([56a4163](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/56a416365bbb0dc84c1386554459b111dc25b451))

## [9.3.0](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/compare/v9.2-beta..v9.3.0) - 2025-02-23

### 🚀 Features

- Automatically derive version using easybuild tooling ([f155ee5](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/f155ee5108de43a6815b1fe0a05594fe9c9901d0))
- Wire up frontend localizations, pass backend localizations to frontend where needed ([5b6c989](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/5b6c989855475368aa7d2d0fd875dc09c884aca9))

### 🐛 Bug Fixes

- *(backend)* Parse in-reference versions properly ([0f9d583](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/0f9d583535d9b519c385b800a8309ebb070d6799))
- *(backend)* Fix unnecessary page creation in /listversions ([2ba83e7](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/2ba83e7a4dcd986e54ddaa0533e9e56156a43400))
- Handle unauthorized from API.Bible in NameFetchingService ([fe8ed7d](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/fe8ed7d27313a011d0fa86cf1ab24a78688fb5b8))
- Missed a spot for 401 catching ([a67d914](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/a67d9146dd1fa85c9ce5d5225a7262a3cb2524bd))
- I found why this watchdog thing isn't working ([cf66539](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/cf6653968325e9d6dd5cdd388741525c996d6896))
- Ignore API.Bible for NameFetching until better solution can be found ([22fe26c](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/22fe26cb2f900c550f711e3e2c21f40bc410a443))
- Sometimes python fstrings are unintuitive ([713016a](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/713016a71aabafeb8b80bb1d64c0dfe1811df27b))
- Ditto ([912e915](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/912e915ca7981c8e4835d969e90b02b3bd5dc34c))
- Return culture with staff-only command error ([53dc87e](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/53dc87e840d52105cf6a92d810fbbccf77846909))
- Add missing fallbacks if culture is null ([9e68842](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/9e6884243020a15ef84ab87a56be800d3e4c4269))
- Missed a spot ([dcb64a1](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/dcb64a10578f34e08b39afb651cae80a10241adb))

### 🚜 Refactor

- *(backend)* Make a function out of getting preferred versions ([d7638d2](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/d7638d2d5b4148dbcb2b43acc4790fc8a658fb05))

### 📚 Documentation

- Begin preparing CHANGELOG.md for 9.3.0 ([9cbe8e7](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/9cbe8e75045b0f0506a0259050e297439aeae5c8))
- Update changelog for release ([245d1cf](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/245d1cf9389d65d1a47ca27457081ec004c239c0))
- Update changelog [ci skip] ([2767998](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/27679985beb4a0b21884aa01bf4ba65c00acb0ea))
- Change 9.3 release date to planned release [ci skip] ([d26647b](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/d26647bb17b24f442ea91c2fe10bbace153297b8))
- Fix some formatting [ci skip] ([31df4a7](https://gitlab.com/kerygmadigital/BibleBot/BibleBot/-/commit/31df4a78e2590b118c1061469a8293bd2eb59de7))

<!-- generated by git-cliff -->
