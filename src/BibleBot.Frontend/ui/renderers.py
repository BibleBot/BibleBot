"""
Copyright (C) 2016-2025 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

from core import constants
from disnake import SeparatorSpacing
from disnake.ui import Container, Section, Separator, TextDisplay, Thumbnail


@staticmethod
def convert_verse_to_container(verse, localization) -> Container:
    container = Container()

    container.accent_color = 6709986

    reference_title = (
        verse["reference"]["asString"] + " - " + verse["reference"]["version"]["name"]
    )

    container.children.append(TextDisplay(f"### {reference_title}"))

    if len(verse["title"]) > 0:
        container.children.append(TextDisplay(f"**{verse['title']}**"))

    container.children.append(TextDisplay(f"{verse['text']}"))

    container.children.append(Separator(divider=True, spacing=SeparatorSpacing.large))

    if verse["reference"]["version"]["publisher"] is not None:
        publisher_info = constants.publisher_to_url[
            verse["reference"]["version"]["publisher"]
        ]

        if publisher_info is not None:
            container.children.append(
                TextDisplay(
                    f"-# {constants.logo_emoji}  **{localization.replace('{0}', constants.version)} ∙ [{publisher_info['name']}](<{publisher_info['url']}>)**"
                )
            )
        else:
            container.children.append(
                TextDisplay(
                    f"-# {constants.logo_emoji}  **{localization.replace('{0}', constants.version)}**"
                )
            )
    else:
        container.children.append(
            TextDisplay(
                f"-# {constants.logo_emoji}  **{localization.replace('{0}', constants.version)}**"
            )
        )

    return container


@staticmethod
def convert_embed_to_container(internal_embed) -> Container:
    container = Container()
    section = None
    section_text = ""

    container.accent_color = internal_embed["color"]

    if internal_embed["thumbnail"] is not None:
        section = Section(accessory=Thumbnail(media=internal_embed["thumbnail"]["url"]))

    if internal_embed["url"] is not None:
        title = TextDisplay(f"### [{internal_embed['title']}]({internal_embed['url']})")
    else:
        title = TextDisplay(f"### {internal_embed['title']}")

    if section is None:
        container.children.append(title)
    else:
        section_text += title.content

    if internal_embed["description"] is not None:
        description = TextDisplay(f"{internal_embed['description']}")

        if section is None:
            container.children.append(description)
        else:
            section_text += f"\n\n{description.content}"

    if internal_embed["fields"] is not None:
        if section is None and len(container.children) > 0:
            container.children.append(
                Separator(divider=False, spacing=SeparatorSpacing.small)
            )

        for field in internal_embed["fields"]:
            field_name = TextDisplay(f"**{field['name']}**")
            field_value = TextDisplay(f"{field['value']}")

            if section is None:
                container.children.append(field_name)
                container.children.append(field_value)
                if field["add_separator_after"]:
                    container.children.append(
                        Separator(divider=True, spacing=SeparatorSpacing.large)
                    )
            else:
                section_text += f"\n\n{field_name.content}"
                section_text += f"\n{field_value.content}"

    if section is not None:
        section.children.append(TextDisplay(section_text))
        container.children.append(section)

    container.children.append(Separator(divider=True, spacing=SeparatorSpacing.large))

    container.children.append(
        TextDisplay(
            f"-# {constants.logo_emoji}  **{internal_embed['footer']['text']}**"
        )
    )

    return container


@staticmethod
def create_error_container(title, description, localization) -> Container:
    container = Container()

    container.accent_color = 16723502

    container.children.append(TextDisplay(f"### {title}"))
    container.children.append(TextDisplay(f"{description}"))

    container.children.append(Separator(divider=True, spacing=SeparatorSpacing.large))

    container.children.append(
        TextDisplay(
            f"-# {constants.logo_emoji}  **{localization['EMBED_FOOTER'].replace('<v>', constants.version)}**"
        )
    )

    return container


@staticmethod
def create_success_container(title, description, localization) -> Container:
    container = Container()

    container.accent_color = 6709986

    container.children.append(TextDisplay(f"### {title}"))
    container.children.append(TextDisplay(f"{description}"))

    container.children.append(Separator(divider=True, spacing=SeparatorSpacing.large))

    container.children.append(
        TextDisplay(
            f"-# {constants.logo_emoji}  **{localization['EMBED_FOOTER'].replace('<v>', constants.version)}**"
        )
    )

    return container


@staticmethod
def mass_create_containers(pages, localization, is_verses=False) -> list[Container]:
    containers = []

    for page in pages:
        if is_verses:
            containers.append(convert_verse_to_container(page, localization))
        else:
            containers.append(convert_embed_to_container(page))

    return containers
