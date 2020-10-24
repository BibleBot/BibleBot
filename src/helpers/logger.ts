require('../helpers/console_wrapper');

import { Snowflake } from 'discord.js';

export function logMessage(level: string, shard: number, sender: Snowflake, guild: Snowflake, channel: Snowflake, msg: string): void {
    const output = `<${sender}@${guild}#${channel}> ${msg}`;

    console.log(output);

    switch (level) {
        case 'info':
            console.log(shard, output);
            break;
        case 'warn':
            console.warn(shard, output);
            break;
        case 'erro':
            console.error(output);
            break;
    }
}