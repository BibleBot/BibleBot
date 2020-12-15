import * as fs from 'fs';
import * as ini from 'ini';

import { log } from './helpers/logger';

import Context from './models/context';
import Version from './models/version';
import Language from './models/language';
import Preference from './models/preference';

import { CommandsRouter } from './routes/commands';
import { VersesRouter } from './routes/verses';

import { fetchBookNames } from './helpers/name_fetcher';

import * as mongoose from 'mongoose';

import { Client } from 'discord.js';
import language from './models/language';
const bot = new Client({shards: 'auto'});

const config = ini.parse(fs.readFileSync(`${__dirname}/config.ini`, 'utf-8'));

const commandsRouter = CommandsRouter.getInstance();
const versesRouter = VersesRouter.getInstance();

const connect = () => {
    mongoose.connect('mongodb://localhost:27017/db', { useNewUrlParser: true, useUnifiedTopology: true }).then(() => {
        return log('info', 0, 'connected to db');
    }).catch((err) => {
        log('err', 0, `error connecting to database: ${err}`);
        return process.exit(1);
    });
};

connect();
mongoose.connection.on('disconnected', connect);
const db = mongoose.connection;

bot.on('ready', () => {
    log('info', 0, 'initialization complete');
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

    log('err', 0, error.message);
});

bot.on('shardReady', shard => {
    log('info', shard + 1, 'shard connected');
    
    bot.user.setPresence({
        activity: {
            name: `${config.biblebot.commandPrefix}biblebot v${process.env.npm_package_version} | Shard: ${String(shard + 1)} / ${String(bot.options.shardCount)}`
        },
        shardID: shard
    });
});

bot.on('shardDisconnect', (_, shard) => {
    log('info', shard + 1, 'shard disconnected');
});
bot.on('shardReconnecting', shard => {
    log('info', shard + 1, 'shard reconnecting');
});

bot.on('shardResume', shard => {
    log('info', shard + 1, 'shard resuming');
});

bot.on('message', message => {
    if (message.author.id === bot.user.id) return;
    if (message.author.id !== config.biblebot.id) return; //devmode for now

    Preference.findOne({ user: message.author.id }, (err, prefs) => {
        if (err) {
            prefs = {
                input: 'default',
            };
        }

        const ctx = new Context(message.author.id, bot, message.channel, message.guild, message.content, prefs, db);

        switch(prefs['input']) {
            case 'default': {
                if (ctx.msg.startsWith(config.biblebot.commandPrefix)) {
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
                        } else if (commandsRouter.isCommand('$', ctx.msg.split(' ')[0])[0] === true) {
                            commandsRouter.processCommand(ctx);
                        }
                }

                break;
            }
        }
    });

    
});

log('info', 0, `BibleBot v${process.env.npm_package_version} by Evangelion Ltd.`);
fetchBookNames(config.biblebot.dry).then(() => {
    bot.login(config.biblebot.token);
});