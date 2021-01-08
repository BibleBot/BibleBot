import Context from '../../models/context';
import Language from '../../models/language';
import Preference from '../../models/preference';

import { createEmbed } from '../../helpers/embed_builder';

import * as defaultUserPreferences from '../../helpers/default_user_preference.json';

export class FormattingSettingsRouter {
    private static instance: FormattingSettingsRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): FormattingSettingsRouter {
        if (!FormattingSettingsRouter.instance) {
            FormattingSettingsRouter.instance = new FormattingSettingsRouter();
        }

        return FormattingSettingsRouter.instance;
    }

    setVerseNumbers(ctx: Context, args: string[]): void {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, '+formatting setversenumbers', lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setversenumbers - no parameter');
            return;
        }

        const value = args[0];
        let actualValue = null;

        switch (value) {
            case 'enable':
            case 'true':
                actualValue = true;
                break;
            case 'disable':
            case 'false':
                actualValue = false;
                break;
            default:
                ctx.channel.send(createEmbed(null, '+formatting setversenumbers', lang.getString('otherformatfail'), true));
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setversenumbers - invalid parameter');
                return;
        }

        if (actualValue == null) {
            return;
        }

        Preference.findOne({ user: ctx.id }, (err, prefs) => {
            if (err) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setversenumbers - cannot save preference');
                ctx.channel.send(createEmbed(null, '+formatting setversenumbers', 'Failed to set preference.', true));
            } else if (!prefs) {
                const derivedFromDefault = {
                    user: ctx.id,
                    ...defaultUserPreferences
                };

                derivedFromDefault.verseNumbers = actualValue;

                const newPreference = new Preference(derivedFromDefault);

                newPreference.save((err, prefs) => {
                    if (err || !prefs) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setversenumbers - cannot save new preference');
                        ctx.channel.send(createEmbed(null, '+formatting setversenumbers', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `formatting setversenumbers ${value}`);
                        ctx.channel.send(createEmbed(null, '+formatting setversenumbers', 'Set verse numbers successfully.', false));
                    }
                });
            } else {
                prefs.verseNumbers = actualValue;

                prefs.save((err, preference) => {
                    if (err || !preference) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setversenumbers - cannot overwrite preference');
                        ctx.channel.send(createEmbed(null, '+formatting setversenumbers', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `formatting setversenumbers ${value} (overwrite)`);
                        ctx.channel.send(createEmbed(null, '+formatting setversenumbers', 'Set verse numbers successfully.', false));
                    }
                });
            }
        });
    }

    setHeadings(ctx: Context, args: string[]): void {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, '+formatting setheadings', lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setheadings - no parameter');
            return;
        }

        const value = args[0];
        let actualValue = null;

        switch (value) {
            case 'enable':
            case 'true':
                actualValue = true;
                break;
            case 'disable':
            case 'false':
                actualValue = false;
                break;
            default:
                ctx.channel.send(createEmbed(null, '+formatting setheadings', lang.getString('otherformatfail'), true));
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setheadings - invalid parameter');
                return;
        }

        if (actualValue == null) {
            return;
        }

        Preference.findOne({ user: ctx.id }, (err, prefs) => {
            if (err) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setheadings - cannot save preference');
                ctx.channel.send(createEmbed(null, '+formatting setheadings', 'Failed to set preference.', true));
            } else if (!prefs) {
                const derivedFromDefault = {
                    user: ctx.id,
                    ...defaultUserPreferences
                };

                derivedFromDefault.headings = actualValue;

                const newPreference = new Preference(derivedFromDefault);

                newPreference.save((err, prefs) => {
                    if (err || !prefs) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setheadings - cannot save new preference');
                        ctx.channel.send(createEmbed(null, '+formatting setheadings', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `formatting setheadings ${value}`);
                        ctx.channel.send(createEmbed(null, '+formatting setheadings', 'Set headings successfully.', false));
                    }
                });
            } else {
                prefs.headings = actualValue;

                prefs.save((err, preference) => {
                    if (err || !preference) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setheadings - cannot overwrite preference');
                        ctx.channel.send(createEmbed(null, '+formatting setheadings', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `formatting setheadings ${value} (overwrite)`);
                        ctx.channel.send(createEmbed(null, '+formatting setheadings', 'Set headings successfully.', false));
                    }
                });
            }
        });
    }

    setDisplayStyle(ctx: Context, args: string[]): void {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, '+formatting setdisplay', lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setdisplay - no parameter');
            return;
        }

        const value = args[0];

        if (!['code', 'embed', 'blockquote', 'default'].includes(value)) {
            ctx.channel.send(createEmbed(null, '+formatting setdisplay', lang.getString('otherformatfail'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setdisplay - invalid parameter');
            return;
        }

        Preference.findOne({ user: ctx.id }, (err, prefs) => {
            if (err) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setheadings - cannot save preference');
                ctx.channel.send(createEmbed(null, '+formatting setdisplay', 'Failed to set preference.', true));
            } else if (!prefs) {
                const derivedFromDefault = {
                    user: ctx.id,
                    ...defaultUserPreferences
                };

                derivedFromDefault.display = value;

                const newPreference = new Preference(derivedFromDefault);

                newPreference.save((err, prefs) => {
                    if (err || !prefs) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setdisplay - cannot save new preference');
                        ctx.channel.send(createEmbed(null, '+formatting setdisplay', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `formatting setdisplay ${value}`);
                        ctx.channel.send(createEmbed(null, '+formatting setdisplay', 'Set display style successfully.', false));
                    }
                });
            } else {
                prefs.display = value;

                prefs.save((err, preference) => {
                    if (err || !preference) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'formatting setdisplay - cannot overwrite preference');
                        ctx.channel.send(createEmbed(null, '+formatting setdisplay', 'Failed to set preference.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `formatting setdisplay ${value} (overwrite)`);
                        ctx.channel.send(createEmbed(null, '+formatting setdisplay', 'Set display style successfully.', false));
                    }
                });
            }
        });
    }

    async getFormattingSettings(ctx: Context): Promise<void> {
        const lang = ctx.language;

        const verseNumbersStatus = ctx.preferences.verseNumbers == true ? lang.getString('enabled') : lang.getString('disabled');
        const headingsStatus = ctx.preferences.headings == true ? lang.getString('enabled') : lang.getString('disabled');

        const message = `${lang.getString('versenumbers').replace('<status>', '**' + verseNumbersStatus + '**')}\n` +
        `${lang.getString('headings').replace('<status>', '**' + headingsStatus + '**')}\n` +
        `${lang.getString('formatting').replace('<value>', '**' + ctx.preferences.display + '**')}\n\n` +
        `__**${lang.getString('subcommands')}**__\n` +
        `**${lang.getCommand('setversenumbers')}** - ${lang.getString('setversenumbersusage')}\n` + 
        `**${lang.getCommand('setheadings')}** - ${lang.getString('setheadingsusage')}\n` +
        `**${lang.getCommand('setdisplay')}** - ${lang.getString('setdisplayusage')}`;
                
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'formatting');
        ctx.channel.send(createEmbed(null, '+formatting', message, false));
    }

    processCommand(ctx: Context, params: Array<string>): void {
        const subcommand = params[0];
        const args = params.slice(1);

        switch (subcommand) {
            case 'setversenumbers':
                this.setVerseNumbers(ctx, args);
                break;
            case 'setheadings':
                this.setHeadings(ctx, args);
                break;
            case 'setdisplay':
                this.setDisplayStyle(ctx, args);
                break;
            default:
                this.getFormattingSettings(ctx);
                break;
        }
    }
}