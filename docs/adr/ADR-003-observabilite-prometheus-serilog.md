# ADR 003 – Observabilité avec Prometheus, Grafana et Serilog

## Statut
Acceptée

## Contexte
BankSimple devait couvrir les 4 Golden Signals dès le départ : latence, trafic, erreurs et saturation. Avec trois microservices indépendants, il était important que les métriques soient visualisés sans avoir à consulter les logs de chaque service séparément. La solution devait être entièrement open-source, conteneurisée et opérationnelle dès le démarrage avec docker-compose up.

## Décision
J'ai adopté la stack Prometheus + Grafana + Serilog. Chaque microservice expose un endpoint  /metrics via AspNetCore qui collecte automatiquement les métriques HTTP  sans modifier le code. Prometheus collecte ces endpoints toutes les 15 secondes. Grafana est provisionné automatiquement avec un dashboard configuré manuellement affichant les 4 Golden Signals pour les trois services simultanément. Serilog produit des logs JSON structurés par requête, utiles pour le diagnostic et l'analyse du fonctionnement. 

## Conséquences
- Les 4 Golden Signals sont visibles en temps réel sur Grafana sans configuration manuelle
- Les métriques sont collectées sans modifier le code des services
- Serilog permet de corréler une requête HTTP précise avec ses logs
- Docker compose up suffit pour tout démarrer
