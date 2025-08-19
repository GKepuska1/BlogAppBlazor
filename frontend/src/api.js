const API_BASE = '/api';

let authToken = localStorage.getItem('token') || '';

export function setAuthToken(token) {
  authToken = token;
  if (token) {
    localStorage.setItem('token', token);
  } else {
    localStorage.removeItem('token');
  }
}

function authHeader() {
  return authToken ? { 'Authorization': `Bearer ${authToken}` } : {};
}

export async function login(credentials) {
  const res = await fetch(`${API_BASE}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(credentials)
  });
  if (!res.ok) throw new Error('Failed to login');
  return res.text();
}

export async function fetchBlogs(page = 1, pageSize = 10) {
  const res = await fetch(`${API_BASE}/blog/${page}/${pageSize}`, {
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to fetch');
  return res.json();
}

export async function createBlog(data) {
  const res = await fetch(`${API_BASE}/blog`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeader() },
    body: JSON.stringify(data)
  });
  if (!res.ok) throw new Error('Failed to create');
  return res.json();
}
