const sources = {
    'bg': 'BibleGateway',
    'ab': 'API.Bible',
    'bh': 'Bible Hub',
    'bs': 'Bible Server'
};

export function isValidSource(source: string): boolean {
    return Object.keys(sources).includes(source.toLowerCase());
}