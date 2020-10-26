import * as fs from 'fs';
import * as ini from 'ini';

import Context from '../models/context';
import Version from '../models/version';

import * as commandList from '../helpers/command_list.json';
import { createEmbed } from '../helpers/embed_builder';
import { create } from 'domain';

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
        const tokens = ctx.msg.slice(1).split(' ');
        const command = tokens[0];
        const args = tokens.slice(1);

        switch (command) {
            case 'setversion': 
                ctx.db.createIndex({ index: { fields: ['_id'] } }).then(() => {
                    ctx.db.find({ selector: {  _id: `version:${args[0]}` } }).then((res) => {
                        if (res.docs.length == 0) {
                            const embed = createEmbed(null, `${config.biblebot.commandPrefix}${command}`, 'Version does not exist.', true);
                            ctx.channel.send(embed);
                            return;
                        }

                        ctx.db.get(`pref:${ctx.id}`).catch((err) => {
                            if (err.name === 'not_found') {
                                return {
                                    _id: `pref:${ctx.id}`,
                                    version: args[0]
                                };
                            }
                        }).then((doc) => {
                            doc['version'] = args[0];
                            return ctx.db.put(doc).then(() => {
                                const embed = createEmbed(null, `${config.biblebot.commandPrefix}${command}`, 'Set version successfully.', false);
                                ctx.channel.send(embed);
                            });
                        });
                    });
                });
                
                break;
            case 'version':
                ctx.db.get(`pref:${ctx.id}`).then((res) => {
                    const embed = createEmbed(null, `${config.biblebot.commandPrefix}${command}`, `You are using ${res['version']}.`, false);
                    ctx.channel.send(embed);
                }).catch((err) => {
                    if (err.name === 'not_found') {
                        const embed = createEmbed(null, `${config.biblebot.commandPrefix}${command}`, 'No version found in database.', true);
                        ctx.channel.send(embed);
                    }
                });
                
                break;
        }
    }
}