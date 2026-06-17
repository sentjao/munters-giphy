import { useEffect, useRef, useState } from 'react';
import { fetchTrending, type GifPageResponse } from '../api/gifsApi';
import { ErrorState } from './ErrorState';
import { GifGrid } from './GifGrid';
import { LoadingState } from './LoadingState';
import { Pagination } from './Pagination';

export function TrendingView() {
  const [offset, setOffset] = useState(0);
  const [data, setData] = useState<GifPageResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  useEffect(() => {
    abortRef.current?.abort();
    const ctrl = new AbortController();
    abortRef.current = ctrl;

    setLoading(true);
    setError(null);

    fetchTrending(offset, ctrl.signal)
      .then(setData)
      .catch((err: unknown) => {
        if (err instanceof Error && err.name !== 'AbortError')
          setError('Failed to load trending GIFs.');
      })
      .finally(() => setLoading(false));

    return () => ctrl.abort();
  }, [offset]);

  const pageSize = data?.pageSize ?? 25;

  return (
    <section>
      <h2>Trending</h2>
      {loading && <LoadingState />}
      {error && <ErrorState message={error} />}
      {!loading && !error && data && (
        <>
          <GifGrid urls={data.urls} />
          <Pagination
            offset={offset}
            pageSize={pageSize}
            totalCount={data.totalCount}
            onPrev={() => setOffset(o => Math.max(0, o - pageSize))}
            onNext={() => setOffset(o => o + pageSize)}
          />
        </>
      )}
    </section>
  );
}
