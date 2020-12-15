import * as ini from 'ini';
import * as fs from 'fs';

import Context from '../models/context';
import Verse from '../models/verse';
import Version from '../models/version';

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
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.guild.id, ctx.channel.id, 'spam attempt');
            return;
        }

        results.forEach((result) => {
            if (utils.isSurroundedByBrackets(config.biblebot.ignoringBrackets, result, msg)) {
                return;
            }

            if (inputType == 'erasmus' && !utils.isSurroundedByBrackets('[]', result, msg)) {
                return;
            }

            let queryVersion = 'RSV';
            if (ctx.preferences.version) {
                queryVersion = ctx.preferences.version;
            }

            Version.findOne({ abbv: queryVersion }).then(async (version) => {
                const reference = await utils.generateReference(result, msg, version);

                if (reference === null) {
                    return;
                }
        
                let processor = bibleGateway;
        
                switch (version['src']) {
                    case 'ab':
                        processor = apiBible;
                        break;
                }
        
                processor.getResult(reference, true, true, (err, data: Verse) => {
                    if (err) {
                        console.error(err);
                        return;
                    }
        
                    const title = `${data.passage()} - ${data.version().name}`;
                    const embed = createEmbed(title, data.title(), data.text(), false);
        
                    ctx.channel.send(embed);
                    ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `${data.passage()} ${data.version().abbv}`);
                });
            });
        }); 
    }
}