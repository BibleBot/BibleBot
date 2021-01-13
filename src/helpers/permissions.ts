import Context from '../models/context';
import { Guild, NewsChannel, Permissions, TextChannel } from 'discord.js';
import { log } from './logger';

export function checkGuildPermissions(ctx: Context, callback: (hasPermission: boolean) => void): void {
    const guild = ctx.guild;

    guild.members.fetch(ctx.id).then((member) => {
        return callback(member.hasPermission(Permissions.FLAGS.MANAGE_GUILD));
    });
}

export function checkBotPermissions(channel: TextChannel | NewsChannel, guild: Guild): boolean {
    const permissionsNeeded = {
        'SEND_MESSAGES': Permissions.FLAGS.SEND_MESSAGES,
        'READ_MESSAGE_HISTORY': Permissions.FLAGS.READ_MESSAGE_HISTORY,
        'MANAGE_MESSAGES': Permissions.FLAGS.MANAGE_MESSAGES,
        'EMBED_LINKS': Permissions.FLAGS.EMBED_LINKS,
        'ADD_REACTIONS': Permissions.FLAGS.ADD_REACTIONS,
        'VIEW_CHANNEL': Permissions.FLAGS.VIEW_CHANNEL
    };

    const channelPerms = channel.permissionsFor(guild.me);

    for (const [key, permission] of Object.entries(permissionsNeeded)) {
        if (!guild.me.hasPermission(permission)) {
            log('err', guild.shardID, `${guild.id} does not have the ${key} permissions`);

            return false;
        }

        if (!channelPerms.has(permission)) {
            log('err', guild.shardID, `${channel.id} on ${guild.id} does not have the ${key} permission`);

            return false;
        }
    }
    
    return true;
}