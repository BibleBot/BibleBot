import { VersionDocument } from './version';

export default class Reference {
    private _book: string;
    private _startingChapter: number;
    private _startingVerse: number;
    private _endingChapter: number;
    private _endingVerse: number;
    public version: VersionDocument;
    private _isOT: boolean;
    private _isNT: boolean;
    private _isDEU: boolean;

    constructor(book: string, startingChapter: number, startingVerse: number, endingChapter: number, endingVerse: number, version: VersionDocument, isOT?: boolean, isNT?: boolean, isDEU?: boolean) {
        this._book = book;
        this._startingChapter = startingChapter;
        this._startingVerse = startingVerse;
        this._endingChapter = endingChapter;
        this._endingVerse = endingVerse;
        this.version = version;
        
        this._isOT = isOT ? isOT : null;
        this._isNT = isNT ? isNT : null;
        this._isDEU = isDEU ? isDEU : null;
    }

    toString(): string {
        let result = `${this._book} ${this._startingChapter}:${this._startingVerse}`;

        if (this._endingChapter && this._endingChapter != this._startingChapter) {
            result += `-${this._endingChapter}:${this._endingVerse}`;
        } else if (this._endingVerse && this._endingVerse != this._startingVerse) {
            result += `-${this._endingVerse}`;
        } else if (this._endingChapter && this._endingVerse == 0) {
            result += '-';
        }

        return `${result}`;
    }

    section(): string {
        return this._isOT ? 'OT' : this._isNT ? 'NT' : this._isDEU ? 'DEU' : null;
    }
}