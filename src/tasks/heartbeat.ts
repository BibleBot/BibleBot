import { Client } from 'discord.js';
import * as cron from 'node-cron';
import { log } from '../helpers/logger';

export const startHeartbeatMonitor = (bot: Client): void => {
    cron.schedule('30 * * * * *', () => {
        try {
            const shards = [...bot.ws.shards.values()];
            let message = '';
            
            for (const shard of shards) {
                message += `${shards.indexOf(shard)}: ${shard.ping}ms, `;
            }

            log('info', null, `heartbeat - ${message.slice(0, -2)}`);
        } catch (e) {
            log('err', null, 'heartbeat failed - are we offline?');
        }
    }).start();
};
