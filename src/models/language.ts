export default class Language {
    private name: string;
    private objectName: string;
    private rawObject: Record<string, string>;
    private defaultVersion: string;

    constructor(name: string, objectName: string, rawObject: Record<string, string>, defaultVersion: string) {
        this.name = name;
        this.objectName = objectName;
        this.rawObject = rawObject;
        this.defaultVersion = defaultVersion;
    }

    get(value: string): string {
        if (Object.keys(this.rawObject).includes(value)) {
            return this.rawObject[value];
        }
    }
}