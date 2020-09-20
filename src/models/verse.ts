import { isValidSource } from '../helpers/verse_utils';
import Reference from './reference';

export default class Verse {
    private _passage: string;
    private _version: string;
    private _title: (string | boolean);
    private _text: string;
    private _ref: Reference;
    private _src: string;

    constructor(passage: string, title: (string | boolean), text: string, reference: Reference, source: string) {
        this._passage = passage;
        this._title = title;
        this._text = text;
        this._ref = reference;
        
        if (!isValidSource(source)) {
            throw new Error('Invalid source specified.');
        }

        this._src = source;
    }

    passage(): string {
        return this._passage;
    }

    version(): string {
        return this._version;
    }

    title(): (string | boolean) {
        return this._title;
    }

    text(): string {
        return this._text;
    }

    reference(): Reference {
        return this._ref;
    }

    source(): string {
        return this._src;
    }
}