# ADR 003 – Observabilité avec Prometheus, Grafana et Serilog

## Statut
Acceptée

## Contexte
BankSimple devait couvrir les 4 Golden Signals dès le départ : latence (P95/P99), trafic (RPS), erreurs (4xx/5xx) et saturation (CPU/RAM). Avec trois microservices indépendants, il était important que les métriques soient centralisées et visualisables sans avoir à consulter les logs de chaque service séparément. La solution devait être entièrement open-source, conteneurisée et opérationnelle dès le démarrage avec docker compose up.

## Décision
J'ai adopté la stack Prometheus + Grafana + Serilog. Chaque microservice expose un endpoint /metrics via le middleware prometheus-net.AspNetCore qui collecte automatiquement les métriques HTTP (durée, taux d'erreurs, requêtes en cours) sans modifier le code métier. Prometheus scrape ces endpoints toutes les 15 secondes. Grafana est provisionné automatiquement avec un dashboard préconfiguré affichant les 4 Golden Signals pour les trois services simultanément. Serilog produit des logs JSON structurés par requête (service, TraceId, StatusCode, durée), utiles pour le diagnostic. Cette stack a été préférée à Application Insights (SaaS Azure, hors contraintes du projet) et à ELK (trop lourd pour la portée du projet).

## Conséquences
- Les 4 Golden Signals sont visibles en temps réel sur Grafana sans configuration manuelle
- Les métriques sont collectées sans modifier le code métier des services
- Serilog permet de corréler une requête HTTP précise avec ses logs via le TraceId
- La stack est entièrement reproductible : un docker compose up suffit pour tout démarrer
