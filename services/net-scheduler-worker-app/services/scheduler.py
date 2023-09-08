import httpx
from framework.logger import get_logger

from domain.models import SchedulerConfig
from services.auth import AuthClient

logger = get_logger(__name__)


class SchedulerService:
    def __init__(
        self,
        http_client: httpx.AsyncClient,
        auth_client: AuthClient,
        scheduler_config: SchedulerConfig
    ):
        self.__http_client = http_client
        self.__auth_client = auth_client
        self.__scheduler_config = scheduler_config

    async def poll_scheduler(
        self
    ):
        endpoint = f'{self.__scheduler_config.base_url}/api/scheduler/schedule/poll'
        logger.info(f'Polling scheduler: {endpoint}')

        logger.info(f'Generating headers')
        headers = await self.__get_headers()

        logger.info(f'Polling net scheduler: {endpoint}')
        response = await self.__http_client.get(
            endpoint,
            headers=headers)

        logger.info(f'Response: {response.status_code}')

        return response.json()

    async def __get_headers(
        self
    ):
        logger.info(f'Fetching token')
        token = await self.__auth_client.get_token()

        return {
            'Authorization': f'Bearer {token}'
        }
