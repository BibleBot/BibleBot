import * as ts from 'typescript';

import Context from '../models/context';

import * as commandList from '../helpers/command_list.json';

import { VersionSettingsRouter } from './settings/versions';
import { LanguageRouter } from './settings/languages';
import { InformationRouter } from './information/router';

const versionSettingsRouter = VersionSettingsRouter.getInstance();
const languageRouter = LanguageRouter.getInstance();
const informationRouter = InformationRouter.getInstance();

export class CommandsRouter {
    private static instance: CommandsRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): CommandsRouter {
        if (!CommandsRouter.instance) {
            CommandsRouter.instance = new CommandsRouter();
        }

        return CommandsRouter.instance;
    }

    isCommand(str: string): boolean {
        return commandList.commands.indexOf(str) > -1;
    }

    isOwnerCommand(str: string): boolean {
        return commandList.owner_commands.indexOf(str) > -1;
    }

    processCommand(ctx: Context): void {
        const tokens = ctx.msg.slice(1).split(' ');
        const command = tokens[0];
        const args = tokens.slice(1);

        switch (command) {
            case 'biblebot':
                informationRouter.getHelp(ctx);
                break;
            case 'search':
                // verseCommandsRouter.search(ctx, args);
                break;
            case 'version':
                versionSettingsRouter.processCommand(ctx, args);
                break;
            case 'random':
                // verseCommandsRouter.randomVerse(ctx);
                break;
            case 'votd':
                // votdRouter.processCommand(ctx, args);
                break;
            case 'formatting':
                // formattingRouter.processCommand(ctx, args);
                break;
            case 'language':
                languageRouter.processCommand(ctx, args);
                break;
            case 'stats':
                // informationRouter.getStats(ctx);
                break;
            case 'creeds':
                // resourceRouter.processCreedCommand(ctx, args);
                break;
            case 'catechisms':
                // resourceRouter.processCatechismCommand(ctx, args);
                break;
            case 'ping':
                ctx.channel.send('pong');
                ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'ping');
                break;
        }
    }

    processOwnerCommand(ctx: Context): void {
        const tokens = ctx.msg.slice(1).split(' ');
        const command = tokens[0];
        const args = tokens.slice(1);

        switch (command) {
            case 'manageversions':
                // managementRouter.processVersion(ctx, args);
                break;
            case 'echo':
                ctx.channel.send(args.join(' '));
                ctx.raw.delete();
                ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `echo ${args.join(' ')}`);
                break;
            case 'eval': 
                ctx.channel.send(eval(ts.transpile(args.join(' '))));
                ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'eval');
                break;
            case 'manageoptout':
                // managementRouter.processIgnore(ctx, args);
                break;
        }
    }
}