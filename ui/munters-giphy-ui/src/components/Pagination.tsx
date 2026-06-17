interface PaginationProps {
  offset: number;
  pageSize: number;
  totalCount: number | null;
  onPrev: () => void;
  onNext: () => void;
}

export function Pagination({ offset, pageSize, totalCount, onPrev, onNext }: PaginationProps) {
  const hasPrev = offset > 0;
  const hasNext = totalCount === null || offset + pageSize < totalCount;

  return (
    <div className="pagination">
      <button onClick={onPrev} disabled={!hasPrev}>← Previous</button>
      <span>{totalCount !== null ? `${offset + 1}–${Math.min(offset + pageSize, totalCount)} of ${totalCount}` : `Offset ${offset}`}</span>
      <button onClick={onNext} disabled={!hasNext}>Next →</button>
    </div>
  );
}
