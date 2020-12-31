import * as mongoose from 'mongoose';

import Reference from './reference';

export default class Verse {
    private _passage: string;
    private _title: string;
    private _text: string;
    private _ref: Reference | string;
    private _ver: mongoose.Document;

    constructor(passage: string, title: string, text: string, reference: Reference | string, version: mongoose.Document) {
        this._passage = passage;
        this._title = title;
        this._text = text;
        this._ref = reference;

        if (version) {
            this._ver = version;
        }
    }


    passage(): string {
        return this._passage;
    }

    version(): mongoose.Document {
        if (this._ver) {
            return this._ver;
        }

        if (Object.prototype.hasOwnProperty.call(this._ref, 'version')) { 
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            return (this._ref as any).version;
        }
    }

    title(): string {
        return this._title;
    }

    text(): string {
        return this._text;
    }

    reference(): Reference | string {
        return this._ref;
    }
}