import * as mongoose from 'mongoose';

import { log } from '../helpers/logger';

import Version from '../models/version';

import * as oldVersions from './old_databases/versiondb.json';

const connect = () => {
    mongoose.connect('mongodb://localhost:27017/db', { useNewUrlParser: true, useUnifiedTopology: true }).then(() => {
        return log('info', 0, 'connected to db');
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
                source = 'bh';
            } else if (['KJVA', 'FBV'].includes(oldVersion['abbv'])) {
                // API.Bible Versions
                source = 'ab';
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
                } else {
                    log('info', 0, `saved ${version.abbv}`);
                }
            });
        }
    }
};

importOldVersions();