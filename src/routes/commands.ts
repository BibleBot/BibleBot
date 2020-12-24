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
        const subcommand = tokens[1];
        const args = tokens.slice(1);

        switch (command) {
            case 'version':
                switch (subcommand) {
                    case 'set':
                        versionSettingsRouter.setUserVersion(ctx, args[1]);
                        break;
                    default:
                        versionSettingsRouter.getUserVersion(ctx);
                        break;
                }
                break;
        }
    }
}