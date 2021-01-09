import Language from '../models/language';

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

const defaultVersion = 'RSV';

const importLanguages = () => {
    const _english = JSON.parse(readFileSync(`${__dirname}/../../i18n/english/english.json`, 'utf-8'));

    Language.findOne({ objectName: 'english' }, (err, lang) => {
        if (!lang) {
            const eng = new Language({
                name: 'English',
                objectName: 'english',
                rawObject: _english,
                defaultVersion
            });

            def.save((err, language) => {
                if (err) {
                    log('err', 0, `unable to save ${eng['name']}`);
                    log('err', 0, err);
                } else {
                    log('info', 0, `saved ${language.name}`);
                }
            });
        } else {
            lang.rawObject = _english;

            lang.save((err, language) => {
                if (err) {
                    log('err', 0, `unable to overwrite ${lang['name']}`);
                    log('err', 0, err);
                } else {
                    log('info', 0, `overwritten ${language.name}`);
                }
            });
        }
    });

    
};

importLanguages();
setTimeout(() => { process.exit(0); }, 2000);