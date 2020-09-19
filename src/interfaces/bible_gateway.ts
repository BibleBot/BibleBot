import axios from 'axios';
import { JSDOM } from 'jsdom';
import { purifyVerseText } from '../helpers/text_purification';

export function getResult(query: string, version: string, headings: boolean, verseNumbers: boolean, 
    callback: (err: Error, data: (null | Record<string, string>)) => void): void {
        axios.get(`https://www.biblegateway.com/passage/?search=${query}&version=${version}&interface=print`).then((res) => {
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

            return callback(null, {
                passage: `${document.getElementsByClassName('bcv')[0].textContent}`,
                version: `${document.getElementsByClassName('translation')[0].textContent} (${version})`,
                title: title,
                text: purifyVerseText(text),
            });
        });
}