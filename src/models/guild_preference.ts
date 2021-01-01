import * as mongoose from 'mongoose';

const GuildPreferenceSchema: mongoose.Schema = new mongoose.Schema({
    guild: { type: String, required: true },
    input: { type: String, required: true },
    language: { type: String, required: true },
    version: { type: String, required: true },
    headings: { type: Boolean, required: true },
    verseNumbers: { type: Boolean, required: true }
});

export default mongoose.model('GuildPreference', GuildPreferenceSchema);