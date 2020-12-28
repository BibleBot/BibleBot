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
        Language.findOne({ objectName: ctx.preferences.language }, (err, lang) => {
            const title = lang.getString('biblebot').replace('<version>', process.env.npm_package_version);

            // TODO: command prefixes and put this in a for loop
            let commandList = lang.getString('commandlist').split('<+>').join('+');
            commandList = commandList.replace('<search>', lang.getCommand('search'))
                                     .replace('<version>', lang.getCommand('version'))
                                     .replace('<random>', lang.getCommand('random'))
                                     .replace('<verseoftheday>', lang.getCommand('verseoftheday'))
                                     .replace('<votd>', lang.getCommand('votd'))
                                     .replace('<formatting>', lang.getCommand('formatting'))
                                     .replace('<language>', lang.getCommand('language'))
                                     .replace('<stats>', lang.getCommand('stats'))
                                     .replace('<invite>', lang.getCommand('invite'))
                                     .replace('<supporters>', lang.getCommand('supporters'))
                                     .replace('<creeds>', lang.getCommand('creeds'))
                                     .replace('<resources>', lang.getCommand('resources'));

            const links = `
            ${lang.getString('website').replace('<website>', 'https://biblebot.xyz')}
            ${lang.getString('code').replace('<repository>', 'https://github.com/BibleBot/BibleBot')}
            ${lang.getString('server').replace('<invite>', 'https://discord.gg/H7ZyHqE')}
            ${lang.getString('terms').replace('<terms>', 'https://biblebot.xyz/terms')}

            **${lang.getString('usage')}**
            `;

            const embed = createEmbed(null, title, null, false);
            embed.addField(lang.getString('commandlistName'), commandList, false);
            embed.addField('\u200B', '\u200B', false);
            embed.addField(lang.getString('links'), links, false);

            ctx.channel.send(embed);
            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'biblebot');
        });
    }
}