import Language from './models/language';
import { readFileSync } from 'fs';

const _english = JSON.parse(readFileSync(__dirname + '/../../i18n/english/english.json', 'utf-8'));

export default {
    english: new Language('english', 'english', _english, 'RSV')
};