# ADR 004 – Décomposition en microservices

## Statut
Acceptée

## Contexte
L'architecture initiale de BankSimple était un monolithe unique couvrant les clients, les comptes et les virements dans une seule application. Cette approche posait plusieurs problèmes : un changement dans le service de paiements nécessitait de redéployer l'ensemble de l'application, les services ne pouvaient pas être scalés indépendamment selon leur charge, et les domaines partageaient la même base de données. Avec l'ajout du load balancing et des tests de charge, il devenait clairement nécessaire de pouvoir scaler uniquement les services sous pression.

## Décision
J'ai décomposé le monolithe en trois microservices distincts selon les bounded contexts du domaine : ClientService (inscription, KYC, authentification JWT+OTP), AccountService (comptes bancaires, dépôts, historique) et PaymentService (virements inter-comptes, détection AML). Chaque service a son propre Dockerfile, sa propre base de données PostgreSQL et son propre déploiement indépendant dans docker-compose. La communication entre services se fait par HTTP REST (PaymentService appelle AccountService pour valider les comptes). Cette découpe permet de scaler chaque service indépendamment via le paramètre --scale de docker-compose, ce que les tests de charge k6 démontrent concrètement en comparant les performances pour N=1 à N=4 instances.

## Conséquences
- Chaque service peut être déployé, redémarré et scalé indépendamment
- Un bug dans PaymentService n'affecte pas ClientService ou AccountService
- La communication inter-services par HTTP ajoute une légère latence réseau
- Chaque service ayant sa propre base de données, il n'y a pas de transactions distribuées natives entre services
