import * as fs from 'fs';
import * as ini from 'ini';

import { MessageEmbed } from 'discord.js';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

const NORMAL_COLOR = 303102;
const ERROR_COLOR = 16723502;

export function createEmbed(author: string, title: string, description: string, isError: boolean): MessageEmbed {
    const embed = new MessageEmbed({
        title,
        color: isError ? ERROR_COLOR : NORMAL_COLOR,
        footer: {
            text: config.biblebot.footer.replace('<version>', process.env.npm_package_version),
            iconURL: config.biblebot.icon
        }
    });

    if (author) {
        embed.setAuthor(author);
    }

    if (description) {
        if (description.length < 2049) {
            embed.setDescription(description);
        } else {
            throw new Error('Description is too long.');
        }
    }

    return embed;     
}