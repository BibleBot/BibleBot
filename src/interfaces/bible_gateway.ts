import axios from 'axios';
import { JSDOM } from 'jsdom';
import { purifyVerseText } from '../helpers/text_purification';
import Reference from '../models/reference';
import Verse from '../models/verse';

export function getResult(ref: Reference, headings: boolean, verseNumbers: boolean, 
    callback: (err: Error, data: Verse) => void): void {
        axios.get(`https://www.biblegateway.com/passage/?search=${ref.toString()}&version=${ref.version}&interface=print`).then((res) => {
            try {
                const { document } = (new JSDOM(res.data)).window;

                const container = document.getElementsByClassName('passage-col')[0];

                Array.from(container.getElementsByClassName('chapternum')).forEach((el: Element) => {
                    el.textContent = `<**${el.textContent.slice(0, -1)}**> `;
                });

                Array.from(container.getElementsByClassName('versenum')).forEach((el: Element) => {
                    el.textContent = `<**${el.textContent.slice(0, -1)}**> `;
                });

                Array.from(container.getElementsByClassName('footnote')).forEach((el: Element) => {
                    el.remove();
                });

                const title = Array.from(container.getElementsByTagName('h3')).map((el: Element) => el.textContent.trim()).join(' / ');
                const text = Array.from(container.getElementsByTagName('p')).map((el: Element) => el.textContent.trim()).join('\n');

                return callback(null, new Verse(
                    `${document.getElementsByClassName('bcv')[0].textContent}`,
                    title,
                    purifyVerseText(text),
                    ref
                ));
            } catch (err) {
                return callback(err, null);
            }
        });
}