import Context from '../../models/context';
import Language from '../../models/language';
import Preference from '../../models/preference';

import { createEmbed } from '../../helpers/embed_builder';

import defaultUserPreferences from '../../helpers/default_user_preference.json';

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

    setUserLanguage(ctx: Context, objectName: string): void {
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
                        language: objectName,
                        ...defaultUserPreferences
                    };

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

    getUserLanguage(ctx: Context): void {
        Preference.findOne({ user: ctx.id }, (err, prefs) => {
            if (err || !prefs) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'language - cannot save preference');
                ctx.channel.send(createEmbed(null, '+language', 'Preferences not found.', true));
            } else {
                Language.findOne({ objectName: ctx.preferences.language }, (err, lang) => {
                    const message = `
                    ${lang.getString('languageused')}
                        
                    __**${lang.getString('subcommands')}**__
                    **${lang.getCommand('set')}** - ${lang.getString('setlanguageusage')}
                    **${lang.getCommand('setserver')}** - ${lang.getString('setserverlanguageusage')}
                    `;

                    ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'language');
                    ctx.channel.send(createEmbed(null, '+language', message, false));
                });
            }
        });
    }

    processCommand(ctx: Context, params: Array<string>): void {
        const subcommand = params[0];
        const args = params.slice(1);

        switch (subcommand) {
            case 'set':
                this.setUserLanguage(ctx, args[0]);
                break;
            default:
                this.getUserLanguage(ctx);
                break;
        }
    }
}