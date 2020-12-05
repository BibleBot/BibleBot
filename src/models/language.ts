import * as mongoose from 'mongoose';

import Version from './version';

const LanguageSchema: mongoose.Schema = new mongoose.Schema({
    name: { type: String, required: true },
    objectName: { type: String, required: true },
    rawObject: { type: mongoose.Mixed, required: true },
    defaultVersion: { type: Version, required: true }
});

LanguageSchema.methods.get = function(value: string): string {
    if (Object.keys(this.rawObject).includes(value)) {
        return this.rawObject[value];
    }
};

export default mongoose.model('Language', LanguageSchema);