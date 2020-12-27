import axios from 'axios';
import { JSDOM } from 'jsdom';
import { purifyVerseText } from '../helpers/text_purification';
import Reference from '../models/reference';
import Verse from '../models/verse';

export function getResult(ref: Reference, headings: boolean, verseNumbers: boolean, 
    callback: (err: Error, data: Verse) => void): void {
        axios.get(`https://www.academic-bible.com/en/online-bibles/septuagint-lxx/read-the-bible-text/bibel/text/lesen/?tx_buhbibelmodul_bibletext%5Bscripture%5D=${ref.toString()}`).then((res) => {
            try {
                const { document } = (new JSDOM(res.data)).window;

                const container = document.getElementsByClassName('greek-container')[0];

                Array.from(container.getElementsByClassName('chapter')).forEach((el: Element) => {
                    // Remove these as verse numbers are provided alongside.
                    el.remove();
                });

                Array.from(container.getElementsByClassName('verse')).forEach((el: Element) => {
                    el.textContent = `<**${el.textContent.slice(0, -1)}**> `;
                });

                const text = Array.from(container.getElementsByClassName('greek')).map((el: Element) => el.textContent.trim()).join(' ');

                return callback(null, new Verse(
                    ref.toString(),
                    null,
                    purifyVerseText(text),
                    ref
                ));
            } catch (err) {
                return callback(err, null);
            }
        });
}