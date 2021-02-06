import * as ts from 'typescript';
import * as fs from 'fs';
import * as ini from 'ini';

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

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

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

    async processCommand(ctx: Context, rescue?: string): Promise<void> {
        const tokens = rescue ? ctx.msg.slice(1).split(' ').slice(1) : ctx.msg.slice(1).split(' ');
        const command = rescue ? rescue : tokens[0];
        const args = tokens.slice(1);

        switch (command) {
            case 'biblebot':
                if (args.length == 0) {
                    await informationRouter.getHelp(ctx);
                } else if (this.isCommand(args[0])) {
                    await this.processCommand(ctx, args[0]);
                }
                break;
            case 'search':
                await verseCommandsRouter.search(ctx, args);
                break;
            case 'version':
                await versionSettingsRouter.processCommand(ctx, args);
                break;
            case 'random':
                await verseCommandsRouter.getRandomVerse(ctx);
                break;
            case 'truerandom':
                await verseCommandsRouter.getTrulyRandomVerse(ctx);
                break;
            case 'dailyverse':
                await dailyVerseRouter.processCommand(ctx, args);
                break;
            case 'formatting':
                await formattingSettingsRouter.processCommand(ctx, args);
                break;
            case 'language':
                await languageRouter.processCommand(ctx, args);
                break;
            case 'stats':
                await informationRouter.getStats(ctx);
                break;
            case 'creeds':
                await resourceCommandsRouter.listCreeds(ctx);
                break;
            case 'catechisms':
                //resourceCommandsRouter.listCatechisms(ctx);
                break;
            case 'nicene325':
                await resourceCommandsRouter.sendEarlyNicene(ctx);
                break;
            case 'nicene':
                await resourceCommandsRouter.sendNicene(ctx);
                break;
            case 'apostles':
                await resourceCommandsRouter.sendApostles(ctx);
                break;
            case 'chalcedon':
                await resourceCommandsRouter.sendChalcedon(ctx);
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
                await miscSettingsRouter.processCommand(ctx, args);
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

        if (ctx.id !== config.biblebot.ownerID) {
            return;
        }

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