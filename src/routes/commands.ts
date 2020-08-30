import Context from '../models/context';

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

    processCommand(ctx: Context): void {
        const command = ctx.msg.slice(1).split(' ')[0];

        switch (command) {
            case 'ping':
                ctx.channel.send('pong');
                break;
        }
    }
}