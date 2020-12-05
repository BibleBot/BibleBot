import * as mongoose from 'mongoose';

const PreferenceSchema: mongoose.Schema = new mongoose.Schema({
    input: { type: String, required: true },
    //language: { type: Language, required: true },
    version: { type: String, required: true },
    headings: { type: Boolean, required: true },
    verseNumbers: { type: Boolean, required: true }
});

export default mongoose.model('Preference', PreferenceSchema);