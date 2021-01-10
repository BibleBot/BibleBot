import * as ini from 'ini';
import * as fs from 'fs';

import axios from 'axios';
import chalk = require('chalk');
import { JSDOM } from 'jsdom';
import * as ora from 'ora';
import * as _ from 'lodash';

import * as defaultNames from './name_data/default_names.json';
import * as apiBibleNames from './name_data/apibible_names.json';
import * as abbreviations from './name_data/abbreviations.json';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

export function getBookNames(): Record<string, Array<string>> {
    const file = fs.readFileSync(`${__dirname}/name_data/completed_names.json`, 'utf-8');
    return JSON.parse(file);
}

export async function fetchBookNames(isDryRun: boolean): Promise<boolean> {
    if (isDryRun) {
        ora({
            prefixText: chalk.cyanBright('[info]'),
            text: 'Name fetching set to dry, skipping...'
        }).succeed();
        
        return new Promise((resolve) => {
            resolve(true);
        });
    }

    const bgVersions = await getBibleGatewayVersions();
    const bgNames = await getBibleGatewayNames(bgVersions);
    
    const abVersions = await getAPIBibleVersions();
    const abNames = await getAPIBibleNames(abVersions);

    const loadingSpinner = ora({
        prefixText: chalk.cyanBright('[info]'),
        text: 'Writing to file...'
    }).start();

    try {
        const content = JSON.stringify(_.mergeWith({}, bgNames, abNames, abbreviations, (objValue, srcValue) => {
            if (_.isArray(objValue)) {
                return objValue.concat(srcValue);
            }
        }));

        if (fs.existsSync(`${__dirname}/name_data/completed_names.json`)) {
            fs.unlinkSync(`${__dirname}/name_data/completed_names.json`);
        }

        fs.writeFileSync(`${__dirname}/name_data/completed_names.json`, content);
    
        loadingSpinner.succeed();
    } catch (err) {
        console.error(err);

        loadingSpinner.fail();
    }

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
        text: `Grabbing names from ${links.length} BibleGateway versions...`,
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

                let dataName = book.getAttribute('data-target').slice(1, -5);
                
                const name = book.textContent.trim();

                if (['3macc', '4macc'].includes(dataName)) {
                    dataName = dataName.slice(0, -2);
                } else if (['gkesth', 'adest', 'addesth'].includes(dataName)) {
                    dataName = 'gkest';
                } else if (['sgthree', 'sgthr', 'prazar'].includes(dataName)) {
                    dataName = 'praz';
                }

                if (names[dataName]) {
                    if (!names[dataName].includes(name)) {
                        names[dataName].push(name);
                    }
                } else {
                    names[dataName] = [ name ];
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
        text: `Grabbing names from ${links.length} API.Bible versions...`,
        interval: links.length * 10
    }).start();

    links.forEach(link => {
        promisesList.push(axios.get(link, { headers: { 'api-key': config.apis.apiBible } }));
    });

    return Promise.all(promisesList).then(values => {
        values.forEach(res => {
            for (const book of res.data.data) {
                const trueId = book.id;
                let name = book.name;
                const abbv = book.abbreviation;

                if (name === undefined) continue;
                name = name.trim();

                try {
                    const id = apiBibleNames[trueId];

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
                    console.error(err);
                }

            }
        });

        loadingSpinner.succeed();
        return names;
    });
}