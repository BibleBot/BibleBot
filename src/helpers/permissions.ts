import Context from '../models/context';
import { Guild, Permissions } from 'discord.js';

export function checkGuildPermissions(ctx: Context, callback: (hasPermission: boolean) => void): void {
    const guild = ctx.guild;

    guild.members.fetch(ctx.id).then((member) => {
        return callback(member.hasPermission(Permissions.FLAGS.MANAGE_GUILD));
    });
}

export function checkBotPermissions(guild: Guild): boolean {
    const permissionsNeeded = [Permissions.FLAGS.SEND_MESSAGES, Permissions.FLAGS.READ_MESSAGE_HISTORY, Permissions.FLAGS.MANAGE_MESSAGES,
                               Permissions.FLAGS.EMBED_LINKS, Permissions.FLAGS.ADD_REACTIONS, Permissions.FLAGS.VIEW_CHANNEL];

    for (const permission of permissionsNeeded) {
        if (!guild.me.hasPermission(permission)) {
            return false;
        }
    }
    
    return true;
}