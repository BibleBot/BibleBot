import * as fs from 'fs';
import * as ini from 'ini';

import { log } from './helpers/logger';

import Context from './models/context';
import Language from './models/language';
import Preference from './models/preference';
import GuildPreference from './models/guild_preference';

import { CommandsRouter } from './routes/commands';
import { VersesRouter } from './routes/verses';

import { fetchBookNames } from './helpers/name_fetcher';

import * as defaultUserPreferences from './helpers/default_user_preference.json';
import * as defaultGuildPreferences from './helpers/default_guild_preference.json';

import * as mongoose from 'mongoose';

import { Client } from 'discord.js';
const bot = new Client({shardCount: 12});

const config = ini.parse(fs.readFileSync(`${__dirname}/config.ini`, 'utf-8'));

const commandsRouter = CommandsRouter.getInstance();
const versesRouter = VersesRouter.getInstance();

import * as heartbeats from './tasks/heartbeat';
import * as dailyVerses from './tasks/daily_verses';
import handleError from './helpers/error_handler';
import { checkBotPermissions } from './helpers/permissions';

const connect = () => {
    mongoose.connect('mongodb://localhost:27017/db', { useNewUrlParser: true, useUnifiedTopology: true }).then(() => {
        return log('info', null, 'connected to db');
    }).catch((err) => {
        log('err', null, `error connecting to database: ${err}`);
        return process.exit(1);
    });
};

bot.on('ready', () => {
    heartbeats.startHeartbeatMonitor(bot);
    log('info', null, 'started heartbeat monitor');

    dailyVerses.startDailyVerse(bot);
    log('info', null, 'started automatic daily verses');

    log('info', null, 'initialization complete');
});

bot.on('error', (error) => {
    handleError(error);
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
        
        if (!checkBotPermissions(message.guild)) {
            // If the bot doesn't have the necessary permissions, don't pursue further.
            return;
        }

        guildID = message.guild.id;
    }

    Preference.findOne({ user: message.author.id }, (err, prefs) => {
        if (err || !prefs) {
            prefs = { ...defaultUserPreferences };
        }

        GuildPreference.findOne({ guild: guildID }, (err, gPrefs) => {
            if (err || !gPrefs || guildID == null) {
                gPrefs = { ...defaultGuildPreferences };
            }

            Language.findOne({ objectName: prefs.language }, (err, lang) => {
                if (err) {
                    throw new Error('Unable to obtain language, probable database error.');
                }
                
                const ctx = new Context(message.author.id, bot, message.channel, message.guild, message.content, lang, prefs, gPrefs, message);
    
                const prefix = ctx.msg.split(' ')[0].slice(0, 1);
                const potentialCommand = lang.getCommandKey(ctx.msg.split(' ')[0].slice(1));
                let couldBeRescue = false;

                if (prefix == gPrefs.prefix || prefix == config.biblebot.commandPrefix) {
                    if (potentialCommand == 'biblebot') {
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

log('info', null, `BibleBot v${process.env.npm_package_version} by Evangelion Ltd.`);

fetchBookNames(config.biblebot.dry == 'True').then(() => {
    connect();
    mongoose.connection.on('disconnected', connect);

    fs.writeFile(__dirname + '/helpers/existing_paginators.json', JSON.stringify({ 'userIDs': [ ] }), (err) => {
        if (err) {
            log('info', null, 'unable to create existing_paginators.json');
        } else {
            log('info', null, 'reset existing paginators');
        }
    });

    bot.login(config.biblebot.token);
});