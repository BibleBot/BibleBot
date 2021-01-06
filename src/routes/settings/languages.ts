import Context from '../../models/context';
import Language from '../../models/language';
import Preference from '../../models/preference';
import GuildPreference from '../../models/guild_preference';

import { createEmbed } from '../../helpers/embed_builder';
import { checkGuildPermissions } from '../../helpers/permissions';

import * as defaultUserPreferences from '../../helpers/default_user_preference.json';
import * as defaultGuildPreferences from '../../helpers/default_guild_preference.json';

export class LanguageRouter {
    private static instance: LanguageRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): LanguageRouter {
        if (!LanguageRouter.instance) {
            LanguageRouter.instance = new LanguageRouter();
        }

        return LanguageRouter.instance;
    }

    setUserLanguage(ctx: Context, args: string[]): void {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, '+language set', lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language set - no parameter');
            return;
        }

        const objectName = args[0];

        Language.findOne({ objectName }).then((language) => {
            if (!language) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `language set - invalid language (${objectName})`);
                ctx.channel.send(createEmbed(null, '+language set', `${objectName} not in database`, true));
                return;
            }

            Preference.findOne({ user: ctx.id }, (err, prefs) => {
                if (err) {
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language set - cannot save preference');
                    ctx.channel.send(createEmbed(null, '+language set', 'Failed to set preference.', true));
                } else if (!prefs) {
                    const derivedFromDefault = {
                        user: ctx.id,
                        ...defaultUserPreferences
                    };

                    derivedFromDefault.language = objectName;

                    const newPreference = new Preference(derivedFromDefault);

                    newPreference.save((err, prefs) => {
                        if (err || !prefs) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language set - cannot save new preference');
                            ctx.channel.send(createEmbed(null, '+language set', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `language set ${objectName}`);
                            ctx.channel.send(createEmbed(null, '+language set', 'Set language successfully.', false));
                        }
                    });
                } else {
                    prefs.language = objectName;

                    prefs.save((err, preference) => {
                        if (err || !preference) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language set - cannot overwrite preference');
                            ctx.channel.send(createEmbed(null, '+language set', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `language set ${objectName} (overwrite)`);
                            ctx.channel.send(createEmbed(null, '+language set', 'Set language successfully.', false));
                        }
                    });
                }
            });
        }).catch(() => {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `language set - invalid version (${objectName})`);
            ctx.channel.send(createEmbed(null, '+language set', `${objectName} not in database`, true));
        });
    }

    setGuildLanguage(ctx: Context, args: string[]): void {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, '+language setserver', lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language setserver - no parameter');
            return;
        }

        const objectName = args[0];

        Language.findOne({ objectName }).then((language) => {
            if (!language) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `language setserver - invalid language (${objectName})`);
                ctx.channel.send(createEmbed(null, '+language setserver', `${objectName} not in database`, true));
                return;
            }

            GuildPreference.findOne({ guild: ctx.guild.id }, (err, prefs) => {
                if (err) {
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language setserver - cannot save preference');
                    ctx.channel.send(createEmbed(null, '+language setserver', 'Failed to set preference.', true));
                } else if (!prefs) {
                    const derivedFromDefault = {
                        guild: ctx.guild.id,
                        ...defaultGuildPreferences
                    };

                    derivedFromDefault.language = objectName;

                    const newPreference = new GuildPreference(derivedFromDefault);

                    newPreference.save((err, prefs) => {
                        if (err || !prefs) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language setserver - cannot save new preference');
                            ctx.channel.send(createEmbed(null, '+language setserver', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `language setserver ${objectName}`);
                            ctx.channel.send(createEmbed(null, '+language setserver', 'Set server language successfully.', false));
                        }
                    });
                } else {
                    prefs.language = objectName;

                    prefs.save((err, preference) => {
                        if (err || !preference) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language setserver - cannot overwrite preference');
                            ctx.channel.send(createEmbed(null, '+language setserver', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `language set ${objectName} (overwrite)`);
                            ctx.channel.send(createEmbed(null, '+language setserver', 'Set server language successfully.', false));
                        }
                    });
                }
            });
        }).catch(() => {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `language setserver - invalid version (${objectName})`);
            ctx.channel.send(createEmbed(null, '+language setserver', `${objectName} not in database`, true));
        });
    }

    getLanguage(ctx: Context): void {
        Preference.findOne({ user: ctx.id }, (err, prefs) => {
            if (err || !prefs) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language - cannot find preference');
                ctx.channel.send(createEmbed(null, '+language', 'Preferences not found.', true));
            } else {
                Language.findOne({ objectName: ctx.preferences.language }, (err, lang) => {
                    Language.findOne({ objectName: ctx.guildPreferences.language }, (err, gLang) => {
                        const message = `${lang.getString('languageused')}\n` +
                        `${gLang.getString('guildlanguageused')}\n\n` +
                        `__**${lang.getString('subcommands')}**__\n` +
                        `**${lang.getCommand('set')}** - ${lang.getString('setlanguageusage')}\n` +
                        `**${lang.getCommand('setserver')}** - ${lang.getString('setserverlanguageusage')}`;

                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'language');
                        ctx.channel.send(createEmbed(null, '+language', message, false));
                    });
                });
            }
        });
    }

    processCommand(ctx: Context, params: Array<string>): void {
        const subcommand = params[0];
        const args = params.slice(1);

        switch (subcommand) {
            case 'set':
                this.setUserLanguage(ctx, args);
                break;
            case 'setserver':
                checkGuildPermissions(ctx, (hasPermission) => {
                    if (hasPermission) {
                        this.setGuildLanguage(ctx, args);
                    } else {
                        Language.findOne({ user: ctx.id }, (err, lang) => {
                            ctx.channel.send(createEmbed(null, '+language setserver', lang.getString('noguildperm'), true));
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language setserver - no permission');
                        });
                    }
                });
                break;
            default:
                this.getLanguage(ctx);
                break;
        }
    }
}