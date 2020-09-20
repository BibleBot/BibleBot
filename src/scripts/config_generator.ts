import * as fs from 'fs';
import * as readline from 'readline';

import * as ini from 'ini';

const outputDirectory = '..';

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

const config = { 'biblebot': {}, 'apis': {} };

const queryToken = () => {
    return new Promise((resolve) => {
        rl.question('Discord API Token: ', (token) => {
            config.biblebot['token'] = token;

            resolve();
        });
    });
};

const queryId = () => {
    return new Promise((resolve) => {
        rl.question('Owner Discord ID: ', (id) => {
            config.biblebot['id'] = id;

            resolve();
        });
    });
};

const queryBrackets = () => {
    return new Promise((resolve) => {
        rl.question('Dividing Brackets (ex. <>): ', (brackets) => {
            config.biblebot['dividingBrackets'] = brackets;

            resolve();
        });
    });
};

const queryPrefix = () => {
    return new Promise((resolve) => {
        rl.question('Command prefix: ', (prefix) => {
            config.biblebot['commandPrefix'] = prefix;

            resolve();
        });
    });
};

const queryAPIBible = () => {
    return new Promise((resolve) => {
        rl.question('API.Bible Key: ', (key) => {
            config.apis['apiBible'] = key;

            resolve();
        });
    });
};

const generateConfiguration = async () => {
    await queryToken();
    await queryId();
    await queryBrackets();
    await queryPrefix();
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