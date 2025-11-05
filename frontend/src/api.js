const API_BASE = '/api';

let authToken = localStorage.getItem('token') || '';
let currentUsername = localStorage.getItem('username') || '';

export function setAuthToken(token, username = '') {
  authToken = token;
  currentUsername = username;
  if (token) {
    localStorage.setItem('token', token);
    if (username) {
      localStorage.setItem('username', username);
    }
  } else {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
  }
}

export function getCurrentUsername() {
  return currentUsername || localStorage.getItem('username') || '';
}

export function logout() {
  setAuthToken('', '');
}

function authHeader() {
  return authToken ? { 'Authorization': `Bearer ${authToken}` } : {};
}

// Auth endpoints
export async function guestLogin() {
  const res = await fetch(`${API_BASE}/auth/guest`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' }
  });
  if (!res.ok) throw new Error('Failed to login as guest');
  const data = await res.json();
  setAuthToken(data.token, data.username);
  return data;
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

// Blog endpoints
export async function fetchBlogs(page = 1, pageSize = 10) {
  const res = await fetch(`${API_BASE}/blog/${page}/${pageSize}`, {
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to fetch blogs');
  return res.json();
}

export async function getBlogById(id) {
  const res = await fetch(`${API_BASE}/blog/${id}`, {
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to fetch blog');
  return res.json();
}

export async function createBlog(data) {
  const res = await fetch(`${API_BASE}/blog`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeader() },
    body: JSON.stringify(data)
  });
  if (!res.ok) throw new Error('Failed to create blog');
  return res.json();
}

export async function updateBlog(id, data) {
  const res = await fetch(`${API_BASE}/blog/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeader() },
    body: JSON.stringify(data)
  });
  if (!res.ok) throw new Error('Failed to update blog');
  return res.json();
}

export async function deleteBlog(id) {
  const res = await fetch(`${API_BASE}/blog/${id}`, {
    method: 'DELETE',
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to delete blog');
  return res.ok;
}

export async function searchBlogs(term) {
  const res = await fetch(`${API_BASE}/blog/Search/${encodeURIComponent(term)}`, {
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to search blogs');
  return res.json();
}

export async function getTotalBlogs() {
  const res = await fetch(`${API_BASE}/blog/total`, {
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to get total');
  return res.json();
}

// Comment endpoints
export async function getComments(blogId) {
  const res = await fetch(`${API_BASE}/comment/${blogId}`, {
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to fetch comments');
  return res.json();
}

export async function createComment(blogId, content) {
  const res = await fetch(`${API_BASE}/comment/${blogId}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeader() },
    body: JSON.stringify({ content })
  });
  if (!res.ok) throw new Error('Failed to create comment');
  return res.json();
}

export async function updateComment(blogId, commentId, content) {
  const res = await fetch(`${API_BASE}/comment/${blogId}/${commentId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeader() },
    body: JSON.stringify({ content })
  });
  if (!res.ok) throw new Error('Failed to update comment');
  return res.json();
}

export async function deleteComment(blogId, commentId) {
  const res = await fetch(`${API_BASE}/comment/${blogId}/${commentId}`, {
    method: 'DELETE',
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to delete comment');
  return res.ok;
}

// Subscription endpoints
export async function checkSubscriptionLimit() {
  const res = await fetch(`${API_BASE}/subscription/check`, {
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to check subscription limit');
  return res.json();
}

export async function getBitcoinPaymentInfo() {
  const res = await fetch(`${API_BASE}/subscription/bitcoin/info`, {
    headers: authHeader()
  });
  if (!res.ok) throw new Error('Failed to get Bitcoin payment info');
  return res.json();
}

export async function verifyBitcoinPayment(transactionId) {
  const res = await fetch(`${API_BASE}/subscription/bitcoin/verify`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeader() },
    body: JSON.stringify({ transactionId })
  });

  const data = await res.json();

  if (!res.ok) {
    throw new Error(data.message || 'Failed to verify payment');
  }

  return data;
}
