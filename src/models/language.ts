import * as mongoose from 'mongoose';

const LanguageSchema: mongoose.Schema = new mongoose.Schema({
    name: { type: String, required: true },
    objectName: { type: String, required: true },
    rawObject: { type: mongoose.Mixed, required: true },
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

export default mongoose.model('Language', LanguageSchema);