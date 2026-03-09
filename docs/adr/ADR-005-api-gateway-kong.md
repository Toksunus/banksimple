# ADR 005 – API Gateway avec Kong

## Statut
Acceptée

## Contexte
Avec trois microservices exposant chacun leurs propres endpoints, le frontend aurait dû connaître l'adresse de chaque service et gérer lui-même le routage, le CORS et la sécurité. Du côté du rate limiting et des métriques, il aurait fallu les réimplémenter dans chaque service séparément. Il fallait donc un point d'entrée unique qui centralise ces responsabilités sans modifier les services eux-mêmes.

## Décision
J'ai déployé Kong comme API Gateway unique sur le port 8090. Kong route toutes les requêtes vers nginx, qui assure ensuite le load balancing vers les services appropriés. Les configurations sont : rate-limiting (50 000 req/min), CORS et Prometheus. Tout est défini dans un fichier kong.yml versionné avec le reste du code, donc la config est reproductible et traçable comme n'importe quel autre fichier du projet, aucune configuration manuelle n'est nécessaire au démarrage. Kong a été préféré à KrakenD pour sa documentation plus complète.

## Conséquences
- Un seul point d'entrée (port 8090) pour toute l'API, ce qui simplifie les appels clients
- Le rate limiting et le CORS sont configurés une seule fois pour tous les services
- La configuration est versionnée dans kong.yml, ce qui la rend reproductible à chaque démarrage