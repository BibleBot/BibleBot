import * as chalk from 'chalk';

['log', 'warn', 'error'].forEach((method) => {
    const old = console[method].bind(console);
    console[method] = (shard: number, ...args) => {
        let color = chalk.cyanBright;
        let loggedMethod = method;

        switch (method) {
            case 'warn':
                color = chalk.yellowBright;
                break;
            case 'error':
                color = chalk.redBright;
                loggedMethod = 'erro';
                break;
            default:
                loggedMethod = 'info';
                break;
        }

        const prefix = color(`[${loggedMethod}]`);
        
        if (shard > 0) {
            args.unshift(chalk.greenBright(`<shard ${shard}>`));
        }

        old.apply(console, [prefix].concat(args));
    };
});