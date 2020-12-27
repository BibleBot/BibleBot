import * as ini from 'ini';
import * as fs from 'fs';

import axios from 'axios';
import { JSDOM } from 'jsdom';

import { purifyVerseText } from '../helpers/text_purification';
import Reference from '../models/reference';
import Verse from '../models/verse';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

const versionTable = {
    KJVA: 'de4e12af7f28f599-02',
    FBV: '65eec8e0b60e656b-01'
};

export function getResult(ref: Reference, headings: boolean, verseNumbers: boolean, 
    callback: (err: Error, data: Verse) => void): void {
        axios.get(`https://api.scripture.api.bible/v1/bibles/${versionTable[ref.version]}/search`, {
            headers: { 'api-key': config.apis.apiBible },
            params: { query: ref.toString(), limit: 1 }
        }).then((res) => {
            try {
                const data = res.data.data.passages;
                let text;

                if (data.length == 0) {
                    return;
                }

                if (data[0].bibleId != versionTable[ref.version]) {
                    console.error(`${ref.version} is no longer able to be used.`);
                    return;
                }

                if (data[0].content.length > 0) {
                    text = data[0].content;
                } else {
                    return;
                }

                const { document } = (new JSDOM(text)).window;

                Array.from(document.getElementsByClassName('v')).forEach((el: Element) => {
                    el.textContent = `<**${el.textContent.slice(0, -1)}**> `;
                });

                const title = Array.from(document.getElementsByTagName('h3')).map((el: Element) => el.textContent.trim()).join(' / ');
                text = Array.from(document.getElementsByClassName('p')).map((el: Element) => el.textContent.trim()).join('\n');

                return callback(null, new Verse(
                    ref.version.name(),
                    title,
                    purifyVerseText(text),
                    ref
                ));
            } catch (err) {
                return callback(err, null);
            }
        });
}