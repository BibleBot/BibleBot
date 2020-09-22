import { PassThrough } from 'stream';
import * as BibleGateway from '../interfaces/bible_gateway';
import Reference from '../models/reference';
import { getBookNames } from './name_fetcher';

const sources = {
    'bg': { name: 'BibleGateway', interface: BibleGateway },
    'ab': { name: 'API.Bible', interface: null },
    'bh': { name: 'Bible Hub', interface: null },
    'bs': { name: 'Bible Server', interface: null }
};

export function isValidSource(source: string): boolean {
    return Object.keys(sources).includes(source.toLowerCase());
}

export function sourceHasInterface(source: string): boolean {
    return sources[source].interface !== null;
}

export function findBooksInMessage(msg: string): Array<Record<string, number>> {
    const msgTokens = msg.split(' ');
    const books = getBookNames();
    const results = [];

    for (const [index, token] of msgTokens.entries()) {
        Object.keys(books).forEach(book => {
            if (books[book].includes(token)) {
                if (book == 'John' || book == 'Ezra') {
                    const previousToken = msgTokens[index - 1];
                    const prevTokenToNumber = Number(previousToken);
                    const boundaries = book == 'John' ? [0, 4] : [0, 3];

                    if (!isNaN(prevTokenToNumber) || prevTokenToNumber !== undefined) {
                        if (boundaries[0] < prevTokenToNumber && prevTokenToNumber < boundaries[1]) {
                            const name = book == 'Ezra' ? 'Esdras' : 'John';

                            results.push({
                                name: `${prevTokenToNumber} ${name}`,
                                index
                            });

                            return;
                        }
                    }

                    results.push({
                        name: `${book}`,
                        index
                    });
                } else if (book == 'Psalms') {
                    const nextToken = msgTokens[index + 1];
                    const nextTokenToNumber = Number(nextToken);

                    if (!isNaN(nextTokenToNumber) || nextTokenToNumber !== undefined) {
                        if (nextTokenToNumber == 151) {
                            results.push({
                                name: `${book} ${nextTokenToNumber}`,
                                index: index + 1
                            });
                        }
                    }
                } else {
                    results.push({
                        name: book,
                        index
                    });
                }
            }
        });
    }

    return results;
}

export function generateReference(result: Record<string, number>): Reference {
    return;
}