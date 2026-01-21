
# Arquitetura — Browser Capture Layer

Browser (listeners JS) → buffer window.__fhRecorder → polling executeScript → Normalizer/Consolidator → Session Builder → session.json

Componentes:
- InjectedRecorderScript (DOM)
- RecorderPoller (runtime)
- EventMapper (JS → domain)
- FillConsolidator (opcional)
