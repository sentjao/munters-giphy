import { useRef, useState } from 'react';
import { fetchSearch, type GifPageResponse } from '../api/gifsApi';
import { ErrorState } from './ErrorState';
import { GifGrid } from './GifGrid';
import { LoadingState } from './LoadingState';
import { Pagination } from './Pagination';

export function SearchView() {
  const [term, setTerm] = useState('');
  const [submittedTerm, setSubmittedTerm] = useState('');
  const [offset, setOffset] = useState(0);
  const [data, setData] = useState<GifPageResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const search = (searchTerm: string, searchOffset: number) => {
    abortRef.current?.abort();
    const ctrl = new AbortController();
    abortRef.current = ctrl;

    setLoading(true);
    setError(null);
    setData(null);

    fetchSearch(searchTerm, searchOffset, ctrl.signal)
      .then(setData)
      .catch((err: unknown) => {
        if (err instanceof Error && err.name !== 'AbortError')
          setError('Failed to search GIFs.');
      })
      .finally(() => setLoading(false));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!term.trim()) return;
    const newOffset = 0;
    setOffset(newOffset);
    setSubmittedTerm(term);
    search(term, newOffset);
  };

  const pageSize = data?.pageSize ?? 25;

  return (
    <section>
      <h2>Search</h2>
      <form onSubmit={handleSubmit} className="search-form">
        <input
          value={term}
          onChange={e => setTerm(e.target.value)}
          placeholder="Search GIFs…"
          aria-label="Search term"
        />
        <button type="submit">Search</button>
      </form>
      {loading && <LoadingState />}
      {error && <ErrorState message={error} />}
      {!loading && !error && data && (
        <>
          <GifGrid urls={data.urls} />
          <Pagination
            offset={offset}
            pageSize={pageSize}
            totalCount={data.totalCount}
            onPrev={() => {
              const newOffset = Math.max(0, offset - pageSize);
              setOffset(newOffset);
              search(submittedTerm, newOffset);
            }}
            onNext={() => {
              const newOffset = offset + pageSize;
              setOffset(newOffset);
              search(submittedTerm, newOffset);
            }}
          />
        </>
      )}
    </section>
  );
}
