import { isValidSource } from '../helpers/verse_utils';

export default class Version {
    private _name: string;
    private abbv: string;
    private src: string;
    private supportsOldTestament: boolean;
    private supportsNewTestament: boolean;
    private supportsDeuterocanon: boolean;

    constructor(name: string, abbv: string, source: string, supportsOldTestament: boolean, supportsNewTestament: boolean, supportsDeuterocanon: boolean) {
        this._name = name;
        this.abbv = abbv.toUpperCase();
        
        if (!isValidSource(source)) {
            throw new Error('Invalid source specified.');
        }

        this.src = source;
        this.supportsOldTestament = supportsOldTestament;
        this.supportsNewTestament = supportsNewTestament;
        this.supportsDeuterocanon = supportsDeuterocanon;
    }

    abbreviation(): string {
        return this.abbv;
    }

    source(): string {
        return this.src;
    }

    name(): string {
        return `${this._name} (${this.abbv})`;
    }
}