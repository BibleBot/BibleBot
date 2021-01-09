# v9 Usage Documentation

*This documentation is written in English. If you are using BibleBot in another language, this will not function as expected.*

**This documentation only mentions the breaking changes between v8 and v9, assume everything not mentioned works like normal.**

**Additionally, all existing v8 preferences have been imported to v9's system. You do not need to reconfigure old settings.**

**BibleBot will no longer work in DMs. We are looking to resolve this in the future.**

## Changes

All group commands (except `+dailyverse`) will print the subcommands alongside any other preferences by default.

* `+version` is the group for all version-related commands. As such:
  - `+setversion` is now `+version set`
  - `+setguildversion` is now `+version setserver`
  - `+versions` is now `+version list`
  - `+versioninfo` is now `+version info`

* `+truerandom` is a new feature to obtain a truly random verse, as opposed to `+random` which will produce more meaningful verses.

* `+verseoftheday` (`+votd`) is now `+dailyverse`. It contains subcommands to setup automation:
  - `+setvotdtime` is now `+dailyverse setup`
  - `+clearvotdtime` is now `+dailyverse clear`
  - `+votdtime` is now `+dailyverse status`

* `+formatting` contains all the commands related to verse formatting, by default it will print all related preferences and subcommands.
  - `+setheadings` is now `+formatting setheadings`
  - `+setversenumbers` is now `+formatting setversenumbers`
  - `+setmode` is now `+formatting setdisplay`

* `+language` is the group for all language-related commands. As such:
  - `+setlanguage` is now `+language set`
  - `+setguildlanguage` is now `+language setserver`
  - `+languages` is now `+language list`


## Merges

* `+version` and `+guildversion` are now `+version`
* `+language` and `+guildlanguage` are now `+language`
* `+servers` and `+users` have been merged into a `+stats` command

## Disables/Removals

* `+supporters` has been disabled temporarily
* `+setannouncements` and `+announcements` have been disabled temporarily
* `+catechisms` and related commands have been disabled temporarily
* BibleHub versions are still disabled temporarily.
* The ELXX and LXX versions are disabled until a better source can be found.