COMPOSE_DEV       = docker compose -f docker/environments/development.yml
COMPOSE_DEV_INFRA = docker compose -f docker/environments/development.infra.yml
COMPOSE_PROD      = docker compose -f docker/environments/production.yml
DOCKER_NETWORK    = cloudmart_net

.PHONY: dev dev-build dev-down dev-reset infra infra-down infra-reset prod prod-down logs network

network:
	docker network inspect $(DOCKER_NETWORK) >/dev/null 2>&1 || \
	docker network create $(DOCKER_NETWORK)

dev: network
	$(COMPOSE_DEV) up -d

dev-build: network
	$(COMPOSE_DEV) up -d --build

dev-down:
	$(COMPOSE_DEV) down

dev-reset:
	$(COMPOSE_DEV) down -v

infra: network
	$(COMPOSE_DEV_INFRA) up -d

infra-down:
	$(COMPOSE_DEV_INFRA) down

infra-reset:
	$(COMPOSE_DEV_INFRA) down -v

prod: network
	$(COMPOSE_PROD) up -d

prod-down:
	$(COMPOSE_PROD) down

logs:
	$(COMPOSE_DEV_INFRA) logs -f $(s)
