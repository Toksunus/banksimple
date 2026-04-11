# ADR 006 – Saga orchestrée pour les virements internes

## Statut
Acceptée

## Contexte
Un virement entre deux comptes BankSimple implique deux opérations sur le même système : débiter le compte source et créditer le compte destinataire. Si le crédit échoue après le débit, le client perd son argent sans que la transaction soit complète. Il fallait un mécanisme qui garantisse la cohérence entre les deux opérations et qui soit capable de compenser automatiquement en cas d'échec.

## Décision
J'ai implémenté un `VirementSagaOrchestrator` qui coordonne le virement en deux étapes séquentielles avec compensation. Chaque étape est tracée en base de données via un `VirementSaga`. Si le débit échoue, la saga s'arrête immédiatement et le virement est marqué `Echoué`. Si le crédit échoue après un débit réussi, la saga déclenche automatiquement une compensation en recrédant le compte source. Cette approche garantit qu'on ne perd jamais de trace de l'état d'un virement, même si le service plante entre deux étapes.

## Conséquences
- Chaque étape de la saga est persistée, ce qui facilite le débogage et l'audit
- L'orchestrateur est synchrone. Si la compensation échoue aussi, l'état `CompensationEchouée` est enregistré pour intervention manuelle
