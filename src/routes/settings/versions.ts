import Context from '../../models/context';
import Version from '../../models/version';
import Language from '../../models/language';
import Preference from '../../models/preference';

import { createEmbed } from '../../helpers/embed_builder';

import defaultUserPreferences from '../../helpers/default_user_preference.json';

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
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `setversion - invalid version (${abbv})`);
                ctx.channel.send(createEmbed(null, '+setversion', `${abbv} not in database`, true));
                return;
            }

            Preference.findOne({ user: ctx.id }, (err, prefs) => {
                if (err) {
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'setversion - cannot save preference');
                    ctx.channel.send(createEmbed(null, '+setversion', 'Failed to set preference.', true));
                } else if (!prefs) {
                    const derivedFromDefault = {
                        user: ctx.id,
                        version: abbv,
                        ...defaultUserPreferences
                    };

                    const newPreference = new Preference(derivedFromDefault);

                    newPreference.save((err, prefs) => {
                        if (err || !prefs) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'setversion - cannot save new preference');
                            ctx.channel.send(createEmbed(null, '+setversion', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `setversion ${abbv}`);
                            ctx.channel.send(createEmbed(null, '+setversion', 'Set version successfully.', false));
                        }
                    });
                } else {
                    prefs.version = abbv;

                    prefs.save((err, preference) => {
                        if (err || !preference) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'setversion - cannot overwrite preference');
                            ctx.channel.send(createEmbed(null, '+setversion', 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `setversion ${abbv} (overwrite)`);
                            ctx.channel.send(createEmbed(null, '+setversion', 'Set version successfully.', false));
                        }
                    });
                }
            });
        }).catch(() => {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `setversion - invalid version (${abbv})`);
            ctx.channel.send(createEmbed(null, '+setversion', `${abbv} not in database`, true));
        });
    }

    getUserVersion(ctx: Context): void {
        Preference.findOne({ user: ctx.id }, (err, prefs) => {
            if (err || !prefs) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version - cannot save preference');
                ctx.channel.send(createEmbed(null, '+version', 'Preferences not found.', true));
            } else {
                Language.findOne({ objectName: prefs.language }, (err, lang) => {
                    Version.findOne({ abbv: prefs.version }, (err, ver) => {
                        const message = `
                        ${lang.getString('versionused').replace('<version>', '**' + ver['name'] + '**')}
                        
                        __**${lang.getString('subcommands')}**__
                        **${lang.getCommand('set')}** - ${lang.getString('setversionusage')}
                        **${lang.getCommand('setserver')}** - ${lang.getString('setserverversionusage')}
                        `;
        
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'version');
                        ctx.channel.send(createEmbed(null, '+version', message, false));
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
                this.setUserVersion(ctx, args[0]);
                break;
            default:
                this.getUserVersion(ctx);
                break;
        }
    }
}