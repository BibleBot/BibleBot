import * as fs from 'fs';
import * as ini from 'ini';

import Context from '../models/context';
import Version from '../models/version';

import * as commandList from '../helpers/command_list.json';

const config = ini.parse(fs.readFileSync(`${__dirname}/../config.ini`, 'utf-8'));

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
        const command = ctx.msg.slice(1).split(' ')[0];

        switch (command) {
            case 'versions': {
                ctx.db.createIndex({
                    index: { fields: ['_id'] }
                }).then(() => {
                    ctx.db.find({
                        selector: {
                            _id: {$regex: 'version:(.*)'}
                        }
                    }).then((res) => {
                        res.docs.forEach((doc) => {
                            console.log(`${doc['version']._name} (${doc['version'].abbv})`);
                        });
                    });
                });

                
                break;
            }
        }
    }
}