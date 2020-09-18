import Context from '../models/context';
import * as bibleGateway from '../interfaces/bible_gateway';

export class VersesRouter {
    private static instance: VersesRouter;

    private constructor() {
        // nothing to see here, move along
    }

    static getInstance(): VersesRouter {
        if (!VersesRouter.instance) {
            VersesRouter.instance = new VersesRouter();
        }

        return VersesRouter.instance;
    }

    processMessage(ctx: Context): void {
        bibleGateway.getResult(ctx.msg, 'RSV', true, true, (err, data) => {
            if (err != null) {
                console.error(err);
            } else {
                console.log(data);
            }
        });
    }
}