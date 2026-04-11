# BankSimple

Plateforme bancaire canadienne en microservices développée dans le cadre du cours GTI611. J'ai conçu cette architecture pour démontrer qu'un système bancaire peut être sécurisé, observable et résilient tout en restant déployable en une seule commande.

## Pourquoi cette architecture

Un monolithe aurait été plus simple à démarrer, mais impossible à scaler par domaine. J'ai donc décomposé le système en trois microservices indépendants (ClientService, AccountService, PaymentService), chacun avec sa propre base de données. Kong sert de point d'entrée unique pour centraliser le routage, le CORS et le rate limiting — ce qui évite de dupliquer cette configuration dans chaque service. nginx distribue la charge entre les replicas avec l'algorithme `least_conn`, ce qui garantit qu'aucun conteneur ne se retrouve surchargé pendant les pics.

## Architecture

| Composant | Rôle | Port |
|-----------|------|------|
| **Kong** | API Gateway — point d'entrée unique, routage, CORS, rate limiting | 8090 |
| **nginx** | Load balancer interne (least_conn) entre les replicas | 80 |
| **ClientService** | Inscription, KYC, authentification JWT + OTP | 8080 |
| **AccountService** | Comptes, dépôts, retraits | 8080 |
| **PaymentService** | Virements, détection AML | 8080 |
| **PostgreSQL** | 3 bases isolées par service — pas de couplage entre domaines | 5432 |
| **Redis** | Cache OTP (5 min), comptes (60 s), idempotence virements (24 h) | 6379 |
| **Prometheus** | Scrape métriques `/metrics` de chaque service toutes les 15 s | 9090 |
| **Grafana** | Dashboard 4 Golden Signals autoprovisionné | 3000 |

## Démarrage rapide

### 1. Lancer tous les services

```bash
docker compose up -d --build
```

Le système est prêt quand tous les conteneurs affichent `(healthy)` :

```bash
docker ps
```

### 2. Lancer le frontend

```bash
cd frontend
npm install      # seulement la première fois
npm run dev
```

Frontend disponible sur `http://localhost:5173`

### 3. Rebuild un service spécifique

```bash
docker compose up -d --build payment-service
docker compose up -d --build account-service
docker compose up -d --build client-service
```

### 4. Voir les logs d'un service

```bash
docker logs banksimple-payment-service-1 --tail 30 --follow
docker logs banksimple-account-service-1 --tail 30 --follow
docker logs banksimple-client-service-1  --tail 30 --follow
```

### 5. Arrêter tout

```bash
docker compose down
```

## Endpoints principaux (via Kong — `localhost:8090`)

### Authentification
| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/api/v1/clients/inscription` | Créer un profil client |
| POST | `/api/v1/clients/{id}/valider-kyc` | Valider le KYC |
| POST | `/api/v1/auth/login` | Login → reçoit OTP par retour |
| POST | `/api/v1/auth/verify-otp` | Vérifier OTP → reçoit JWT |

### Comptes
| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/api/v1/clients/{id}/comptes` | Créer un compte |
| GET | `/api/v1/clients/{id}/comptes` | Lister les comptes (cache Redis 60 s) |
| POST | `/api/v1/clients/{id}/comptes/{compteId}/depot` | Déposer des fonds |
| DELETE | `/api/v1/clients/{id}/comptes/{compteId}` | Fermer un compte |

### Virements
| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/api/v1/virements` | Virement interne (saga + AML, idempotent 24 h) |
| POST | `/api/v1/virements/externe` | Virement interbanque via BBC |
| POST | `/api/v1/virements/comptes/{compteId}/register-bbc` | Enregistrer la clé du compte au BBC |
| GET | `/api/v1/virements/compte/{compteId}` | Historique des virements |

> Toutes les routes (sauf inscription/login) requièrent `Authorization: Bearer <token>`

## Swagger

Disponible sur chaque service en développement :
- ClientService : `http://localhost:5001/swagger`
- AccountService : `http://localhost:5002/swagger`
- PaymentService : `http://localhost:5003/swagger`

## Collection Postman

Importer `docs/collections/BankSimple.postman_collection.json` dans Postman.
Variables : `{{baseURL}}` = `http://localhost:8090`, `{{token}}` = JWT après login.

## Observabilité

J'ai choisi Prometheus + Grafana parce que la stack est entièrement open-source, conteneurisable et opérationnelle sans configuration manuelle. Le dashboard est autoprovisionné au démarrage.

| URL | Service |
|-----|---------|
| http://localhost:3002 | Grafana (admin / admin) |
| http://localhost:9095 | Prometheus |
| http://localhost:5050 | pgAdmin (admin@banksimple.ca / admin) |

Les **4 Golden Signals** sont visibles en temps réel :
- **Trafic** — RPS total par service
- **Erreurs** — taux 4xx/5xx
- **Latence** — P95 / P99
- **Saturation** — CPU / RAM par service

## Tests de charge (k6)

```bash
k6 run -e BASE_URL=http://localhost:8090 k6/load-test.js

k6 run -e BASE_URL=http://localhost:8090 k6/stress-test.js
```

### Résultats comparatifs (load-test, N instances)

| N | P95 | P99 | Erreurs | Pic RPS |
|:-:|:---:|:---:|:-------:|:-------:|
| 1 | ~25 ms | ~50 ms | 0 % | ~50 req/s |
| 2 | ~25 ms | ~45 ms | 0 % | ~50 req/s |
| 3 | ~25 ms | ~50 ms | 0 % | ~13 req/s* |
| 4 | ~60 ms | ~115 ms | 0 % | ~9 req/s* |

*Dégradation du débit due aux limites de ressources locales (12 conteneurs .NET simultanés), pas au logiciel. Aucune erreur sur l'ensemble des scénarios.

Captures Grafana disponibles dans `docs/images/`.

## Scalabilité horizontale

```bash
docker compose up -d --scale client-service=2 --scale account-service=2 --scale payment-service=2
```

nginx s'adapte automatiquement aux nouveaux conteneurs via le DNS interne Docker — sans toucher à la config.

## Documentation

| Document | Chemin |
|----------|--------|
| Architecture Arc42 | `docs/arc42/doc.md` |
| ADRs (6 décisions) | `docs/adr/` |
| Vues 4+1 (PlantUML) | `docs/views/` |
| Collection Postman | `docs/collections/` |
| Captures Grafana | `docs/images/` |

## Stack technique

- **Backend** : C# / .NET 8, ASP.NET Core, Entity Framework Core
- **Bases de données** : PostgreSQL 16 (3 instances isolées)
- **Cache** : Redis 7
- **API Gateway** : Kong 3.6
- **Load Balancer** : nginx (least_conn)
- **Observabilité** : Prometheus + Grafana + Serilog (JSON)
- **Tests de charge** : k6
- **CI** : GitHub Actions (build + tests xUnit)
- **Conteneurisation** : Docker + docker-compose
