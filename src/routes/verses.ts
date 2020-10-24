import * as ini from 'ini';
import * as fs from 'fs';

import Context from '../models/context';
import Verse from '../models/verse';

import * as bibleGateway from '../interfaces/bible_gateway';
import * as apiBible from '../interfaces/api_bible';
import { createEmbed } from '../helpers/embed_builder';
import * as utils from '../helpers/verse_utils';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

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

    processMessage(ctx: Context, inputType: string): void {
        // get rid of newlines and instead put a space between lines
        const msg = ctx.msg.split(/\r?\n/).join(' ');
        
        if (!msg.includes(' ')) { return; }

        const results = utils.findBooksInMessage(msg);

        if (results.length > 5) {
            ctx.channel.send('Please don\'t spam me.');
            ctx.logMessage('erro', ctx.shard, ctx.id, ctx.guild.id, ctx.channel.id, 'spam attempt');
            return;
        }

        results.forEach((result) => {
            if (utils.isSurroundedByBrackets(config.biblebot.ignoringBrackets, result, msg)) {
                return;
            }

            if (inputType == 'erasmus' && !utils.isSurroundedByBrackets('[]', result, msg)) {
                return;
            }

            const reference = utils.generateReference(result, msg);

            if (reference === null) {
                return;
            }

            let processor = bibleGateway;

            switch (reference.version.source()) {
                case 'ab':
                    processor = apiBible;
                    break;
            }

            processor.getResult(reference, true, true, (err, data: Verse) => {
                if (err) {
                    console.error(err);
                    return;
                }

                const embed = createEmbed(`${data.passage} - ${data.version().name}`, data.title(), data.text(), false);

                ctx.channel.send(embed);
            });
            
        });
    }
}