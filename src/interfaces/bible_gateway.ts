import * as fetch from 'node-fetch';
import { JSDOM } from 'jsdom';
import { purifyVerseText } from '../helpers/text_purification';
import Reference from '../models/reference';
import Verse from '../models/verse';
import { VersionDocument } from '../models/version';

export function search(query: string, version: VersionDocument, callback: (err: Error, res: Array<Record<string,string>>) => void): void {
    query = escape(query);

    const url = `https://www.biblegateway.com/quicksearch/?search=${query}&version=${version.abbv}&searchtype=all&limit=50000&interface=print`;

    const results = [];

    fetch(url).then(async (res) => {
        try {
            const { document } = (new JSDOM(await res.text())).window;

            Array.from(document.getElementsByClassName('row')).forEach((row) => {
                const result = {};

                Array.from(row.getElementsByClassName('bible-item-extras')).forEach((el) => {
                    el.remove();
                });

                Array.from(row.getElementsByTagName('h3')).forEach((el) => {
                    el.remove();
                });

                result['title'] = row.getElementsByClassName('bible-item-title').item(0);
                result['text'] = row.getElementsByClassName('bible-item-text').item(0);


                if (result['title'] && result['title']) {
                    result['title'] = result['title'].textContent;
                    result['text'] = purifyVerseText(result['text'].textContent.slice(1, -1));

                    results.push(result);
                }
            });

            return callback(null, results);
        } catch (err) {
            return callback(err, null);
        }
    });
}

export function getResult(ref: Reference | string, headings: boolean, verseNumbers: boolean, version: VersionDocument, 
    callback: (err: Error, data: Verse) => void): void {
        if (ref instanceof Reference) {
            version = ref.version;
        }

        fetch(`https://www.biblegateway.com/passage/?search=${ref.toString()}&version=${version.abbv}&interface=print`).then(async (res) => {
            try {
                const { document } = (new JSDOM(await res.text())).window;

                const container = document.getElementsByClassName('passage-col')[0];

                Array.from(container.getElementsByClassName('chapternum')).forEach((el: Element) => {
                    if (verseNumbers) {
                        el.textContent = '<**1**> ';
                    } else {
                        el.remove();
                    }
                });

                Array.from(container.getElementsByClassName('versenum')).forEach((el: Element) => {
                    if (verseNumbers) {
                        el.textContent = `<**${el.textContent.slice(0, -1)}**> `;
                    } else {
                        el.remove();
                    }
                });

                Array.from(container.getElementsByTagName('br')).forEach((el: Element) => {
                    el.before(document.createTextNode('\n'));
                    el.remove();
                });

                Array.from(container.getElementsByClassName('crossreference')).forEach((el: Element) => {
                    el.remove();
                });

                Array.from(container.getElementsByClassName('footnote')).forEach((el: Element) => {
                    el.remove();
                });

                const title = headings ? Array.from(container.getElementsByTagName('h3')).map((el: Element) => el.textContent.trim()).join(' / ') : null;
                const text = Array.from(container.getElementsByTagName('p')).map((el: Element) => el.textContent.trim()).join('\n');

                return callback(null, new Verse(
                    `${document.getElementsByClassName('bcv')[0].textContent}`,
                    title,
                    purifyVerseText(text),
                    ref,
                    version
                ));
            } catch (err) {
                if (err.message !== 'Cannot read property \'getElementsByClassName\' of undefined') {
                    return callback(err, null);
                }
            }
        }).catch((err) => {
            return callback(err, null);
        });
}