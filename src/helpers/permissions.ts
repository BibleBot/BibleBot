import Context from '../models/context';
import { Permissions } from 'discord.js';

export function checkGuildPermissions(ctx: Context, callback: (hasPermission: boolean) => void): void {
    const guild = ctx.guild;

    guild.members.fetch(ctx.id).then((member) => {
        return callback(member.hasPermission(Permissions.FLAGS.MANAGE_GUILD));
    });
}