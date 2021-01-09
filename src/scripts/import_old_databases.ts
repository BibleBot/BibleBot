import * as mongoose from 'mongoose';

import { log } from '../helpers/logger';

import Version from '../models/version';
import Preference from '../models/preference';
import GuildPreference from '../models/guild_preference';

import * as oldVersions from './old_databases/versiondb.json';
import * as oldPreferences from './old_databases/db.json';
import * as oldGuildPreferences from './old_databases/guilddb.json';

const connect = () => {
    mongoose.connect('mongodb://localhost:27017/db', { useNewUrlParser: true, useUnifiedTopology: true }).then(() => {
        log('info', 0, 'connected to db');

        importOldVersions();
        importOldPreferences();
        importOldGuildPreferences();
    }).catch((err) => {
        log('err', 0, `error connecting to database: ${err}`);
        return process.exit(1);
    });
};

connect();
mongoose.connection.on('disconnected', connect);

const importOldVersions = () => {
    for (const key in oldVersions) {
        if (Object.prototype.hasOwnProperty.call(oldVersions, key)) {
            const oldVersion = oldVersions[key];
            let source = 'bg';

            if (['BSB', 'NHEB', 'WBT'].includes(oldVersion['abbv'])) {
                // BibleHub Versions
                // source = 'bh';
                continue;
            } else if (['KJVA', 'FBV'].includes(oldVersion['abbv'])) {
                // API.Bible Versions
                source = 'ab';
            } else if (['ELXX', 'LXX'].includes(oldVersion['abbv'])) {
                continue;
            }

            if (oldVersion['abbv'] == 'NKJV') {
                // RIP NKJV - 2016 to 2020
                continue;
            }

            const newVersion = new Version({
                name: oldVersion['name'], 
                abbv: oldVersion['abbv'],
                src: source,
                supportsOldTestament: oldVersion['hasOT'],
                supportsNewTestament: oldVersion['hasNT'],
                supportsDeuterocanon: oldVersion['hasDEU']
            });

            newVersion.save((err, version) => {
                if (err) {
                    log('err', 0, `unable to save ${oldVersion['abbv']}`);
                    log('err', 0, err);

                    console.log(oldVersion);
                    console.log('----');
                    console.log(newVersion);
                } else {
                    log('info', 0, `saved version ${version.abbv}`);
                }
            });
        }
    }
};

const importOldPreferences = () => {
    for (const key in oldPreferences) {
        if (Object.prototype.hasOwnProperty.call(oldPreferences, key)) {
            const oldPreference = oldPreferences[key];

            if (oldPreference == null) {
                continue;
            }
            
            // example
            // {"id": 186046294286925824, "version": "RSV", "verseNumbers": "enable", "headings": "enable", "language": "english", "mode": "code"}

            const formatMap = {
                'enable': true,
                'disable': false
            };

            const newPreference = new Preference({
                user: oldPreference.id.toString(),
                input: 'default',
                language: 'english',
                version: oldPreference.version ? oldPreference.version : 'RSV',
                headings: oldPreference.headings ? formatMap[oldPreference.headings] : true,
                verseNumbers: oldPreference.verseNumbers ? formatMap[oldPreference.verseNumbers] : true,
                display: oldPreference.mode ? oldPreference.mode : 'default'
            });

            newPreference.save((err, pref) => {
                if (err) {
                    log('err', 0, `unable to save ${oldPreference.id}`);
                    log('err', 0, err);

                    console.log(oldPreference);
                    console.log('----');
                    console.log(newPreference);
                } else {
                    log('info', 0, `saved user ${pref.user}`);
                }
            });
        }
    }

    
    console.log('prefs done');
};

const importOldGuildPreferences = () => {
    for (const key in oldGuildPreferences) {
        if (Object.prototype.hasOwnProperty.call(oldPreferences, key)) {
            const oldGuildPreference = oldGuildPreferences[key];

            if (oldGuildPreference == null) {
                continue;
            }

            // example
            // {"id": 362503610006765568, "time": "13:00", "channel": 366888351326011394, "channel_name": "daily-verse"}

            const newGuildPreference = new GuildPreference({
                guild: oldGuildPreference.id.toString(),
                prefix: '+',
                ignoringBrackets: '<>',
                language: 'english',
                version: oldGuildPreference.version ? oldGuildPreference.version : 'RSV'
            });

            if (oldGuildPreference.channel && oldGuildPreference.time) {
                if (oldGuildPreference.time != 'clear') {
                    newGuildPreference.dailyVerseChannel = oldGuildPreference.channel.toString();
                    newGuildPreference.dailyVerseTime = oldGuildPreference.time;
                    newGuildPreference.dailyVerseTz = 'UTC';
                }
            }

            newGuildPreference.save((err, pref) => {
                if (err) {
                    log('err', 0, `unable to save ${oldGuildPreference.id}`);
                    log('err', 0, err);

                    console.log(oldGuildPreference);
                    console.log('----');
                    console.log(newGuildPreference);
                } else {
                    log('info', 0, `saved guild ${pref.guild}`);
                }
            });
        }
    }

    
    console.log('gprefs done');
};