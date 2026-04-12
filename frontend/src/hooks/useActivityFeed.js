import { useState, useEffect, useCallback } from 'react'
import { auditlogsApi } from '../api/api'

export default function useActivityFeed(limit = 10) {
  const [entries, setEntries] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const fetch = useCallback(() => {
    setLoading(true)
    setError(null)
    auditlogsApi
      .getRecent(limit)
      .then(({ data }) => {
        const raw = Array.isArray(data) ? data : (data?.data ?? [])
        setEntries(raw)
      })
      .catch(() => setError('Ngarkimi dështoi.'))
      .finally(() => setLoading(false))
  }, [limit])

  useEffect(() => { fetch() }, [fetch])

  return { entries, loading, error, refresh: fetch }
}
