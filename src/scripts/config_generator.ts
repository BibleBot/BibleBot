import * as fs from 'fs';
import * as readline from 'readline';

import * as ini from 'ini';

const outputDirectory = '..';

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

const config = { 'biblebot': { 'dry': false }, 'apis': {} };

const queryToken = () => {
    return new Promise((resolve) => {
        rl.question('Discord API Token: ', (token) => {
            config.biblebot['token'] = token;

            resolve(null);
        });
    });
};

const queryId = () => {
    return new Promise((resolve) => {
        rl.question('Owner Discord ID: ', (id) => {
            config.biblebot['ownerID'] = id;

            resolve(null);
        });
    });
};

const queryBrackets = () => {
    return new Promise((resolve) => {
        rl.question('Ignoring Brackets (ex. <>): ', (brackets) => {
            config.biblebot['ignoringBrackets'] = brackets;

            resolve(null);
        });
    });
};

const queryPrefix = () => {
    return new Promise((resolve) => {
        rl.question('Command prefix: ', (prefix) => {
            config.biblebot['commandPrefix'] = prefix;

            resolve(null);
        });
    });
};

const queryShards = () => {
    return new Promise((resolve) => {
        rl.question('Shard count: ', (shards) => {
            config.biblebot['shards'] = shards;

            resolve(null);
        });
    });
};

const queryFooter = () => {
    return new Promise((resolve) => {
        rl.question('Footer Text: ', (footer) => {
            config.biblebot['footer'] = footer;

            resolve(null);
        });
    });
};

const queryIconURL = () => {
    return new Promise((resolve) => {
        rl.question('Icon URL: ', (icon) => {
            config.biblebot['icon'] = icon;

            resolve(null);
        });
    });
};

const queryAPIBible = () => {
    return new Promise((resolve) => {
        rl.question('API.Bible Key: ', (key) => {
            config.apis['apiBible'] = key;

            resolve(null);
        });
    });
};

const generateConfiguration = async () => {
    await queryToken();
    await queryId();
    await queryBrackets();
    await queryPrefix();
    await queryShards();
    await queryFooter();
    await queryIconURL();
    await queryAPIBible();

    rl.close();

    fs.writeFile(`${__dirname}/${outputDirectory}/config.ini`, ini.stringify(config), (err) => {
        if (err) {
            console.error('Error writing configuration file, please ensure proper permissions.');
        } else {
            console.log('Wrote configuration file successfully.');
        }
    });
};

generateConfiguration();