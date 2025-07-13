import { Activity, RefreshCw } from 'lucide-react';
import React, { useEffect, useState } from 'react';
import { colors } from '../config/consts';

const MINER_URL =
    'https://miner-ef-blockchain.caprover.edersonfernandes.tec.br/miner-logs.txt';

const MinerLogsPage: React.FC = () => {
    const [logs, setLogs] = useState<string>('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string>('');
    const [autoRefresh, setAutoRefresh] = useState(false);

    const fetchLogs = async () => {
        setLoading(true);
        setError('');

        try {
            const response = await fetch(MINER_URL, {
                method: 'GET',
                headers: {
                    'Content-Type': 'text/plain',
                },
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const text = await response.text();
            setLogs(text);
        } catch (err) {
            setError(
                err instanceof Error ? err.message : 'Failed to fetch logs',
            );
            console.error('Error fetching logs:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchLogs();
    }, []);

    useEffect(() => {
        if (autoRefresh) {
            const interval = setInterval(fetchLogs, 5000); // Refresh every 5 seconds
            return () => clearInterval(interval);
        }
    }, [autoRefresh]);

    return (
        <div className="container mx-auto px-4 py-8 max-w-7xl">
            <div className="mb-6">
                <h1
                    className="text-3xl font-bold mb-2"
                    style={{ color: colors.secondary }}
                >
                    Miner Logs
                </h1>
                <p className="text-lg" style={{ color: colors.medium }}>
                    Real-time logs from the blockchain miner
                </p>
            </div>

            <div className="mb-4 flex flex-wrap gap-4 items-center">
                <button
                    onClick={fetchLogs}
                    disabled={loading}
                    className="flex items-center space-x-2 px-4 py-2 rounded-md
                             text-white font-medium transition-colors
                             disabled:opacity-50"
                    style={{
                        backgroundColor: loading
                            ? colors.medium
                            : colors.accent,
                    }}
                    onMouseEnter={e => {
                        if (!loading) {
                            e.currentTarget.style.backgroundColor =
                                colors.primary;
                        }
                    }}
                    onMouseLeave={e => {
                        if (!loading) {
                            e.currentTarget.style.backgroundColor =
                                colors.accent;
                        }
                    }}
                >
                    <RefreshCw
                        className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`}
                    />
                    <span>{loading ? 'Refreshing...' : 'Refresh'}</span>
                </button>

                <label className="flex items-center space-x-2 text-sm">
                    <input
                        type="checkbox"
                        checked={autoRefresh}
                        onChange={e => setAutoRefresh(e.target.checked)}
                        className="rounded"
                        style={{ accentColor: colors.accent }}
                    />
                    <span style={{ color: colors.medium }}>
                        Auto-refresh (5s)
                    </span>
                </label>

                <div className="flex items-center space-x-2">
                    <Activity
                        className="w-4 h-4"
                        style={{
                            color: autoRefresh ? colors.primary : colors.medium,
                        }}
                    />
                    <span
                        className="text-sm"
                        style={{
                            color: autoRefresh ? colors.primary : colors.medium,
                        }}
                    >
                        {autoRefresh ? 'Live' : 'Manual'}
                    </span>
                </div>
            </div>

            {error && (
                <div
                    className="mb-4 p-4 rounded-md border-l-4"
                    style={{
                        backgroundColor: '#fee2e2',
                        borderLeftColor: '#dc2626',
                        color: '#991b1b',
                    }}
                >
                    <p className="font-medium">Error loading logs:</p>
                    <p>{error}</p>
                </div>
            )}

            <div
                className="rounded-lg border p-4"
                style={{
                    backgroundColor: colors.secondary,
                    borderColor: colors.medium,
                }}
            >
                <pre
                    className="whitespace-pre-wrap text-sm font-mono overflow-auto max-h-96"
                    style={{ color: '#f1f5f9' }}
                >
                    {logs || 'No logs available...'}
                </pre>
            </div>

            <div className="mt-4 text-sm" style={{ color: colors.medium }}>
                <p>
                    Logs are fetched from:{' '}
                    <code className="font-mono bg-gray-100 px-1 rounded">
                        {MINER_URL}
                    </code>
                </p>
                <p>Last updated: {new Date().toLocaleString()}</p>
            </div>
        </div>
    );
};

export default MinerLogsPage;
