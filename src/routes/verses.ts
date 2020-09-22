import Context from '../models/context';
import * as bibleGateway from '../interfaces/bible_gateway';
import { createEmbed } from '../helpers/embed_builder';
import * as utils from '../helpers/verse_utils';

export class VersesRouter {
    private static instance: VersesRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): VersesRouter {
        if (!VersesRouter.instance) {
            VersesRouter.instance = new VersesRouter();
        }

        return VersesRouter.instance;
    }

    processMessage(ctx: Context): void {
        // get rid of newlines and instead put a space between lines
        const msg = ctx.msg.split(/\r?\n/).join(' ');
        
        if (!msg.includes(' ')) { return; }

        const results = utils.findBooksInMessage(msg);

        if (results.length > 6) {
            ctx.channel.send('Please don\'t spam me.');
            return;
        }

        results.forEach(result => {
            console.log(result);
            
            /*const reference = utils.generateReference(result);

            if (reference === undefined) {
                return;
            }*/

            
        });
    }
}