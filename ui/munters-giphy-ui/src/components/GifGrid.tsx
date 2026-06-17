interface GifGridProps {
  urls: string[];
}

export function GifGrid({ urls }: GifGridProps) {
  if (urls.length === 0) {
    return <p className="state-message">No GIFs found.</p>;
  }

  return (
    <div className="gif-grid">
      {urls.map((url, i) => (
        <img key={url + i} src={url} alt={`GIF ${i + 1}`} loading="lazy" />
      ))}
    </div>
  );
}
