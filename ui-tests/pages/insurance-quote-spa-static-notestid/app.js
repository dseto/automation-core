// SeguroSim • Static SPA (no backend) - v1.3
// Goal: robust UI surface for Automation.Recorder -> logs -> feature generation -> execution.

const $ = (sel, root=document) => root.querySelector(sel);
const $$ = (sel, root=document) => Array.from(root.querySelectorAll(sel));

const view = $('#view');
const breadcrumbs = $('#breadcrumbs');
const pageActions = $('#pageActions');

const modalBackdrop = $('#modalBackdrop');
const modalTitle = $('#modalTitle');
const modalBody = $('#modalBody');
const modalFoot = $('#modalFoot');
const btnCloseModal = $('#btnCloseModal');

const loadingOverlay = $('#loadingOverlay');
const loadingText = $('#loadingText');

const toasts = $('#toasts');

const notifPanel = $('#notifPanel');
const notifList = $('#notifList');

const contextMenu = $('#contextMenu');
const btnOpenContextMenu = $('#btnOpenContextMenu');

const userPill = $('#userPill');
const btnLogout = $('#btnLogout');

const state = {
  auth: {
    isLoggedIn: false,
    user: null,
  },
  quote: {
    tab: 'client',
    validation: { fields: [] },
    data: {
      name: '',
      cpf: '',
      email: '',
      city: '',
      birthDate: '',
      productType: '',
      startDate: '',
      coverage: 'basic',
      addons: { glass:false, roadside:false, theft:true },
      notes: '',
    },
    lastSimulation: null,
  },
  notifications: [
    { id: 'n1', title: 'Sistema em modo DEMO', body: 'Nenhum backend. Tudo roda no browser.', ts: 'agora', read:false },
    { id: 'n2', title: 'Dica de Recorder', body: 'Priorize seletores  para reduzir flakiness.', ts: 'há 2 min', read:false },
    { id: 'n3', title: 'Rota nova', body: 'Tela de pesquisa e grid editável adicionados (v1.1).', ts: 'hoje', read:true },
  ],
  policies: [
    { id:'P-1001', client:'Ana Souza', product:'Auto', status:'Ativa', premium: 'R$ 1.245,90', start:'2026-01-10' },
    { id:'P-1002', client:'Bruno Lima', product:'Residencial', status:'Pendente', premium: 'R$ 612,30', start:'2026-01-18' },
    { id:'P-1003', client:'Carla Mendes', product:'Vida', status:'Cancelada', premium: 'R$ 210,00', start:'2025-12-01' },
  ],
  search: {
    q: '',
    product: 'Todos',
    status: 'Todos',
    results: [],
  }
};

// ---------- utilities ----------
function escapeHtml(s){
  return String(s).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}

function moneyBR(n){
  const v = Number(n || 0);
  const s = v.toFixed(2);
  const [a,b] = s.split('.');
  const withThousands = a.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  return `R$ ${withThousands},${b}`;
}

function pctBR(n){
  const v = Number(n || 0);
  return `${v.toFixed(2).replace('.', ',')}%`;
}

function toast(title, sub=''){
  const el = document.createElement('div');
  el.className = 'toast';
el.innerHTML = `
    <div class="toast-title">${escapeHtml(title)}</div>
    ${sub ? `<div class="toast-sub">${escapeHtml(sub)}</div>` : ''}
  `;
  toasts.appendChild(el);
  setTimeout(()=> el.remove(), 3200);
}

function showLoading(message='Carregando…', ms=900){
  loadingText.textContent = message;
  loadingOverlay.style.display = 'flex';
  loadingOverlay.setAttribute('aria-hidden','false');
  return new Promise(res => setTimeout(()=>{
    loadingOverlay.style.display = 'none';
    loadingOverlay.setAttribute('aria-hidden','true');
    res();
  }, ms));
}

function openModal(title, bodyHtml, footHtml=''){
  modalTitle.textContent = title;
  modalBody.innerHTML = bodyHtml;
  modalFoot.innerHTML = footHtml;
  modalBackdrop.style.display = 'flex';
  modalBackdrop.setAttribute('aria-hidden','false');
}
function closeModal(){
  modalBackdrop.style.display = 'none';
  modalBackdrop.setAttribute('aria-hidden','true');
}
btnCloseModal.addEventListener('click', closeModal);
modalBackdrop.addEventListener('click', (e)=>{
  if(e.target === modalBackdrop) closeModal();
});

function setActiveNav(){
  const hash = location.hash || '#/login';
  $$('.nav-item').forEach(a=> a.classList.remove('active'));
  const match = $(`.nav-item[href="${hash.split('?')[0]}"]`);
  if(match) match.classList.add('active');
}

function setBreadcrumbs(parts){
  breadcrumbs.textContent = parts.join(' / ');
}

function setPageActions(html=''){
  pageActions.innerHTML = html;
}

function saveAuth(){
  localStorage.setItem('seguroSimAuth', JSON.stringify(state.auth));
}
function loadAuth(){
  try{
    const raw = localStorage.getItem('seguroSimAuth');
    if(!raw) return;
    const a = JSON.parse(raw);
    state.auth = a && typeof a === 'object' ? a : state.auth;
  }catch{}
}

function updateUserPill(){
  if(state.auth.isLoggedIn && state.auth.user){
    userPill.textContent = state.auth.user.name;
  }else{
    userPill.textContent = 'Visitante';
  }
}

// ---------- notifications panel ----------
function renderNotifications(){
  notifList.innerHTML = '';
  state.notifications.forEach(n => {
    const el = document.createElement('div');
    el.className = 'notif' + (n.read ? ' read' : '');
el.innerHTML = `
      <div class="badge" aria-hidden="true"></div>
      <div style="flex:1;min-width:0">
        <div class="notif-title">${escapeHtml(n.title)}</div>
        <div class="notif-body">${escapeHtml(n.body)}</div>
        <div class="notif-meta">${escapeHtml(n.ts)}</div>
      </div>
      <button class="btn tiny toggleRead">${n.read?'Não lida':'Marcar lida'}</button>
    `;
    const btn = $('.toggleRead', el);
    btn.addEventListener('click', ()=>{
      n.read = !n.read;
      renderNotifications();
      toast('Notificação atualizada', n.read ? 'Marcada como lida' : 'Marcada como não lida');
    });
    notifList.appendChild(el);
  });
}

function openNotifications(){
  renderNotifications();
  notifPanel.style.display = 'block';
  notifPanel.setAttribute('aria-hidden','false');
}
function closeNotifications(){
  notifPanel.style.display = 'none';
  notifPanel.setAttribute('aria-hidden','true');
}

$('#btnOpenNotifications').addEventListener('click', ()=>{
  const open = notifPanel.style.display === 'block';
  if(open) closeNotifications(); else openNotifications();
});
$('#btnCloseNotifications').addEventListener('click', closeNotifications);

// ---------- context menu ----------
function toggleContextMenu(){
  const open = contextMenu.style.display === 'block';
  contextMenu.style.display = open ? 'none' : 'block';
  contextMenu.setAttribute('aria-hidden', open ? 'true' : 'false');
  // place near button
  const r = btnOpenContextMenu.getBoundingClientRect();
  contextMenu.style.left = (r.left) + 'px';
  contextMenu.style.top = (r.bottom + 10) + 'px';
}
btnOpenContextMenu.addEventListener('click', toggleContextMenu);

contextMenu.addEventListener('click', (e)=>{
  const btn = e.target.closest('button[data-action]');
  if(!btn) return;
  const action = btn.getAttribute('data-action');
  contextMenu.style.display = 'none';
  contextMenu.setAttribute('aria-hidden','true');

  if(action === 'openAbout'){
    openModal('Sobre', `
      <p>SeguroSim é um site estático para exercitar o ciclo Recorder → log → .feature.</p>
      <p class="helper">Sugestão: use seletores <code></code> e valide geração de steps com de-para.</p>
    `, `<button class="btn primary" onclick="closeModal()">OK</button>`);
  }
  if(action === 'openChangelog'){
    openModal('Changelog (v1.1)', `
      <ul>
        <li>Tabs da cotação agora funcionam (Cliente / Produto / Revisão).</li>
        <li>Login simulado com rota protegida (Dashboard, Cotação, Pesquisa, Apólices, Configurações).</li>
        <li>Loading overlay (spinner) em ações como simular e pesquisar.</li>
        <li>Notificações + popup.</li>
        <li>Autocomplete de cidade.</li>
        <li>Datas (calendário nativo) e grid editável em Apólices.</li>
      </ul>
    `, `<button class="btn" onclick="closeModal()">Fechar</button>`);
  }
  if(action === 'openShortcuts'){
    openModal('Atalhos', `
      <ul>
        <li><b>Alt+L</b> → ir para Login</li>
        <li><b>Alt+Q</b> → ir para Nova cotação</li>
        <li><b>Alt+S</b> → ir para Pesquisa</li>
      </ul>
    `, `<button class="btn" onclick="closeModal()">Fechar</button>`);
  }
});

document.addEventListener('click', (e)=>{
  if(e.target.closest('#contextMenu') || e.target.closest('#btnOpenContextMenu')) return;
  contextMenu.style.display = 'none';
  contextMenu.setAttribute('aria-hidden','true');

  if(e.target.closest('#notifPanel') || e.target.closest('#btnOpenNotifications')) return;
  closeNotifications();
});

document.addEventListener('keydown', (e)=>{
  if(e.altKey && e.key.toLowerCase() === 'l') location.hash = '#/login';
  if(e.altKey && e.key.toLowerCase() === 'q') location.hash = '#/quote';
  if(e.altKey && e.key.toLowerCase() === 's') location.hash = '#/search';
});

// ---------- auth ----------
function requireAuth(){
  if(!state.auth.isLoggedIn){
    try{ toast('Acesso restrito', 'Faça login para continuar.'); }catch{}
    window.location.replace('login.html');
    return false;
  }
  return true;
}


btnLogout.addEventListener('click', ()=>{
  state.auth.isLoggedIn = false;
  state.auth.user = null;
  saveAuth();
  updateUserPill();
  toast('Sessão encerrada');
  setTimeout(()=> window.location.replace('login.html'), 150);
});

// ---------- views ----------
function renderLogin(){
  setBreadcrumbs(['Login']);
  setPageActions('');
  view.innerHTML = `
    <div class="card">
      <h2>Entrar</h2>
      <p class="helper">Login simulado (sem backend). Qualquer usuário/senha funciona.</p>

      <div class="row">
        <label class="label">Usuário
          <input class="input" id="loginUser" placeholder="qa.demo" />
        </label>

        <label class="label">Senha
          <input class="input" id="loginPass" type="password" placeholder="••••••••" />
        </label>
      </div>

      <div class="inline" style="margin-top:12px">
        <label class="label" style="flex:1">Token (MFA simulado)
          <input class="input" id="loginMfa" placeholder="000000" />
        </label>
        <button class="btn primary" id="btnLogin">Entrar</button>
      </div>
    </div>
  `;

  $('#btnLogin').addEventListener('click', async ()=>{
    const user = ($('#loginUser').value || 'qa.demo').trim();
    await showLoading('Validando credenciais…', 900);
    state.auth.isLoggedIn = true;
    state.auth.user = { name: user };
    saveAuth();
    updateUserPill();
    toast('Login realizado', `Bem-vindo, ${user}`);
    location.hash = '#/dashboard';
  });
}

function renderDashboard(){
  if(!requireAuth()) return;
  setBreadcrumbs(['Dashboard']);
  setPageActions(`
    <button class="btn" id="btnDashOpenModal">Abrir modal</button>
    <button class="btn primary" id="btnDashGoQuote">Nova cotação</button>
  `);
  view.innerHTML = `
    <div class="card">
      <h2>Visão geral</h2>
      <div class="row">
        <div class="card" style="margin:0">
          <h3 style="margin:0 0 6px">Cotações</h3>
          <div style="font-size:26px;font-weight:800">128</div>
          <div class="helper">Últimos 30 dias</div>
        </div>
        <div class="card" style="margin:0">
          <h3 style="margin:0 0 6px">Apólices</h3>
          <div style="font-size:26px;font-weight:800">42</div>
          <div class="helper">Ativas</div>
        </div>
      </div>

      <hr class="sep"/>

      <div class="inline">
        <button class="btn" id="btnDashToast">Gerar toast</button>
        <button class="btn" id="btnDashLoading">Simular carregamento</button>
      </div>
    </div>
  `;

  $('#btnDashOpenModal').addEventListener('click', ()=>{
    openModal('Janela modal', `
      <p>Este modal serve para testar eventos de abrir/fechar e conteúdo.</p>
      <label class="label">Campo no modal
        <input class="input" placeholder="Digite algo..." />
      </label>
    `, `
      <button class="btn" onclick="closeModal()">Cancelar</button>
      <button class="btn primary" onclick="closeModal()">OK</button>
    `);
  });
  $('#btnDashGoQuote').addEventListener('click', ()=> location.hash = '#/quote');

  $('#btnDashToast').addEventListener('click', ()=> toast('Toast do Dashboard','Evento simples para log'));
  $('#btnDashLoading').addEventListener('click', async ()=>{
    await showLoading('Carregando dashboard…', 1200);
    toast('Pronto', 'Carregamento concluído');
  });
}

// Quote tabs content helpers
const CITY_SUGGESTIONS = ['São Paulo', 'Santo André', 'São Bernardo do Campo', 'Campinas', 'Rio de Janeiro', 'Curitiba', 'Belo Horizonte', 'Porto Alegre', 'Florianópolis', 'Brasília'];

const PRICING = {
  Auto: {
    basic: { basePremium: 980.00, baseFranchise: 1500.00, juros: 2.50, iof: 3.38, parcelas: 12 },
    full:  { basePremium: 1490.00, baseFranchise: 1200.00, juros: 2.50, iof: 3.38, parcelas: 12 },
    addons: {
      glass:    { label: 'Vidros', premium: 120.00, franchise: 300.00 },
      roadside: { label: 'Assistência 24h', premium: 80.00, franchise: 0.00 },
      theft:    { label: 'Roubo/Furto', premium: 150.00, franchise: 900.00 },
    }
  },
  Residencial: {
    basic: { basePremium: 650.00, baseFranchise: 500.00, juros: 2.10, iof: 3.38, parcelas: 10 },
    full:  { basePremium: 980.00, baseFranchise: 350.00, juros: 2.10, iof: 3.38, parcelas: 10 },
    addons: {
      glass:    { label: 'Danos elétricos', premium: 90.00, franchise: 200.00 },
      roadside: { label: 'Assistência residencial', premium: 60.00, franchise: 0.00 },
      theft:    { label: 'Roubo/Furto', premium: 110.00, franchise: 650.00 },
    }
  },
  Vida: {
    basic: { basePremium: 210.00, baseFranchise: 0.00, juros: 1.90, iof: 0.38, parcelas: 12 },
    full:  { basePremium: 350.00, baseFranchise: 0.00, juros: 1.90, iof: 0.38, parcelas: 12 },
    addons: {
      glass:    { label: 'Invalidez', premium: 55.00, franchise: 0.00 },
      roadside: { label: 'Diária hospitalar', premium: 40.00, franchise: 0.00 },
      theft:    { label: 'Doenças graves', premium: 75.00, franchise: 0.00 },
    }
  }
};

function validateRequiredQuote(){
  const d = state.quote.data;
  const missing = [];
  if(!d.name.trim()) missing.push('client.name');
  if(!d.cpf.trim()) missing.push('client.cpf');
  if(!d.email.trim()) missing.push('client.email');
  if(!d.birthDate.trim()) missing.push('client.birthDate');
  if(!d.city.trim()) missing.push('client.city');
  if(!d.productType.trim()) missing.push('product.type');
  if(!d.startDate.trim()) missing.push('product.startDate');
  if(!d.coverage.trim()) missing.push('product.coverage');
  return missing;
}

function simulateDeterministicQuote(){
  const d = state.quote.data;
  const model = PRICING[d.productType] || PRICING.Auto;
  const plan = model[d.coverage] || model.basic;

  const rows = [];
  rows.push({ coverage: d.coverage === 'full' ? 'Cobertura completa' : 'Cobertura básica', premium: plan.basePremium, franchise: plan.baseFranchise });

  const addMap = model.addons;
  Object.entries(d.addons).forEach(([k, enabled])=>{
    if(!enabled) return;
    const a = addMap[k];
    if(!a) return;
    rows.push({ coverage: a.label, premium: a.premium, franchise: a.franchise });
  });

  const premioTotal = rows.reduce((s,r)=> s + r.premium, 0);
  const iofRate = plan.iof;
  const jurosRate = plan.juros;
  const parcelas = plan.parcelas;

  const totalComIof = premioTotal * (1 + iofRate/100);
  const totalFinanciado = totalComIof * (1 + jurosRate/100);
  const valorParcela = totalFinanciado / parcelas;

  return {
    ts: new Date().toLocaleString('pt-BR'),
    rows,
    premioTotal,
    premium: moneyBR(premioTotal),
    iofRate,
    jurosRate,
    parcelas,
    totalComIof,
    totalFinanciado,
    valorParcela,
    valorParcelaFmt: moneyBR(valorParcela),
  };
}

function renderQuote(){
  if(!requireAuth()) return;
  setBreadcrumbs(['Nova cotação']);
  setPageActions(`
    <button class="btn" id="btnQuoteReset">Limpar</button>
    <button class="btn primary" id="btnQuoteSubmit">Enviar</button>
  `);

  view.innerHTML = `
    <div class="card">
      <h2>Cotação de Seguro</h2>
      <div class="tabs">
        <div class="tab ${state.quote.tab==='client'?'active':''}" data-tab="client">Cliente</div>
        <div class="tab ${state.quote.tab==='product'?'active':''}" data-tab="product">Produto</div>
        <div class="tab ${state.quote.tab==='review'?'active':''}" data-tab="review">Revisão</div>
      </div>

      <div id="quoteTabBody"></div>
    </div>
  `;

  const tabBody = $('#quoteTabBody');

  function mountTab(){
    if(state.quote.tab === 'client'){
      tabBody.innerHTML = `
        <div class="row">
          <label class="label">Nome completo
            <input class="input" id="qName" placeholder="Ex: Daniel Seto" value="${escapeHtml(state.quote.data.name)}" />
          </label>
          <label class="label">CPF
            <input class="input" id="qCpf" placeholder="000.000.000-00" value="${escapeHtml(state.quote.data.cpf)}" />
          </label>
        </div>

        <div class="row" style="margin-top:12px">
          <label class="label">E-mail
            <input class="input" id="qEmail" placeholder="email@exemplo.com" value="${escapeHtml(state.quote.data.email)}" />
          </label>

          <label class="label">Data de nascimento (calendário)
            <input class="input" id="qBirth" type="date" value="${escapeHtml(state.quote.data.birthDate)}" />
          </label>
        </div>

        <div class="row" style="margin-top:12px">
          <label class="label autocomplete">Cidade (autocomplete)
            <input class="input" id="qCity" placeholder="Digite para sugerir..." value="${escapeHtml(state.quote.data.city)}" autocomplete="off"/>
            <div class="suggestions" id="citySuggestions"></div>
            <div class="helper">Clique numa sugestão ou continue digitando.</div>
          </label>

          <div class="card" style="margin:0">
            <div class="helper" style="margin:0 0 8px">Preferências do cliente</div>
            <label class="inline">
              <input type="checkbox" id="qOptin" />
              Receber ofertas por e-mail
            </label>
            <label class="inline">
              <input type="checkbox" id="qVip" />
              Cliente VIP (simulado)
            </label>
          </div>
        </div>

        <hr class="sep"/>

        <div class="inline">
          <button class="btn" id="btnToProduct">Ir para Produto →</button>
        </div>
      `;

      const qCity = $('#qCity');
      const sug = $('#citySuggestions');

      function closeSug(){ sug.style.display='none'; sug.innerHTML=''; }
      function openSug(items){
        sug.innerHTML = items.map(c => `<button class="sugg-item" type="button">${escapeHtml(c)}</button>`).join('');
        sug.style.display = items.length ? 'block' : 'none';
        $$('.sugg-item', sug).forEach(btn => btn.addEventListener('click', ()=>{
          qCity.value = btn.textContent;
          state.quote.data.city = qCity.value;
          closeSug();
        }));
      }

      qCity.addEventListener('input', ()=>{
        state.quote.data.city = qCity.value;
        const q = qCity.value.trim().toLowerCase();
        if(q.length < 2){ closeSug(); return; }
        const items = CITY_SUGGESTIONS.filter(c => c.toLowerCase().includes(q)).slice(0,6);
        openSug(items);
      });
      qCity.addEventListener('blur', ()=> setTimeout(closeSug, 150));
      qCity.addEventListener('focus', ()=>{
        const q = qCity.value.trim().toLowerCase();
        if(q.length >= 2){
          const items = CITY_SUGGESTIONS.filter(c => c.toLowerCase().includes(q)).slice(0,6);
          openSug(items);
        }
      });

      $('#qName').addEventListener('input', e => state.quote.data.name = e.target.value);
      $('#qCpf').addEventListener('input', e => state.quote.data.cpf = e.target.value);
      $('#qEmail').addEventListener('input', e => state.quote.data.email = e.target.value);
      $('#qBirth').addEventListener('input', e => state.quote.data.birthDate = e.target.value);

      $('#btnToProduct').addEventListener('click', ()=>{
        state.quote.tab = 'product';
        renderQuote();
      });
    }

    if(state.quote.tab === 'product'){
      tabBody.innerHTML = `
        <div class="row">
          <label class="label">Tipo de seguro
            <select class="input" id="qProductType">
              <option ${state.quote.data.productType===''?'selected':''}> </option>
              <option ${state.quote.data.productType==='Auto'?'selected':''}>Auto</option>
              <option ${state.quote.data.productType==='Residencial'?'selected':''}>Residencial</option>
              <option ${state.quote.data.productType==='Vida'?'selected':''}>Vida</option>
            </select>
          </label>

          <label class="label">Início de vigência (calendário)
            <input class="input" id="qStartDate" type="date" value="${escapeHtml(state.quote.data.startDate)}" />
          </label>
        </div>

        <div class="card" style="margin-top:12px">
          <div class="helper" style="margin:0 0 8px">Cobertura (radio)</div>
          <label class="inline">
            <input type="radio" name="cov" ${state.quote.data.coverage==='basic'?'checked':''} />
            Básica
          </label>
          <label class="inline">
            <input type="radio" name="cov" ${state.quote.data.coverage==='full'?'checked':''} />
            Completa
          </label>
        </div>

        <div class="card" style="margin-top:12px">
          <div class="helper" style="margin:0 0 8px">Adicionais (checkbox)</div>
          <div class="row">
            <label class="inline">
              <input type="checkbox" ${state.quote.data.addons.glass?'checked':''} />
              Vidros
            </label>
            <label class="inline">
              <input type="checkbox" ${state.quote.data.addons.roadside?'checked':''} />
              Assistência 24h
            </label>
          </div>
          <div class="row" style="margin-top:10px">
            <label class="inline">
              <input type="checkbox" ${state.quote.data.addons.theft?'checked':''} />
              Roubo/Furto
            </label>
            <label class="label">Observações
              <textarea class="input" id="qNotes">${escapeHtml(state.quote.data.notes)}</textarea>
            </label>
          </div>
        </div>

        <hr class="sep"/>
        <div class="inline">
          <button class="btn" id="btnBackClient">← Voltar</button>
          <button class="btn" id="btnSimulate">Simular (spinner)</button>
          <button class="btn primary" id="btnToReview">Ir para Revisão →</button>
        </div>
      `;

      $('#qProductType').addEventListener('change', e => state.quote.data.productType = e.target.value.trim());
      $('#qStartDate').addEventListener('input', e => state.quote.data.startDate = e.target.value);

      // radios
      $('[="quote.product.coverage.basic.radio"]').addEventListener('change', ()=> state.quote.data.coverage='basic');
      $('[="quote.product.coverage.full.radio"]').addEventListener('change', ()=> state.quote.data.coverage='full');

      // checkboxes
      $('[="quote.product.addons.glass.checkbox"]').addEventListener('change', e => state.quote.data.addons.glass = e.target.checked);
      $('[="quote.product.addons.roadside.checkbox"]').addEventListener('change', e => state.quote.data.addons.roadside = e.target.checked);
      $('[="quote.product.addons.theft.checkbox"]').addEventListener('change', e => state.quote.data.addons.theft = e.target.checked);

      $('#qNotes').addEventListener('input', e => state.quote.data.notes = e.target.value);

      $('#btnBackClient').addEventListener('click', ()=>{ state.quote.tab='client'; renderQuote(); });
      $('#btnToReview').addEventListener('click', ()=>{ state.quote.tab='review'; renderQuote(); });

      $('#btnSimulate').addEventListener('click', async ()=>{
        const missing = validateRequiredQuote();
        state.quote.validation.fields = missing;

        if(missing.length){
          toast('Validação', 'Preencha os campos obrigatórios antes de simular.');

          const list = missing.map(m => `<li>${escapeHtml(m)}</li>`).join('');
          openModal('Campos obrigatórios pendentes', `
            <div>
              <p class="helper">Preencha os campos abaixo e tente novamente.</p>
              <ul style="margin:8px 0 0;padding-left:18px">${list}</ul>
            </div>
          `, `
            <button class="btn primary" onclick="closeModal()">OK</button>
          `);

          // Leva o usuário para a aba mais provável (cliente/produto)
          state.quote.tab = missing.some(x=>x.startsWith('client.')) ? 'client' : 'product';
          renderQuote();
          return;
        }

        await showLoading('Calculando prêmio…', 1200);

        const sim = simulateDeterministicQuote();
        state.quote.lastSimulation = sim;

        // Modal com grid (premio + franquia por cobertura)
        const rowsHtml = sim.rows.map((r, idx)=>`
          <tr>
            <td>${escapeHtml(r.coverage)}</td>
            <td>${moneyBR(r.premium)}</td>
            <td>${moneyBR(r.franchise)}</td>
          </tr>
        `).join('');

        openModal('Resultado da simulação', `
          <div>
            <table class="table">
              <thead>
                <tr>
                  <th>Cobertura</th>
                  <th>Prêmio</th>
                  <th>Franquia</th>
                </tr>
              </thead>
              <tbody>
                ${rowsHtml}
                <tr>
                  <td><b>Total</b></td>
                  <td><b>${sim.premium}</b></td>
                  <td><span class="helper">—</span></td>
                </tr>
              </tbody>
            </table>

            <hr class="sep"/>

            <div class="row">
              <div>
                <div class="helper">Condições</div>
                <div><b>IOF:</b> ${pctBR(sim.iofRate)}</div>
                <div><b>Juros:</b> ${pctBR(sim.jurosRate)}</div>
                <div><b>Parcelas:</b> ${sim.parcelas}x</div>
              </div>
              <div>
                <div class="helper">Totais</div>
                <div><b>Total com IOF:</b> ${moneyBR(sim.totalComIof)}</div>
                <div><b>Total financiado:</b> ${moneyBR(sim.totalFinanciado)}</div>
                <div><b>Valor parcela:</b> ${sim.valorParcelaFmt}</div>
              </div>
            </div>

            <p class="helper" style="margin-top:10px">
              Valores são <b>fixos/determinísticos</b> para facilitar asserts em testes automatizados.
            </p>
          </div>
        `, `
          <button class="btn" onclick="closeModal()">Fechar</button>
          <button class="btn primary" id="btnModalToReview">Ir para Revisão</button>
        `);

        setTimeout(()=>{
          const b = $('#btnModalToReview');
          if(b) b.addEventListener('click', ()=>{ closeModal(); state.quote.tab='review'; renderQuote(); });
        }, 0);

        toast('Simulação concluída', sim.premium);
      });
    }

    if(state.quote.tab === 'review'){
      const d = state.quote.data;
      const sim = state.quote.lastSimulation;
      tabBody.innerHTML = `
        <div class="card" style="margin:0">
          <h3 style="margin:0 0 10px">Revisão</h3>

          <div class="row">
            <div>
              <div class="helper">Cliente</div>
              <div><b>Nome:</b> ${escapeHtml(d.name || '—')}</div>
              <div><b>CPF:</b> ${escapeHtml(d.cpf || '—')}</div>
              <div><b>E-mail:</b> ${escapeHtml(d.email || '—')}</div>
              <div><b>Cidade:</b> ${escapeHtml(d.city || '—')}</div>
              <div><b>Nascimento:</b> ${escapeHtml(d.birthDate || '—')}</div>
            </div>
            <div>
              <div class="helper">Produto</div>
              <div><b>Tipo:</b> ${escapeHtml(d.productType || '—')}</div>
              <div><b>Início:</b> ${escapeHtml(d.startDate || '—')}</div>
              <div><b>Cobertura:</b> ${escapeHtml(d.coverage)}</div>
              <div><b>Adicionais:</b> ${escapeHtml(Object.entries(d.addons).filter(([k,v])=>v).map(([k])=>k).join(', ') || 'nenhum')}</div>
            </div>
          </div>

          <hr class="sep"/>

          <div class="helper" style="margin-bottom:6px">Simulação</div>
          <div>
            ${sim ? `
              <div><b>Prêmio total:</b> <span>${escapeHtml(sim.premium)}</span></div>
              <div class="helper">Parcelas: ${sim.parcelas}x • Valor: ${escapeHtml(sim.valorParcelaFmt || '')}</div>

              <table class="table" style="margin-top:10px">
                <thead>
                  <tr><th>Cobertura</th><th>Prêmio</th><th>Franquia</th></tr>
                </thead>
                <tbody>
                  ${sim.rows.map((r,i)=>`
                    <tr>
                      <td>${escapeHtml(r.coverage)}</td>
                      <td>${moneyBR(r.premium)}</td>
                      <td>${moneyBR(r.franchise)}</td>
                    </tr>
                  `).join('')}
                  <tr>
                    <td><b>Total</b></td>
                    <td><b>${escapeHtml(sim.premium)}</b></td>
                    <td><span class="helper">—</span></td>
                  </tr>
                </tbody>
              </table>

              <div class="helper" style="margin-top:8px">Última simulação: ${escapeHtml(sim.ts)}</div>
            ` : `<div class="helper">Nenhuma simulação realizada.</div>`}
          </div>

          <hr class="sep"/>

          <div class="inline">
            <button class="btn" id="btnBackProduct">← Voltar</button>
            <button class="btn primary" id="btnFinishQuote">Finalizar cotação</button>
          </div>
        </div>
      `;

      $('#btnBackProduct').addEventListener('click', ()=>{ state.quote.tab='product'; renderQuote(); });
      $('#btnFinishQuote').addEventListener('click', async ()=>{
        await showLoading('Salvando cotação…', 1000);
        toast('Cotação finalizada', 'Registro local criado (simulado)');
        openModal('Cotação enviada', `
          <p>Cotação enviada com sucesso (simulado).</p>
          <p class="helper">Você pode ir para Pesquisa para localizar resultados (simulado).</p>
        `, `
          <button class="btn" id="btnGoSearch">Ir para Pesquisa</button>
          <button class="btn primary" onclick="closeModal()">Fechar</button>
        `);
        setTimeout(()=>{
          const b = $('#btnGoSearch');
          if(b) b.addEventListener('click', ()=>{ closeModal(); location.hash = '#/search'; });
        }, 0);
      });
    }
  }

  // tabs click handlers
  $$('.tab').forEach(t => t.addEventListener('click', ()=>{
    state.quote.tab = t.getAttribute('data-tab');
    renderQuote();
  }));

  mountTab();

  // top actions
  $('#btnQuoteReset').addEventListener('click', ()=>{
    state.quote.data = { name:'', cpf:'', email:'', city:'', birthDate:'', productType:'', startDate:'', coverage:'basic', addons:{glass:false,roadside:false,theft:true}, notes:'' };
    state.quote.lastSimulation = null;
    toast('Formulário limpo');
    renderQuote();
  });
  $('#btnQuoteSubmit').addEventListener('click', ()=>{
    toast('Validação', 'Ação de submit para capturar em log');
    openModal('Enviar cotação', `
      <p>Simulação de envio: confirme para continuar.</p>
    `, `
      <button class="btn" onclick="closeModal()">Cancelar</button>
      <button class="btn primary" id="btnConfirmSubmit">Confirmar</button>
    `);
    setTimeout(()=>{
      const b = $('#btnConfirmSubmit');
      if(b) b.addEventListener('click', ()=>{ closeModal(); state.quote.tab='review'; renderQuote(); });
    }, 0);
  });
}

function renderSearch(){
  if(!requireAuth()) return;
  setBreadcrumbs(['Pesquisar']);
  setPageActions(`
    <button class="btn" id="btnSearchClear">Limpar</button>
    <button class="btn primary" id="btnSearchRun">Pesquisar (spinner)</button>
  `);

  const results = state.search.results;

  view.innerHTML = `
    <div class="card">
      <h2>Pesquisa de cotações/apólices</h2>

      <div class="row">
        <label class="label">Texto
          <input class="input" id="sQ" placeholder="Ex: Ana, P-1001..." value="${escapeHtml(state.search.q)}" />
        </label>
        <label class="label">Produto
          <select class="input" id="sProduct">
            ${['Todos','Auto','Residencial','Vida'].map(p=>`<option ${state.search.product===p?'selected':''}>${p}</option>`).join('')}
          </select>
        </label>
      </div>

      <div class="row" style="margin-top:12px">
        <label class="label">Status
          <select class="input" id="sStatus">
            ${['Todos','Ativa','Pendente','Cancelada'].map(p=>`<option ${state.search.status===p?'selected':''}>${p}</option>`).join('')}
          </select>
        </label>
        <div class="card" style="margin:0">
          <div class="helper" style="margin:0 0 8px">Opções</div>
          <label class="inline">
            <input type="checkbox" id="sOnlyRecent"/>
            Somente últimos 30 dias (simulado)
          </label>
          <label class="inline">
            <input type="checkbox" id="sIncludeQuotes" checked/>
            Incluir cotações (simulado)
          </label>
        </div>
      </div>

      <hr class="sep"/>

      <div class="helper">Resultados</div>
      <table class="table">
        <thead>
          <tr>
            <th>ID</th><th>Cliente</th><th>Produto</th><th>Status</th><th>Ação</th>
          </tr>
        </thead>
        <tbody id="searchResultsBody">
          ${results.length ? results.map(r => `
            <tr>
              <td>${escapeHtml(r.id)}</td>
              <td>${escapeHtml(r.client)}</td>
              <td>${escapeHtml(r.product)}</td>
              <td>${escapeHtml(r.status)}</td>
              <td><button class="btn tiny open-result" data-id="${r.id}">Abrir</button></td>
            </tr>
          `).join('') : `<tr><td colspan="5" class="helper">Nenhum resultado. Execute uma pesquisa.</td></tr>`}
        </tbody>
      </table>
    </div>
  `;

  $('#sQ').addEventListener('input', e=> state.search.q = e.target.value);
  $('#sProduct').addEventListener('change', e=> state.search.product = e.target.value);
  $('#sStatus').addEventListener('change', e=> state.search.status = e.target.value);

  $('#btnSearchClear').addEventListener('click', ()=>{
    state.search.q=''; state.search.product='Todos'; state.search.status='Todos'; state.search.results=[];
    toast('Filtros limpos');
    renderSearch();
  });

  $('#btnSearchRun').addEventListener('click', async ()=>{
    await showLoading('Consultando base…', 1100);

    const q = state.search.q.trim().toLowerCase();
    const prod = state.search.product;
    const st = state.search.status;

    let rows = state.policies.map(p => ({...p}));
    if(prod !== 'Todos') rows = rows.filter(r => r.product === prod);
    if(st !== 'Todos') rows = rows.filter(r => r.status === st);
    if(q) rows = rows.filter(r =>
      r.id.toLowerCase().includes(q) ||
      r.client.toLowerCase().includes(q) ||
      r.product.toLowerCase().includes(q)
    );

    state.search.results = rows.slice(0, 12);
    toast('Pesquisa concluída', `${state.search.results.length} resultado(s)`);
    renderSearch();
  });

  $$('button.open-result').forEach(btn => btn.addEventListener('click', ()=>{
    const id = btn.getAttribute('data-id');
    const item = state.policies.find(p => p.id === id);
    openModal(`Detalhe ${id}`, `
      <div>
        <p><b>Cliente:</b> ${escapeHtml(item?.client || '')}</p>
        <p><b>Produto:</b> ${escapeHtml(item?.product || '')}</p>
        <p><b>Status:</b> ${escapeHtml(item?.status || '')}</p>
        <p class="helper">Modal aberto a partir de resultado da pesquisa.</p>
      </div>
    `, `<button class="btn primary" onclick="closeModal()">OK</button>`);
  }));
}

function renderPolicies(){
  if(!requireAuth()) return;
  setBreadcrumbs(['Apólices']);
  setPageActions(`
    <button class="btn" id="btnPoliciesAdd">Adicionar (modal)</button>
    <button class="btn primary" id="btnPoliciesSave">Salvar grid</button>
  `);

  view.innerHTML = `
    <div class="card">
      <h2>Apólices (grid editável)</h2>
      <p class="helper">Algumas células são editáveis (contenteditable) para testar Recorder + execução.</p>

      <table class="table">
        <thead>
          <tr>
            <th>ID</th><th>Cliente</th><th>Produto</th><th>Status</th><th>Prêmio</th><th>Início</th><th>Ação</th>
          </tr>
        </thead>
        <tbody>
          ${state.policies.map(p=>`
            <tr>
              <td>${escapeHtml(p.id)}</td>
              <td>
                <div class="grid-cell" contenteditable="true" data-id="${p.id}" data-field="client">${escapeHtml(p.client)}</div>
              </td>
              <td>${escapeHtml(p.product)}</td>
              <td>
                <select class="input" data-id="${p.id}" data-field="status">
                  ${['Ativa','Pendente','Cancelada'].map(s=>`<option ${p.status===s?'selected':''}>${s}</option>`).join('')}
                </select>
              </td>
              <td>
                <div class="grid-cell" contenteditable="true" data-id="${p.id}" data-field="premium">${escapeHtml(p.premium)}</div>
              </td>
              <td>
                <input class="input" type="date" data-id="${p.id}" data-field="start" value="${escapeHtml(p.start)}"/>
              </td>
              <td>
                <button class="btn tiny open-policy" data-id="${p.id}">Abrir</button>
                <button class="btn tiny danger delete-policy" data-id="${p.id}">Excluir</button>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    </div>
  `;

  $('#btnPoliciesAdd').addEventListener('click', ()=>{
    openModal('Adicionar apólice', `
      <div class="row">
        <label class="label">Cliente
          <input class="input" id="addClient" placeholder="Nome do cliente"/>
        </label>
        <label class="label">Produto
          <select class="input" id="addProduct">
            <option>Auto</option><option>Residencial</option><option>Vida</option>
          </select>
        </label>
      </div>
      <div class="row" style="margin-top:12px">
        <label class="label">Status
          <select class="input" id="addStatus">
            <option>Ativa</option><option>Pendente</option><option>Cancelada</option>
          </select>
        </label>
        <label class="label">Início
          <input class="input" type="date" id="addStart"/>
        </label>
      </div>
      <div class="row" style="margin-top:12px">
        <label class="label">Prêmio
          <input class="input" id="addPremium" placeholder="R$ 0,00"/>
        </label>
      </div>
    `, `
      <button class="btn" onclick="closeModal()">Cancelar</button>
      <button class="btn primary" id="btnConfirmAdd">Adicionar</button>
    `);

    setTimeout(()=>{
      const b = $('#btnConfirmAdd');
      if(!b) return;
      b.addEventListener('click', ()=>{
        const id = 'P-' + String(Math.floor(1000 + Math.random()*8999));
        state.policies.unshift({
          id,
          client: ($('#addClient').value || 'Novo Cliente').trim(),
          product: $('#addProduct').value,
          status: $('#addStatus').value,
          premium: ($('#addPremium').value || 'R$ 0,00').trim(),
          start: $('#addStart').value || new Date().toISOString().slice(0,10),
        });
        closeModal();
        toast('Apólice adicionada', id);
        renderPolicies();
      });
    }, 0);
  });

  $('#btnPoliciesSave').addEventListener('click', async ()=>{
    await showLoading('Persistindo alterações…', 900);
    toast('Grid salvo', 'Persistência local (simulada)');
  });

  $$('select[data-field="status"]').forEach(sel => sel.addEventListener('change', (e)=>{
    const id = e.target.getAttribute('data-id');
    const row = state.policies.find(p => p.id === id);
    if(row) row.status = e.target.value;
  }));
  $$('input[data-field="start"]').forEach(inp => inp.addEventListener('input', (e)=>{
    const id = e.target.getAttribute('data-id');
    const row = state.policies.find(p => p.id === id);
    if(row) row.start = e.target.value;
  }));
  $$('div[contenteditable="true"][data-field]').forEach(div => {
    div.addEventListener('input', (e)=>{
      const id = e.target.getAttribute('data-id');
      const field = e.target.getAttribute('data-field');
      const row = state.policies.find(p => p.id === id);
      if(row) row[field] = e.target.textContent;
    });
  });

  $$('button.open-policy').forEach(btn => btn.addEventListener('click', ()=>{
    const id = btn.getAttribute('data-id');
    const p = state.policies.find(x => x.id === id);
    openModal(`Apólice ${id}`, `
      <div>
        <p><b>Cliente:</b> ${escapeHtml(p.client)}</p>
        <p><b>Produto:</b> ${escapeHtml(p.product)}</p>
        <p><b>Status:</b> ${escapeHtml(p.status)}</p>
        <p><b>Prêmio:</b> ${escapeHtml(p.premium)}</p>
        <p><b>Início:</b> ${escapeHtml(p.start)}</p>
      </div>
    `, `<button class="btn primary" onclick="closeModal()">OK</button>`);
  }));

  $$('button.delete-policy').forEach(btn => btn.addEventListener('click', ()=>{
    const id = btn.getAttribute('data-id');
    openModal('Confirmar exclusão', `
      <p>Excluir a apólice <b>${escapeHtml(id)}</b>?</p>
    `, `
      <button class="btn" onclick="closeModal()">Cancelar</button>
      <button class="btn danger" id="btnConfirmDelete">Excluir</button>
    `);
    setTimeout(()=>{
      const b = $('#btnConfirmDelete');
      if(!b) return;
      b.addEventListener('click', ()=>{
        state.policies = state.policies.filter(p => p.id !== id);
        closeModal();
        toast('Apólice excluída', id);
        renderPolicies();
      });
    }, 0);
  }));
}

function renderSettings(){
  if(!requireAuth()) return;
  setBreadcrumbs(['Configurações']);
  setPageActions(`<button class="btn primary" id="btnSettingsSave">Salvar</button>`);

  view.innerHTML = `
    <div class="card">
      <h2>Configurações</h2>
      <div class="row">
        <label class="label">Ambiente (dropdown)
          <select class="input" id="setEnv">
            <option>DEV</option><option selected>HML</option><option>PRD</option>
          </select>
        </label>
        <label class="label">Idioma
          <select class="input" id="setLang">
            <option selected>pt-BR</option><option>en-US</option>
          </select>
        </label>
      </div>

      <div class="row" style="margin-top:12px">
        <div class="card" style="margin:0">
          <div class="helper" style="margin:0 0 8px">Flags</div>
          <label class="inline">
            <input type="checkbox" id="darkMode"/>
            Modo escuro (simulado)
          </label>
          <label class="inline">
            <input type="checkbox" id="telemetry" checked/>
            Telemetria (simulado)
          </label>
        </div>

        <div class="card" style="margin:0">
          <div class="helper" style="margin:0 0 8px">Perfil</div>
          <label class="label">Perfil (radio)
            <div class="inline">
              <label class="inline">
                <input type="radio" name="role" checked/> QA
              </label>
              <label class="inline">
                <input type="radio" name="role"/> Dev
              </label>
            </div>
          </label>
        </div>
      </div>
    </div>
  `;

  $('#btnSettingsSave').addEventListener('click', async ()=>{
    await showLoading('Salvando configurações…', 900);
    toast('Configurações salvas');
  });
}

// ---------- router ----------
const ROUTES = {
  '#/dashboard': renderDashboard,
  '#/quote': renderQuote,
  '#/search': renderSearch,
  '#/policies': renderPolicies,
  '#/settings': renderSettings,
};

function route(){
  const raw = location.hash || '#/dashboard';
  const path = raw.split('?')[0];
  setActiveNav();

  const fn = ROUTES[path] || renderDashboard;
  fn();
}

window.addEventListener('hashchange', route);

// Help button
$('#btnOpenHelp').addEventListener('click', ()=>{
  openModal('Ajuda', `
    <div>
      <p>Este site é um "alvo" para testes do Automation.Recorder.</p>
      <ul>
        <li>Rotas: Login, Dashboard, Cotação (tabs), Pesquisa (spinner), Apólices (grid editável), Configurações.</li>
        <li>Seletores recomendados: <code></code>.</li>
      </ul>
    </div>
  `, `<button class="btn primary" onclick="closeModal()">OK</button>`);
});

// Sidebar toggle (simple: hide/show)
$('#btnToggleSidebar').addEventListener('click', ()=>{
  const sb = $('#sidebar');
  const hidden = sb.style.display === 'none';
  sb.style.display = hidden ? 'flex' : 'none';
  toast(hidden ? 'Menu aberto' : 'Menu fechado');
});

// Boot
loadAuth();
updateUserPill();

if(!state.auth.isLoggedIn){
  window.location.replace('login.html');
}

// Default route
if(!location.hash){
  location.hash = '#/dashboard';
}
route();
