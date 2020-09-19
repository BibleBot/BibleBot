import { MessageEmbed } from 'discord.js';

const NORMAL_COLOR = 303102;
const ERROR_COLOR = 16723502;

export function createEmbed(author: string, title: string, description: string, isError: boolean): MessageEmbed {
    if (description.length < 2048) {
        return new MessageEmbed({
            author: {
                name: author
            },
            title,
            description,
            color: isError ? ERROR_COLOR : NORMAL_COLOR,
            footer: {
                text: `BibleBot v${process.env.npm_package_version}`,
                iconURL: 'https://cdn.discordapp.com/avatars/367665336239128577/b8ab407073f4a3be980d8fa6a03e9586.png'
            }
        });
    } else {
        throw new Error('Description is too long.');
    }
}