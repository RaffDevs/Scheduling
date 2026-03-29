# Scheduling Agent

## Responsabilidade

Gerar escala mensal.

---

## Objetivo

Distribuição justa e equilibrada.

---

## Regras

- Evitar repetição próxima
- Priorizar quem participou menos
- Garantir rotatividade

---

## Algoritmo

1. Obter domingos do mês
2. Obter pessoas ativas
3. Ordenar por menor participação recente
4. Evitar repetição recente
5. Distribuir

---

## Edge cases

- Poucas pessoas
- Mês com 5 domingos
- Repetição inevitável

---

## Output

Lista de ScheduleEntry

---

## NÃO FAZER

- Não acessar banco diretamente
- Não criar UI