import Context from '../../models/context';
import Version from '../../models/version';
import Language from '../../models/language';
import Preference from '../../models/preference';
import GuildPreference from '../../models/guild_preference';

import { createEmbed, translateCommand } from '../../helpers/embed_builder';
import { checkGuildPermissions } from '../../helpers/permissions';
import { Paginator } from '../../helpers/paginator';

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

    setUserVersion(ctx: Context, args: string[]): void {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'set']), lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version set - no parameter');
            return;
        }

        const abbv = args[0];

        Version.findOne({ abbv }).then((version) => {
            if (!version) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `version set - invalid version (${abbv})`);
                ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'set']), `${abbv} not in database`, true));
                return;
            }

            Preference.findOne({ user: ctx.id }, (err, prefs) => {
                if (err) {
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version set - cannot save preference');
                    ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'set']), 'Failed to set preference.', true));
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
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'set']), 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `version set ${abbv}`);
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'set']), 'Set version successfully.', false));
                        }
                    });
                } else {
                    prefs.version = abbv;

                    prefs.save((err, preference) => {
                        if (err || !preference) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version set - cannot overwrite preference');
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'set']), 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `version set ${abbv} (overwrite)`);
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'set']), 'Set version successfully.', false));
                        }
                    });
                }
            });
        }).catch(() => {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `version set - invalid version (${abbv})`);
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'set']), `${abbv} not in database`, true));
        });
    }

    setGuildVersion(ctx: Context, args: string[]): void {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'setserver']), lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version setserver - no parameter');
            return;
        }

        const abbv = args[0];

        Version.findOne({ abbv }).then((version) => {
            if (!version) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `version setserver - invalid version (${abbv})`);
                ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'setserver']), `${abbv} not in database`, true));
                return;
            }

            GuildPreference.findOne({ user: ctx.id }, (err, prefs) => {
                if (err) {
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version setserver - cannot save preference');
                    ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'setserver']), 'Failed to set preference.', true));
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
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'setserver']), 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `version set ${abbv}`);
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'setserver']), 'Set server version successfully.', false));
                        }
                    });
                } else {
                    prefs.version = abbv;

                    prefs.save((err, preference) => {
                        if (err || !preference) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version setserver - cannot overwrite preference');
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'setserver']), 'Failed to set preference.', true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `version setserver ${abbv} (overwrite)`);
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'setserver']), 'Set server version successfully.', false));
                        }
                    });
                }
            });
        }).catch(() => {
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `version setserver - invalid version (${abbv})`);
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'setserver']), `${abbv} not in database`, true));
        });
    }

    async getVersionList(ctx: Context): Promise<void> {
        const versions = await Version.find({}).sort({ abbv: 'ascending' }).lean();

        const pages = [];
        const maxResultsPerPage = 25;
        const versionsUsed = [];
        let totalPages = Number(Math.ceil(versions.length / maxResultsPerPage));

        if (totalPages == 0) {
            totalPages = 1;
        }

        for (let i = 0; i < totalPages; i++) {
            const pageCounter = ctx.language.getString('pageOf').replace('<num>', i + 1)
                                                                .replace('<total>', totalPages);
                                                                
            const embed = createEmbed(null, `${translateCommand(ctx, ['+', 'version', 'list'])} - ${pageCounter}`, null, false);

            let count = 0;
            let versionList = '';

            for (const version of versions) {
                if (count < maxResultsPerPage) {
                    if (!versionsUsed.includes(version.name)) {
                        versionList += `${version.name}\n`;

                        versionsUsed.push(version.name);
                        versions.shift();
                        count++;
                    }
                }
            }

            embed.setDescription(versionList);

            pages.push(embed);
        }

        try {
            const paginator = new Paginator(pages, ctx.id, 180);
            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'version list');
            paginator.run(ctx.channel);
        } catch (err) {
            if (err.message == 'User already using paginator.') {
                ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'list']), ctx.language.getString('plswait'), true));
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, err.message);
            }
        }
    }

    async getVersionInfo(ctx: Context, args: string[]): Promise<void> {
        const lang = ctx.language;

        if (args.length == 0) {
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'info']), lang.getString('expectedparameter'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version info - no parameter');
            return;
        }

        const abbv = args[0];

        const version = await Version.findOne({ abbv }).exec();

        if (version) {
            const message = lang.getString('versioninfo').replace('<versionname>', version.name)
                                                         .replace('<hasOT>', `**${version.supportsOldTestament ? lang.getArgument('yes') : lang.getArgument('no')}**`)
                                                         .replace('<hasNT>', `**${version.supportsNewTestament ? lang.getArgument('yes') : lang.getArgument('no')}**`)
                                                         .replace('<hasDEU>', `**${version.supportsDeuterocanon ? lang.getArgument('yes') : lang.getArgument('no')}**`);
            
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'info']), message, false));
            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `version info ${abbv}`);
        } else {
            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'info']), lang.getString('versioninfofailed'), true));
            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `version info - invalid version (${abbv})`);
        }
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
        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version']), message, false));
    }

    processCommand(ctx: Context, params: Array<string>): void {
        const subcommand = params[0];
        const args = params.slice(1);

        switch (subcommand) {
            case 'set':
                this.setUserVersion(ctx, args);
                break;
            case 'setserver':
                checkGuildPermissions(ctx, (hasPermission) => {
                    if (hasPermission) {
                        this.setGuildVersion(ctx, args);
                    } else {
                        Language.findOne({ user: ctx.id }, (err, lang) => {
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'version', 'setserver']), lang.getString('noguildperm'), true));
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version setserver - no permission');
                        });
                    }
                });
                break;
            case 'list':
                this.getVersionList(ctx);
                break;
            case 'info':
                this.getVersionInfo(ctx, args);
                break;
            default:
                this.getVersion(ctx);
                break;
        }
    }
}