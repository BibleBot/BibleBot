import * as fs from 'fs';
import * as ini from 'ini';

require('./helpers/console_wrapper');

import Context from './models/context';
import { CommandsRouter } from './routes/commands';
import { VersesRouter } from './routes/verses';

import { Client } from 'discord.js';
const bot = new Client({shards: 'auto'});

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
            name: `${config.biblebot.commandPrefix}biblebot v${config.meta.version} | Shard: ${String(shard + 1)} / ${String(bot.options.shardCount)}`
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

    const ctx = new Context(message.author.id, message.channel, message.guild, message.content);

    if (ctx.msg.startsWith(config.biblebot.commandPrefix)) {
        commandsRouter.processCommand(ctx);
    } else if (ctx.msg.includes(':')) {
        versesRouter.processMessage(ctx);
    }
});

console.log(0, `BibleBot v${process.env.npm_package_version} by Seraphim R.P. (vypr)`);
bot.login(config.biblebot.token);