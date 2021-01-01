import * as fs from 'fs';
import * as ini from 'ini';

import { log } from './helpers/logger';

import Context from './models/context';
import Preference from './models/preference';
import GuildPreference from './models/guild_preference';

import { CommandsRouter } from './routes/commands';
import { VersesRouter } from './routes/verses';

import { fetchBookNames } from './helpers/name_fetcher';

import * as defaultUserPreferences from './helpers/default_user_preference.json';
import * as defaultGuildPreferences from './helpers/default_guild_preference.json';

import * as mongoose from 'mongoose';

import { Client } from 'discord.js';
const bot = new Client({shards: 'auto'});

const config = ini.parse(fs.readFileSync(`${__dirname}/config.ini`, 'utf-8'));

const commandsRouter = CommandsRouter.getInstance();
const versesRouter = VersesRouter.getInstance();

import * as heartbeats from './tasks/heartbeat';

const connect = () => {
    mongoose.connect('mongodb://localhost:27017/db', { useNewUrlParser: true, useUnifiedTopology: true }).then(() => {
        return log('info', null, 'connected to db');
    }).catch((err) => {
        log('err', null, `error connecting to database: ${err}`);
        return process.exit(1);
    });
};

let db;

bot.on('ready', () => {
    heartbeats.startHeartbeatMonitor(bot);
    log('info', null, 'initialization complete');
});

bot.on('error', (error) => {
    const date = new Date();
    const fileTimestamp = `${date.getFullYear()}-${date.getMonth()}-${date.getDate()}`;
    const errorTimestamp = `${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}`;

    const output = `${errorTimestamp}
    
    name: ${error.name}
    
    msg: ${error.message}
    
    stack: ${error.stack}
    
    ---`;

    const dir = `${__dirname}/../error_logs`;

    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir);
    }

    fs.appendFileSync(`${dir}/log-${fileTimestamp}.txt`, output);

    log('err', null, error.message);
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
    if (message.author.id !== config.biblebot.id) return; //devmode for now

    Preference.findOne({ user: message.author.id }, (err, prefs) => {
        if (err || !prefs) {
            prefs = { ...defaultUserPreferences };
        }

        GuildPreference.findOne({ guild: message.guild.id }, (err, gPrefs) => {
            if (err || !gPrefs) {
                gPrefs = { ...defaultGuildPreferences };
            }
    
            const ctx = new Context(message.author.id, bot, message.channel, message.guild, message.content, prefs, gPrefs, db, message);
    
            const potentialCommand = ctx.msg.split(' ')[0].slice(1);
    
            switch(prefs['input']) {
                case 'default': {
                    if (commandsRouter.isOwnerCommand(potentialCommand)) {
                        commandsRouter.processOwnerCommand(ctx);
                    } else if (commandsRouter.isCommand(potentialCommand)) {
                        commandsRouter.processCommand(ctx);
                    } else if (ctx.msg.includes(':')) {
                        versesRouter.processMessage(ctx, 'default');
                    }
    
                    break;
                }
                case 'erasmus': {
                    // tl;dr - Erasmus verse processing is invoked by mention in beginning of message
                    // or if verse is surrounded by square brackets or if message starts with '$'
                    if (ctx.msg.startsWith('$')) {
                            if (ctx.msg.includes(':')) {
                                versesRouter.processMessage(ctx, 'erasmus');
                            } else if (commandsRouter.isCommand(potentialCommand)) {
                                commandsRouter.processCommand(ctx);
                            }
                    }
    
                    break;
                }
            }
        });
    });

    
});

log('info', null, `BibleBot v${process.env.npm_package_version} by Evangelion Ltd.`);
fetchBookNames(config.biblebot.dry == 'True').then(() => {
    connect();
    mongoose.connection.on('disconnected', connect);
    db = mongoose.connection;

    bot.login(config.biblebot.token);
});