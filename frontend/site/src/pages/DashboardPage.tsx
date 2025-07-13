import {
    AlertCircle,
    Blocks,
    Code,
    ExternalLink,
    RefreshCw,
    Server,
    Wallet,
} from 'lucide-react';
import React, { useEffect, useState } from 'react';
import BlockchainIcon from '../components/BlockchainIcon';
import { colors } from '../config/consts';
import type {
    ApiError,
    BlockchainStatus,
    BlockInfo,
} from '../types/blockchain';

const API_BASE = import.meta.env.VITE_API_URL;

interface ApiResponse<T> {
    data: T | null;
    loading: boolean;
    error: ApiError | null;
}

const DashboardPage: React.FC = () => {
    const [status, setStatus] = useState<ApiResponse<BlockchainStatus>>({
        data: null,
        loading: false,
        error: null,
    });

    const [nextBlock, setNextBlock] = useState<ApiResponse<BlockInfo>>({
        data: null,
        loading: false,
        error: null,
    });

    const fetchData = async <T,>(
        endpoint: string,
        setter: React.Dispatch<React.SetStateAction<ApiResponse<T>>>,
    ): Promise<void> => {
        setter(prev => ({ ...prev, loading: true, error: null }));

        try {
            const response = await fetch(`${API_BASE}${endpoint}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    Accept: 'application/json',
                },
            });

            if (!response.ok) {
                throw new Error(
                    `HTTP ${response.status}: ${response.statusText}`,
                );
            }

            const text = await response.text();

            if (!text || text.trim() === '') {
                console.warn(`Empty response from ${endpoint}`);
                setter({ data: null, loading: false, error: null });
                return;
            }

            try {
                const data: T = JSON.parse(text);
                console.log(`Response from ${endpoint}:`, data);
                setter({ data, loading: false, error: null });
            } catch (parseError) {
                console.error(`JSON parse error for ${endpoint}:`, text);
                throw new Error(`Invalid JSON response: ${parseError}`);
            }
        } catch (err) {
            const error: ApiError = {
                message:
                    err instanceof Error
                        ? err.message
                        : 'Unknown error occurred',
                status: 0,
            };
            setter({ data: null, loading: false, error });
        }
    };

    useEffect(() => {
        fetchData<BlockchainStatus>('/status', setStatus);
    }, []);

    const handleRefresh = (): void => {
        fetchData<BlockchainStatus>('/status', setStatus);
        if (nextBlock.data) {
            fetchData<BlockInfo>('/blocks/next', setNextBlock);
        }
    };

    const ErrorDisplay: React.FC<{ error: ApiError }> = ({ error }) => (
        <div
            className="border rounded-lg p-4 mb-6"
            style={{
                backgroundColor: '#fef2f2',
                borderColor: '#fecaca',
            }}
        >
            <div className="flex items-center space-x-2">
                <AlertCircle className="w-5 h-5 text-red-500" />
                <span className="text-red-700 font-medium">API Error</span>
            </div>
            <p className="text-red-600 mt-1">{error.message}</p>
        </div>
    );

    return (
        <div
            className="min-h-screen p-6"
            style={{
                background: `linear-gradient(135deg, ${colors.light}, #ffffff)`,
            }}
        >
            <div className="max-w-6xl mx-auto">
                <div className="bg-white rounded-xl shadow-lg p-8 mb-8">
                    <div className="flex items-center justify-between mb-4">
                        <div className="flex items-center space-x-4">
                            <div
                                className="p-3 rounded-lg"
                                style={{ backgroundColor: colors.light }}
                            >
                                <BlockchainIcon
                                    className="w-10 h-10"
                                    style={{ color: colors.primary }}
                                />
                            </div>
                            <div>
                                <h1 className="text-3xl font-bold text-gray-900">
                                    EF Blockchain
                                </h1>
                                <p className="text-gray-600">
                                    Educational Blockchain Implementation
                                </p>
                            </div>
                        </div>

                        <button
                            onClick={handleRefresh}
                            disabled={status.loading}
                            className="flex items-center space-x-2 text-white px-4
                                py-2 rounded-lg disabled:opacity-50 transition-colors"
                            style={{
                                backgroundColor: colors.primary,
                                cursor: 'pointer',
                            }}
                            onMouseEnter={e =>
                                ((
                                    e.target as HTMLElement
                                ).style.backgroundColor = colors.secondary)
                            }
                            onMouseLeave={e =>
                                ((
                                    e.target as HTMLElement
                                ).style.backgroundColor = colors.primary)
                            }
                        >
                            <RefreshCw
                                className={`w-4 h-4 ${
                                    status.loading ? 'animate-spin' : ''
                                }`}
                            />
                            <span>Refresh</span>
                        </button>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-6">
                        <div
                            className="p-4 rounded-lg"
                            style={{ backgroundColor: colors.light }}
                        >
                            <div className="flex items-center space-x-2">
                                <Server
                                    className="w-5 h-5"
                                    style={{ color: colors.primary }}
                                />
                                <span
                                    className="font-semibold"
                                    style={{ color: colors.secondary }}
                                >
                                    Blocks
                                </span>
                            </div>
                            <p
                                className="mt-1 text-xl font-bold"
                                style={{ color: colors.secondary }}
                            >
                                {status.loading
                                    ? '...'
                                    : status.data?.blocks ?? 'N/A'}
                            </p>
                        </div>

                        <div
                            className="p-4 rounded-lg"
                            style={{ backgroundColor: colors.light }}
                        >
                            <div className="flex items-center space-x-2">
                                <Code
                                    className="w-5 h-5"
                                    style={{ color: colors.primary }}
                                />
                                <span
                                    className="font-semibold"
                                    style={{ color: colors.secondary }}
                                >
                                    Difficulty
                                </span>
                            </div>
                            <p
                                className="mt-1 text-xl font-bold"
                                style={{ color: colors.secondary }}
                            >
                                {status.loading
                                    ? '...'
                                    : status.data?.difficulty ?? 'N/A'}
                            </p>
                        </div>

                        <div
                            className="p-4 rounded-lg"
                            style={{ backgroundColor: colors.light }}
                        >
                            <div className="flex items-center space-x-2">
                                <Wallet
                                    className="w-5 h-5"
                                    style={{ color: colors.primary }}
                                />
                                <span
                                    className="font-semibold"
                                    style={{ color: colors.secondary }}
                                >
                                    Mempool
                                </span>
                            </div>
                            <p
                                className="mt-1 text-xl font-bold"
                                style={{ color: colors.secondary }}
                            >
                                {status.loading
                                    ? '...'
                                    : status.data?.mempool ?? 'N/A'}
                            </p>
                        </div>
                    </div>
                </div>

                {(status.error || nextBlock.error) && (
                    <ErrorDisplay error={status.error || nextBlock.error!} />
                )}

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
                    <div className="bg-white rounded-lg shadow-md p-6">
                        <h3 className="text-lg font-semibold mb-3 flex items-center">
                            <Server
                                className="w-5 h-5 mr-2"
                                style={{ color: colors.primary }}
                            />
                            Blockchain Status
                        </h3>
                        <p className="text-gray-600 mb-4">
                            Get current blockchain information
                        </p>

                        <button
                            onClick={() =>
                                fetchData<BlockchainStatus>(
                                    '/status',
                                    setStatus,
                                )
                            }
                            disabled={status.loading}
                            className="w-full text-white font-medium py-2 px-4
                                rounded-lg disabled:opacity-50 transition-colors"
                            style={{ backgroundColor: colors.primary }}
                            onMouseEnter={e =>
                                ((
                                    e.target as HTMLElement
                                ).style.backgroundColor = colors.secondary)
                            }
                            onMouseLeave={e =>
                                ((
                                    e.target as HTMLElement
                                ).style.backgroundColor = colors.primary)
                            }
                        >
                            {status.loading ? 'Loading...' : 'Get Status'}
                        </button>

                        {status.data && (
                            <div
                                className="mt-4 p-3 rounded-lg"
                                style={{ backgroundColor: colors.light }}
                            >
                                <div
                                    className="text-xs mb-2"
                                    style={{ color: colors.medium }}
                                >
                                    Response:
                                </div>
                                <pre className="text-xs overflow-x-auto max-h-150">
                                    {JSON.stringify(status.data, null, 2)}
                                </pre>
                            </div>
                        )}
                    </div>

                    <div className="bg-white rounded-lg shadow-md p-6">
                        <h3 className="text-lg font-semibold mb-3 flex items-center">
                            <Blocks
                                className="w-5 h-5 mr-2"
                                style={{ color: colors.accent }}
                            />
                            Next Block Info
                        </h3>
                        <p className="text-gray-600 mb-4">
                            Get mining information for next block
                        </p>

                        <button
                            onClick={() =>
                                fetchData<BlockInfo>(
                                    '/blocks/next',
                                    setNextBlock,
                                )
                            }
                            disabled={nextBlock.loading}
                            className="w-full text-white font-medium py-2 px-4
                                rounded-lg disabled:opacity-50 transition-colors"
                            style={{ backgroundColor: colors.accent }}
                            onMouseEnter={e =>
                                ((
                                    e.target as HTMLElement
                                ).style.backgroundColor = colors.medium)
                            }
                            onMouseLeave={e =>
                                ((
                                    e.target as HTMLElement
                                ).style.backgroundColor = colors.accent)
                            }
                        >
                            {nextBlock.loading
                                ? 'Loading...'
                                : 'Get Next Block'}
                        </button>

                        {nextBlock.data && (
                            <div
                                className="mt-4 p-3 rounded-lg"
                                style={{ backgroundColor: colors.light }}
                            >
                                <div
                                    className="text-xs mb-2"
                                    style={{ color: colors.medium }}
                                >
                                    Block #{nextBlock.data.index} | Difficulty:{' '}
                                    {nextBlock.data.difficulty}
                                </div>
                                <div
                                    className="text-xs overflow-x-auto max-h-150
                                    overflow-y-auto"
                                >
                                    <pre>
                                        {JSON.stringify(
                                            nextBlock.data,
                                            null,
                                            2,
                                        )}
                                    </pre>
                                </div>
                            </div>
                        )}
                    </div>

                    <div className="bg-white rounded-lg shadow-md p-6">
                        <h3 className="text-lg font-semibold mb-3 flex items-center">
                            <Code
                                className="w-5 h-5 mr-2"
                                style={{ color: colors.medium }}
                            />
                            API Documentation
                        </h3>

                        <div>
                            <p className="text-gray-600 mb-4">
                                Interactive Swagger documentation
                            </p>

                            <a
                                href={`${API_BASE}/swagger`}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="w-full text-white font-medium py-2 px-4 rounded-lg
                                inline-flex items-center justify-center transition-colors"
                                style={{
                                    backgroundColor: colors.medium,
                                    textDecoration: 'none',
                                }}
                            >
                                Open Swagger UI
                                <ExternalLink className="w-4 h-4 ml-2" />
                            </a>
                        </div>

                        <div className="mt-5">
                            <p className="text-gray-600 mb-4">
                                Documentation built with DocFX
                            </p>

                            <a
                                href="https://docs-ef-blockchain.caprover.edersonfernandes.tec.br"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="w-full text-white font-medium py-2 px-4 rounded-lg
                                inline-flex items-center justify-center transition-colors"
                                style={{
                                    backgroundColor: colors.medium,
                                    textDecoration: 'none',
                                }}
                            >
                                DocFX
                                <ExternalLink className="w-4 h-4 ml-2" />
                            </a>
                        </div>

                        <div className="mt-5">
                            <p className="text-gray-600 mb-4">
                                Coverage report by tests
                            </p>

                            <a
                                href="https://docs-ef-blockchain.caprover.edersonfernandes.tec.br/coverage/index.html"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="w-full text-white font-medium py-2 px-4 rounded-lg
                                inline-flex items-center justify-center transition-colors"
                                style={{
                                    backgroundColor: colors.medium,
                                    textDecoration: 'none',
                                }}
                            >
                                Tests Coverage
                                <ExternalLink className="w-4 h-4 ml-2" />
                            </a>
                        </div>

                        <div className="mt-5">
                            <p className="text-gray-600 mb-4">
                                Show me the code
                            </p>

                            <a
                                href="https://github.com/efernandes-tech/web3-001-ef-blockchain"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="w-full text-white font-medium py-2 px-4 rounded-lg
                                inline-flex items-center justify-center transition-colors"
                                style={{
                                    backgroundColor: colors.medium,
                                    textDecoration: 'none',
                                }}
                            >
                                Repo GitHub
                                <ExternalLink className="w-4 h-4 ml-2" />
                            </a>
                        </div>
                    </div>
                </div>

                <div className="bg-white rounded-xl shadow-lg p-8">
                    <h2 className="text-2xl font-bold mb-6">
                        Environment Links
                    </h2>

                    <h3 className="font-semibold text-gray-900 flex items-center">
                        <div
                            className="w-3 h-3 rounded-full mr-2"
                            style={{ backgroundColor: colors.primary }}
                        ></div>
                        {import.meta.env.PROD
                            ? 'Production (CapRover)'
                            : 'Local (Docker)'}
                    </h3>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-3">
                        <div className="space-y-3">
                            <a
                                href={`${API_BASE}/status`}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="block p-4 rounded-lg border transition-colors"
                                style={{
                                    backgroundColor: colors.light,
                                    borderColor: colors.accent,
                                    color: colors.secondary,
                                    textDecoration: 'none',
                                }}
                            >
                                <div className="font-medium text-green-900">
                                    Status Endpoint
                                </div>
                                <div className="text-green-600 text-sm font-mono">
                                    {API_BASE}/status
                                </div>
                            </a>
                        </div>
                    </div>
                </div>

                <div className="text-center mt-8 text-gray-500">
                    <p>Created by Éderson Fernandes</p>
                </div>
            </div>
        </div>
    );
};

export default DashboardPage;
