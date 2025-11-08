"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

# use frontend venv or perish

import os
import pymongo
import iso639

client = pymongo.MongoClient(os.environ.get("MONGODB_CONN"))
db = client.BibleBotBackend

versionToLocale = {
    "ADB1905": "tgl-PH",
    "AKJV": "eng-GB",
    "ALB": "sqi-AL",
    "AMP": "eng-US",
    "AMPC": "eng-US",
    "APSD-CEB": "ceb-PH",
    "ARC": "por-BR",
    "ASV": "eng-US",
    "B21": "ces-CZ",
    "BDS": "fra-FR",
    "BG1940": "bul-BG",
    "BLP": "spa-ES",
    "BLPH": "spa-419",
    "BOB": "bul-BG",
    "BPH": "dan-DK",
    "BSB": "eng-US",
    "BULG": "bul-BG",
    "CARS": "rus-RU",
    "CARSA": "rus-RU",
    "CARST": "rus-TJ",
    "CBT": "bul-BG",
    "CCB": "zho-Hans",
    "CEB": "eng-US",
    "CEI": "ita-IT",
    "CEV": "eng-US",
    "CJB": "eng-US",
    "CKW": "cak-GT",
    "CNVS": "zho-Hans",
    "CSB": "eng-US",
    "CSBT": "zho-Hant",
    "CUV": "zho-Hant",
    "CUVMPS": "zho-Hans",
    "CUVS": "zho-Hans",
    "DARBY": "eng-IE",
    "DHH": "spa-419",
    "DN1933": "dan-DK",
    "DNB1930": "nor-NO",
    "DRA": "eng-US",
    "EHV": "eng-US",
    "ELXX": "grc-GR",
    "ERV": "eng-US",
    "ERV-BG": "bul-BG",
    "ERV-HI": "hin-IN",
    "ERV-HU": "hun-HU",
    "ERV-PA": "pan-IN",
    "ERV-RU": "rus-RU",
    "ERV-SR": "srp-RS",
    "ERV-TA": "tam-IN",
    "ERV-TH": "tha-TH",
    "ESV": "eng-US",
    "ESVUK": "eng-GB",
    "EXB": "eng-US",
    "FBV": "eng-US",
    "FSV": "fil-PH",
    "GNT": "eng-US",
    "GNV": "eng-GB",
    "GW": "eng-US",
    "HCSB": "eng-US",
    "HCV": "hat-US",
    "HLGN": "hil-PH",
    "HOF": "deu-DE",
    "HTB": "nld-NL",
    "ICB": "eng-US",
    "ICELAND": "isl-IS",
    "ISV": "eng-US",
    "JLB": "jpn-JP",
    "JPS1917": "eng-US",
    "JUB": "eng-US",
    "KAR": "hun-HU",
    "KJ21": "eng-GB",
    "KJV": "eng-GB",
    "KLB": "kor-KP",
    "LBLA": "spa-419",
    "LEB": "eng-US",
    "LND": "ita-IT",
    "LSB": "eng-US",
    "LSG": "fra-FR",
    "LSV": "eng-US",
    "LUTH1545": "deu-DE",
    "LXX": "grc-GR",
    "MAORI": "mri-NZ",
    "MBBTAG": "tgl-PH",
    "MEV": "eng-US",
    "MNT": "mkd-MK",
    "MSG": "eng-US",
    "NABRE": "eng-US",
    "NASB": "eng-US",
    "NASB1995": "eng-US",
    "NCV": "eng-US",
    "NEG1979": "fra-CH",
    "NET": "eng-US",
    "NGU-DE": "deu-CH",
    "NIV": "eng-US",
    "NKJV": "eng-US",
    "NLD1939": "nld-NL",
    "NLT": "eng-US",
    "NOG": "eng-US",
    "NP": "pol-PL",
    "NRSV": "eng-US",
    "NRSVA": "eng-GB",
    "NRSVACE": "eng-GB",
    "NRSVCE": "eng-US",
    "NRSVue": "eng-US",
    "NRT": "rus-RU",
    "NTFE": "eng-US",
    "NTLH": "por-BR",
    "NTLR": "ron-RO",
    "NTV": "spa-US",
    "NVB": "vie-VN",
    "NVI": "spa-US",
    "OJB": "eng-US",
    "OL": "por-PT",
    "PAT1904": "grc-GR",
    "PDT": "spa-US",
    "R1933": "fin-FI",
    "RMNN": "ron-RO",
    "RSV": "eng-US",
    "RSVCE": "eng-US",
    "RUSV": "rus-RU",
    "RV1885": "eng-GB",
    "RVA": "spa-ES",
    "RVA-2015": "spa-US",
    "RVR1960": "spa-419",
    "RVR1977": "spa-US",
    "SBLGNT": "grc-GR",
    "SCH1951": "deu-CH",
    "SCH2000": "deu-CH",
    "SFB": "swe-SE",
    "SFB15": "swe-SE",
    "SG21": "fra-CH",
    "SND": "tgl-PH",
    "SOM": "som-SO",
    "SV1917": "swe-SE",
    "SZ-PL": "pol-PL",
    "TLA": "spa-US",
    "TLB": "eng-US",
    "TLV": "eng-US",
    "TNCV": "tha-TH",
    "TR1550": "grc-GR",
    "TR1894": "grc-GR",
    "UBG": "pol-PL",
    "UKR": "ukr-UA",
    "VFL": "por-PT",
    "VULGATE": "lat-VA",
    "WEB": "eng-US",
    "WHNU": "grc-GR",
    "WYC": "eng-GB",
    "YLT": "eng-GB",
}

versionsToDelete = ["AMU", "USP"]


def deleteUnusedVersions():
    versions = db.Versions
    for abbv in versionsToDelete:
        deleted_changes = versions.delete_one({"Abbreviation": abbv})
        if deleted_changes.deleted_count == 1:
            print(f"[info] deleted {abbv}")
        else:
            print(f"[err] could not delete {abbv}")


def addLocalesToVersions():
    versions = db.Versions
    unmatched_languages = []
    #  nop1_languages = []

    for abbv, locale in versionToLocale.items():
        language_part, country_part = locale.split("-")

        try:
            language = iso639.Language.match(language_part)
            # if language.part1 is not None:
            #     print(
            #         f"[info] {language_part} matches {language.part1} ({language.name})"
            #     )
            # else:
            #     if language_part not in unmatched_languages:
            #         unmatched_languages.append(language_part)

            correct_language = (
                language.part1 if language.part1 is not None else language_part
            )

            update_changes = versions.update_one(
                {"Abbreviation": abbv},
                {"$set": {"Locale": f"{correct_language}-{country_part}"}},
            )

            if update_changes.modified_count == 1:
                print(
                    f"[info] added locale '{correct_language}-{country_part}' to {abbv}"
                )
            else:
                print(f"[err] could not add locale to {abbv}")

        except:
            if language_part not in unmatched_languages:
                print(f"[err] could not add locale to {abbv}")
                unmatched_languages.append(language_part)

    if len(unmatched_languages) > 0:
        print(f"[info] could not find matches for {', '.join(unmatched_languages)}")

    versions_without_locale = versions.find({"Locale": {"$exists": False}}).to_list()
    if len(versions_without_locale) > 0:
        print(
            f"[info] versions without locale set: {', '.join([v['Abbreviation'] for v in versions_without_locale])}"
        )


deleteUnusedVersions()
addLocalesToVersions()
