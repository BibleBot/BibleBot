import * as mongoose from 'mongoose';

import Reference from './reference';

export default class Verse {
    private _passage: string;
    private _title: string;
    private _text: string;
    private _ref: Reference;

    constructor(passage: string, title: string, text: string, reference: Reference) {
        this._passage = passage;
        this._title = title;
        this._text = text;
        this._ref = reference;
    }

    passage(): string {
        return this._passage;
    }

    version(): mongoose.Document {
        return this._ref.version;
    }

    title(): string {
        return this._title;
    }

    text(): string {
        return this._text;
    }

    reference(): Reference {
        return this._ref;
    }
}