import Context from '../../models/context';
import GuildPreference from '../../models/guild_preference';

import { createEmbed } from '../../helpers/embed_builder';

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

    setGuildPrefix(ctx: Context, prefix: string): void {
        if (prefix.length != 1) {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setprefix - not single character');
            ctx.channel.send(createEmbed(null, '+misc setprefix', 'The prefix must be a single character.', true));
            
            return;
        }

        GuildPreference.findOne({ user: ctx.id }, (err, prefs) => {
            if (err) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setprefix - cannot save preference');
                ctx.channel.send(createEmbed(null, '+misc setprefix', 'Failed to set preference.', true));
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
                        ctx.channel.send(createEmbed(null, '+misc setprefix', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `misc setprefix ${prefix}`);
                        ctx.channel.send(createEmbed(null, '+misc setprefix', 'Set prefix successfully.', false));
                    }
                });
            } else {
                prefs.prefix = prefix;

                prefs.save((err, preference) => {
                    if (err || !preference) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setprefix - cannot overwrite preference');
                        ctx.channel.send(createEmbed(null, '+misc setprefix', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `misc setprefix ${prefix} (overwrite)`);
                        ctx.channel.send(createEmbed(null, '+misc setprefix', 'Set prefix successfully.', false));
                    }
                });
            }
        });
    }

    setGuildBrackets(ctx: Context, brackets: string): void {
        const validBrackets = ['<>', '[]', '{}', '()'];

        if (brackets.length != 2 && validBrackets.includes(brackets)) {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setbrackets - invalid input');
            ctx.channel.send(createEmbed(null, '+misc setbrackets', 'invalid input.', true));
            
            return;
        }

        GuildPreference.findOne({ guild: ctx.guild.id }, (err, prefs) => {
            if (err) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setbrackets - cannot save preference');
                ctx.channel.send(createEmbed(null, '+misc setbrackets', 'Failed to set preference.', true));
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
                        ctx.channel.send(createEmbed(null, '+misc setbrackets', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `misc setbrackets ${brackets}`);
                        ctx.channel.send(createEmbed(null, '+misc setbrackets', 'Set brackets successfully.', false));
                    }
                });
            } else {
                prefs.brackets = brackets;

                prefs.save((err, preference) => {
                    if (err || !preference) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'misc setbrackets - cannot overwrite preference');
                        ctx.channel.send(createEmbed(null, '+misc setbrackets', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `misc setbrackets ${brackets} (overwrite)`);
                        ctx.channel.send(createEmbed(null, '+misc setbrackets', 'Set brackets successfully.', false));
                    }
                });
            }
        });
    }

    async getMiscSettings(ctx: Context): Promise<void> {
        const lang = ctx.language;

        const message = `
        ${lang.getString('guildprefixused').replace('<prefix>', '**' + ctx.guildPreferences.prefix + '**')}
        ${lang.getString('guildbracketsused').replace('<brackets>', '**' + ctx.guildPreferences.ignoringBrackets + '**')}
        
        __**${lang.getString('subcommands')}**__
        **${lang.getCommand('setprefix')}** - ${lang.getString('setprefixusage')}
        **${lang.getCommand('setbrackets')}** - ${lang.getString('setbracketsusage')}`;
                
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'misc');
        ctx.channel.send(createEmbed(null, '+misc', message, false));
    }

    processCommand(ctx: Context, params: Array<string>): void {
        const subcommand = params[0];
        const args = params.slice(1);

        switch (subcommand) {
            case 'setprefix':
                this.setGuildPrefix(ctx, args[0]);
                break;
            case 'setbrackets':
                this.setGuildBrackets(ctx, args[0]);
                break;
            default:
                this.getMiscSettings(ctx);
                break;
        }
    }
}