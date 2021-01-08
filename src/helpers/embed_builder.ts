import * as fs from 'fs';
import * as ini from 'ini';

import Context from '../models/context';

import { MessageEmbed } from 'discord.js';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

const NORMAL_COLOR = 303102;
const ERROR_COLOR = 16723502;

export function createEmbed(author: string, title: string, description: string, isError: boolean, copyright?: string): MessageEmbed {
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

    if (copyright) {
        embed.setFooter(`${copyright} // ${embed.footer}`);
    }

    return embed;     
}

export function createNumberedEmbeds(ctx: Context, resource: Record<string, unknown>, paragraph?: string): MessageEmbed[] {
    const title = (resource.title as string);
    const author = resource.author;
    const image = resource.image;
    const copyright = (resource.copyright as string);
    const category = resource.category;
    const resourceParagraphs = (resource.paragraphs as Array<Record<string, string>>);

    if (!paragraph) {
        return [createTitlePage(title, author, image, copyright, category)];
    }

    const paragraphs = [];

    if (paragraph.includes('-')) {
        const paragraphRange = paragraph.split('-');

        if (paragraphRange.length > 2) {
            return [createEmbed(null, title, ctx.language.getString('invalidrange'), true)];
        }

        for (const paragraph of paragraphRange) {
            if (Number.isInteger(paragraph)) {
                paragraphs.push(paragraphs);
            }
        }

        paragraphs.forEach((para, index) => {
            if (0 < index && index < paragraphs.length) {
                if (paragraphs[index - 1] > para) {
                    return [createEmbed(null, title, ctx.language.getString('invalidrange'), true)];
                }
            }
        });
    }

    const pages = [];
    let highestNumber = 0;

    if (paragraphs.length > 1) {
        for (let i = 0; i < paragraphs.length; i++) {
            if (0 < i && i < resourceParagraphs.length) {
                if (i > highestNumber) {
                    highestNumber = i;
                }
            }
        }

        if ((highestNumber - paragraphs[0]) > 14) {
            return [createEmbed(null, title, ctx.language.getString('invalidrange'), true)];
        }

        for (let i = paragraphs[0]; i < (highestNumber + 1); i++) {
            pages.push(createEmbed(null, `${title} - Paragraph ${i}`, resourceParagraphs[i - 1].text, false, copyright));
        }
    } else {
        if (0 < paragraphs[0] && paragraphs[0] < resourceParagraphs.length) {
            pages.push(createEmbed(null, `${title} - Paragraph ${paragraphs[0]}`, resourceParagraphs[paragraphs[0] - 1].text, false, copyright));
        }
    }

    return pages;
}

function createTitlePage(title, author, image, copyright, category): MessageEmbed {
    return null;
}