# Rapport – Déploiement initial de l’application **BankSimple**

## 1. Contexte général du projet

Le projet **BankSimple** est une application bancaire. L’objectif de la phase initiale est de mettre en place une base technique solide permettant :

- l’exposition d’une API RESTful ;
- la persistance des données ;
- un déploiement fiable et reproductible a chaque push ;
- une architecture évolutive vers les microservices.

Cette phase se concentre volontairement sur la fondation de l’application, avant l’implémentation complète des cas d’utilisation métier.

---

## 2. Architecture générale de l’application

L’application a été conçue selon une architecture en couches inspirée de l’architecture hexagonale, afin de garantir une bonne séparation des responsabilités et de faciliter l’évolution future vers une architecture microservices.

La solution est composée de quatre projets distincts :

### BankSimple.Domain
- Contient la logique métier pure (entités, règles).
- Ce projet ne dépend d’aucune technologie externe.

### BankSimple.Application
- Contient les cas d’utilisation qui construit la logique du site.

### BankSimple.Infrastructure
- Contient les implémentations techniques, notamment la persistance des données avec Entity Framework Core et PostgreSQL.

### BankSimple.Api
- Expose l’API RESTful, configure les dépendances, la documentation Swagger et les endpoints de santé.

Cette structure permet de maintenir des dépendances dirigées, où les couches internes (**Domain**, **Application**) ne dépendent jamais des couches externes (**Infrastructure**, **API**).

---

## 3. Mise en place de l’API REST

L’application est développée en **.NET 8**. L’API REST est hébergée dans le projet **BankSimple.Api**.

Les éléments suivants ont été configurés :

- **Contrôleurs REST** pour exposer les endpoints HTTP.
- **Swagger / OpenAPI** afin de documenter automatiquement l’API.
- **Endpoint de santé `/healthy`**, utilisé pour vérifier l’état de l’application (prérequis pour le déploiement et la supervision).

L’application peut être démarrée localement avec succès, confirmant que l’API est fonctionnelle avant toute intégration avec la base de données.

---

## 4. Déploiement de la base de données

### 4.1 Choix technologique

La persistance des données repose sur **PostgreSQL**, un système de gestion de base de données relationnelle robuste et largement utilisé en production.

Afin de garantir la reproductibilité de l’environnement, la base de données est déployée via **Docker Compose**.

### 4.2 Déploiement via Docker

Un fichier `docker-compose.yml` est utilisé pour démarrer un conteneur PostgreSQL configuré avec :

- un nom de base de données dédié ;
- un utilisateur et un mot de passe spécifiques ;
- un volume persistant pour conserver les données.

Le conteneur expose le port standard **5432** et inclut un **healthcheck**, permettant de vérifier automatiquement que la base de données est prête à recevoir des connexions.

Après démarrage, le service PostgreSQL atteint l’état **healthy**, confirmant que l’environnement de persistance est opérationnel.

---

## 5. Mise en place de la persistance avec Entity Framework Core

### 5.1 Configuration d’Entity Framework Core

La persistance est gérée via **Entity Framework Core (EF Core)**, version **8.0**, compatible avec **.NET 8**.

Les dépendances suivantes ont été ajoutées au projet **BankSimple.Infrastructure** :

- Entity Framework Core
- Provider PostgreSQL (**Npgsql**)
- Outils de conception EF Core

Ces versions ont été explicitement fixées afin d’éviter toute incompatibilité avec des versions plus récentes du framework.

### 5.2 DbContext

Un `DbContext` nommé **BankSimpleDbContext** a été créé dans la couche **Infrastructure**. Il représente le point central d’accès à la base de données et servira ultérieurement à définir les ensembles d’entités (`DbSet`).

Ce `DbContext` est injecté dans l’API via le système de dépendances de .NET.

### 5.3 Configuration de la connexion à la base de données

La chaîne de connexion est définie dans le fichier de configuration de l’API (`appsettings.Development.json`). Elle permet à l’API de se connecter à la base PostgreSQL déployée dans le conteneur Docker.

Cette approche respecte les bonnes pratiques de configuration, en séparant la configuration de l’application du code source.

---

## 6. Gestion des migrations de base de données

### 6.1 Installation des outils de migration

Les outils de ligne de commande d’Entity Framework Core (`dotnet-ef`) ont été installés en tant qu’outil local au projet.

Ce choix permet :

- d’assurer la reproductibilité des commandes ;
- d’éviter toute dépendance à la configuration globale de la machine ;
- de faciliter l’intégration future dans un pipeline CI/CD.

### 6.2 Création de la migration initiale

Une migration initiale nommée **Initial** a été créée. Cette migration correspond à l’état de base de la base de données et initialise la structure nécessaire au suivi des migrations.

### 6.3 Application de la migration

La migration a été appliquée à la base de données PostgreSQL via les outils EF Core.

À l’issue de cette opération :

- la base de données est correctement initialisée ;
- la table système `__EFMigrationsHistory` est créée ;
- l’état de la base est synchronisé avec le code.

Cela confirme que le mécanisme de déploiement et de versionnement de la base de données fonctionne correctement.

---

## 7. Validation du déploiement

Plusieurs vérifications ont été effectuées :

- compilation complète de la solution sans erreurs ;
- connexion réussie entre l’API et la base de données ;
- création et application réussie des migrations ;
- endpoint `/healthy` accessible.

Ces validations démontrent que l’application est déployable, fonctionnelle et prête à évoluer.

---

## 8. Conclusion

À ce stade, l’application **BankSimple** dispose :

- d’une architecture logicielle claire et modulaire ;
- d’une API REST opérationnelle ;
- d’une base de données déployée de manière reproductible ;
- d’un mécanisme de migration robuste ;
- d’une fondation solide pour l’implémentation des cas d’utilisation métier.

Cette base technique permet désormais :

- l’ajout progressif des fonctionnalités ;
- la mise en place de l’observabilité et des tests de charge ;
- puis l’évolution contrôlée vers une architecture microservices.
