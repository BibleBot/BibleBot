import * as BibleGateway from '../interfaces/bible_gateway';
import Reference from '../models/reference';

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

export function findBooksInMessage(msg: string): Array<Record<string, unknown>> {
    return;
}

export function generateReference(result: Record<string, unknown>): Reference {
    return;
}