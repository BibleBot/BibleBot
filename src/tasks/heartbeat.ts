import { Client } from 'discord.js';
import * as cron from 'node-cron';
import { log } from '../helpers/logger';

export const startHeartbeatMonitor = (bot: Client): void => {
    cron.schedule('30 * * * * *', () => {
        try {
            log('info', bot.shard.ids[0], `heartbeat (${Math.ceil(bot.ws.ping)}ms)`);
        } catch {
            log('err', bot.shard.ids[0], 'heartbeat failed - are we offline?');
        }
    }).start();
};
