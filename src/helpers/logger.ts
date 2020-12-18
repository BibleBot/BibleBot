import * as chalk from 'chalk';
import { Snowflake, DMChannel, NewsChannel, TextChannel } from 'discord.js';

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

    const sourceSection = '<' + chalk.blueBright(sender) + '@' + chalk.magentaBright(`${guild}`) + '#' + chalk.greenBright(`${chan}`) + '>';
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
        const shardPrefix = chalk.blackBright(`[shard ${shard}]`) + ' ';
        output = `${levelPrefix} ${shardPrefix}${msg}`;
    } else {
        output = `${levelPrefix} ${msg}`;
    }
    

    internalLog(output);
}