import * as fs from 'fs';
import { log } from './logger';

export default function handleError(error: Error): void {
    const date = new Date();
    const fileTimestamp = `${date.getFullYear()}-${date.getMonth()}-${date.getDate()}`;
    const errorTimestamp = `${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}`;

    const output = `${errorTimestamp}
    
    name: ${error.name}
    
    msg: ${error.message}
    
    stack: ${error.stack}
    
    ---`;

    const dir = `${__dirname}/../../error_logs`;

    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir);
    }

    fs.appendFileSync(`${dir}/log-${fileTimestamp}.txt`, output);

    log('err', null, error.message);
}