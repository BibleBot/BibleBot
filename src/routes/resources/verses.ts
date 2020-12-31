import axios from 'axios';
import { JSDOM } from 'jsdom';

import Context from '../../models/context';
import Version from '../../models/version';

import * as utils from '../../helpers/verse_utils';

export class VerseCommandsRouter {
    private static instance: VerseCommandsRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): VerseCommandsRouter {
        if (!VerseCommandsRouter.instance) {
            VerseCommandsRouter.instance = new VerseCommandsRouter();
        }

        return VerseCommandsRouter.instance;
    }

    getRandomVerse(ctx: Context): void {
        axios.get('https://dailyverses.net/random-bible-verse').then((res) => {
            try {
                const { document } = (new JSDOM(res.data)).window;

                const container = document.getElementsByClassName('vr')[0];
                const verse = container.getElementsByClassName('vc')[0].textContent;

                let queryVersion = 'RSV';
                if (ctx.preferences.version) {
                    queryVersion = ctx.preferences.version;
                }

                Version.findOne({ abbv: queryVersion }, (err, version) => {
                    ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'used +random for next reference');
                    utils.processVerse(ctx, version, verse);
                });
            } catch (err) {
                return;
            }
        });
    }
}