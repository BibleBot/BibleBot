import Language from '../models/language';
import Version from '../models/version';

import * as mongoose from 'mongoose';

import { log } from '../helpers/logger';

import { readFileSync } from 'fs';

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

const importLanguages = () => {
    const _default = JSON.parse(readFileSync(`${__dirname}/../../../i18n/default/default.json`, 'utf-8'));

    const def = new Language({
        name: 'Default',
        objectName: 'default',
        rawObject: _default,
        defaultVersion: Version.find({ abbv: 'RSV' }).exec()
    });

    def.save((err, language) => {
        if (err) {
            log('err', 0, `unable to save ${def['name']}`);
            log('err', 0, err);
        } else {
            log('info', 0, `saved ${language.name}`);
        }
    });
};

importLanguages();