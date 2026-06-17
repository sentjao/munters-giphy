export interface GifPageResponse {
  urls: string[];
  offset: number;
  pageSize: number;
  count: number;
  totalCount: number | null;
}

const BASE_URL = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? '';

async function get<T>(url: string, signal: AbortSignal): Promise<T> {
  const response = await fetch(url, { signal });
  if (!response.ok) throw new Error(`HTTP ${response.status}`);
  return response.json() as Promise<T>;
}

export function fetchTrending(offset: number, signal: AbortSignal): Promise<GifPageResponse> {
  return get<GifPageResponse>(`${BASE_URL}/api/gifs/trending?offset=${offset}`, signal);
}

export function fetchSearch(term: string, offset: number, signal: AbortSignal): Promise<GifPageResponse> {
  return get<GifPageResponse>(
    `${BASE_URL}/api/gifs/search?term=${encodeURIComponent(term)}&offset=${offset}`,
    signal,
  );
}
