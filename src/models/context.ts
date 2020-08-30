import Language from './language';
import * as Discord from 'discord.js';

export default class Context {
    id: string;
    channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel;
    guild: Discord.Guild;
    msg: string;
    lang: Language;

    constructor(id: string, channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel, guild: Discord.Guild, msg: string) {
        this.id = id;
        this.msg = msg;
        this.channel = channel;
        this.guild = guild;
    }
}