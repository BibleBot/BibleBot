import * as Discord from 'discord.js';
import { logInteraction } from '../helpers/logger';
import { GuildPreferenceDocument } from './guild_preference';
import { LanguageDocument } from './language';
import { PreferenceDocument } from './preference';

export default class Context {
    id: string;
    bot: Discord.Client;
    channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel;
    guild: Discord.Guild;
    msg: string;
    language: LanguageDocument;
    preferences: PreferenceDocument;
    guildPreferences: GuildPreferenceDocument;
    shard: number;
    logInteraction;
    raw: Discord.Message;

    constructor(id: string, bot: Discord.Client, channel: Discord.TextChannel | Discord.DMChannel | Discord.NewsChannel, guild: Discord.Guild,
                msg: string, language: LanguageDocument, preferences: PreferenceDocument, guildPreferences: GuildPreferenceDocument, raw: Discord.Message) {
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