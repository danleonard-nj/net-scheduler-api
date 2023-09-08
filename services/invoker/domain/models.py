from framework.serialization import Serializable


class AuthConfig(Serializable):
    def __init__(
        self,
        client_id,
        client_secret,
        scopes,
        grant_type,
        identity_url
    ):
        self.client_id = client_id
        self.client_secret = client_secret
        self.scopes = scopes
        self.grant_type = grant_type
        self.identity_url = identity_url

    @staticmethod
    def from_config(data):
        return AuthConfig(
            client_id=data.get('client_id'),
            client_secret=data.get('client_secret'),
            scopes=data.get('scopes', []),
            grant_type=data.get('grant_type', 'client_credentials'),
            identity_url=data.get('identity_url'))


class AuthRequest(Serializable):
    def __init__(
        self,
        client_id,
        client_secret,
        scope,
        grant_type='client_credentials'
    ):
        self.client_id = client_id
        self.client_secret = client_secret
        self.scope = scope
        self.grant_type = grant_type

    @staticmethod
    def from_config(data: AuthConfig):
        data = data.to_dict()
        scopes = ' '.join(data.get('scopes', []))

        return AuthRequest(
            client_id=data.get('client_id'),
            client_secret=data.get('client_secret'),
            scope=scopes,
            grant_type=data.get('grant_type', 'client_credentials'))


class SchedulerConfig:
    def __init__(
        self,
        base_url,
        interval
    ):
        self.base_url = base_url
        self.interval = interval

    @staticmethod
    def from_config(data):
        return SchedulerConfig(
            base_url=data.get('base_url'),
            interval=data.get('interval', 1))
