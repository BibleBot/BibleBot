import * as ini from 'ini';
import * as fs from 'fs';

import axios from 'axios';
import chalk = require('chalk');
import { JSDOM } from 'jsdom';
import * as ora from 'ora';

import * as defaultNames from './name_data/default_names.json';
import * as apiBibleNames from './name_data/apibible_names.json';
import * as abbreviations from './name_data/abbreviations.json';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

export async function fetchBookNames(): Promise<boolean> {
    const bgVersions = await getBibleGatewayVersions();
    const bgNames = await getBibleGatewayNames(bgVersions);
    
    const abVersions = await getAPIBibleVersions();
    const abNames = await getAPIBibleNames(abVersions);

    const loadingSpinner = ora({
        prefixText: chalk.cyanBright('[info]'),
        text: 'Writing to file...'
    }).start();

    fs.writeFileSync(`${__dirname}/name_data/completed_names.json`, JSON.stringify({ ...bgNames, ...abNames, ...abbreviations }));
    
    loadingSpinner.succeed();

    return new Promise((resolve) => {
        resolve(true);
    });
}

function getBibleGatewayVersions(): Promise<Record<string, string>> {
    const loadingSpinner = ora({
        prefixText: chalk.cyanBright('[info]'),
        text: 'Grabbing BibleGateway versions...'
    }).start();
    
    return axios.get('https://www.biblegateway.com/versions/').then(res => {
        const versions = {};

        const { document } = (new JSDOM(res.data)).window;
        const container = document.getElementsByClassName('info-table')[0];

        Array.from(container.getElementsByClassName('translation-name')).forEach((element: Element) => {
            element = element.children.item(0);

            if (element.children.item(0)) {
                versions[`${element.textContent}`] = `https://www.biblegateway.com${element.children.item(0).getAttribute('href')}`;
            }
        });

        loadingSpinner.succeed();
        return versions;
    });
}

function getBibleGatewayNames(versions: Record<string, string>): Promise<Record<string, Array<string>>> {
    const names = {};
    const promisesList = [];
    const links = Object.values(versions);

    const loadingSpinner = ora({
        prefixText: chalk.cyanBright('[info]'),
        text: `Grabbing BibleGateway names from ${links.length} versions...`,
        interval: links.length * 10
    }).start();

    links.forEach(link => {
        promisesList.push(axios.get(encodeURI(link)));
    });

    return Promise.all(promisesList).then(values => {
        values.forEach(res => {
            const { document } = (new JSDOM(res.data)).window;
            const container = document.getElementsByClassName('chapterlinks')[0];

            if (container === undefined) {
                return;
            }

            Array.from(container.getElementsByClassName('book-name')).forEach((book: Element) => {
                Array.from(book.getElementsByTagName('span')).forEach((span: Element) => {
                    span.remove();
                });

                const englishName = defaultNames[book.getAttribute('data-target').slice(1, -5)];
                const name = book.textContent.trim();

                if (names[englishName]) {
                    if (!names[englishName].includes(name)) {
                        names[englishName].push(name);
                    }
                } else {
                    names[englishName] = [ name ];
                }
            });
        });

        loadingSpinner.succeed();
        return names;
    });
}

function getAPIBibleVersions(): Promise<Record<string, string>> {
    const loadingSpinner = ora({
        prefixText: chalk.cyanBright('[info]'),
        text: 'Grabbing API.Bible versions...'
    }).start();
    
    return axios.get('https://api.scripture.api.bible/v1/bibles', { headers: { 'api-key': config.apis.apiBible } }).then(res => {
        const versions = {};

        for (const version of res.data.data) {
            versions[version.name] = `https://api.scripture.api.bible/v1/bibles/${version.id}/books`; 
        }

        loadingSpinner.succeed();
        return versions;
    });
}

function getAPIBibleNames(versions: Record<string, string>): Promise<Record<string, Array<string>>> {
    const names = {};
    const promisesList = [];
    const links = Object.values(versions);

    const loadingSpinner = ora({
        prefixText: chalk.cyanBright('[info]'),
        text: `Grabbing API.Bible names from ${links.length} versions...`,
        interval: links.length * 10
    }).start();

    links.forEach(link => {
        promisesList.push(axios.get(link, { headers: { 'api-key': config.apis.apiBible } }));
    });

    return Promise.all(promisesList).then(values => {
        values.forEach(res => {
            for (const book of res.data.data) {
                let id = book.id;
                let name = book.name;
                const abbv = book.abbreviation;

                if (name === undefined) continue;
                name = name.trim();

                try {
                    id = apiBibleNames[id];

                    if ((id == '1sam' && name == '1 Kings') || (id == '2sam' && name == '2 Kings') || (['3 Kings', '4 Kings'].includes(abbv))) {
                        continue;
                    }

                    if (names[id]) {
                        if (!names[id].includes(name)) {
                            names[id].push(name);
                        }
                    } else {
                        names[id] = [ name ];
                    }
                } catch (err) {
                    console.log(`Inconsistency found: ${id} in ${book.bibleId}`);
                }

            }
        });

        loadingSpinner.succeed();
        return names;
    });
}