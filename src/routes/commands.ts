import * as fs from 'fs';
import * as ini from 'ini';

import * as mongoose from 'mongoose';

import Context from '../models/context';
import Preference from '../models/preference';
import Version from '../models/version';
import Language from '../models/language';

import * as commandList from '../helpers/command_list.json';
import { createEmbed } from '../helpers/embed_builder';

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
                Version.findOne({ abbv: args[0] }).then((version) => {
                    if (!version) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `cannot save preference - invalid version (${args[0]})`);
                        ctx.channel.send(createEmbed(null, '+setversion', `${args[0]} not in database`, true));
                        return;
                    }

                    Preference.findOne({ user: ctx.id }, (err, prefs) => {
                        if (err) {
                            ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'cannot save preference - db error');
                            ctx.channel.send(createEmbed(null, '+setversion', 'Failed to set preference.', true));
                        } else if (!prefs) {
                            const newPreference = new Preference({
                                user: ctx.id,
                                input: 'default',
                                version: args[0],
                                headings: true,
                                verseNumbers: true
                            });

                            newPreference.save((err, prefs) => {
                                if (err || !prefs) {
                                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'cannot save preference - db error');
                                    ctx.channel.send(createEmbed(null, '+setversion', 'Failed to set preference.', true));
                                } else {
                                    ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `setversion ${args[0]}`);
                                    ctx.channel.send(createEmbed(null, '+setversion', 'Set version successfully.', false));
                                }
                            });
                        } else {
                            prefs.version = args[0];

                            prefs.save((err, preference) => {
                                if (err || !preference) {
                                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'cannot overwrite preference - db error');
                                    ctx.channel.send(createEmbed(null, '+setversion', 'Failed to set preference.', true));
                                } else {
                                    ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, `setversion ${args[0]} (overwrite)`);
                                    ctx.channel.send(createEmbed(null, '+setversion', 'Set version successfully.', false));
                                }
                            });
                        }
                    });
                }).catch(() => {
                    ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, `cannot save preference - invalid version (${args[0]})`);
                    ctx.channel.send(createEmbed(null, '+setversion', `${args[0]} not in database`, true));
                });
                
                
                break;
            case 'version':
                Preference.findOne({ user: ctx.id }, (err, prefs) => {
                    if (err) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'cannot find preference - db error');
                        ctx.channel.send(createEmbed(null, '+version', 'A database error has occurred.', true));
                    } else if (!prefs) {
                        ctx.logInteraction('err', ctx.shard, ctx.id, ctx.channel, 'version');
                        ctx.channel.send(createEmbed(null, '+version', 'Preferences not found.', true));
                    } else {
                        ctx.logInteraction('info', ctx.shard, ctx.id, ctx.channel, 'version');
                        ctx.channel.send(createEmbed(null, '+version', `You are currently using ${prefs['version']}.`, false));
                    }
                });
                
                break;
        }
    }
}