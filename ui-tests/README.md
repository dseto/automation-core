# UI Tests E2E  MetricsSimple

## Status Atual

 **Infraestrutura E2E Operacional**
- 6 cenários BDD implementados
- 2 testes PASSANDO (Toggle de visibilidade de senha)
- 4 testes em análise (fluxo de login)
- Ambiente: Azure Static Web Apps
- Execução: ~14s headless

## Estrutura de Pastas

```
ui-tests/
├── features/          # Arquivos .feature (Gherkin)
├── ui/                # UI Maps (YAML)
├── data/              # Data Maps (YAML)
├── Steps/             # Step Definitions (C#)
├── scripts/           # Scripts PowerShell de automação
├── pages/             # Páginas HTML para testes locais
├── artifacts/         # Evidências e outputs (session.json, screenshots, etc.)
│   └── recorder/      # Session logs do Free-Hands Recorder
└── docs/              # Documentação dos testes
```

## Executar

```powershell
cd ui-tests
.\RUN_TESTS.ps1
```

## Free-Hands Recorder

Para capturar interações manuais e gerar rascunhos de cenários:

```powershell
cd ui-tests\scripts
$env:AUTOMATION_RECORD = 'true'
$env:BASE_URL = 'https://sua-app.com'
.\run-free-hands.ps1
```

O `session.json` será salvo em `ui-tests\artifacts\recorder\`.
