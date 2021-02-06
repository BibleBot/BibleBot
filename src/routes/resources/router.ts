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

    async listCreeds(ctx: Context): Promise<void> {
        const lang = ctx.language;

        const message = `${lang.getString('creeds_text')}` +
        `**+${lang.getCommand('apostles')}** - ${lang.getString('apostles_name')}\n` +
        `**+${lang.getCommand('nicene325')}** - ${lang.getString('nicene325_name')}\n` +
        `**+${lang.getCommand('nicene')}** - ${lang.getString('nicene_name')}\n` +
        `**+${lang.getCommand('chalcedon')}** - ${lang.getString('chalcedon_name')}`;

        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'creeds']), message, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'creeds');
    }

    async listCatechisms(ctx: Context): Promise<void> {
        const lang = ctx.language;

        const message = `${lang.getString('catechisms_text')}` +
        `**+${lang.getCommand('ccc')}** - **Catechism of the Catholic Church (1992)**\n` +
        `**+${lang.getCommand('lsc')}** - **Luther's Small Catechism (1529)**`;

        ctx.channel.send(createEmbed(null, translateCommand(ctx, ['+', 'catechisms']), message, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'catechisms');
    }

    // TODO: clean these into one function
    async sendApostles(ctx: Context): Promise<void> {
        ctx.channel.send(createEmbed(null, creeds.apostles.title, creeds.apostles.text, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'apostles');
    }

    async sendEarlyNicene(ctx: Context): Promise<void> {
        ctx.channel.send(createEmbed(null, creeds.nicene325.title, creeds.nicene325.text, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'nicene325');
    }

    async sendNicene(ctx: Context): Promise<void> {
        ctx.channel.send(createEmbed(null, creeds.nicene.title, creeds.nicene.text, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'nicene');
    }

    async sendChalcedon(ctx: Context): Promise<void> {
        ctx.channel.send(createEmbed(null, creeds.chalcedon.title, creeds.chalcedon.text, false));
        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'chalcedon');
    }

    async sendCCC(ctx: Context, args: string[], isFallback?: boolean): Promise<void> {
        if (ctx.guild.id == '238001909716353025' && !isFallback) {
            return;
        }
    }
}