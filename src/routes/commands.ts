import * as fs from 'fs';
import * as ini from 'ini';

import Context from '../models/context';

import * as commandList from '../helpers/command_list.json';

import { VersionSettingsRouter } from './settings/versions';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

const versionSettingsRouter = VersionSettingsRouter.getInstance();

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

    isCommand(prefix: string, firstWord: string): Array<boolean> {
        const results = [false, false, false];

        if (firstWord.startsWith(prefix)) {
            firstWord = firstWord.slice(1);

            if (commandList.commands.indexOf(firstWord) > -1) {
                results[0] = true;
            }

            if (commandList.guild_commands.indexOf(firstWord) > -1) {
                results[1] = true;
            }

            if (commandList.owner_commands.indexOf(firstWord) > -1) {
                results[2] = true;
            }
        }

        return results;
    }

    processCommand(ctx: Context): void {
        const tokens = ctx.msg.slice(1).split(' ');
        const command = tokens[0];
        const args = tokens.slice(1);

        switch (command) {
            case 'biblebot':
                // informationRouter.getHelp(ctx);
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
                // languageRouter.processCommand(ctx, args);
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
                // ctx.send(args.join(' '));
                // ctx.msg.delete();
                break;
            case 'eval':
                // ?
                break;
            case 'manageoptout':
                // managementRouter.processIgnore(ctx, args);
                break;
            case 'leave':
                // ?
                break;
        }
    }
}