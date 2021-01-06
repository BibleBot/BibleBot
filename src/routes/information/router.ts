import Context from '../../models/context';
import Preference from '../../models/preference';
import GuildPreference from '../../models/guild_preference';
import Version from '../../models/version';
import Language from '../../models/language';

import * as git from 'git-last-commit';
import * as os from 'os';
import * as fs from 'fs';
import * as ini from 'ini';

const packageJson = JSON.parse(fs.readFileSync(__dirname + '/../../../package.json', 'utf-8'));
const config = ini.parse(fs.readFileSync(`${__dirname}/../../config.ini`, 'utf-8'));

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

        const links = `${lang.getString('website').replace('<website>', 'https://biblebot.xyz')}\n` +
        `${lang.getString('code').replace('<repository>', 'https://github.com/BibleBot/BibleBot')}\n` +
        `${lang.getString('server').replace('<invite>', 'https://discord.gg/H7ZyHqE')}\n` +
        `${lang.getString('terms').replace('<terms>', 'https://biblebot.xyz/terms')}\n\n` +
        `**${lang.getString('usage')}**`;

        const embed = createEmbed(null, title, desc, false);
        embed.addField(lang.getString('commandlistName'), commandList, false);
        embed.addField('\u200B', '—————————————', false);
        embed.addField(lang.getString('links'), links, false);

        ctx.channel.send(embed);
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'biblebot');
    }

    async getStats(ctx: Context): Promise<void> {
        const lang = ctx.language;
        let userCount = 0;

        for (const guild of ctx.bot.guilds.cache.values()) {
            userCount += guild.memberCount;
        }

        const prefCount = await Preference.estimatedDocumentCount().exec();
        const gPrefCount = await GuildPreference.estimatedDocumentCount().exec();
        const versionCount = await Version.estimatedDocumentCount().exec();
        const languageCount = await Language.estimatedDocumentCount().exec();

        const platformMap = {
            'aix': 'IBM AIX',
            'darwin': 'Darwin/MacOS',
            'freebsd': 'FreeBSD',
            'linux': 'Linux',
            'openbsd': 'OpenBSD',
            'sunos': 'SunOS',
            'win32': 'Windows'
        };

        const platform = lang.getString('runningon').replace('<platform>', `**${platformMap[os.platform()]} ${os.release()}** (${os.arch()})`);

        git.getLastCommit((err, commit) => {
            const message = `**${lang.getString('shardcount')}** ${ctx.bot.ws.shards.size}\n` +
            `**${lang.getString('cachedguilds')}** ${ctx.bot.guilds.cache.size}\n` +
            `**${lang.getString('cachedchannels')}** ${ctx.bot.channels.cache.size}\n` +
            `**${lang.getString('cachedusers')}** ${userCount}\n\n` +

            `**${lang.getString('preferencecount')}** ${prefCount}\n` +
            `**${lang.getString('guildprefcount')}** ${gPrefCount}\n` +
            `**${lang.getString('versioncount')}** ${versionCount}\n` +
            `**${lang.getString('languagecount')}** ${languageCount}\n\n` +

            `**BibleBot:** ${process.env.npm_package_version} ([${commit.shortHash}](https://github.com/BibleBot/BibleBot/commit/${commit.hash}))\n` +
            `**Discord.js:** ${packageJson.dependencies['discord.js'].slice(1)}\n` + 
            `**Mongoose:** ${packageJson.dependencies.mongoose.slice(1)}\n\n` +
            
            `${platform}`;

            const embed = createEmbed(null, '+stats', message, false);
            embed.setThumbnail(config.biblebot.icon);

            ctx.channel.send(embed);
            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'stats');
        });
    }
}