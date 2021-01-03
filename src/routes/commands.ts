import * as ts from 'typescript';

import Context from '../models/context';

import * as commandList from '../helpers/command_list.json';

import { InformationRouter } from './information/router';
import { VerseCommandsRouter } from './resources/verses';
import { DailyVerseRouter } from './resources/daily_verse';
import { VersionSettingsRouter } from './settings/versions';
import { LanguageRouter } from './settings/languages';
import { MiscSettingsRouter } from './settings/misc';

const informationRouter = InformationRouter.getInstance();
const verseCommandsRouter = VerseCommandsRouter.getInstance();
const dailyVerseRouter = DailyVerseRouter.getInstance();
const versionSettingsRouter = VersionSettingsRouter.getInstance();
const languageRouter = LanguageRouter.getInstance();
const miscSettingsRouter = MiscSettingsRouter.getInstance();

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

    processCommand(ctx: Context, rescue?: string): void {
        const tokens = rescue ? ctx.msg.slice(1).split(' ').slice(1) : ctx.msg.slice(1).split(' ');
        const command = rescue ? rescue : tokens[0];
        const args = tokens.slice(1);

        switch (command) {
            case 'biblebot':
                if (args.length == 0) {
                    informationRouter.getHelp(ctx);
                } else if (this.isCommand(args[0])) {
                    this.processCommand(ctx, args[0]);
                }
                break;
            case 'search':
                // verseCommandsRouter.search(ctx, args);
                break;
            case 'version':
                versionSettingsRouter.processCommand(ctx, args);
                break;
            case 'random':
                verseCommandsRouter.getRandomVerse(ctx);
                break;
            case 'truerandom':
                verseCommandsRouter.getTrulyRandomVerse(ctx);
                break;
            case 'dailyverse':
                dailyVerseRouter.processCommand(ctx, args);
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
            case 'misc':
                miscSettingsRouter.processCommand(ctx, args);
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