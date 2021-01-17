import * as mongoose from 'mongoose';

export interface Preference {
    user: string;
    input: string;
    language: string;
    version: string;
    headings: boolean;
    verseNumbers: boolean;
    display: string;
}

export interface PreferenceDocument extends Preference, mongoose.Document {
    // just to please the compiler
}

export type PreferenceModel = mongoose.Model<PreferenceDocument>

const PreferenceSchema = new mongoose.Schema<PreferenceDocument>({
    user: { type: String, required: true },
    input: { type: String, required: true },
    language: { type: String, required: true },
    version: { type: String, required: true },
    headings: { type: Boolean, required: true },
    verseNumbers: { type: Boolean, required: true },
    display: { type: String, required: true }
});

export default mongoose.model<PreferenceDocument, PreferenceModel>('Preference', PreferenceSchema);