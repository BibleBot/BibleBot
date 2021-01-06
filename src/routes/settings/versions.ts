import Context from '../../models/context';
import Version from '../../models/version';
import Language from '../../models/language';
import Preference from '../../models/preference';
import GuildPreference from '../../models/guild_preference';

import { createEmbed } from '../../helpers/embed_builder';
import { checkGuildPermissions } from '../../helpers/permissions';

import * as defaultUserPreferences from '../../helpers/default_user_preference.json';
import * as defaultGuildPreferences from '../../helpers/default_guild_preference.json';

export class VersionSettingsRouter {
    private static instance: VersionSettingsRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): VersionSettingsRouter {
        if (!VersionSettingsRouter.instance) {
            VersionSettingsRouter.instance = new VersionSettingsRouter();
        }

        return VersionSettingsRouter.instance;
    }

    setUserVersion(ctx: Context, abbv: string): void {
        Version.findOne({ abbv }).then((version) => {
            if (!version) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `version set - invalid version (${abbv})`);
                ctx.channel.send(createEmbed(null, '+version set', `${abbv} not in database`, true));
                return;
            }

            Preference.findOne({ user: ctx.id }, (err, prefs) => {
                if (err) {
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version set - cannot save preference');
                    ctx.channel.send(createEmbed(null, '+version set', 'Failed to set preference.', true));
                } else if (!prefs) {
                    const derivedFromDefault = {
                        user: ctx.id,
                        ...defaultUserPreferences
                    };

                    derivedFromDefault.version = abbv;

                    const newPreference = new Preference(derivedFromDefault);

                    newPreference.save((err, prefs) => {
                        if (err || !prefs) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version set - cannot save new preference');
                            ctx.channel.send(createEmbed(null, '+version set', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `version set ${abbv}`);
                            ctx.channel.send(createEmbed(null, '+version set', 'Set version successfully.', false));
                        }
                    });
                } else {
                    prefs.version = abbv;

                    prefs.save((err, preference) => {
                        if (err || !preference) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version set - cannot overwrite preference');
                            ctx.channel.send(createEmbed(null, '+version set', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `version set ${abbv} (overwrite)`);
                            ctx.channel.send(createEmbed(null, '+version set', 'Set version successfully.', false));
                        }
                    });
                }
            });
        }).catch(() => {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `version set - invalid version (${abbv})`);
            ctx.channel.send(createEmbed(null, '+version set', `${abbv} not in database`, true));
        });
    }

    setGuildVersion(ctx: Context, abbv: string): void {
        Version.findOne({ abbv }).then((version) => {
            if (!version) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `version setserver - invalid version (${abbv})`);
                ctx.channel.send(createEmbed(null, '+version setserver', `${abbv} not in database`, true));
                return;
            }

            GuildPreference.findOne({ user: ctx.id }, (err, prefs) => {
                if (err) {
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version setserver - cannot save preference');
                    ctx.channel.send(createEmbed(null, '+version setserver', 'Failed to set preference.', true));
                } else if (!prefs) {
                    const derivedFromDefault = {
                        guild: ctx.guild.id,
                        ...defaultGuildPreferences
                    };

                    derivedFromDefault.version = abbv;

                    const newPreference = new GuildPreference(derivedFromDefault);

                    newPreference.save((err, prefs) => {
                        if (err || !prefs) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version setserver - cannot save new preference');
                            ctx.channel.send(createEmbed(null, '+version setserver', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `version set ${abbv}`);
                            ctx.channel.send(createEmbed(null, '+version setserver', 'Set server version successfully.', false));
                        }
                    });
                } else {
                    prefs.version = abbv;

                    prefs.save((err, preference) => {
                        if (err || !preference) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version setserver - cannot overwrite preference');
                            ctx.channel.send(createEmbed(null, '+version setserver', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `version setserver ${abbv} (overwrite)`);
                            ctx.channel.send(createEmbed(null, '+version setserver', 'Set server version successfully.', false));
                        }
                    });
                }
            });
        }).catch(() => {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `version setserver - invalid version (${abbv})`);
            ctx.channel.send(createEmbed(null, '+version setserver', `${abbv} not in database`, true));
        });
    }

    async getVersion(ctx: Context): Promise<void> {
        const lang = ctx.language;
        
        const userVersion = await Version.findOne({ abbv: ctx.preferences.version }).exec();
        const guildVersion = await Version.findOne({ abbv: ctx.guildPreferences.version }).exec();

        const message = `${lang.getString('versionused').replace('<version>', '**' + userVersion.name + '**')}\n` +
        `${lang.getString('guildversionused').replace('<version>', '**' + guildVersion.name + '**')}\n\n` +
        `__**${lang.getString('subcommands')}**__\n` +
        `**${lang.getCommand('set')}** - ${lang.getString('setversionusage')}\n` +
        `**${lang.getCommand('setserver')}** - ${lang.getString('setserverversionusage')}`;
                
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'version');
        ctx.channel.send(createEmbed(null, '+version', message, false));
    }

    processCommand(ctx: Context, params: Array<string>): void {
        const subcommand = params[0];
        const args = params.slice(1);

        switch (subcommand) {
            case 'set':
                this.setUserVersion(ctx, args[0]);
                break;
            case 'setserver':
                checkGuildPermissions(ctx, (hasPermission) => {
                    if (hasPermission) {
                        this.setGuildVersion(ctx, args[0]);
                    } else {
                        Language.findOne({ user: ctx.id }, (err, lang) => {
                            ctx.channel.send(createEmbed(null, '+version setserver', lang.getString('noguildperm'), true));
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version setserver - no permission');
                        });
                    }
                });
                break;
            default:
                this.getVersion(ctx);
                break;
        }
    }
}