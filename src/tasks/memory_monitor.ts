import { Client } from 'discord.js';
import * as cron from 'node-cron';
import { log } from '../helpers/logger';

export const startMemoryMonitor = (bot: Client): void => {
    cron.schedule('45 * * * * *', () => {
        log('info', bot.shard.ids[0], `memory usage - ${Math.round(process.memoryUsage().rss / 1024 / 1024 * 100) / 100} MB`);        
    }).start();
};
