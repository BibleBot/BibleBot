import * as mongoose from 'mongoose';

const VersionSchema: mongoose.Schema = new mongoose.Schema({
    name: { type: String, required: true },
    abbv: { type: String, required: true },
    src: { type: String, required: true },
    supportsOldTestament: { type: Boolean, required: true },
    supportsNewTestament: { type: Boolean, required: true },
    supportsDeuterocanon: { type: Boolean, required: true },
});

export default mongoose.model('Version', VersionSchema);