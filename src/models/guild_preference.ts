import * as mongoose from 'mongoose';

export interface GuildPreference {
    guild: string;
    prefix: string;
    ignoringBrackets: string;
    language: string;
    version: string;
    dailyVerseChannel: string;
    dailyVerseTime: string;
    dailyVerseTz: string;
}

export interface GuildPreferenceDocument extends GuildPreference, mongoose.Document {
    // just to please the compiler
}

export type GuildPreferenceModel = mongoose.Model<GuildPreferenceDocument>;

const GuildPreferenceSchema = new mongoose.Schema<GuildPreferenceDocument>({
    guild: { type: String, required: true },
    prefix: { type: String, required: true },
    ignoringBrackets: { type: String, required: true },
    language: { type: String, required: true },
    version: { type: String, required: true },
    dailyVerseChannel: { type: String, required: false },
    dailyVerseTime: { type: String, required: false },
    dailyVerseTz: { type: String, required: false }
});

export default mongoose.model<GuildPreferenceDocument, GuildPreferenceModel>('GuildPreference', GuildPreferenceSchema);