import * as mongoose from 'mongoose';

export interface Version {
    name: string;
    abbv: string;
    src: string;
    supportsOldTestament: boolean;
    supportsNewTestament: boolean;
    supportsDeuterocanon: boolean;
}

export interface VersionDocument extends Version, mongoose.Document {
    // just to please the compiler
}

export type VersionModel = mongoose.Model<VersionDocument>

const VersionSchema = new mongoose.Schema<VersionDocument, VersionModel>({
    name: { type: String, required: true },
    abbv: { type: String, required: true },
    src: { type: String, required: true },
    supportsOldTestament: { type: Boolean, required: true },
    supportsNewTestament: { type: Boolean, required: true },
    supportsDeuterocanon: { type: Boolean, required: true },
});

export default mongoose.model<VersionDocument, VersionModel>('Version', VersionSchema);