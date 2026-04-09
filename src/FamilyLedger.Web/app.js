const api = '/api/v1';
const userSessionKey = 'familyledger-user-session';
const adminSessionKey = 'familyledger-admin-session';

const state = {
  mode: 'real',
  userSession: loadJson(userSessionKey),
  adminSession: loadJson(adminSessionKey),
  userData: { profiles: [], accounts: [], transactions: [] },
  adminData: null,
  userView: 'overview',
  adminView: 'overview',
  test: { token: null, profileToken: null, profileId: null, accountId: null }
};

const els = {
  modeStatus: document.getElementById('mode-status'),
  navHome: document.getElementById('nav-home'),
  navUser: document.getElementById('nav-user'),
  navAdmin: document.getElementById('nav-admin'),
  navTest: document.getElementById('nav-test'),
  sessionActions: document.getElementById('session-actions'),
  screens: [...document.querySelectorAll('.screen')],
  userPanels: [...document.querySelectorAll('[data-user-panel]')],
  adminPanels: [...document.querySelectorAll('[data-admin-panel]')],
  userMenuBtns: [...document.querySelectorAll('[data-user-view]')],
  adminMenuBtns: [...document.querySelectorAll('[data-admin-view]')],
  landingSessionCards: document.getElementById('landing-session-cards'),
  userRegisterStatus: document.getElementById('user-register-status'),
  userLoginStatus: document.getElementById('user-login-status'),
  userSessionSummary: document.getElementById('user-session-summary'),
  userOverviewMetrics: document.getElementById('user-overview-metrics'),
  userOverviewStatus: document.getElementById('user-overview-status'),
  userProfileSwitcher: document.getElementById('user-profile-switcher'),
  userProfilesTable: document.getElementById('user-profiles-table'),
  userAccountsTable: document.getElementById('user-accounts-table'),
  userTransactionsTable: document.getElementById('user-transactions-table'),
  userRecentTransactions: document.getElementById('user-recent-transactions'),
  userProfileStatus: document.getElementById('user-profile-status'),
  userAccountStatus: document.getElementById('user-account-status'),
  userTransactionStatus: document.getElementById('user-transaction-status'),
  userTransactionAccount: document.getElementById('user-transaction-account'),
  adminLoginStatus: document.getElementById('admin-login-status'),
  adminSessionSummary: document.getElementById('admin-session-summary'),
  adminOverviewMetrics: document.getElementById('admin-overview-metrics'),
  adminOverviewCallout: document.getElementById('admin-overview-callout'),
  adminUsersTable: document.getElementById('admin-users-table'),
  adminProfilesSummaryTable: document.getElementById('admin-profiles-summary-table'),
  adminMembershipsTable: document.getElementById('admin-memberships-table'),
  adminTransactionsTable: document.getElementById('admin-transactions-table'),
  adminSettingsList: document.getElementById('admin-settings-list'),
  adminProfileStatus: document.getElementById('admin-profile-status'),
  testLog: document.getElementById('test-log'),
  testStateCards: document.getElementById('test-state-cards')
};

function loadJson(key) {
  const raw = localStorage.getItem(key);
  return raw ? JSON.parse(raw) : null;
}

function saveJson(key, value) {
  if (!value) localStorage.removeItem(key);
  else localStorage.setItem(key, JSON.stringify(value));
}

function decodeJwt(token) {
  try {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
  } catch {
    return {};
  }
}

function claim(payload, key) {
  return payload[key] ?? payload[`http://schemas.xmlsoap.org/ws/2005/05/identity/claims/${key}`];
}

function isTokenExpired(token) {
  if (!token) return true;
  const payload = decodeJwt(token);
  if (!payload.exp) return false;
  return payload.exp * 1000 <= Date.now();
}

function isUserSessionValid() {
  return !!(state.userSession?.token && state.userSession.userId && !isTokenExpired(state.userSession.token));
}

function isAdminSessionValid() {
  return !!(state.adminSession?.token && state.adminSession.userId && !isTokenExpired(state.adminSession.token));
}

function clearUserSession() {
  state.userSession = null;
  state.userData = { profiles: [], accounts: [], transactions: [] };
  saveJson(userSessionKey, null);
}

function clearAdminSession() {
  state.adminSession = null;
  state.adminData = null;
  saveJson(adminSessionKey, null);
}

function syncStoredSessions() {
  if (!isUserSessionValid()) clearUserSession();
  else if (state.userSession?.profileToken && isTokenExpired(state.userSession.profileToken)) {
    state.userSession.profileToken = null;
    state.userSession.activeProfileId = null;
    saveJson(userSessionKey, state.userSession);
  }

  if (!isAdminSessionValid()) clearAdminSession();
}

function activeUserEmail() {
  if (!isUserSessionValid()) return null;
  const payload = decodeJwt(state.userSession.token);
  return claim(payload, "emailaddress") || claim(payload, "email") || null;
}

function activeAdminEmail() {
  if (!isAdminSessionValid()) return null;
  const payload = decodeJwt(state.adminSession.token);
  return claim(payload, "emailaddress") || claim(payload, "email") || null;
}

function setStatus(target, kind, title, detail = "") {
  target.innerHTML = "<div class=\"status-box " + escapeHtml(kind) + "\"><strong>" + escapeHtml(title) + "</strong><span>" + escapeHtml(detail) + "</span></div>";
}

function clearStatus(target) {
  target.innerHTML = '';
}

function escapeHtml(value) {
  return String(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

function metric(label, value) {
  return `<dl class="metric"><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(String(value))}</dd></dl>`;
}

function formatDateTime(value) {
  if (!value) return 'N/A';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString();
}

async function request(path, method = 'GET', body = null, token = null) {
  const res = await fetch(`${api}${path}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: body ? JSON.stringify(body) : null
  });

  const text = await res.text();
  const data = text ? JSON.parse(text) : null;
  if (!res.ok) throw data ?? { title: 'Request failed', detail: `HTTP ${res.status}` };
  return data;
}

function routeToScreen(hash) {
  if (hash.startsWith('#/user/app')) return 'screen-user-app';
  if (hash.startsWith('#/user/register')) return 'screen-user-register';
  if (hash.startsWith('#/user/login')) return 'screen-user-login';
  if (hash.startsWith('#/admin/app')) return 'screen-admin-app';
  if (hash.startsWith('#/admin/login')) return 'screen-admin-login';
  if (hash.startsWith('#/test')) return 'screen-test';
  return 'screen-landing';
}

function navigate(hash) {
  if (location.hash === hash) renderRoute();
  else location.hash = hash;
}

function normalizeRoute(hash) {
  const current = hash || "#/landing";
  syncStoredSessions();

  if (current.startsWith("#/user/app") && !isUserSessionValid()) return "#/user/login";
  if ((current.startsWith("#/user/login") || current.startsWith("#/user/register")) && isUserSessionValid()) return "#/user/app";
  if (current.startsWith("#/admin/app") && !isAdminSessionValid()) return "#/admin/login";
  if (current.startsWith("#/admin/login") && isAdminSessionValid()) return "#/admin/app";

  return current;
}

function renderTopbar() {
  const userLoggedIn = isUserSessionValid();
  const adminLoggedIn = isAdminSessionValid();

  els.navUser.textContent = userLoggedIn ? "User Dashboard" : "User Login";
  els.navUser.href = userLoggedIn ? "#/user/app" : "#/user/login";
  els.navAdmin.textContent = adminLoggedIn ? "Admin Console" : "Admin Login";
  els.navAdmin.href = adminLoggedIn ? "#/admin/app" : "#/admin/login";

  if (adminLoggedIn) els.modeStatus.textContent = "Admin signed in: " + activeAdminEmail();
  else if (userLoggedIn) els.modeStatus.textContent = "User signed in: " + activeUserEmail();
  else els.modeStatus.textContent = "Not signed in";

  const actions = [];
  if (userLoggedIn) actions.push("<button type=\"button\" id=\"topbar-user-logout\" class=\"mini-ghost\">User logout</button>");
  if (adminLoggedIn) actions.push("<button type=\"button\" id=\"topbar-admin-logout\" class=\"mini-ghost\">Admin logout</button>");
  els.sessionActions.innerHTML = actions.join("");

  document.getElementById("topbar-user-logout")?.addEventListener("click", () => {
    clearUserSession();
    renderRoute();
    navigate("#/landing");
  });
  document.getElementById("topbar-admin-logout")?.addEventListener("click", () => {
    clearAdminSession();
    renderRoute();
    navigate("#/landing");
  });
}

function renderRoute() {
  const normalizedHash = normalizeRoute(location.hash || "#/landing");
  if (normalizedHash !== (location.hash || "#/landing")) {
    location.hash = normalizedHash;
    return;
  }

  const screenId = routeToScreen(normalizedHash);
  els.screens.forEach(screen => screen.classList.toggle("is-active", screen.id === screenId));
  renderTopbar();
  renderLandingCards();
  renderUserMenu();
  renderAdminMenu();
}

function renderLandingCards() {
  const userPayload = isUserSessionValid() ? decodeJwt(state.userSession.token) : null;
  const adminPayload = isAdminSessionValid() ? decodeJwt(state.adminSession.token) : null;
  els.landingSessionCards.innerHTML = [
    metric("User session", isUserSessionValid() ? "Signed in" : "Not signed in"),
    metric("User email", userPayload ? (claim(userPayload, "emailaddress") || claim(userPayload, "email") || "Unknown") : "None"),
    metric("Admin session", isAdminSessionValid() ? "Signed in" : "Not signed in"),
    metric("Admin email", adminPayload ? (claim(adminPayload, "emailaddress") || claim(adminPayload, "email") || "Unknown") : "None")
  ].join("");
}

function syncUserActionAvailability() {
  const hasUser = isUserSessionValid();
  const hasProfile = !!state.userSession?.profileToken && !!state.userSession?.activeProfileId;
  const hasAccounts = state.userData.accounts.length > 0;
  const accountForm = document.getElementById("user-account-form");
  const transactionForm = document.getElementById("user-transaction-form");

  [...accountForm.querySelectorAll("input, button")].forEach(el => { el.disabled = !hasUser || !hasProfile; });
  [...transactionForm.querySelectorAll("input, select, button")].forEach(el => { el.disabled = !hasUser || !hasProfile || !hasAccounts; });

  if (!hasProfile) {
    setStatus(els.userAccountStatus, "info", "Profile required", "Create or switch to a profile before creating accounts.");
    setStatus(els.userTransactionStatus, "info", "Profile required", "Create or switch to a profile before creating transactions.");
    return;
  }

  clearStatus(els.userAccountStatus);
  if (!hasAccounts) setStatus(els.userTransactionStatus, "info", "Account required", "Create an account before recording transactions.");
  else clearStatus(els.userTransactionStatus);
}

function renderUserMenu() {
  els.userMenuBtns.forEach(btn => btn.classList.toggle('is-active', btn.dataset.userView === state.userView));
  els.userPanels.forEach(panel => panel.classList.toggle('is-active', panel.dataset.userPanel === state.userView));
}

function renderAdminMenu() {
  els.adminMenuBtns.forEach(btn => btn.classList.toggle('is-active', btn.dataset.adminView === state.adminView));
  els.adminPanels.forEach(panel => panel.classList.toggle('is-active', panel.dataset.adminPanel === state.adminView));
}

function renderTable(target, columns, rows) {
  if (!rows.length) {
    target.innerHTML = '<div class="empty">No data yet.</div>';
    return;
  }
  const head = columns.map(column => `<th>${escapeHtml(column.label)}</th>`).join('');
  const body = rows.map(row => `<tr>${columns.map(column => `<td class="${column.code ? 'code' : ''}">${escapeHtml(column.render(row))}</td>`).join('')}</tr>`).join('');
  target.innerHTML = `<table><thead><tr>${head}</tr></thead><tbody>${body}</tbody></table>`;
}

async function hydrateUserDashboard() {
  if (!isUserSessionValid()) {
    navigate('#/user/login');
    return;
  }

  const payload = decodeJwt(state.userSession.token);
  const email = claim(payload, 'emailaddress') || claim(payload, 'email') || 'Unknown';
  const profiles = await request('/auth/profiles', 'GET', null, state.userSession.token);
  state.userData.profiles = profiles;

  let accounts = [];
  let transactions = [];
  if (state.userSession.profileToken && state.userSession.activeProfileId) {
    try {
      accounts = await request('/accounts', 'GET', null, state.userSession.profileToken);
      transactions = (await request('/transactions?limit=25&offset=0', 'GET', null, state.userSession.profileToken)).data ?? [];
    } catch (error) {
      state.userSession.profileToken = null;
      state.userSession.activeProfileId = null;
      saveJson(userSessionKey, state.userSession);
      setStatus(els.userOverviewStatus, 'warn', 'Profile session expired', 'Select a profile again to continue.');
    }
  }

  state.userData.accounts = accounts;
  state.userData.transactions = transactions;

  els.userSessionSummary.innerHTML = `
    <div><strong>Email</strong><div>${escapeHtml(email)}</div></div>
    <div><strong>User ID</strong><div class="code">${escapeHtml(state.userSession.userId)}</div></div>
    <div><strong>Active profile</strong><div>${escapeHtml(state.userSession.activeProfileId || 'None')}</div></div>`;

  els.userOverviewMetrics.innerHTML = [
    metric('Profiles', profiles.length),
    metric('Accounts', accounts.length),
    metric('Transactions', transactions.length),
    metric('Status', state.userSession.activeProfileId ? 'Profile active' : 'Needs profile')
  ].join('');

  if (!state.userSession.activeProfileId) {
    setStatus(els.userOverviewStatus, 'info', 'No active profile selected', profiles.length ? 'Choose an existing profile below or create a new one.' : 'Create your first profile to start using the ledger.');
  } else {
    setStatus(els.userOverviewStatus, 'success', 'Account state loaded', 'Your dashboard reflects saved backend data.');
  }

  renderTable(els.userProfilesTable, [
    { label: 'Profile', render: row => row.name },
    { label: 'Currency', render: row => row.currency },
    { label: 'Role', render: row => row.role },
    { label: 'Profile ID', render: row => row.profileId, code: true }
  ], profiles);

  els.userProfileSwitcher.innerHTML = profiles.length
    ? profiles.map(profile => `
        <button type="button" class="switch-card ${profile.profileId === state.userSession.activeProfileId ? 'selected' : ''}" data-switch-profile="${profile.profileId}">
          <strong>${escapeHtml(profile.name)}</strong>
          <span>${escapeHtml(profile.currency)} • ${escapeHtml(profile.role)}</span>
        </button>`).join('')
    : '<div class="empty">No profiles yet. Create one in the Profiles screen.</div>';

  document.querySelectorAll('[data-switch-profile]').forEach(btn => {
    btn.addEventListener('click', async () => {
      try {
        const data = await request(`/auth/switch-profile/${btn.dataset.switchProfile}`, 'POST', null, state.userSession.token);
        state.userSession.profileToken = data.accessToken;
        state.userSession.activeProfileId = data.activeProfileId;
        saveJson(userSessionKey, state.userSession);
        await hydrateUserDashboard();
      } catch (error) {
        setStatus(els.userProfileStatus, 'error', error.title || 'Profile switch failed', error.detail || 'Could not switch profile.');
      }
    });
  });

  renderTable(els.userAccountsTable, [
    { label: 'Name', render: row => row.name },
    { label: 'Type', render: row => row.type },
    { label: 'Balance', render: row => String(row.balanceOverride ?? 0) },
    { label: 'Currency', render: row => row.currency }
  ], accounts);

  renderTable(els.userTransactionsTable, [
    { label: 'Description', render: row => row.description },
    { label: 'Amount', render: row => `${row.amount} ${row.direction}` },
    { label: 'Category', render: row => row.category },
    { label: 'Date', render: row => row.date }
  ], transactions);

  renderTable(els.userRecentTransactions, [
    { label: 'Description', render: row => row.description },
    { label: 'Amount', render: row => `${row.amount} ${row.direction}` },
    { label: 'Date', render: row => row.date }
  ], transactions.slice(0, 8));

  const options = accounts.length
    ? accounts.map(account => `<option value="${escapeHtml(account.id)}">${escapeHtml(account.name)} (${escapeHtml(account.currency)})</option>`).join('')
    : '<option value="">Create an account first</option>';
  els.userTransactionAccount.innerHTML = options;
  syncUserActionAvailability();
}

async function hydrateAdminDashboard() {
  if (!isAdminSessionValid()) {
    navigate('#/admin/login');
    return;
  }

  const payload = decodeJwt(state.adminSession.token);
  const email = claim(payload, 'emailaddress') || claim(payload, 'email') || 'Unknown';
  const overview = await request('/admin/overview', 'GET', null, state.adminSession.token);
  state.adminData = overview;

  els.adminSessionSummary.innerHTML = `
    <div><strong>Email</strong><div>${escapeHtml(email)}</div></div>
    <div><strong>Admin user ID</strong><div class="code">${escapeHtml(state.adminSession.userId)}</div></div>
    <div><strong>Environment</strong><div>${escapeHtml(overview.settings.environment)}</div></div>`;

  els.adminOverviewMetrics.innerHTML = [
    metric('Users', overview.dashboard.totalUsers),
    metric('Profiles', overview.dashboard.totalProfiles),
    metric('Memberships', overview.dashboard.totalMemberships),
    metric('Transactions', overview.dashboard.totalTransactions)
  ].join('');
  setStatus(els.adminOverviewCallout, 'success', 'Admin data loaded', 'Metrics and operational records are coming from the live backend.');

  renderTable(els.adminUsersTable, [
    { label: 'Name', render: row => row.displayName },
    { label: 'Email', render: row => row.email },
    { label: 'Super Admin', render: row => row.isSuperAdmin ? 'Yes' : 'No' },
    { label: 'User ID', render: row => row.id, code: true }
  ], overview.users ?? []);

  renderTable(els.adminProfilesSummaryTable, [
    { label: 'Profile', render: row => row.name },
    { label: 'Currency', render: row => row.currency },
    { label: 'Members', render: row => String(row.memberCount) },
    { label: 'Profile ID', render: row => row.id, code: true }
  ], overview.profiles ?? []);

  renderTable(els.adminMembershipsTable, [
    { label: 'User', render: row => `${row.displayName} (${row.email})` },
    { label: 'Profile', render: row => row.profileName },
    { label: 'Role', render: row => row.role },
    { label: 'Joined', render: row => formatDateTime(row.joinedAt) }
  ], overview.memberships ?? []);

  renderTable(els.adminTransactionsTable, [
    { label: 'Description', render: row => row.description },
    { label: 'Profile', render: row => row.profileName },
    { label: 'Account', render: row => row.accountName },
    { label: 'Amount', render: row => `${row.amount} ${row.direction}` },
    { label: 'Created', render: row => formatDateTime(row.createdAt) }
  ], overview.transactions ?? []);

  els.adminSettingsList.innerHTML = Object.entries(overview.settings).map(([key, value]) => `
    <div class="settings-row">
      <strong>${escapeHtml(key)}</strong>
      <span>${escapeHtml(Array.isArray(value) ? value.join(', ') : String(value))}</span>
    </div>`).join('');
}

function updateTestState() {
  els.testStateCards.innerHTML = [
    metric('User token', state.test.token ? 'Ready' : 'Missing'),
    metric('Profile', state.test.profileId || 'None'),
    metric('Profile token', state.test.profileToken ? 'Ready' : 'Missing'),
    metric('Account', state.test.accountId || 'None')
  ].join('');
}

function appendTestLog(entry) {
  const stamp = new Date().toLocaleTimeString();
  els.testLog.textContent = `[${stamp}] ${JSON.stringify(entry, null, 2)}\n\n${els.testLog.textContent}`;
}

function bindEvents() {
  els.userMenuBtns.forEach(btn => btn.addEventListener('click', () => {
    state.userView = btn.dataset.userView;
    renderUserMenu();
  }));

  els.adminMenuBtns.forEach(btn => btn.addEventListener('click', () => {
    state.adminView = btn.dataset.adminView;
    renderAdminMenu();
  }));

  document.getElementById('user-register-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      const data = await request('/auth/register-user', 'POST', payload);
      state.userSession = { token: data.accessToken, userId: data.userId, activeProfileId: null, profileToken: null };
      saveJson(userSessionKey, state.userSession);
      setStatus(els.userRegisterStatus, 'success', 'Account created', 'Your user record is saved. Redirecting to dashboard.');
      await hydrateUserDashboard();
      navigate('#/user/app');
    } catch (error) {
      setStatus(els.userRegisterStatus, 'error', error.title || 'Registration failed', error.detail || 'Could not create account.');
    }
  });

  document.getElementById('user-login-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      const data = await request('/auth/login', 'POST', payload);
      state.userSession = { token: data.accessToken, userId: data.userId, activeProfileId: null, profileToken: null };
      saveJson(userSessionKey, state.userSession);
      setStatus(els.userLoginStatus, 'success', 'Login successful', 'Loading your saved account state.');
      await hydrateUserDashboard();
      navigate('#/user/app');
    } catch (error) {
      setStatus(els.userLoginStatus, 'error', error.title || 'Login failed', error.detail || 'Could not login.');
    }
  });

  document.getElementById('user-profile-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      const create = await request('/auth/profiles', 'POST', payload, state.userSession.token);
      const switched = await request(`/auth/switch-profile/${create.profileId}`, 'POST', null, state.userSession.token);
      state.userSession.profileToken = switched.accessToken;
      state.userSession.activeProfileId = switched.activeProfileId;
      saveJson(userSessionKey, state.userSession);
      setStatus(els.userProfileStatus, 'success', 'Profile created', 'The new profile is active and ready for accounts.');
      await hydrateUserDashboard();
      state.userView = 'overview';
      renderUserMenu();
    } catch (error) {
      setStatus(els.userProfileStatus, 'error', error.title || 'Profile creation failed', error.detail || 'Could not create profile.');
    }
  });

  document.getElementById('user-account-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      payload.balanceOverride = Number(payload.balanceOverride);
      await request('/accounts', 'POST', payload, state.userSession.profileToken);
      setStatus(els.userAccountStatus, 'success', 'Account created', 'The new account is saved to the database.');
      await hydrateUserDashboard();
    } catch (error) {
      setStatus(els.userAccountStatus, 'error', error.title || 'Account creation failed', error.detail || 'Could not create account.');
    }
  });

  document.getElementById('user-transaction-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      payload.amount = Number(payload.amount);
      await request('/transactions', 'POST', payload, state.userSession.profileToken);
      setStatus(els.userTransactionStatus, 'success', 'Transaction created', 'The ledger activity is saved and visible below.');
      await hydrateUserDashboard();
    } catch (error) {
      setStatus(els.userTransactionStatus, 'error', error.title || 'Transaction failed', error.detail || 'Could not create transaction.');
    }
  });

  document.getElementById('user-refresh-btn').addEventListener('click', () => hydrateUserDashboard());
  document.getElementById('user-logout-btn').addEventListener('click', () => {
    clearUserSession();
    navigate('#/landing');
    renderRoute();
  });

  document.getElementById('admin-login-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      const data = await request('/auth/login', 'POST', payload);
      if (!data.isSuperAdmin) throw { title: 'Forbidden', detail: 'This user is not a super admin.' };
      state.adminSession = { token: data.accessToken, userId: data.userId };
      saveJson(adminSessionKey, state.adminSession);
      setStatus(els.adminLoginStatus, 'success', 'Admin login successful', 'Loading operations console.');
      await hydrateAdminDashboard();
      navigate('#/admin/app');
    } catch (error) {
      setStatus(els.adminLoginStatus, 'error', error.title || 'Admin login failed', error.detail || 'Could not login as admin.');
    }
  });

  document.getElementById('admin-refresh-btn').addEventListener('click', () => hydrateAdminDashboard());
  document.getElementById('admin-logout-btn').addEventListener('click', () => {
    clearAdminSession();
    navigate('#/landing');
    renderRoute();
  });

  document.getElementById('admin-create-profile-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      await request('/admin/profiles', 'POST', payload, state.adminSession.token);
      setStatus(els.adminProfileStatus, 'success', 'Profile created', 'The profile was provisioned by admin and saved.');
      await hydrateAdminDashboard();
    } catch (error) {
      setStatus(els.adminProfileStatus, 'error', error.title || 'Profile creation failed', error.detail || 'Could not create profile.');
    }
  });

  document.getElementById('admin-assign-member-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      await request(`/admin/profiles/${payload.profileId}/members/${payload.userId}`, 'PUT', { role: payload.role }, state.adminSession.token);
      setStatus(els.adminProfileStatus, 'success', 'Member assigned', 'Membership change saved successfully.');
      await hydrateAdminDashboard();
    } catch (error) {
      setStatus(els.adminProfileStatus, 'error', error.title || 'Assign failed', error.detail || 'Could not assign member.');
    }
  });

  document.getElementById('admin-revoke-member-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      await request(`/admin/profiles/${payload.profileId}/members/${payload.userId}`, 'DELETE', null, state.adminSession.token);
      setStatus(els.adminProfileStatus, 'success', 'Member revoked', 'Membership removal saved successfully.');
      await hydrateAdminDashboard();
    } catch (error) {
      setStatus(els.adminProfileStatus, 'error', error.title || 'Revoke failed', error.detail || 'Could not revoke member.');
    }
  });

  document.getElementById('test-register-form').addEventListener('submit', async event => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const payload = Object.fromEntries(form.entries());
    payload.isSuperAdmin = !!form.get('isSuperAdmin');
    try {
      const data = await request('/auth/register-user', 'POST', payload);
      state.test.token = data.accessToken;
      appendTestLog({ register: data });
      updateTestState();
    } catch (error) {
      appendTestLog({ error: error.detail || error.title || 'Register failed' });
    }
  });

  document.getElementById('test-login-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      const data = await request('/auth/login', 'POST', payload);
      state.test.token = data.accessToken;
      appendTestLog({ login: data });
      updateTestState();
    } catch (error) {
      appendTestLog({ error: error.detail || error.title || 'Login failed' });
    }
  });

  document.getElementById('test-profile-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      const create = await request('/auth/profiles', 'POST', payload, state.test.token);
      const switched = await request(`/auth/switch-profile/${create.profileId}`, 'POST', null, state.test.token);
      state.test.profileId = create.profileId;
      state.test.profileToken = switched.accessToken;
      appendTestLog({ createProfile: create, switchProfile: switched });
      updateTestState();
    } catch (error) {
      appendTestLog({ error: error.detail || error.title || 'Profile failed' });
    }
  });

  document.getElementById('test-account-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      payload.balanceOverride = Number(payload.balanceOverride);
      const data = await request('/accounts', 'POST', payload, state.test.profileToken);
      state.test.accountId = data.id;
      appendTestLog({ account: data });
      updateTestState();
    } catch (error) {
      appendTestLog({ error: error.detail || error.title || 'Account failed' });
    }
  });

  document.getElementById('test-transaction-form').addEventListener('submit', async event => {
    event.preventDefault();
    try {
      const payload = Object.fromEntries(new FormData(event.currentTarget).entries());
      payload.amount = Number(payload.amount);
      payload.accountId = state.test.accountId;
      const data = await request('/transactions', 'POST', payload, state.test.profileToken);
      appendTestLog({ transaction: data });
      updateTestState();
    } catch (error) {
      appendTestLog({ error: error.detail || error.title || 'Transaction failed' });
    }
  });

  window.addEventListener('hashchange', async () => {
    renderRoute();
    if (location.hash.startsWith('#/user/app') && isUserSessionValid()) await hydrateUserDashboard();
    if (location.hash.startsWith('#/admin/app') && isAdminSessionValid()) await hydrateAdminDashboard();
  });
}

async function clearStaleClientCaches() {
  if (location.hostname !== "localhost" && location.hostname !== "127.0.0.1") return;

  if ("serviceWorker" in navigator) {
    const registrations = await navigator.serviceWorker.getRegistrations();
    await Promise.all(registrations.map(registration => registration.unregister()));
  }

  if ("caches" in window) {
    const keys = await caches.keys();
    await Promise.all(keys.filter(key => key.startsWith("familyledger-")).map(key => caches.delete(key)));
  }
}

async function boot() {
  await clearStaleClientCaches();
  syncStoredSessions();
  bindEvents();
  renderRoute();
  updateTestState();
  if (!location.hash) navigate('#/landing');
  if (isUserSessionValid() && location.hash.startsWith('#/user/app')) await hydrateUserDashboard();
  if (isAdminSessionValid() && location.hash.startsWith('#/admin/app')) await hydrateAdminDashboard();
}

boot();
