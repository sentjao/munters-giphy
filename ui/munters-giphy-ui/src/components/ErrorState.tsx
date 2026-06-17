interface ErrorStateProps {
  message: string;
}

export function ErrorState({ message }: ErrorStateProps) {
  return <p className="state-message state-message--error">{message}</p>;
}
