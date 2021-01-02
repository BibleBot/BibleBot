import * as Discord from 'discord.js';
import * as mongoose from 'mongoose';
import { logInteraction } from '../helpers/logger';

export default class Context {
    id: string;
    bot: Discord.Client;
    channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel;
    guild: Discord.Guild;
    msg: string;
    language: mongoose.Document;
    preferences: mongoose.Document;
    guildPreferences: mongoose.Document;
    shard: number;
    logInteraction;
    raw: Discord.Message;

    constructor(id: string, bot: Discord.Client, channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel, guild: Discord.Guild,
                msg: string, language: mongoose.Document, preferences: mongoose.Document, guildPreferences: mongoose.Document, raw: Discord.Message) {
        this.id = id;
        this.bot = bot;
        this.msg = msg;
        this.channel = channel;
        this.guild = guild;
        this.preferences = preferences;
        this.language = language;
        this.guildPreferences = guildPreferences;
        this.shard = guild.shardID;
        this.logInteraction = logInteraction;
        this.raw = raw;
    }
}