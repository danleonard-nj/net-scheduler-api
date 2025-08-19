import asyncio

from framework.logger import get_logger
from services.processor import ProcessorService
from utilities.utils import ContainerProvider

logger = get_logger(__name__)

provider = ContainerProvider.get_service_provider()

processor = provider.resolve(ProcessorService)


async def main():
    while True:
        try:
            await processor.run()
        except Exception as e:
            logger.exception('Unhandled exception encountered in processor', e)
            await asyncio.sleep(5)


loop = asyncio.new_event_loop()
loop.run_until_complete(main())
