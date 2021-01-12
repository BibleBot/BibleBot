import { Client, TextChannel } from 'discord.js';
import * as cron from 'node-cron';
import * as moment from 'moment';
import 'moment-timezone';

import GuildPreference from '../models/guild_preference';
import { DailyVerseRouter } from '../routes/resources/daily_verse';
import Context from '../models/context';
import Language from '../models/language';

const dailyVerseRouter = DailyVerseRouter.getInstance();

export const startDailyVerse = (bot: Client): void => {
    cron.schedule('* * * * *', () => {
        const currentTime = moment().tz('UTC').format('HH:mm');
        
        GuildPreference.find({ dailyVerseTime: currentTime }, (err, guilds) => {
            if (err) {
                return;
            } else if (guilds.length > 0) {
                for (const guildPref of guilds) {
                    try {
                        bot.guilds.cache.filter((guild) => { return guildPref.guild == guild.id; }).forEach((guild) => {
                            const chan = guild.channels.resolve(guildPref.dailyVerseChannel);
                            const ctx = new Context('auto', bot, (chan as TextChannel), guild, null, null,
                                                    { headings: true, verseNumbers: true, display: 'embed' }, guildPref, null);

                            Language.findOne({ objectName: guildPref.language }, (err, lang) => {
                                (chan as TextChannel).send(lang.getString('votd'));
                                dailyVerseRouter.sendDailyVerse(ctx, {
                                    version: guildPref.version
                                });
                            });
                        });
                    } catch {
                        continue;
                    }
                }
            }
        });
    }).start();
};