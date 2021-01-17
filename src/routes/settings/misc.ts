import Context from '../../models/context';
import GuildPreference from '../../models/guild_preference';

import { createEmbed, translateCommand } from '../../helpers/embed_builder';
import { checkGuildPermissions } from '../../helpers/permissions';

import * as defaultGuildPreferences from '../../helpers/default_guild_preference.json';

export class MiscSettingsRouter {
    private static instance: MiscSettingsRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): MiscSettingsRouter {
        if (!MiscSettingsRouter.instance) {
            MiscSettingsRouter.instance = new MiscSettingsRouter();
        }

        return MiscSettingsRouter.instance;
    }

    setGuildPrefix(ctx: Context, args: string[]): void {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setprefix']), lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setprefix - no parameter');
            return;
        }

        const prefix = args[0];

        if (prefix.length != 1) {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setprefix - not single character');
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setprefix']), lang.getString('prefixonechar'), true));
            
            return;
        }

        GuildPreference.findOne({ guild: ctx.guild.id }, (err, prefs) => {
            if (err) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setprefix - cannot save preference');
                ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setprefix']), lang.getString('failedpreference'), true));
            } else if (!prefs) {
                const derivedFromDefault = {
                    guild: ctx.guild.id,
                    ...defaultGuildPreferences
                };

                derivedFromDefault.prefix = prefix;

                const newPreference = new GuildPreference(derivedFromDefault);

                newPreference.save((err, prefs) => {
                    if (err || !prefs) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setprefix - cannot save new preference');
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setprefix']), lang.getString('failedpreference'), true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `misc setprefix ${prefix}`);
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setprefix']), lang.getString('prefixsuccess'), false));
                    }
                });
            } else {
                prefs.prefix = prefix;

                prefs.save((err, preference) => {
                    if (err || !preference) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setprefix - cannot overwrite preference');
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setprefix']), lang.getString('failedpreference'), true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `misc setprefix ${prefix} (overwrite)`);
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setprefix']), lang.getString('prefixsuccess'), false));
                    }
                });
            }
        });
    }

    setGuildBrackets(ctx: Context, args: string[]): void {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setbrackets']), lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setbrackets - no parameter');
            return;
        }

        const brackets = args[0];
        const validBrackets = ['<>', '[]', '{}', '()'];

        if (brackets.length != 2 && validBrackets.includes(brackets)) {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setbrackets - invalid input');
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setbrackets']), lang.getString('guildbracketsfail'), true));

            return;
        }

        GuildPreference.findOne({ guild: ctx.guild.id }, (err, prefs) => {
            if (err) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setbrackets - cannot save preference');
                ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setbrackets']), lang.getString('failedpreference'), true));
            } else if (!prefs) {
                const derivedFromDefault = {
                    guild: ctx.guild.id,
                    ...defaultGuildPreferences
                };

                derivedFromDefault.ignoringBrackets = brackets;

                const newPreference = new GuildPreference(derivedFromDefault);

                newPreference.save((err, prefs) => {
                    if (err || !prefs) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setbrackets - cannot save new preference');
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setbrackets']), lang.getString('failedpreference'), true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `misc setbrackets ${brackets}`);
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setbrackets']), lang.getString('setguildbracketssuccess'), false));
                    }
                });
            } else {
                prefs.brackets = brackets;

                prefs.save((err, preference) => {
                    if (err || !preference) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setbrackets - cannot overwrite preference');
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setbrackets']), lang.getString('failedpreference'), true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `misc setbrackets ${brackets} (overwrite)`);
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setbrackets']), lang.getString('setguildbracketssuccess'), false));
                    }
                });
            }
        });
    }

    async getMiscSettings(ctx: Context): Promise<void> {
        const lang = ctx.language;

        const message = `${lang.getString('guildprefixused').replace('<prefix>', '**' + ctx.guildPreferences.prefix + '**')}\n` +
        `${lang.getString('guildbracketsused').replace('<brackets>', '**' + ctx.guildPreferences.ignoringBrackets + '**')}\n\n` +
        `__**${lang.getString('subcommands')}**__\n` +
        `**${lang.getCommand('setprefix')}** - ${lang.getString('setprefixusage')}\n` + 
        `**${lang.getCommand('setbrackets')}** - ${lang.getString('setbracketsusage')}`;
                
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'misc');
        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc']), message, false));
    }

    processCommand(ctx: Context, params: Array<string>): void {
        const subcommand = ctx.language.getCommandKey(params[0]);
        const args = params.slice(1);

        switch (subcommand) {
            case 'setprefix':
                checkGuildPermissions(ctx, (hasPermission) => {
                    if (hasPermission) {
                        this.setGuildPrefix(ctx, args);
                    } else {
                        const lang = ctx.language;

                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setprefix']), lang.getString('noguildperm'), true));
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setprefix - no permission');
                    }
                });
                break;
            case 'setbrackets':
                checkGuildPermissions(ctx, (hasPermission) => {
                    if (hasPermission) {
                        this.setGuildBrackets(ctx, args);
                    } else {
                        const lang = ctx.language;

                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'misc', 'setbrackets']), lang.getString('noguildperm'), true));
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setbrackets - no permission');
                    }
                });
                break;
            default:
                this.getMiscSettings(ctx);
                break;
        }
    }
}