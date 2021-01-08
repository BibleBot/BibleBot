import * as mongoose from 'mongoose';

const PreferenceSchema: mongoose.Schema = new mongoose.Schema({
    user: { type: String, required: true },
    input: { type: String, required: true },
    language: { type: String, required: true },
    version: { type: String, required: true },
    headings: { type: Boolean, required: true },
    verseNumbers: { type: Boolean, required: true },
    display: { type: String, required: true }
});

export default mongoose.model('Preference', PreferenceSchema);