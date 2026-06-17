import { useState } from 'react';
import { SearchView } from './components/SearchView';
import { TrendingView } from './components/TrendingView';
import './App.css';

type Tab = 'trending' | 'search';

export function App() {
  const [tab, setTab] = useState<Tab>('trending');

  return (
    <div className="app">
      <header className="app-header">
        <h1>Munters GIPHY</h1>
        <nav className="app-nav">
          <button
            className={tab === 'trending' ? 'active' : ''}
            onClick={() => setTab('trending')}
          >
            Trending
          </button>
          <button
            className={tab === 'search' ? 'active' : ''}
            onClick={() => setTab('search')}
          >
            Search
          </button>
        </nav>
      </header>
      <main className="app-main">
        {tab === 'trending' ? <TrendingView /> : <SearchView />}
      </main>
    </div>
  );
}
