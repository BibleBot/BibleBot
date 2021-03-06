import * as fs from 'fs';
import * as ini from 'ini';

import { log } from './helpers/logger';

import Context from './models/context';
import Language from './models/language';
import { LanguageDocument } from './models/language';
import Preference from './models/preference';
import { PreferenceDocument } from './models/preference';
import GuildPreference from './models/guild_preference';
import { GuildPreferenceDocument } from './models/guild_preference';

import { CommandsRouter } from './routes/commands';
import { VersesRouter } from './routes/verses';

import * as defaultUserPreferences from './helpers/default_user_preference.json';
import * as defaultGuildPreferences from './helpers/default_guild_preference.json';

import * as mongoose from 'mongoose';

import { Client, DMChannel } from 'discord.js';

const config = ini.parse(fs.readFileSync(`${__dirname}/config.ini`, 'utf-8'));

const bot = new Client({
    retryLimit: 10,
    messageCacheMaxSize: 100,
    messageCacheLifetime: 600,
    messageEditHistoryMaxSize: 100
});

const commandsRouter = CommandsRouter.getInstance();
const versesRouter = VersesRouter.getInstance();

import * as heartbeats from './tasks/heartbeat';
import * as dailyVerses from './tasks/daily_verses';
import * as memoryUsage from './tasks/memory_monitor';
import handleError from './helpers/error_handler';
import { checkBotPermissions } from './helpers/permissions';

process.on('unhandledRejection', (err: Error, promise) => {
    console.log('---------');
    console.log('Unhandled promise rejection:');
    console.log(promise);
    console.log('Error:', err.message);
    console.log(err.stack);
    console.log('---------');
});

bot.on('ready', () => {
    log('info', bot.shard.ids[0], 'shard connected to db');

    heartbeats.startHeartbeatMonitor(bot);
    log('info', bot.shard.ids[0], 'started heartbeat monitor');

    dailyVerses.startDailyVerse(bot);
    log('info', bot.shard.ids[0], 'started automatic daily verses');

    memoryUsage.startMemoryMonitor(bot);
    log('info', bot.shard.ids[0], 'started memory monitor');
});

bot.on('error', (error) => {
    handleError(error);
});

bot.on('debug', (debug) => {
    if (process.env.NODE_ENV == 'dev') {
        console.log(debug);
    }
});

bot.on('shardReady', shard => {
    
    log('info', shard, 'shard connected');
    
    bot.user.setPresence({
        activity: {
            name: `${config.biblebot.commandPrefix}biblebot v${process.env.npm_package_version} | Shard: ${String(shard + 1)} / ${String(bot.options.shardCount)}`
        },
        shardID: shard
    });
});

bot.on('shardDisconnect', (_, shard) => {
    log('info', shard, 'shard disconnected');
});
bot.on('shardReconnecting', shard => {
    log('info', shard, 'shard reconnecting');
});

bot.on('shardResume', shard => {
    log('info', shard, 'shard resuming');
});

bot.on('message', message => {
    if (message.author.id === bot.user.id) return;
    let guildID = null;
    
    if (message.guild) {
        // Ignore Discord.bots.gg and Top.gg's server.
        // The bot has to be there in order to be listed.

        if (['110373943822540800', '264445053596991498'].includes(message.guild.id)) {
            return;
        }
        
        if (!(message.channel instanceof DMChannel)) {
            let shouldLog = false;

            if (message.content == '+permcheck') {
                shouldLog = true;
            }

            if (!checkBotPermissions(message.channel, message.guild, shouldLog)) {
                // If the bot doesn't have the necessary permissions, don't pursue further.
                return;
            }
        }

        guildID = message.guild.id;
    } else {
        return;
    }

    Preference.findOne({ user: message.author.id }, (err, prefs: PreferenceDocument) => {
        if (err || !prefs) {
            prefs = ({ ...defaultUserPreferences } as PreferenceDocument);
        }

        GuildPreference.findOne({ guild: guildID }, (err, gPrefs: GuildPreferenceDocument) => {
            if (err || !gPrefs) {
                gPrefs = ({ ...defaultGuildPreferences } as GuildPreferenceDocument);
            }

            Language.findOne({ objectName: prefs.language }, (err, lang: LanguageDocument) => {
                if (err) {
                    throw new Error('Unable to obtain language, probable database error.');
                }

                if (message.author.bot) {
                    prefs = ({ ...defaultUserPreferences } as PreferenceDocument);

                    prefs['user'] = message.author.id;
                    prefs['version'] = gPrefs.version;
                    prefs['language'] = gPrefs.language;
                }
                
                const ctx = new Context(message.author.id, bot, message.channel, message.guild, message.content, lang, prefs, gPrefs, message);
    
                const prefix = ctx.msg.split(' ')[0].slice(0, 1);
                const firstItem = ctx.msg.split(' ')[0].slice(1);
                const potentialCommand = lang.getCommandKey(firstItem);
                let couldBeRescue = false;

                if (prefix == gPrefs.prefix || prefix == config.biblebot.commandPrefix) {
                    if (potentialCommand == 'biblebot' || firstItem == 'biblebot') {
                        couldBeRescue = true;
                    }
                }
        
                try {
                    switch(prefs['input']) {
                        case 'default': {
                            if (prefix == gPrefs.prefix || couldBeRescue) {
                                if (commandsRouter.isOwnerCommand(potentialCommand)) {
                                    commandsRouter.processOwnerCommand(ctx);
                                } else if (commandsRouter.isCommand(potentialCommand)) {
                                    commandsRouter.processCommand(ctx);
                                }
                            } else if (ctx.msg.includes(':')) {
                                versesRouter.processMessage(ctx, 'default');
                            }
            
                            break;
                        }
                        //case 'erasmus': {
                        //    // tl;dr - Erasmus verse processing is invoked by mention in beginning of message
                        //    // or if verse is surrounded by square brackets or if message starts with '$'
                        //    if (ctx.msg.startsWith('$')) {
                        //            if (ctx.msg.includes(':')) {
                        //                versesRouter.processMessage(ctx, 'erasmus');
                        //            } else if (commandsRouter.isCommand(potentialCommand)) {
                        //                commandsRouter.processCommand(ctx);
                        //            }
                        //    }
                        //
                        //    break;
                        //}
                    }
                } catch (err) {
                    handleError(err);
                }
            });
        });
    });

    
});


mongoose.connect(config.biblebot.mongoURL, { useNewUrlParser: true, useUnifiedTopology: true }).then(() => {
    bot.login(config.biblebot.token);
}).catch((err) => {
    log('err', null, `error connecting to database: ${err}`);
    return process.exit(1);
});