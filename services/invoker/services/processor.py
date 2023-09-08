import asyncio

from framework.logger import get_logger

from domain.models import SchedulerConfig
from services.scheduler import SchedulerService

logger = get_logger(__name__)


class ProcessorService:
    def __init__(
        self,
        scheduler_service: SchedulerService,
        scheduler_config: SchedulerConfig
    ):
        self.__scheduler_service = scheduler_service
        self.__scheduler_config = scheduler_config

    async def run(
        self
    ):
        logger.info(f'Starting processor')

        while True:
            try:
                logger.info('Polling scheduler')
                await self.__scheduler_service.poll_scheduler()

                logger.info(
                    f'Waiting {self.__scheduler_config.interval} seconds')
                await asyncio.sleep(self.__scheduler_config.interval)

            except Exception as e:
                logger.exception(e)
                await asyncio.sleep(5)
