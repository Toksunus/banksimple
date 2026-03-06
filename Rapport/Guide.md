# 🏦 CanBankX — Projet d’architecture logicielle
## Phase 1 — API REST, Observabilité & Performance

---

## 📌 Contexte

Vous jouez le rôle d’**architecte logiciel** pour **CanBankX**, une plateforme bancaire commerciale canadienne destinée aux particuliers et aux PME.

Le secteur bancaire canadien impose :
- une **conformité réglementaire stricte** (FINTRAC, OSFI, LPRPDE),
- une **stabilité opérationnelle élevée**,
- une **auditabilité complète**,
- une **performance mesurable**,
- une **disponibilité continue**.

Ce projet vise à concevoir et implémenter une **architecture moderne, observable et évolutive**, en respectant le cahier des charges fourni.

---

## 🎯 Objectifs du projet

La phase 1 vise à :

- Exposer une **API RESTful sécurisée**
- Mettre en place l’**observabilité complète**
- Mesurer et optimiser les **performances**
- Préparer la transition vers une **architecture microservices**
- Documenter les décisions architecturales

---

## 🎓 Objectifs d’apprentissage

À l’issue du projet, l’étudiant·e sera capable de :

- Concevoir une API REST conforme aux bonnes pratiques
- Documenter une API avec **OpenAPI / Swagger**
- Sécuriser une API (CORS, Basic Auth, JWT)
- Mettre en place **Prometheus & Grafana**
- Mesurer les **4 Golden Signals**
- Réaliser des **tests de charge réalistes**
- Implémenter **Load Balancing** et **Caching**
- Migrer vers des **microservices**
- Utiliser une **API Gateway**
- Maintenir une documentation **Arc42 + 4+1**
- Formaliser des **ADR (Architecture Decision Records)**

---

## 🧩 Domaines métier (DDD)

### Clients & Identité
- Profils clients
- KYC / AML
- Authentification
- Gestion des rôles

### Comptes Bancaires
- Comptes chèques
- Comptes épargne
- Soldes
- Intérêts

### Paiements & Transferts
- Virements internes
- Interac simulé
- Paiements programmés

### Crédit & Produits
- Marges de crédit simples
- Prêts simulés

### Comptabilité & Règlements
- Journal comptable append-only
- Clôture journalière (EOD)

### Conformité & Audit
- Surveillance AML
- Journaux immuables
- Rapports réglementaires

---

## 🧪 Cas d’utilisation (Must)

### UC-01 — Inscription & KYC
**Objectif** : Créer un profil client conforme AML  
**Résultat** : Profil actif

**Flux principal**
1. Saisie des données personnelles
2. Création du profil `Pending`
3. Vérification OTP / MFA
4. Validation KYC → `Active`

---

### UC-02 — Authentification & MFA
1. Identifiant + mot de passe
2. Vérification MFA
3. Session sécurisée créée

---

### UC-03 — Ouverture de compte
1. Choix du type de compte
2. Création du compte
3. Journalisation comptable

---

### UC-04 — Consultation des soldes
1. Sélection du compte
2. Retour du solde et de l’historique

---

### UC-05 — Virement bancaire
1. Saisie du bénéficiaire et du montant
2. Contrôles solde & AML
3. Exécution
4. Journalisation
5. Confirmation client

---

### UC-06 — Paiement de factures
1. Sélection du fournisseur
2. Validation
3. Confirmation et audit

---

### UC-07 — Détection AML
1. Analyse post-trade
2. Détection de patterns suspects
3. Signalement interne

---

### UC-08 — Clôture journalière (EOD)
1. Agrégation des transactions
2. Génération des relevés
3. Archivage réglementaire

---

## ⚙️ Exigences non fonctionnelles (NFR)

### Performance

| Architecture     | Latence P95 | Débit |
|------------------|-------------|-------|
| Microservices    | ≤ 500 ms    | ≥ 600 ops/s |
| Event-driven     | ≤ 250 ms    | ≥ 1000 ops/s |

### Disponibilité
- Microservices : ≥ 95 %
- Event-driven : ≥ 99 %

### Observabilité
- Logs structurés
- Métriques Prometheus
- Dashboards Grafana
- 4 Golden Signals dès la phase 1

---

## 🧱 Contraintes techniques

Langages autorisés :
- Java
- C#
- Go
- Rust
- C++

⚠️ Python et JavaScript / TypeScript fortement déconseillés.

---

## 🏗️ Architecture — Phase 1

### API REST
- Routes versionnées (`/api/v1`)
- Codes HTTP standards
- Erreurs JSON normalisées
- Documentation Swagger publiée

### Style architectural
- Hexagonal **ou**
- MVC avec séparation stricte Domain / Infrastructure

---

## 📐 Documentation architecturale

### Modèle 4+1
- Vue logique
- Vue processus (C&C)
- Vue déploiement
- Vue développement
- Scénarios

### Arc42 (sections 1 à 8)
- Contexte
- Contraintes
- Concepts de solution
- Décisions
- Qualité
- Risques

---

## 🧾 ADR (≥ 3 requis)

Exemples :
- Choix du style architectural
- Stratégie de persistance et transactions
- Gestion des erreurs et versionnement
- Conformité et audit (exactly-once)

---

## 💾 Persistance & intégrité

- Modèle ER / UML
- Migrations reproductibles
- Seeds
- Transactions ACID
- Contraintes FK, index
- Idempotency Key
- Journal append-only

---

## 📊 Observabilité & tests de charge

### 4 Golden Signals
- Latence (P95 / P99)
- Trafic (RPS)
- Erreurs (4xx / 5xx)
- Saturation (CPU / RAM / threads)

### Outils
- Prometheus
- Grafana
- k6 / JMeter
- NGINX / HAProxy / Traefik

### Scénarios
- 1 → 4 instances
- Comparaison latence / débit / erreurs
- Test de panne (kill d’instance)

---

## 🚀 API Gateway

Solutions possibles :
- Kong
- KrakenD
- Spring Cloud Gateway

Fonctionnalités :
- Routage dynamique
- CORS
- API Key
- Throttling
- Load Balancing

Comparaisons :
- Appels directs
- Appels via Gateway

---

## 🐳 CI/CD & conteneurisation

### Docker
- Dockerfile multi-stage
- docker-compose
- Healthcheck `/health`

### CI
- Lint
- Build
- Tests (unitaires, intégration, E2E)
- Pipeline < 10 minutes

### CD
- Déploiement en une commande
- Rollback simple

---

## ✅ Définition de Fini (DoD)

- UC Must implémentés de bout-en-bout
- Observabilité complète opérationnelle
- Tests de charge comparatifs
- Load Balancing et cache mesurés
- API Gateway fonctionnelle
- Documentation Arc42 + 4+1 complète
- ≥ 3 ADR validés
- Rapport final + dépôt projet
- Reproductibilité < 30 minutes

---

## 🧮 Grille d’évaluation

| Critère | Pondération |
|--------|-------------|
| Analyse métier & DDD | 15 % |
| API REST & Sécurité | 15 % |
| Persistance & Intégrité | 15 % |
| Observabilité & Charge | 20 % |
| Load Balancing & Cache | 10 % |
| Microservices & Gateway | 15 % |
| Documentation & ADR | 10 % |

---

## 🏁 Barème

- **A (85–100 %)** : Projet complet, robuste, mesuré
- **B (70–84 %)** : Solide avec lacunes mineures
- **C (60–69 %)** : Fonctionnel mais incomplet
- **D (50–59 %)** : Architecture fragile
- **F (< 50 %)** : Non fonctionnel