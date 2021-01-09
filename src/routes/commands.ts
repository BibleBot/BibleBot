import * as ts from 'typescript';

import Context from '../models/context';

import * as commandList from '../helpers/command_list.json';

import { InformationRouter } from './information/router';
import { VerseCommandsRouter } from './resources/verses';
import { DailyVerseRouter } from './resources/daily_verse';
import { VersionSettingsRouter } from './settings/versions';
import { LanguageRouter } from './settings/languages';
import { MiscSettingsRouter } from './settings/misc';
import { FormattingSettingsRouter } from './settings/formatting';
import { ResourcesCommandsRouter } from './resources/router';

const informationRouter = InformationRouter.getInstance();
const verseCommandsRouter = VerseCommandsRouter.getInstance();
const dailyVerseRouter = DailyVerseRouter.getInstance();
const versionSettingsRouter = VersionSettingsRouter.getInstance();
const languageRouter = LanguageRouter.getInstance();
const miscSettingsRouter = MiscSettingsRouter.getInstance();
const formattingSettingsRouter = FormattingSettingsRouter.getInstance();
const resourceCommandsRouter = ResourcesCommandsRouter.getInstance();

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

    getCommandFromTranslation(ctx: Context, str: string): string {
        const lang = ctx.language;

        return lang.getCommandKey(str);
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
                verseCommandsRouter.search(ctx, args);
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
                formattingSettingsRouter.processCommand(ctx, args);
                break;
            case 'language':
                languageRouter.processCommand(ctx, args);
                break;
            case 'stats':
                informationRouter.getStats(ctx);
                break;
            case 'creeds':
                resourceCommandsRouter.listCreeds(ctx);
                break;
            case 'catechisms':
                //resourceCommandsRouter.listCatechisms(ctx);
                break;
            case 'nicene325':
                resourceCommandsRouter.sendEarlyNicene(ctx);
                break;
            case 'nicene':
                resourceCommandsRouter.sendNicene(ctx);
                break;
            case 'apostles':
                resourceCommandsRouter.sendApostles(ctx);
                break;
            case 'chalcedon':
                resourceCommandsRouter.sendChalcedon(ctx);
                break;
            case 'ccc':
            case 'bbccc':
                //if (command == 'bbccc') {
                //    resourceCommandsRouter.sendCCC(ctx, args, true);
                //} else {
                //    resourceCommandsRouter.sendCCC(ctx, args);
                //}
                break;
            case 'lsc':
                //resourceCommandsRouter.sendLSC(ctx, args);
                break;
            case 'misc':
                miscSettingsRouter.processCommand(ctx, args);
                break;
            case 'invite':
                ctx.channel.send('https://discordapp.com/oauth2/authorize?client_id=361033318273384449&scope=bot&permissions=93248');
                ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'invite');
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