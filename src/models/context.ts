import Language from './language';
import * as Discord from 'discord.js';
import * as PouchDB from 'pouchdb';
import { logMessage } from '../helpers/logger';

export default class Context {
    id: string;
    bot: Discord.Client;
    channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel;
    guild: Discord.Guild;
    msg: string;
    lang: Language;
    preferences: (PouchDB.Core.IdMeta & PouchDB.Core.GetMeta) | Record<string, unknown>;
    db: PouchDB.Database;
    shard: number;
    logMessage;

    constructor(id: string, bot: Discord.Client, channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel, guild: Discord.Guild,
                msg: string, preferences: (PouchDB.Core.IdMeta & PouchDB.Core.GetMeta) | Record<string, unknown>, db: PouchDB.Database) {
        this.id = id;
        this.bot = bot;
        this.msg = msg;
        this.channel = channel;
        this.guild = guild;
        this.preferences = preferences;
        this.db = db;
        this.shard = guild.shardID + 1;
        this.logMessage = logMessage;
    }
}