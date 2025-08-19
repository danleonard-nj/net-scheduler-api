import httpx
from domain.models import SchedulerConfig
from framework.logger import get_logger
from services.auth import AuthClient

logger = get_logger(__name__)


class SchedulerService:
    def __init__(
        self,
        http_client: httpx.AsyncClient,
        auth_client: AuthClient,
        scheduler_config: SchedulerConfig
    ):
        self._http_client = http_client
        self._auth_client = auth_client
        self._scheduler_config = scheduler_config

    async def poll_scheduler(
        self
    ):
        endpoint = f'{self._scheduler_config.base_url}/api/scheduler/schedule/poll'
        logger.info(f'Polling scheduler: {endpoint}')

        logger.info(f'Generating headers')
        headers = await self._get_headers()

        logger.info(f'Polling net scheduler: {endpoint}')
        response = await self._http_client.get(
            endpoint,
            headers=headers)

        logger.info(f'Response: {response.status_code}')
        logger.info(f'Content: {response.json()}')

        return response.json()

    async def _get_headers(
        self
    ):
        logger.info(f'Fetching token')
        token = await self._auth_client.get_token()

        return {
            'Authorization': f'Bearer {token}'
        }
