import * as PouchDB from 'pouchdb';

import { log } from '../helpers/logger';

import Version from '../models/version';

import * as oldVersions from './old_databases/versiondb.json';

const db = new PouchDB('db');
db.sync('http://localhost:5984/db', { live: true }).on('error', (err) => {
    log('err', 0, 'couldn\'t sync to remote db');
    console.log(err);
});

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

            const newVersion = new Version(oldVersion['name'], oldVersion['abbv'], source, oldVersion['hasOT'], oldVersion['hasNT'], oldVersion['hasDEU']);

            db.put({
                _id: `version:${newVersion.abbreviation()}`,
                data: newVersion
            }).then(() => {
                log('info', 0, `added ${newVersion.abbreviation()}`);
            }).catch((err) => {
                log('err', 0, `unable to insert ${newVersion.abbreviation()}`);
                log('err', 0, err);
            });
        }
    }
};

importOldVersions();