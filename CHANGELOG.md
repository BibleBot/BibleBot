# Changelog

All notable changes to this project will be documented in this file.

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
