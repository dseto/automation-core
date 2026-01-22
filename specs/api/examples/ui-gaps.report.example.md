# UI Gaps Report (exemplo)

Ordem do report:
1) `draftLine` asc  
2) `severity` (error > warn > info)  
3) `code` asc

| id | line | severity | code | message |
|---|---:|---|---|---|
| UIGAP-0001 | 7 | error | UI_MAP_KEY_NOT_FOUND | Element key não encontrada no UIMap |
| UIGAP-0002 | 10 | warn | AMBIGUOUS_MATCH | Mais de um candidato para o testId "btn-login" |
