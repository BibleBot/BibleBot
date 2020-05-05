# This Python file uses the following encoding: utf-8
"""
    Copyright (c) 2018-2020 Elliott Pardee <me [at] thevypr [dot] com>
    This file is part of BibleBot.

    BibleBot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BibleBot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BibleBot.  If not, see <http://www.gnu.org/licenses/>.
"""

import re
import os, sys
import aiohttp
from bs4 import BeautifulSoup

import quantumrandom
from .verseReader import returnArrayOfVerse

quantumMinimumInt =  777778 #(31,102 verses in bible) -- sure evenly distributed?

def remove_html(text):
    return re.sub(r"<[^<]+?>", "", text)

def purify_text(text):
    result = text.replace("“", "\"")
    result = result.replace("[", " <")
    result = result.replace("]", "> ")
    result = result.replace("”", "\"")
    result = result.replace("‘", "'")
    result = result.replace("’", "'")
    result = result.replace(",", ", ")
    result = result.replace(".", ". ")
    result = result.replace(". \"", ".\"")
    result = result.replace(". '", ".'")
    result = result.replace(" .", ".")
    result = result.replace(", \"", ",\"")
    result = result.replace(", '", ",'")
    result = result.replace("!", "! ")
    result = result.replace("! \"", "!\"")
    result = result.replace("! '", "!'")
    result = result.replace("?", "? ")
    result = result.replace("? \"", "?\"")
    result = result.replace("? '", "?'")
    result = result.replace(":", ": ")
    result = result.replace(";", "; ")
    result = result.replace("¶ ", "")
    result = result.replace("â", "\"")  # biblehub beginning smart quote
    result = result.replace(" â", "\"")  # biblehub ending smart quote
    result = result.replace("â", "-")  # biblehub dash unicode
    return re.sub(r"\s+", " ", result)

#Uses ANU quantum random
#Note: much of the numeric versesRequested control flow if > 1 is based on the assumption that
    #python will throw some error if accessing out of bounds indexes for the entire verseList.
async def get_quantum_random_verse(versesRequested):
    randNum = quantumrandom.randint(1, quantumMinimumInt) #ANU
    #using modular array access so that more bits of randomness can easily be changed
              
    newVerseList = returnArrayOfVerse() #todo: maybe a bit computationally inefficient but idc for now
    modSpot = int(randNum) %  int(len(newVerseList))
    verseID = newVerseList[modSpot]
    verseIDNum = verseID.verseNum

    if (versesRequested.isdigit() and int(versesRequested) > 1 and int(versesRequested) < 4): #preventing overlapping chapters/books
        if(int(versesRequested) == 2): #edge verses catching bounds
            #last verse- of chapter, should break before accessing last index[len] error out of bound
            if modSpot == 31101 or (int(verseID.chapter) != int(newVerseList[modSpot+1].chapter)):                
                return verseID.bookAbbr + str(verseID.chapter) + ":" + str(verseID.verseNum-1) + "-" + str(verseIDNum) #have to increment verse
            else: #can safely access next verse
                return verseID.bookAbbr + str(verseID.chapter) + ":" + str(verseID.verseNum) + "-" + str(verseIDNum+1)
        elif int(versesRequested) == 3: #using selected verse as middle verse
             #should break before accessing index[-1] out of bounds error
            if modSpot == 0 or (int(verseID.chapter) != int(newVerseList[modSpot-1].chapter)): #get next 2 if beginning of chapter
                return verseID.bookAbbr + str(verseID.chapter) + ":" + str(verseID.verseNum) + "-" + str(verseIDNum+2)
            elif modSpot == 31101 or (int(verseID.chapter) != int(newVerseList[modSpot+1].chapter)):
                return verseID.bookAbbr + str(verseID.chapter) + ":" + str(verseID.verseNum-2) + "-" + str(verseIDNum)
            else: #middle, get prior and next
                return verseID.bookAbbr + str(verseID.chapter) + ":" + str(verseIDNum-1) + "-" + str(verseIDNum+1)
    return verseID.bookAbbr + str(verseID.chapter) + ":" + str(verseID.verseNum)



async def get_random_verse():
    url = "https://dailyverses.net/random-bible-verse"

    async with aiohttp.ClientSession() as session:
        async with session.get(url) as resp:
            if resp is not None:
                soup = BeautifulSoup(await resp.text(), "lxml")
                verse = soup.find(
                    "div", {"class": "bibleChapter"}).find("a").get_text()

                return verse

async def get_votd():
    url = "https://www.biblegateway.com/reading-plans/verse-of-the-day/next"

    async with aiohttp.ClientSession() as session:
        async with session.get(url) as resp:
            if resp is not None:
                soup = BeautifulSoup(await resp.text(), "lxml")
                verse = soup.find(True, {"class": "rp-passage-display"}).get_text()

                return verse

# print(get_quantum_random_verse()))    