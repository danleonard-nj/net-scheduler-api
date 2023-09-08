import json

import httpx
from framework.caching.memory_cache import MemoryCache
from framework.di.service_collection import ServiceCollection
from framework.di.static_provider import ProviderBase
from framework.logger import get_logger
from domain.models import AuthConfig, SchedulerConfig

from services.auth import AuthClient
from services.processor import ProcessorService
from services.scheduler import SchedulerService

logger = get_logger(__name__)


def get_configuration():
    with open('config.json', 'r') as file:
        data = json.loads(file.read())
        return data


config = get_configuration()


def configure_auth_config(container):
    return AuthConfig.from_config(
        data=config.get('auth'))


def configure_scheduler_config(container):
    return SchedulerConfig.from_config(
        data=config.get('scheduler'))


def configure_http_client(container):
    return httpx.AsyncClient(timeout=None)


class ContainerProvider(ProviderBase):
    @classmethod
    def configure_container(cls):
        container = ServiceCollection()

        container.add_singleton(
            dependency_type=httpx.AsyncClient,
            factory=configure_http_client)

        container.add_singleton(
            dependency_type=AuthConfig,
            factory=configure_auth_config)

        container.add_singleton(
            dependency_type=SchedulerConfig,
            factory=configure_scheduler_config)

        container.add_singleton(AuthClient)
        container.add_singleton(SchedulerService)
        container.add_singleton(MemoryCache)
        container.add_singleton(ProcessorService)

        return container
