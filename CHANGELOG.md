# Changelog

All notable changes to this project will be documented in this file.

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [unreleased]

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
