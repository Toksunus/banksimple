# ADR 001 – Choix du style architectural principal

## Statut
Acceptée

## Contexte
BankSimple est une plateforme bancaire canadienne destinée aux clients particuliers, permettant la gestion de comptes, les dépôts, les retraits et les virements avec détection AML. Le système doit être sécurisé, conforme aux exigences réglementaires (KYC, FINTRAC) et suffisamment modulaire pour évoluer indépendamment par domaine.

## Décision
Une architecture MVC classique fonctionne pour une petite app monolithique, mais dès qu'on découpe en services, les dépendances cachées entre couches deviennent des points de faiblesse. Il fallait donc choisir un style architectural qui isole le domaine métier des préoccupations techniques.

J'ai adopté la Clean Architecture, une approche très populaire en C# séparée en quatre couches (Domain, Application, Infrastructure, API) avec les principes du Domain-Driven Design. Le domaine contient uniquement les entités et les règles métier sans aucune référence à EF Core, PostgreSQL ou ASP.NET. Ainsi, si la base de données change, le domaine reste intact. Les interfaces sont définies dans le domaine et implémentées dans l'infrastructure. Cette séparation garantit que la logique bancaire (AML, KYC, virements) peut être testée indépendamment, et que remplacer un composant technique de la base de données n'affecte que la couche Infrastructure. Cela facilite l'évolution et m'a permis de facilement découper la logique en microservices, chaque service ayant son propre domaine.

## Conséquences
- La logique métier (AML, KYC, virements) est testable sans base de données active
- Remplacer PostgreSQL ou EF Core n'impacte que la couche Infrastructure
- Chaque microservice a un domaine isolé, ce qui facilite la scalabilité horizontale
- La structure en couches ajoute un peu de complexité initiale, mais simplifie la maintenance à long terme
