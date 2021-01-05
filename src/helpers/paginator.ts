import * as discord from 'discord.js';

import * as fs from 'fs';

const FILE_PATH = __dirname + '/existing_paginators.json';
let existingPaginators;

// inspired by @Jo3-L's discord-paginator

export class Paginator {
    private pages: Array<discord.MessageEmbed>;
    private timeout: number;
    private userID: discord.Snowflake;
    private currentPage: number;
    private totalPages: number;

    constructor(pages: Array<discord.MessageEmbed>, userID: discord.Snowflake, timeout: number) {
        if (!pages) {
            throw Error('No embeds found.');
        } else if (timeout == 0 || timeout > 180) {
            timeout = 180;
        }

        this.pages = pages;
        this.timeout = timeout * 1000;
        this.userID = userID;

        this.currentPage = 1;
        this.totalPages = pages.length + 1;

        // existingPaginators is initialized *before* the reset in src/init.ts, so...
        existingPaginators = JSON.parse(fs.readFileSync(FILE_PATH, 'utf-8'));

        if (existingPaginators.userIDs.includes(userID)) {
            throw Error('User already using paginator.');
        } else {
            existingPaginators.userIDs.push(userID);
            
            fs.writeFileSync(FILE_PATH, JSON.stringify(existingPaginators));
            existingPaginators = JSON.parse(fs.readFileSync(FILE_PATH, 'utf-8'));
        }
    }

    async run(chan: discord.TextChannel | discord.DMChannel | discord.NewsChannel): Promise<void> {
        const msg = await chan.send(this.pages[this.currentPage - 1]);

        if (this.pages.length > 1) {
            const emojis = ['⬅️', '➡️', '🛑'];

            for (const emoji of emojis) {
                await msg.react(emoji);
            }
    
            const reactionCollector = msg.createReactionCollector((reaction, user) => {
                return user.id == this.userID && emojis.includes(reaction.emoji.name);
            }, { time: this.timeout });
    
            reactionCollector.on('collect', (reaction, user) => {
                if (reaction.emoji.name == emojis[0]) {
                    if (this.currentPage != 1) {
                        this.currentPage -= 1;
                    }
                } else if (reaction.emoji.name == emojis[1]) {
                    if (this.currentPage != this.totalPages) {
                        this.currentPage += 1;
                    }
                } else if (reaction.emoji.name == emojis[2]) {
                    reactionCollector.stop();
                }
    
                msg.edit(this.pages[this.currentPage - 1]);
                reaction.users.remove(user.id);
                reactionCollector.resetTimer();
            });
    
            reactionCollector.on('end', () => {
                msg.reactions.removeAll();

                existingPaginators.userIDs = existingPaginators.userIDs.filter((val) => {
                    return val != this.userID;
                });
            
                fs.writeFileSync(FILE_PATH, JSON.stringify(existingPaginators));
                existingPaginators = JSON.parse(fs.readFileSync(FILE_PATH, 'utf-8'));
            });
        }
    }
}