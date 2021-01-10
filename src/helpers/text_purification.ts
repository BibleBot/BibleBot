const nuisances = {
    '    ': ' ',
    '  ': ' ',
    '“': '"',
    '”': '"',
    '\n': ' ',
    '¶ ': '',
    ' , ': ', ',
    ' .': '.'
};

export function removePunctuation(text: string): string {
    return text.replace(/[^\w\s]|_/g, '').replace(/\s+/g, ' ');
}

export function purifyVerseText(text: string): string {
    for (const nuisance in nuisances) {
        // typescript, implement String.prototype.replaceAll() already!
        text = text.split(nuisance).join(nuisances[nuisance]); 
    }

    return text;
}