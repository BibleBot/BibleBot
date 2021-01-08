import axios from 'axios';
import { JSDOM } from 'jsdom';
import * as moment from 'moment';
import 'moment-timezone';

import Context from '../../models/context';
import Version from '../../models/version';
import Language from '../../models/language';
import GuildPreference from '../../models/guild_preference';

import * as utils from '../../helpers/verse_utils';
import { createEmbed, translateCommand } from '../../helpers/embed_builder';
import { checkGuildPermissions } from '../../helpers/permissions';

import * as defaultGuildPreferences from '../../helpers/default_guild_preference.json';

export class DailyVerseRouter {
    private static instance: DailyVerseRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): DailyVerseRouter {
        if (!DailyVerseRouter.instance) {
            DailyVerseRouter.instance = new DailyVerseRouter();
        }

        return DailyVerseRouter.instance;
    }

    sendDailyVerse(ctx: Context, task?: Record<string, string>): void {
        axios.get('https://www.biblegateway.com/reading-plans/verse-of-the-day/next').then((res) => {
            try {
                const { document } = (new JSDOM(res.data)).window;

                const verse = document.getElementsByClassName('rp-passage-display')[0].textContent;
                let queryVersion = 'RSV';

                if (!task) {
                    if (ctx.preferences.version) {
                        queryVersion = ctx.preferences.version;
                    }
                } else {
                    queryVersion = task.version;
                }

                Version.findOne({ abbv: queryVersion }, (err, version) => {
                    utils.processVerse(ctx, version, verse);
                    ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'dailyverse');
                });
            } catch (err) {
                return;
            }
        });
    }

    async setupAutomation(ctx: Context, args: Array<string>): Promise<void> {
        const lang = ctx.language;

        if (args.length != 2) {
            ctx.channel.send(ctx.language.getString('setvotdtimeusage'));
        } else {
            const tz = args[1];

            // take 24h format, assign it the specified tz, convert to utc, format back to time-only
            const time = moment.tz(`1970-01-01 ${args[0]}`, tz).tz('UTC').format('HH:mm');

            GuildPreference.findOne({ guild: ctx.guild.id }, (err, gPrefs) => {
                if (err) {
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'dailyverse setup - cannot save preference');
                    ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'setup']), lang.getString('failedpreference'), true));
                } else if (!gPrefs) {
                    const derivedFromDefault = {
                        guild: ctx.guild.id,
                        dailyVerseTime: time,
                        dailyVerseChannel: ctx.channel.id,
                        dailyVerseTz: tz,
                        ...defaultGuildPreferences
                    };


                    const newPreference = new GuildPreference(derivedFromDefault);

                    newPreference.save((err, prefs) => {
                        if (err || !prefs) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'dailyverse setup - cannot save new preference');
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'setup']), lang.getString('failedpreference'), true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `dailyverse setup ${args[0]} ${tz}`);
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'setup']), lang.getString('setvotdtimesuccess'), false));
                        }
                    });
                } else {
                    gPrefs.dailyVerseTime = time;
                    gPrefs.dailyVerseChannel = ctx.channel.id;
                    gPrefs.dailyVerseTz = tz;

                    gPrefs.save((err, preference) => {
                        if (err || !preference) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'dailyverse setup - cannot overwrite preference');
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'setup']), lang.getString('failedpreference'), true));
                        } else {
                            ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `dailyverse setup ${args[0]} ${tz} (overwrite)`);
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'setup']), lang.getString('setvotdtimesuccess'), false));
                        }
                    });
                }
            });
        }
    }

    clearAutomation(ctx: Context): void {
        const lang = ctx.language;

        GuildPreference.findOne({ guild: ctx.guild.id }, (err, gPrefs) => {
            if (err) {
                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'dailyverse clear - cannot save preference');
                ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'clear']), lang.getString('failedpreference'), true));
            } else if (!gPrefs) {
                const derivedFromDefault = {
                    guild: ctx.guild.id,
                    dailyVerseTime: null,
                    dailyVerseChannel: null,
                    dailyVerseTz: null,
                    ...defaultGuildPreferences
                };


                const newPreference = new GuildPreference(derivedFromDefault);

                newPreference.save((err, prefs) => {
                    if (err || !prefs) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'dailyverse clear - cannot save new preference');
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'clear']), lang.getString('failedpreference'), true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'dailyverse clear');
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'clear']), lang.getString('clearvotdtimesuccess'), false));
                    }
                });
            } else {
                gPrefs.dailyVerseTime = null;
                gPrefs.dailyVerseChannel = null;
                gPrefs.dailyVerseTz = null;

                gPrefs.save((err, preference) => {
                    if (err || !preference) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'dailyverse clear - cannot overwrite preference');
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'clear']), lang.getString('failedpreference'), true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'dailyverse clear (overwrite)');
                        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'clear']), lang.getString('clearvotdtimesuccess'), false));
                    }
                });
            }
        });
    }

    automationStatus(ctx: Context): void {
        const lang = ctx.language;

        GuildPreference.findOne({ guild: ctx.guild.id }, async (err, gPrefs) => {
            // TODO: use guild language
            try {
                const checkArray = [gPrefs.dailyVerseTime, gPrefs.dailyVerseChannel, gPrefs.dailyVerseTz];
                for (const check of checkArray) {
                    if (check == null) {
                        throw Error();
                    }
                }

                // I hate how messy moment makes this but it's still 100x easier than normal JS times.
                const version = await Version.findOne({ abbv: gPrefs.version }).exec();
                const time = moment.tz(`1970-01-01 ${gPrefs.dailyVerseTime}`, 'UTC').tz(gPrefs.dailyVerseTz).format('hh:mma');

                const status = lang.getString('votdtimeused').replace('<time>', time)
                                                             .replace('<tz>', `**${gPrefs.dailyVerseTz}**`)
                                                             .replace('<channel>', `<#${gPrefs.dailyVerseChannel}>`)
                                                             .replace('<version>', `**${version.name}**`);

                ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'dailyverse status');
                ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'status']), status, false));
            } catch {
                const status = lang.getString('novotdtimeused');

                ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'dailyverse status');
                ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'status']), status, true));
            }
            
        });
    }

    processCommand(ctx: Context, params: Array<string>): void {
        const subcommand = ctx.language.getCommandKey(params[0]);
        const args = params.slice(1);

        switch (subcommand) {
            case 'setup':
                checkGuildPermissions(ctx, (hasPermission) => {
                    if (hasPermission) {
                        this.setupAutomation(ctx, args);
                    } else {
                        Language.findOne({ user: ctx.id }, (err, lang) => {
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'setup']), lang.getString('noguildperm'), true));
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'dailyverse setup - no permission');
                        });
                    }
                });
                break;
            case 'status':
                this.automationStatus(ctx);
                break;
            case 'clear':
                checkGuildPermissions(ctx, (hasPermission) => {
                    if (hasPermission) {
                        this.clearAutomation(ctx);
                    } else {
                        Language.findOne({ user: ctx.id }, (err, lang) => {
                            ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'dailyverse', 'clear']), lang.getString('noguildperm'), true));
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'dailyverse clear - no permission');
                        });
                    }
                });
                break;
            default:
                this.sendDailyVerse(ctx);
                break;
        }
    }
}