import Context from '../../models/context';

import { createEmbed, translateCommand } from '../../helpers/embed_builder';

import * as creeds from '../../resources/english/creeds.json';

export class ResourcesCommandsRouter {
    private static instance: ResourcesCommandsRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): ResourcesCommandsRouter {
        if (!ResourcesCommandsRouter.instance) {
            ResourcesCommandsRouter.instance = new ResourcesCommandsRouter();
        }

        return ResourcesCommandsRouter.instance;
    }

    listCreeds(ctx: Context): void {
        const lang = ctx.language;

        const message = `${lang.getString('creeds_text')}` +
        `**+${lang.getCommand('apostles')}** - ${lang.getString('apostles_name')}\n` +
        `**+${lang.getCommand('nicene325')}** - ${lang.getString('nicene325_name')}\n` +
        `**+${lang.getCommand('nicene')}** - ${lang.getString('nicene_name')}\n` +
        `**+${lang.getCommand('chalcedon')}** - ${lang.getString('chalcedon_name')}`;

        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'creeds']), message, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'creeds');
    }

    listCatechisms(ctx: Context): void {
        const lang = ctx.language;

        const message = `${lang.getString('catechisms_text')}` +
        `**+${lang.getCommand('ccc')}** - **Catechism of the Catholic Church (1992)**\n` +
        `**+${lang.getCommand('lsc')}** - **Luther's Small Catechism (1529)**`;

        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'catechisms']), message, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'catechisms');
    }

    // TODO: clean these into one function
    sendApostles(ctx: Context): void {
        ctx.channel.send(createEmbed(null, creeds.apostles.title, creeds.apostles.text, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'apostles');
    }

    sendEarlyNicene(ctx: Context): void {
        ctx.channel.send(createEmbed(null, creeds.nicene325.title, creeds.nicene325.text, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'nicene325');
    }

    sendNicene(ctx: Context): void {
        ctx.channel.send(createEmbed(null, creeds.nicene.title, creeds.nicene.text, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'nicene');
    }

    sendChalcedon(ctx: Context): void {
        ctx.channel.send(createEmbed(null, creeds.chalcedon.title, creeds.chalcedon.text, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'chalcedon');
    }

    sendCCC(ctx: Context, args: string[], isFallback?: boolean): void {
        if (ctx.guild.id == '238001909716353025' && !isFallback) {
            return;
        }
    }
}