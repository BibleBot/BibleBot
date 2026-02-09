"""
Copyright (C) 2016-2026 Kerygma Digital Co.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
You can obtain one at https://mozilla.org/MPL/2.0/.
"""

import json
import os
from pathlib import Path

from core import checks
from core.i18n import bb_i18n
from disnake import Localized, Message, OptionChoice, Role
from disnake.ext import commands
from disnake.interactions import ApplicationCommandInteraction
from disnake.ui import Container
from helpers import channels, sending
from logger import VyLogger
from services import backend
from ui import renderers as containers
from ui.confirmation_prompt import ConfirmationPrompt
from ui.paginator import ComponentPaginator

i18n = bb_i18n()

logger = VyLogger("default")


class VerseCommands(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

        # Load book names for autocompletion
        try:
            cwd = Path(__file__).parent
            json_path = cwd / "../../BibleBot.Backend/Data/NameFetching/book_names.json"
            with open(json_path.resolve(), "r", encoding="utf-8") as f:
                data = json.load(f)
                # Flatten the list of lists into a single list of names
                self.book_names = sorted(
                    list(set(name for names in data.values() for name in names))
                )
        except Exception as e:
            logger.error(f"Failed to load book names for autocomplete: {e}")
            self.book_names = []

    # async def verse_autocomplete(
    #     self, inter: ApplicationCommandInteraction, string: str
    # ) -> Optional[list[str]]:
    #     user_experiments = await experiments.get_user_frontend_experiments(
    #         inter.author.id
    #     )
    #     if "VerseCmdAutocompleteExperiment" not in user_experiments:
    #         return []
    #     elif (
    #         user_experiments["VerseCmdAutocompleteExperiment"] == "Control"
    #         and inter.author.id != 186046294286925824
    #     ):
    #         return []

    #     try:
    #         if not hasattr(self, "book_names") or not self.book_names:
    #             return []

    #         string_lower = string.lower()
    #         suggestions = []

    #         # Find the last word boundary to use as a potential start for a book name
    #         # We search backwards for spaces or the start of the string
    #         potential_starts = [0]
    #         for i, char in enumerate(string):
    #             if char == " " or char == "/":
    #                 potential_starts.append(i + 1)

    #         # Iterate potential starts from right to left
    #         for start_index in reversed(potential_starts):
    #             query = string_lower[start_index:].strip()
    #             if not query:
    #                 continue

    #             # If the query is just a number (e.g. "1"), don't autocomplete yet
    #             # This prevents "Genesis 1" from suggesting "Genesis 1 Corinthians"
    #             if query.isdigit():
    #                 continue

    #             prefix = string[:start_index]

    #             # Check if this query matches any book name
    #             matches = []
    #             for name in self.book_names:
    #                 if name.lower().startswith(query):
    #                     matches.append(prefix + name)

    #             if matches:
    #                 suggestions = matches
    #                 break  # Found the most specific suffix match

    #         if not string:
    #             return self.book_names[:25]

    #         return suggestions[:25]
    #     except Exception as e:
    #         sentry_sdk.set_context("experiment", "VerseCmdAutocompleteExperiment")
    #         sentry_sdk.capture_exception(error=e)
    #         return []

    @commands.slash_command(description=Localized(key="CMD_SEARCH_DESC"))
    async def search(
        self,
        inter: ApplicationCommandInteraction,
        query: str,  # TODO: add description to param
        subset: str = commands.Param(
            # TODO: add description to param
            choices=[
                OptionChoice(
                    Localized("SEARCH_SUBSET_OT", key="SEARCH_SUBSET_OT"), "1"
                ),
                OptionChoice(
                    Localized("SEARCH_SUBSET_NT", key="SEARCH_SUBSET_NT"), "2"
                ),
                OptionChoice(
                    Localized("SEARCH_SUBSET_DEU", key="SEARCH_SUBSET_DEU"), "3"
                ),
            ],
            default="0",
        ),
        version: str = "null",
    ):
        await inter.response.defer(ephemeral=checks.inter_is_user(inter))
        resp = await backend.submit_command(
            inter.channel,
            inter.author,
            f"+search subset:{subset} version:{version} {query}",
        )

        if isinstance(resp, list):
            paginator = ComponentPaginator(resp, inter.author.id)
            await paginator.send(inter)
        else:
            await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_VERSE_DESC"))
    @commands.install_types(user=True)
    async def verse(
        self,
        inter: ApplicationCommandInteraction,
        reference: str = commands.Param(description=Localized(key="VERSE_PARAM")),
    ):
        await inter.response.defer()

        ctx = await channels.get_channel_context_from_interaction(inter)

        if ctx is None:
            return None

        req_body = {
            "UserId": str(inter.author.id),
            "GuildId": ctx.guild_id,
            "ChannelId": ctx.channel_id,
            "ThreadId": ctx.thread_id,
            "IsThread": ctx.is_thread,
            "IsBot": inter.author.bot,
            "IsDM": ctx.is_thread,
            "Body": reference,
        }

        endpoint = os.environ.get("ENDPOINT", "")

        resp = await backend.submit_verse_raw(endpoint, req_body)

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if resp is None:
            await sending.safe_send_interaction(
                inter.followup, localization["CMD_VERSE_FAIL"], ephemeral=True
            )
        elif isinstance(resp, Container):
            await sending.safe_send_interaction(inter.followup, components=resp)
        elif isinstance(resp, ComponentPaginator):
            await resp.send(inter)
        elif isinstance(resp, list):
            if len(resp) == 0:
                await sending.safe_send_interaction(
                    inter.followup, localization["CMD_VERSE_FAIL"], ephemeral=True
                )
            elif isinstance(resp[0], Container):
                await sending.safe_send_interaction(inter.followup, components=resp)
            elif isinstance(resp[0], str):
                for item in resp:
                    await sending.safe_send_interaction(inter.followup, item)

    @commands.message_command(name=Localized(key="CMD_VERSE_MSG_NAME"))
    @commands.install_types(user=True)
    async def verse_msg(self, inter: ApplicationCommandInteraction, msg: Message):
        await self.verse(inter, msg.content)

    @commands.slash_command(description=Localized(key="CMD_RANDOM_DESC"))
    async def random(self, inter: ApplicationCommandInteraction):
        await inter.response.defer()

        resp = await backend.submit_command(inter.channel, inter.author, "+random")

        if isinstance(resp, str):
            await sending.safe_send_interaction(inter.followup, content=resp)
        else:
            await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_TRUERANDOM_DESC"))
    async def truerandom(self, inter: ApplicationCommandInteraction):
        await inter.response.defer()

        resp = await backend.submit_command(inter.channel, inter.author, "+random true")

        if isinstance(resp, str):
            await sending.safe_send_interaction(inter.followup, content=resp)
        else:
            await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_DAILYVERSE_DESC"))
    async def dailyverse(self, inter: ApplicationCommandInteraction):
        await inter.response.defer()
        resp = await backend.submit_command(inter.channel, inter.author, "+dailyverse")

        if isinstance(resp, str):
            await sending.safe_send_interaction(inter.followup, content=resp)
        else:
            await sending.safe_send_interaction(inter.followup, components=resp)

    @commands.slash_command(description=Localized(key="CMD_SETDAILYVERSE_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def setdailyverse(
        self,
        inter: ApplicationCommandInteraction,
        time: str = "",  # TODO: add description to param
        tz: str = "",  # TODO: add description to param
    ):
        # POTENTIAL TODO: use modal for configuring this?
        # although may be limited in max options for timezone
        await inter.response.defer()

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if checks.inter_is_not_dm(inter):
            if not checks.author_has_manage_server_permission(inter):
                await sending.safe_send_interaction(
                    inter.followup,
                    components=containers.create_error_container(
                        localization["PERMS_ERROR_LABEL"],
                        localization["PERMS_ERROR_DESC"],
                        localization,
                    ),
                    ephemeral=True,
                )
                return

            resp = None
            if time is None or tz is None:
                resp = await backend.submit_command(
                    inter.channel, inter.author, "+dailyverse set"
                )
            else:
                resp = await backend.submit_command(
                    inter.channel, inter.author, f"+dailyverse set {time} {tz}"
                )

            await sending.safe_send_interaction(inter.followup, components=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    "/setdailyverse",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_DAILYVERSESTATUS_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def dailyversestatus(self, inter: ApplicationCommandInteraction):
        await inter.response.defer()

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if checks.inter_is_not_dm(inter):
            resp = await backend.submit_command(
                inter.channel, inter.author, "+dailyverse status"
            )

            await sending.safe_send_interaction(inter.followup, components=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    "/dailyversestatus",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_CLEARDAILYVERSE_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def cleardailyverse(self, inter: ApplicationCommandInteraction):
        await inter.response.defer()

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if checks.inter_is_not_dm(inter):
            if not checks.author_has_manage_server_permission(inter):
                await sending.safe_send_interaction(
                    inter.followup,
                    components=containers.create_error_container(
                        localization["PERMS_ERROR_LABEL"],
                        localization["PERMS_ERROR_DESC"],
                        localization,
                    ),
                    ephemeral=True,
                )
                return

            resp = await backend.submit_command(
                inter.channel, inter.author, "+dailyverse clear"
            )

            await sending.safe_send_interaction(inter.followup, components=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    "/cleardailyverse",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_SETDAILYVERSEROLE_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def setdailyverserole(
        self, inter: ApplicationCommandInteraction, role: Role
    ):  # TODO: add description to param
        await inter.response.defer()

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if checks.inter_is_not_dm(inter):
            if not checks.author_has_manage_server_permission(inter):
                await sending.safe_send_interaction(
                    inter.followup,
                    components=containers.create_error_container(
                        localization["PERMS_ERROR_LABEL"],
                        localization["PERMS_ERROR_DESC"],
                        localization,
                    ),
                    ephemeral=True,
                )
                return

            if not role.is_default() and not role.mentionable:
                await sending.safe_send_interaction(
                    inter.followup,
                    components=containers.create_error_container(
                        "/setdailyverserole",
                        localization["SETDAILYVERSEROLE_UNMENTIONABLE"],
                        localization,
                    ),
                    ephemeral=True,
                )

            if role.is_default():
                prompt = ConfirmationPrompt(
                    f"+dailyverse role {role.id}", inter.author, localization
                )

                await prompt.send(inter)
            else:
                resp = await backend.submit_command(
                    inter.channel, inter.author, f"+dailyverse role {role.id}"
                )

                await sending.safe_send_interaction(inter.followup, components=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    "/setdailyverserole",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return

    @commands.slash_command(description=Localized(key="CMD_CLEARDAILYVERSEROLE_DESC"))
    @commands.install_types(guild=True)
    @commands.contexts(guild=True, bot_dm=False, private_channel=False)
    async def cleardailyverserole(self, inter: ApplicationCommandInteraction):
        await inter.response.defer()

        localization = i18n.get_i18n_or_default(inter.locale.name)

        if checks.inter_is_not_dm(inter):
            if not checks.author_has_manage_server_permission(inter):
                await sending.safe_send_interaction(
                    inter.followup,
                    components=containers.create_error_container(
                        localization["PERMS_ERROR_LABEL"],
                        localization["PERMS_ERROR_DESC"],
                        localization,
                    ),
                    ephemeral=True,
                )
                return

            resp = await backend.submit_command(
                inter.channel, inter.author, "+dailyverse clearrole"
            )

            await sending.safe_send_interaction(inter.followup, components=resp)
        else:
            await sending.safe_send_interaction(
                inter.followup,
                components=containers.create_error_container(
                    "/cleardailyverserole",
                    localization["AUTOMATIC_DAILY_VERSE_NODMS"],
                    localization,
                ),
                ephemeral=True,
            )
            return
