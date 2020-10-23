import * as fs from 'fs';
import * as ini from 'ini';

//require('./helpers/console_wrapper');

import Context from './models/context';
import { CommandsRouter } from './routes/commands';
import { VersesRouter } from './routes/verses';

import { fetchBookNames } from './helpers/name_fetcher';

import * as PouchDB from 'pouchdb';

import * as pdbFind from 'pouchdb-find';
PouchDB.plugin(pdbFind);

import { Client } from 'discord.js';
const bot = new Client({shards: 'auto'});

const db = new PouchDB('biblebot');
db.sync('http://localhost:5984/db', { live: true }).on('error', (err) => {
    console.error(err);
});

const config = ini.parse(fs.readFileSync(`${__dirname}/config.ini`, 'utf-8'));

const commandsRouter = CommandsRouter.getInstance();
const versesRouter = VersesRouter.getInstance();

bot.on('ready', () => {
    console.log(0, 'initialization complete');
});

bot.on('shardReady', shard => {
    console.log(shard + 1, 'shard connected');
    
    bot.user.setPresence({
        activity: {
            name: `${config.biblebot.commandPrefix}biblebot v${process.env.npm_package_version} | Shard: ${String(shard + 1)} / ${String(bot.options.shardCount)}`
        },
        shardID: shard
    });
});

bot.on('shardDisconnect', (_, shard) => {
    console.log(shard + 1, 'shard disconnected');
});
bot.on('shardReconnecting', shard => {
    console.log(shard + 1, 'shard reconnecting');
});

bot.on('shardResume', shard => {
    console.log(shard + 1, 'shard resuming');
});

bot.on('message', message => {
    if (message.author.id === bot.user.id) return;
    if (message.author.id !== config.biblebot.id) return; //devmode for now


    db.get(`preference:${message.author.id}`).catch((err) => {
        if (err.name === 'not_found') {
            return {
                _id: `preference:${message.author.id}`,
                input: 'default',
                language: 'english',
                version: 'RSV',
                headings: true,
                verseNumbers: true
            };
        }
    }).then((prefs) => {
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

console.log(0, `BibleBot v${process.env.npm_package_version} by Seraphim R.P. (vypr)`);
fetchBookNames(config.biblebot.dry).then(() => {
    bot.login(config.biblebot.token);
});