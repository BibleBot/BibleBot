import * as fs from 'fs';
import * as ini from 'ini';

import * as chalk from 'chalk';
import { Snowflake, DMChannel, NewsChannel, TextChannel } from 'discord.js';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

export function logInteraction(level: string, shard: number, sender: Snowflake, channel: TextChannel | DMChannel | NewsChannel, msg: string): void {
    let guild;
    let chan;
    
    if (channel instanceof DMChannel) {
        guild = 'DMs';
        chan = 'DMs';
    } else {
        guild = channel.guild.id;
        chan = channel.id;
    }

    let actualSender = sender;
    if (sender == config.biblebot.ownerID) {
        actualSender = 'owner';
    }

    const sourceSection = '<' + chalk.blueBright(actualSender) + '@' + chalk.magentaBright(`${guild}`) + '#' + chalk.greenBright(`${chan}`) + '>';
    const output = `${sourceSection} ${msg}`;
    log(level, shard, output);
}

export function log(level: string, shard: number, msg: string): void {
    let internalLog = console.log;
    let color = chalk.cyanBright;
    let output;

    switch (level) {
        case 'warn':
            internalLog = console.warn;
            color = chalk.yellowBright;
            break;
        case 'err':
            level = 'erro';
            internalLog = console.error;
            color = chalk.redBright;
            break;
    }

    const levelPrefix = color(`[${level}]`);
    if (shard != null) {
        const shardInString = shard < 10 ? `0${shard}` : `${shard}`;
        const shardPrefix = chalk.blackBright(`[shard ${shardInString}]`);
        output = `${levelPrefix} ${shardPrefix} ${msg}`;
    } else {
        output = `${levelPrefix} ${msg}`;
    }
    

    internalLog(output);
}