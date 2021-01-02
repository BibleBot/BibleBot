import Context from '../../models/context';
import Language from '../../models/language';

import { createEmbed } from '../../helpers/embed_builder';

export class InformationRouter {
    private static instance: InformationRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): InformationRouter {
        if (!InformationRouter.instance) {
            InformationRouter.instance = new InformationRouter();
        }

        return InformationRouter.instance;
    }

    getHelp(ctx: Context): void {
        const lang = ctx.language;
        const title = lang.getString('biblebot').replace('<version>', process.env.npm_package_version);
        const desc = lang.getString('credit');

        // TODO: command prefixes
        let commandList = lang.getString('commandlist').split('<+>').join('+');

        const replacements = ['search', 'version', 'random',
                              'verseoftheday', 'votd', 'formatting',
                              'language', 'stats', 'invite',
                              'supporters', 'creeds', 'resources'];

        for (const replacement of replacements) {
            commandList = commandList.replace(`<${replacement}>`, lang.getCommand(replacement));
        }

        const links = `
        ${lang.getString('website').replace('<website>', 'https://biblebot.xyz')}
        ${lang.getString('code').replace('<repository>', 'https://github.com/BibleBot/BibleBot')}
        ${lang.getString('server').replace('<invite>', 'https://discord.gg/H7ZyHqE')}
        ${lang.getString('terms').replace('<terms>', 'https://biblebot.xyz/terms')}

        **${lang.getString('usage')}**
        `;

        const embed = createEmbed(null, title, desc, false);
        embed.addField(lang.getString('commandlistName'), commandList, false);
        embed.addField('\u200B', '—————————————', false);
        embed.addField(lang.getString('links'), links, false);

        ctx.channel.send(embed);
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'biblebot');
    }
}