import * as Discord from 'discord.js';
import * as mongoose from 'mongoose';
import { logInteraction } from '../helpers/logger';

export default class Context {
    id: string;
    bot: Discord.Client;
    channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel;
    guild: Discord.Guild;
    msg: string;
    preferences: mongoose.Document;
    db: mongoose.Connection;
    shard: number;
    logInteraction;

    constructor(id: string, bot: Discord.Client, channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel, guild: Discord.Guild,
                msg: string, preferences: mongoose.Document, db: mongoose.Connection) {
        this.id = id;
        this.bot = bot;
        this.msg = msg;
        this.channel = channel;
        this.guild = guild;
        this.preferences = preferences;
        this.db = db;
        this.shard = guild.shardID + 1;
        this.logInteraction = logInteraction;
    }
}