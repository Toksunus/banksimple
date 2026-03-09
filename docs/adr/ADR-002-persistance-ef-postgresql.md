# ADR 002 – Persistance avec PostgreSQL et Entity Framework Core

## Statut
Acceptée

## Contexte
BankSimple exige des transactions ACID strictes, notamment pour les virements, où les deux opérations (débit et crédit) doivent réussir ensemble ou échouer ensemble. Dans une architecture microservices, chaque service doit aussi avoir sa propre base de données isolée pour éviter le couplage entre domaines. Il fallait donc choisir un système de gestion de Base de Données fiable, open-source, compatible avec Docker, et un ORM qui s'intègre bien avec la "Clean Architecture" établi dans le projet.

## Décision
J'ai choisi PostgreSQL 16 comme système de gestion de Base de Données pour tous les services, avec trois bases distinctes (banksimple_clients, banksimple_accounts, banksimple_payments) et Entity Framework Core 8 comme ORM. Le DbContext est confiné à la couche Infrastructure — le domaine ne le connaît jamais. Les migrations code-first s'appliquent automatiquement au démarrage des conteneurs. Les transactions ACID sont implémentées avec BeginTransactionAsync dans les repositories critiques, notamment pour les virements. PostgreSQL a été préféré à SQL Server pour son caractère open-source et sa facilité de conteneurisation, et à MongoDB parce que le modèle relationnel est mieux adapté aux contraintes d'intégrité bancaires.

## Conséquences
- Les virements sont atomiques : un échec à mi-parcours annule les deux opérations
- Chaque service a sa propre base de données, ce qui évite tout couplage entre domaines
- Les migrations sont reproductibles et versionnées dans le code
- Le domaine reste indépendant d'EF Core, ce qui permet de tester la logique métier sans base de données active
