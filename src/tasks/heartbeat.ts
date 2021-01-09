import { Client } from 'discord.js';
import * as cron from 'node-cron';
import { log } from '../helpers/logger';

export const startHeartbeatMonitor = (bot: Client): void => {
    cron.schedule('30 * * * * *', () => {
        try {
            const shards = [...bot.ws.shards.values()];
            const pings = [];
            
            for (const shard of shards) {
                pings.push(`${shards.indexOf(shard)}: ${shard.ping}ms`);
            }

            log('info', null, `heartbeat (avg. ${bot.ws.ping}ms) - ${pings.join(', ')}`);
        } catch {
            log('err', null, 'heartbeat failed - are we offline?');
        }
    }).start();
};
