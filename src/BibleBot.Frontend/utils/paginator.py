"""
MIT License

Copyright (c) 2022 Aarno Dorian
Copyright (c) 2022-2025 Kerygma Digital Co.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"""

from disnake import ui, ButtonStyle
from logger import VyLogger
from utils.i18n import i18n as i18n_class

i18n = i18n_class()

logger = VyLogger("default")


class CreatePaginator(ui.View):
    """
    Paginator for Embeds.
    Parameters:
    ----------
    embeds: List[Embed]
        List of embeds which are in the Paginator. Paginator starts from first embed.
    author: int
        The ID of the author who can interact with the buttons. Anyone can interact with the Paginator Buttons if not specified.
    timeout: float
        How long the Paginator should timeout in, after the last interaction.

    """

    def __init__(self, embeds: list, author: int = 123, timeout: float = 0.0):
        if not timeout:
            super().__init__()
        else:
            super().__init__(timeout=timeout)
        self.embeds = embeds
        self.author = author
        self.CurrentEmbed = 0

    @ui.button(emoji="⬅️", style=ButtonStyle.grey)
    async def previous(self, button, inter):
        localization = i18n.get_i18n_or_default(inter.locale.name)
        try:
            if inter.author.id != self.author:
                return await inter.send(
                    localization["PAGINATOR_FORBIDDEN"], ephemeral=True
                )

            potential_page = self.CurrentEmbed - 1
            if potential_page < 0:
                potential_page = len(self.embeds) - 1

            await inter.response.edit_message(embed=self.embeds[potential_page])
            self.CurrentEmbed = potential_page
        except:
            pass

    @ui.button(emoji="➡️", style=ButtonStyle.grey)
    async def next(self, button, inter):
        localization = i18n.get_i18n_or_default(inter.locale.name)
        try:
            if inter.author.id != self.author:
                return await inter.send(
                    localization["PAGINATOR_FORBIDDEN"], ephemeral=True
                )

            potential_page = self.CurrentEmbed + 1
            if potential_page > len(self.embeds) - 1:
                potential_page = 0

            await inter.response.edit_message(embed=self.embeds[potential_page])
            self.CurrentEmbed = potential_page
        except:
            pass
