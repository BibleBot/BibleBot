import * as ini from 'ini';
import * as fs from 'fs';

import axios from 'axios';
import { JSDOM } from 'jsdom';
import mongoose from 'mongoose';

import { purifyVerseText } from '../helpers/text_purification';
import Reference from '../models/reference';
import Verse from '../models/verse';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

const versionTable = {
    KJVA: 'de4e12af7f28f599-01',
    FBV: '65eec8e0b60e656b-01'
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export function search(query: string, version: mongoose.Document, callback: (err: Error, res: Array<Record<string, string>>) => void): void {
    // Temporary workaround to get the compiler to stop complaining in verse_utils.ts.
    return null;
}

export function getResult(ref: Reference | string, headings: boolean, verseNumbers: boolean, version: mongoose.Document,
    callback: (err: Error, data: Verse) => void): void {
        if (ref instanceof Reference) {
            version = ref.version;
        }

        axios.get(`https://api.scripture.api.bible/v1/bibles/${versionTable[version.abbv]}/search`, {
            headers: { 'api-key': config.apis.apiBible },
            params: { query: ref.toString(), limit: 1 }
        }).then((res) => {
            try {
                const data = res.data.data.passages;
                let text;

                if (data.length == 0) {
                    return;
                }

                if (data[0].bibleId != versionTable[version.abbv]) {
                    console.error(`${version.abbv} is no longer able to be used.`);
                    return;
                }

                if (data[0].content.length > 0) {
                    text = data[0].content;
                } else {
                    return;
                }

                const { document } = (new JSDOM(text)).window;

                Array.from(document.getElementsByClassName('v')).forEach((el: Element) => {
                    el.textContent = `<**${el.textContent}**> `;
                });

                const title = Array.from(document.getElementsByTagName('h3')).map((el: Element) => el.textContent.trim()).join(' / ');
                text = Array.from(document.getElementsByTagName('p')).map((el: Element) => el.textContent.trim()).join('\n');

                return callback(null, new Verse(
                    data[0].reference,
                    title,
                    purifyVerseText(text),
                    ref,
                    version
                ));
            } catch (err) {
                return callback(err, null);
            }
        });
}