const api = '/api/v1';
const logEl = document.getElementById('log');

const state = {
  token: null,
  profileToken: null,
  profileId: null,
  accountId: null
};

function log(data) {
  logEl.textContent += `${JSON.stringify(data, null, 2)}\n\n`;
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
  const data = await res.json();
  if (!res.ok) throw data;
  return data;
}

document.getElementById('register-form').addEventListener('submit', async e => {
  e.preventDefault();
  const f = new FormData(e.currentTarget);
  const payload = Object.fromEntries(f.entries());
  payload.isSuperAdmin = !!f.get('isSuperAdmin');
  const data = await request('/auth/register-user', 'POST', payload);
  state.token = data.accessToken;
  log({ register: data });
});

document.getElementById('login-form').addEventListener('submit', async e => {
  e.preventDefault();
  const payload = Object.fromEntries(new FormData(e.currentTarget).entries());
  const data = await request('/auth/login', 'POST', payload);
  state.token = data.accessToken;
  log({ login: data });
});

document.getElementById('profile-form').addEventListener('submit', async e => {
  e.preventDefault();
  const payload = Object.fromEntries(new FormData(e.currentTarget).entries());
  const create = await request('/auth/profiles', 'POST', payload, state.token);
  state.profileId = create.profileId;
  const switchProfile = await request(`/auth/switch-profile/${state.profileId}`, 'POST', null, state.token);
  state.profileToken = switchProfile.accessToken;
  log({ createProfile: create, switchProfile });
});

document.getElementById('account-form').addEventListener('submit', async e => {
  e.preventDefault();
  const payload = Object.fromEntries(new FormData(e.currentTarget).entries());
  payload.balanceOverride = Number(payload.balanceOverride);
  const data = await request('/accounts', 'POST', payload, state.profileToken);
  state.accountId = data.id;
  log({ account: data });
});

document.getElementById('transaction-form').addEventListener('submit', async e => {
  e.preventDefault();
  const payload = Object.fromEntries(new FormData(e.currentTarget).entries());
  payload.amount = Number(payload.amount);
  payload.accountId = state.accountId;
  const data = await request('/transactions', 'POST', payload, state.profileToken);
  log({ transaction: data });
});

if ('serviceWorker' in navigator) {
  navigator.serviceWorker.register('/sw.js').catch(() => null);
}
