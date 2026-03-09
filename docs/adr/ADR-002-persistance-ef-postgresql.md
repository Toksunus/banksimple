# ADR 002 – Persistance avec PostgreSQL et Entity Framework Core

## Statut
Acceptée

## Contexte
BankSimple exige des transactions ACID strictes, notamment pour les virements, où les deux opérations (débit et crédit) doivent réussir ensemble ou échouer ensemble. Dans une architecture microservices, chaque service doit aussi avoir sa propre base de données isolée pour éviter le couplage entre domaines. Il fallait donc choisir un système de gestion de Base de Données fiable, open-source, compatible avec Docker, et un ORM qui s'intègre bien avec la "Clean Architecture" établi dans le projet.

## Décision
J'ai choisi PostgreSQL comme système de gestion de Base de Données pour tous les services, avec trois bases distinctes (banksimple_clients, banksimple_accounts, banksimple_payments) et Entity Framework Core comme ORM. Le DbContext est confiné à la couche Infrastructure, ce qui veut dire que le domaine ne le connaît jamais. Les EF migrations s'appliquent automatiquement au démarrage des conteneurs. Puisque le débit et le crédit se font sur deux services séparés, une transaction ACID classique est impossible. J'ai donc géré la cohérence par compensation : si le crédit échoue après le débit, je re-crédite la source pour annuler. PostgreSQL a été préféré à SQL Server pour son caractère open-source et sa facilité de conteneurisation, et à MongoDB parce que le modèle relationnel est mieux adapté aux contraintes d'intégrité bancaires.

## Conséquences
- Un échec du crédit après le débit déclenche un remboursement automatique de la source
- Chaque service a sa propre base de données, ce qui évite tout couplage entre domaines
- Les migrations sont reproductibles et versionnées dans le code