# ADR 005 – API Gateway avec Kong

## Statut
Acceptée

## Contexte
Avec trois microservices exposant chacun leurs propres endpoints, les clients de l'API auraient dû connaître l'adresse de chaque service et gérer eux-mêmes le routage, le CORS et la sécurité. De plus, des préoccupations transversales comme le rate limiting et les métriques auraient dû être réimplémentées dans chaque service séparément. Il fallait donc un point d'entrée unique qui centralise ces responsabilités sans modifier les services eux-mêmes.

## Décision
J'ai déployé Kong 3.6 en mode déclaratif (database-off) comme API Gateway unique sur le port 8090. Kong route toutes les requêtes vers nginx, qui assure ensuite le load balancing vers les services appropriés. Les plugins configurés sont : rate-limiting (50 000 req/min pour absorber les tests de charge), CORS (origines autorisées), prometheus (métriques Kong exposées à Prometheus) et response-transformer (ajout de l'en-tête X-Gateway pour la traçabilité). Le mode déclaratif avec un fichier kong.yml versionné garantit la reproductibilité : aucune configuration manuelle n'est nécessaire au démarrage. Kong a été préféré à KrakenD pour sa documentation plus complète et à Spring Cloud Gateway parce que le projet est en .NET et non en Java.

## Conséquences
- Un seul point d'entrée (port 8090) pour toute l'API, ce qui simplifie les appels clients
- Le rate limiting et le CORS sont configurés une seule fois pour tous les services
- La configuration est versionnée dans kong.yml, ce qui la rend reproductible à chaque démarrage
- Ajouter un nouveau plugin (authentification, logging) ne nécessite aucune modification des services
