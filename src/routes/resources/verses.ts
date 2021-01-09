import axios from 'axios';
import { JSDOM } from 'jsdom';

import Context from '../../models/context';
import Version from '../../models/version';

import * as utils from '../../helpers/verse_utils';

import * as bibleGateway from '../../interfaces/bible_gateway';
import * as apiBible from '../../interfaces/api_bible';

import { createEmbed, translateCommand } from '../../helpers/embed_builder';
import { Paginator } from '../../helpers/paginator';

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
                    utils.processVerse(ctx, version, verse, true);
                    ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'random');
                });
            } catch (err) {
                return;
            }
        });
    }

    getTrulyRandomVerse(ctx: Context): void {
        axios.get('https://thywordistrue.com/verse_generator').then((res) => {
            try {
                const { document } = (new JSDOM(res.data)).window;

                const container = document.getElementsByClassName('random_verse')[0];
                const verse = container.getElementsByTagName('small')[0].textContent;

                let queryVersion = 'RSV';
                if (ctx.preferences.version) {
                    queryVersion = ctx.preferences.version;
                }

                Version.findOne({ abbv: queryVersion }, (err, version) => {
                    utils.processVerse(ctx, version, verse, true);
                    ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'truerandom');
                });
            } catch (err) {
                return;
            }
        });
    }

    async search(ctx: Context, args: Array<string>): Promise<void> {
        const version = await Version.findOne({ abbv: ctx.preferences.version }).exec();
        let processor = bibleGateway;

        switch (version.src) {
            case 'ab':
                processor = apiBible;
                break;
        }

        if (args.join(' ').length > 3) {
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'search']), ctx.language.getString('queryTooShort'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'search (too short)');
        }

        processor.search(args.join(' '), version, (err, data: Array<Record<string, string>>) => {
            if (err) {
                console.error(err);
                return;
            }

            const pages = [];
            const maxResultsPerPage = 6;
            const versesUsed = [];
            let totalPages = Number(Math.ceil(data.length / maxResultsPerPage));

            if (totalPages == 0) {
                totalPages = 1;
            } else if (totalPages > 100) {
                totalPages = 100;
            }

            for (let i = 0; i < totalPages; i++) {
                const pageCounter = ctx.language.getString('pageOf').replace('<num>', i + 1)
                                                                    .replace('<total>', totalPages);
                                                                    
                const embed = createEmbed(null, `${ctx.language.getString('searchResults')} "${args.join(' ')}"`, pageCounter, false);

                if (data.length > 0) {
                    let count = 0;

                    for (const item of data) {
                        if (item.text.length < 700) {
                            if (count < maxResultsPerPage) {
                                if (!versesUsed.includes(item.title)) {
                                    embed.addField(item.title, item.text, false);

                                    data.shift();
                                    versesUsed.push(item.title);
                                    count++;
                                }
                            }
                        }
                    }
                } else {
                    embed.setTitle(ctx.language.getString('nothingFound').replace('<query>', args.join(' ')));
                    embed.setDescription('You may have to try a different query.');
                }

                pages.push(embed);
            }

            try {
                const paginator = new Paginator(pages, ctx.id, 180);
                ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `search ${args.join(' ')}`);
                paginator.run(ctx.channel);
            } catch (err) {
                if (err.message == 'User already using paginator.') {
                    ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'search']), ctx.language.getString('plswait'), true));
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, err.message);
                }
            }
        });
    }
}