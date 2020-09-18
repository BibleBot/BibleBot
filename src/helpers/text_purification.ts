const nuisances = {
    '  ': ' ',
    '“': '"',
    '”': '"',
};

export function purifyVerseText(text: string): string {
    for (const nuisance in nuisances) {
        // typescript, implement String.prototype.replaceAll() already!
        text = text.split(nuisance).join(nuisances[nuisance]); 
    }

    return text;
}