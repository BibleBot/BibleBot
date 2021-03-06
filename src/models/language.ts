import * as mongoose from 'mongoose';

export interface Language {
    name: string;
    objectName: string;
    rawObject: Record<string, string>;
    defaultVersion: string;
}

export interface LanguageDocument extends Language, mongoose.Document {
    getString(val: string): string;
    getCommand(val: string): string;
    getArgument(val: string): string;
    getCommandKey(val: string): string;
}

export type LanguageModel = mongoose.Model<LanguageDocument>

const LanguageSchema = new mongoose.Schema<LanguageDocument>({
    name: { type: String, required: true },
    objectName: { type: String, required: true },
    rawObject: { type: mongoose.Schema.Types.Mixed, required: true },
    defaultVersion: { type: String, required: true }
});

LanguageSchema.methods.getString = function(value: string): string {
    if (Object.keys(this.rawObject).includes(value)) {
        return this.rawObject[value];
    }
};

LanguageSchema.methods.getCommand = function(value: string): string {
    if (Object.keys(this.rawObject.commands).includes(value)) {
        return this.rawObject.commands[value];
    }
};

LanguageSchema.methods.getArgument = function(value: string): string {
    if (Object.keys(this.rawObject.arguments).includes(value)) {
        return this.rawObject.arguments[value];
    }
};

LanguageSchema.methods.getCommandKey = function(value: string): string {
    return Object.keys(this.rawObject.commands).find(key => this.rawObject.commands[key] === value) || null;
};

export default mongoose.model<LanguageDocument, LanguageModel>('Language', LanguageSchema);