import json
import pathlib


class i18n:
    def __init__(self):
        self.default = json.loads(open("./default_i18n.json").read())
        self.en_US = json.loads(open("./locale/en_US.json").read())
        self.en_GB = json.loads(open("./locale/en_GB.json").read())

    def get_i18n_or_default(self, name):
        if hasattr(self, name):
            return getattr(self, name)
        else:
            return self.default
