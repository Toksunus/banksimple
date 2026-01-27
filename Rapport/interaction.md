# Modèle de Domaine — Description des interactions

## Vue d’ensemble
Le modèle de domaine de la plateforme bancaire Banksimple est centré autour du **Client**.  
Il décrit les interactions nécessaires pour identifier le client, sécuriser l’accès au système, gérer les comptes bancaires, effectuer des transactions, assurer la conformité réglementaire et produire les relevés bancaires.

-------------------------------------------------------------------------------------------------------------

## 1. Client – Authentification
Cette interaction permet de gérer l’accès sécurisé du client au système bancaire.

Le client possède des informations d’authentification (identifiant, mot de passe, MFA).  
Elle est utilisée lors de la connexion afin de garantir que seul un utilisateur autorisé accède aux services.

**Cas d’utilisation**
- UC-02 — Authentification & MFA

---------------------------------------

## 2. Client – Session
Cette interaction représente une connexion active du client à la plateforme Banksimple.

Une session est créée après une authentification réussie et possède une durée de validité.  
Un client peut avoir plusieurs sessions actives ou passées.

**Cas d’utilisation**
- UC-02 — Authentification & MFA

---------------------------------------

## 3. Client – Vérification KYC
Cette interaction assure la conformité réglementaire liée à l’identité du client.

Le système vérifie les informations fournies par le client (KYC/AML).  
Une vérification réussie permet l’activation du profil client.

**Cas d’utilisation**
- UC-01 — Inscription & Vérification d’identité

---------------------------------------

## 4. Client – Compte bancaire
Cette interaction permet au client de posséder et gérer un ou plusieurs comptes bancaires.

Chaque compte peut être de type chèque ou épargne et possède un solde et un statut.  
Elle permet l’ouverture de comptes et la consultation des informations financières.

**Cas d’utilisation associés :**
- UC-03 — Ouverture d’un compte bancaire  
- UC-04 — Consultation des soldes et historiques

---------------------------------------

## 5. Compte bancaire – Transaction
Cette interaction représente tous les mouvements financiers associés à un compte bancaire.

Chaque transaction correspond à un débit ou un crédit affectant le solde du compte.  
Les transactions sont conservées pour l’historique et l’audit.

**Cas d’utilisation associés :**
- UC-04 — Consultation  
- UC-05 — Virement bancaire  
- UC-06 — Paiement de factures

---------------------------------------

## 6. Transaction – Virement / Paiement de facture
Cette interaction spécialise les types de transactions bancaires.

Les virements permettent le transfert de fonds, tandis que les paiements de factures servent à régler des fournisseurs.  
Ces classes héritent de la transaction générique.

**Cas d’utilisation associés :**
- UC-05 — Virement bancaire  
- UC-06 — Paiement de factures

---------------------------------------

## 7. Transaction – Contrôle AML
Cette interaction permet d’analyser les transactions afin de détecter des activités suspectes.

Chaque transaction peut être évaluée selon un niveau de risque.  
Les transactions à risque élevé peuvent être signalées pour analyse ou audit.

**Cas d’utilisation associés :**
- UC-07 — Détection d’activités suspectes (AML)

---------------------------------------

## 8. Client / Transaction – Journal d’audit
Cette interaction garantit la traçabilité complète des actions du système.

Toutes les actions importantes sont journalisées afin de respecter les exigences réglementaires.  
Le journal d’audit permet de reconstituer l’historique des opérations.

**Cas d’utilisation associés :**
- UC-06 — Paiement de factures  
- UC-07 — AML  
- UC-08 — Clôture journalière

---------------------------------------

## 9. Client – Produit de crédit
Cette interaction permet au client d’accéder à des produits de crédit simples.

Les produits de crédit définissent un montant autorisé, un taux d’intérêt et un solde utilisé.

**Cas d’utilisation associés :**
- UC-06 — Gestion de produits bancaires simples

---------------------------------------

## 10. Clôture journalière – Relevé bancaire – Compte bancaire
Cette interaction correspond au traitement de fin de journée bancaire (EOD).

Les transactions sont agrégées afin de générer des relevés bancaires quotidiens, qui sont ensuite archivés et mis à disposition du client.

**Cas d’utilisation associés :**
- UC-08 — Clôture journalière & relevés

---------------------------------------

## Résumé
Le modèle de domaine décrit comment le client est authentifié, gère ses comptes, effectue des transactions, lesquelles sont contrôlées, auditées et consolidées lors de la clôture journalière, tout en respectant les exigences réglementaires du domaine bancaire canadien.
