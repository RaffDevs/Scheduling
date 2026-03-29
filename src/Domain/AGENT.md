# Domain Agent

## Responsabilidade

Definir entidades e regras básicas do domínio.

## Entidades

Person:
- Id
- Name
- IsActive
- CreatedAt

Schedule:
- Id
- Month
- Year
- CreatedAt

ScheduleEntry:
- Id
- ScheduleId
- Date
- PersonId

SwapHistory:
- Id
- ScheduleEntryId
- OldPersonId
- NewPersonId
- SwappedAt

---

## Regras

- Entidades devem ser simples
- Evitar lógica complexa aqui
- Sem dependência de infraestrutura

---

## NÃO FAZER

- Não usar EF aqui
- Não acessar banco
- Não implementar algoritmo de escala

---

## Objetivo

Representar o problema de forma clara.