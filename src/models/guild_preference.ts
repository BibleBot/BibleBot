import * as mongoose from 'mongoose';

const GuildPreferenceSchema: mongoose.Schema = new mongoose.Schema({
    guild: { type: String, required: true },
    prefix: { type: String, required: true },
    ignoringBrackets: { type: String, required: true },
    language: { type: String, required: true },
    version: { type: String, required: true },
    dailyVerseChannel: { type: String, required: false },
    dailyVerseTime: { type: String, required: false },
    dailyVerseTz: { type: String, required: false }
});

export default mongoose.model('GuildPreference', GuildPreferenceSchema);