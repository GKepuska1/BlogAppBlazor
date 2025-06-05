const API_BASE = '/api';

export async function fetchBlogs() {
  const res = await fetch(`${API_BASE}/blog`);
  if (!res.ok) throw new Error('Failed to fetch');
  return res.json();
}

export async function createBlog(data) {
  const res = await fetch(`${API_BASE}/blog`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });
  if (!res.ok) throw new Error('Failed to create');
  return res.json();
}
