import httpx
from framework.caching.memory_cache import MemoryCache
from framework.logger import get_logger
from framework.validators.nulls import none_or_whitespace

from domain.models import AuthConfig, AuthRequest

logger = get_logger(__name__)


class AuthClient:
    def __init__(
        self,
        http_client: httpx.AsyncClient,
        memory_cache: MemoryCache,
        auth_config: AuthConfig
    ):
        self.__http_client = http_client
        self.__memory_cache = memory_cache
        self.__auth_config = auth_config

    async def get_token(
        self
    ):
        # Fetch token from memory cache
        token = self.__memory_cache.get('auth-client-token')

        # Return the token if it exists
        if not none_or_whitespace(token):
            logger.info(f'Using cached auth token')
            return token

        # Generate an auth request from config
        auth_request = AuthRequest.from_config(
            data=self.__auth_config)

        response = await self.__http_client.post(
            url=self.__auth_config.identity_url,
            data=auth_request.to_dict())

        # Failure to fetch the token
        if not response.is_success:
            logger.info(
                f'Failed to fetch token: {response.status_code}: {response.text}')

            raise Exception(f'Scheduler poll failed: {response.text}')

        token = response.json().get('access_token')

        # Set the token in memory cache
        self.__memory_cache.set('auth-client-token', token, 60)

        return token
