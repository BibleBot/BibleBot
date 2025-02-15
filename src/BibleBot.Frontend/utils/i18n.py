import json


class i18n:
    default = json.loads(open("./default_i18n.json").read())
    en_US = json.loads(open("./locale/en_US.json").read())
    en_GB = json.loads(open("./locale/en_GB.json").read())

    def get_i18n_or_default(self, name):
        if hasattr(self, name):
            return getattr(self, name)
        else:
            return self.default
