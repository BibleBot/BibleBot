import * as fs from 'fs';
import * as ini from 'ini';

import { ShardingManager } from 'discord.js';
import { fetchBookNames } from './helpers/name_fetcher';
import { log } from './helpers/logger';

const config = ini.parse(fs.readFileSync(`${__dirname}/config.ini`, 'utf-8'));

const shardManager = new ShardingManager('./dist/bot.js', {
    token: config.biblebot.token,
    totalShards: 'auto',

});

log('info', null, `BibleBot v${process.env.npm_package_version} by Kerygma Digital`);

fetchBookNames(config.biblebot.dry == 'True').then(() => {
    fs.writeFile(__dirname + '/helpers/existing_paginators.json', JSON.stringify({ 'userIDs': [ ] }), (err) => {
        if (err) {
            log('info', null, 'unable to create existing_paginators.json');
        } else {
            log('info', null, 'reset existing paginators');
        }
    });

    shardManager.spawn();
});