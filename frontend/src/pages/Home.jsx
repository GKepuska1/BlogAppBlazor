import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { fetchBlogs, searchBlogs } from '../api'

function Home() {
  const [blogs, setBlogs] = useState([])
  const [page, setPage] = useState(1)
  const [pageSize] = useState(10)
  const [loading, setLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [isSearching, setIsSearching] = useState(false)

  useEffect(() => {
    loadBlogs()
  }, [page])

  const loadBlogs = async () => {
    setLoading(true)
    try {
      const data = await fetchBlogs(page, pageSize)
      setBlogs(data)
      setIsSearching(false)
      setSearchTerm('')
    } catch (error) {
      console.error('Failed to load blogs:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleSearch = async (e) => {
    e.preventDefault()
    if (!searchTerm.trim()) {
      loadBlogs()
      return
    }

    setLoading(true)
    setIsSearching(true)
    try {
      const data = await searchBlogs(searchTerm)
      setBlogs(data)
    } catch (error) {
      console.error('Search failed:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleClearSearch = () => {
    setSearchTerm('')
    setIsSearching(false)
    loadBlogs()
  }

  return (
    <div className="page-container">
      <div className="search-container">
        <form onSubmit={handleSearch} className="search-form">
          <input
            type="text"
            placeholder="Search blogs by title..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
          <button type="submit" className="button">
            Search
          </button>
          {isSearching && (
            <button
              type="button"
              onClick={handleClearSearch}
              className="button button-secondary"
            >
              Clear
            </button>
          )}
        </form>
      </div>

      {loading ? (
        <div className="loading">Loading...</div>
      ) : (
        <>
          <div className="blog-grid">
            {blogs.length === 0 ? (
              <div className="empty-state">
                <p>No blogs found. {isSearching ? 'Try a different search term.' : 'Be the first to create one!'}</p>
              </div>
            ) : (
              blogs.map((blog) => (
                <div key={blog.id} className="blog-card">
                  <Link to={`/blog/${blog.id}`} className="blog-link">
                    <h2 className="blog-title">{blog.name}</h2>
                    <div className="blog-meta">
                      <span className="blog-author">by {blog.user}</span>
                      <span className="blog-date">
                        {new Date(blog.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                    {blog.tags && blog.tags.length > 0 && (
                      <div className="tags">
                        {blog.tags.map((tag, idx) => (
                          <span key={idx} className="tag">
                            {tag.name}
                          </span>
                        ))}
                      </div>
                    )}
                  </Link>
                </div>
              ))
            )}
          </div>

          {!isSearching && blogs.length > 0 && (
            <div className="pagination">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="button"
              >
                Previous
              </button>
              <span className="page-number">Page {page}</span>
              <button
                onClick={() => setPage((p) => p + 1)}
                disabled={blogs.length < pageSize}
                className="button"
              >
                Next
              </button>
            </div>
          )}
        </>
      )}
    </div>
  )
}

export default Home
