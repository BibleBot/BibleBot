import Version from './version';

export default class Reference {
    private _book: string;
    private _startingChapter: number;
    private _startingVerse: number;
    private _endingChapter: number;
    private _endingVerse: number;
    public version: Version;
    private _isOT: boolean;
    private _isNT: boolean;
    private _isDEU: boolean;

    constructor(book: string, startingChapter: number, startingVerse: number, endingChapter: number, endingVerse: number, version: Version, isOT?: boolean, isNT?: boolean, isDEU?: boolean) {
        this._book = book;
        this._startingChapter = startingChapter;
        this._startingVerse = startingVerse;
        this._endingChapter = endingChapter;
        this._endingVerse = endingVerse;
        this.version = version;
        
        this._isOT = (isOT !== undefined);
        this._isNT = (isNT !== undefined);
        this._isDEU = (isDEU !== undefined);
    }

    toString(): string {
        let result = `${this._book} ${this._startingChapter}:${this._startingVerse}`;

        if (this._endingChapter) {
            result += `-${this._endingChapter}:${this._endingVerse}`;
        } else if (this._endingVerse) {
            result += `-${this._endingVerse}`;
        }

        return `${result}`;
    }

    section(): string {
        return this._isOT ? 'OT' : this._isNT ? 'NT' : this._isDEU ? 'DEU' : null;
    }
}