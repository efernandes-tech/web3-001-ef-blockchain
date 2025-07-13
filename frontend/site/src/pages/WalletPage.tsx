import {
    ArrowUpRight,
    Copy,
    Eye,
    EyeOff,
    Plus,
    RefreshCw,
    Search,
    Wallet,
} from 'lucide-react';
import React, { useEffect, useState } from 'react';
import { colors } from '../config/consts';
import type { WalletData, WalletState } from '../types/wallet-types';

const BLOCKCHAIN_SERVER = import.meta.env.VITE_API_URL;

const WalletPage: React.FC = () => {
    const [wallet, setWallet] = useState<WalletState>({
        publicKey: null,
        privateKey: null,
        balance: 0,
        isLoading: false,
    });

    const [showPrivateKey, setShowPrivateKey] = useState(false);
    const [activeTab, setActiveTab] = useState<string>('overview');

    const [sendForm, setSendForm] = useState({
        toWallet: '',
        amount: '',
    });

    const [searchHash, setSearchHash] = useState('');
    const [searchResult, setSearchResult] = useState(null);

    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const createWallet = async () => {
        try {
            setError(null);
            setWallet(prev => ({ ...prev, isLoading: true }));

            const response = await fetch(`${BLOCKCHAIN_SERVER}/wallets`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: 'My Wallet' }),
            });

            if (!response.ok) throw new Error('Failed to create wallet');

            const newWallet = await response.json();

            setWallet({
                publicKey: newWallet.publicKey,
                privateKey: newWallet.privateKey,
                balance: 0,
                isLoading: false,
            });

            setSuccess('Wallet created successfully!');
        } catch (err) {
            setError(
                err instanceof Error ? err.message : 'Failed to create wallet',
            );
            setWallet(prev => ({ ...prev, isLoading: false }));
        }
    };

    const recoverWallet = async () => {
        const privateKey = prompt('Enter your private key or WIF:');
        if (!privateKey) return;

        try {
            setError(null);
            setWallet(prev => ({ ...prev, isLoading: true }));

            const response = await fetch(
                `${BLOCKCHAIN_SERVER}/wallets/recover`,
                {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ privateKey }),
                },
            );

            if (!response.ok) throw new Error('Failed to recover wallet');

            const recoverWallet = await response.json();

            setWallet({
                publicKey: recoverWallet.publicKey,
                privateKey: recoverWallet.privateKey,
                balance: recoverWallet.balance,
                isLoading: false,
            });

            setSuccess('Wallet recovered successfully!');
        } catch (err) {
            console.error('err:', err);
            setError('Invalid private key');
        }
    };

    const getBalance = async (publicKey?: string) => {
        const walletKey = publicKey || wallet.publicKey;
        if (!walletKey) {
            setError('No wallet available');
            return;
        }

        try {
            setError(null);
            setWallet(prev => ({ ...prev, isLoading: true }));

            const response = await fetch(
                `${BLOCKCHAIN_SERVER}/wallets/${walletKey}`,
            );

            if (!response.ok) throw new Error('Failed to get balance');

            const walletData: WalletData = await response.json();

            setWallet(prev => ({
                ...prev,
                balance: walletData.balance,
                isLoading: false,
            }));
        } catch (err) {
            setError(
                err instanceof Error ? err.message : 'Failed to get balance',
            );
            setWallet(prev => ({ ...prev, isLoading: false }));
        }
    };

    const sendTransaction = async () => {
        if (!wallet.publicKey || !wallet.privateKey) {
            setError('No wallet available');
            return;
        }

        if (!sendForm.toWallet || sendForm.toWallet.length < 66) {
            setError('Invalid recipient wallet address');
            return;
        }

        const amount = parseInt(sendForm.amount);
        if (!amount || amount <= 0) {
            setError('Invalid amount');
            return;
        }

        try {
            setError(null);
            setWallet(prev => ({ ...prev, isLoading: true }));

            const prepareResponse = await fetch(
                `${BLOCKCHAIN_SERVER}/transactions/prepare`,
                {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        fromWalletAddress: wallet.publicKey,
                        toWalletAddress: sendForm.toWallet,
                        fromWalletPrivateKey: wallet.privateKey,
                        amount: amount,
                    }),
                },
            );

            if (!prepareResponse.ok) {
                const errorData = await prepareResponse.json();
                throw new Error(
                    errorData.error || 'Failed to prepare transaction',
                );
            }

            const prepared = await prepareResponse.json();

            const response = await fetch(`${BLOCKCHAIN_SERVER}/transactions`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(prepared),
            });

            if (!response.ok) {
                const errorData = await response.text();
                throw new Error(`Transaction failed: ${errorData}`);
            }

            const result = await response.json();

            setSuccess(
                `Transaction accepted! Waiting for miners. Hash: ${
                    result.hash || result.transactionHash || 'Pending'
                }`,
            );
            setSendForm({ toWallet: '', amount: '' });

            setTimeout(() => getBalance(), 2000);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Transaction failed');
        } finally {
            setWallet(prev => ({ ...prev, isLoading: false }));
        }
    };

    const searchTransaction = async () => {
        if (!searchHash.trim()) {
            setError('Please enter a transaction hash');
            return;
        }

        try {
            setError(null);
            setSearchResult(null);

            const response = await fetch(
                `${BLOCKCHAIN_SERVER}/transactions/${searchHash}`,
            );

            if (!response.ok) throw new Error('Transaction not found');

            const result = await response.json();
            setSearchResult(result);
        } catch (err) {
            setError(
                err instanceof Error ? err.message : 'Transaction not found',
            );
        }
    };

    const copyToClipboard = (text: string) => {
        navigator.clipboard.writeText(text);
        setSuccess('Copied to clipboard!');
    };

    useEffect(() => {
        if (error || success) {
            const timer = setTimeout(() => {
                setError(null);
                setSuccess(null);
            }, 3000);
            return () => clearTimeout(timer);
        }
    }, [error, success]);

    return (
        <div
            className="min-h-screen p-4"
            style={{ backgroundColor: colors.light }}
        >
            <div className="max-w-4xl mx-auto">
                <div className="bg-white rounded-lg shadow-md p-6 mb-6">
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                            <Wallet
                                className="w-8 h-8"
                                style={{ color: colors.primary }}
                            />
                            <h1
                                className="text-2xl font-bold"
                                style={{ color: colors.secondary }}
                            >
                                EF Blockchain Wallet
                            </h1>
                        </div>

                        {wallet.publicKey && (
                            <button
                                onClick={() => getBalance()}
                                disabled={wallet.isLoading}
                                className="flex items-center gap-2 px-4 py-2 text-white rounded-lg hover:opacity-90 disabled:opacity-50 transition-opacity"
                                style={{ backgroundColor: colors.accent }}
                            >
                                <RefreshCw
                                    className={`w-4 h-4 ${
                                        wallet.isLoading ? 'animate-spin' : ''
                                    }`}
                                />
                                Refresh
                            </button>
                        )}
                    </div>

                    <div className="mt-4">
                        {wallet.publicKey ? (
                            <div className="space-y-2">
                                <p
                                    className="text-sm"
                                    style={{ color: colors.medium }}
                                >
                                    Connected as:
                                </p>
                                <div
                                    className="flex items-center gap-2 p-3 rounded-lg"
                                    style={{ backgroundColor: colors.light }}
                                >
                                    <code
                                        className="text-sm font-mono flex-1 truncate"
                                        style={{ color: colors.secondary }}
                                    >
                                        {wallet.publicKey}
                                    </code>
                                    <button
                                        onClick={() =>
                                            copyToClipboard(wallet.publicKey!)
                                        }
                                        className="p-1 hover:opacity-80 rounded transition-opacity"
                                        style={{
                                            backgroundColor:
                                                'rgba(60, 183, 184, 0.1)',
                                        }}
                                    >
                                        <Copy
                                            className="w-4 h-4"
                                            style={{ color: colors.accent }}
                                        />
                                    </button>
                                </div>

                                {wallet.privateKey && (
                                    <div
                                        className="flex items-center gap-2 p-3 rounded-lg"
                                        style={{
                                            backgroundColor:
                                                'rgba(255, 110, 32, 0.1)',
                                        }}
                                    >
                                        <span
                                            className="text-sm"
                                            style={{ color: colors.medium }}
                                        >
                                            Private Key:
                                        </span>
                                        <code
                                            className="text-sm font-mono flex-1 truncate"
                                            style={{ color: colors.secondary }}
                                        >
                                            {showPrivateKey
                                                ? wallet.privateKey
                                                : '•'.repeat(64)}
                                        </code>
                                        <button
                                            onClick={() =>
                                                setShowPrivateKey(
                                                    !showPrivateKey,
                                                )
                                            }
                                            className="p-1 hover:opacity-80 rounded transition-opacity"
                                            style={{
                                                backgroundColor:
                                                    'rgba(255, 110, 32, 0.2)',
                                            }}
                                        >
                                            {showPrivateKey ? (
                                                <EyeOff
                                                    className="w-4 h-4"
                                                    style={{
                                                        color: colors.primary,
                                                    }}
                                                />
                                            ) : (
                                                <Eye
                                                    className="w-4 h-4"
                                                    style={{
                                                        color: colors.primary,
                                                    }}
                                                />
                                            )}
                                        </button>
                                        <button
                                            onClick={() =>
                                                copyToClipboard(
                                                    wallet.privateKey!,
                                                )
                                            }
                                            className="p-1 hover:opacity-80 rounded transition-opacity"
                                            style={{
                                                backgroundColor:
                                                    'rgba(255, 110, 32, 0.2)',
                                            }}
                                        >
                                            <Copy
                                                className="w-4 h-4"
                                                style={{
                                                    color: colors.primary,
                                                }}
                                            />
                                        </button>
                                    </div>
                                )}
                            </div>
                        ) : (
                            <p style={{ color: colors.medium }}>
                                You aren't logged in.
                            </p>
                        )}
                    </div>
                </div>

                {!wallet.publicKey && (
                    <div className="bg-white rounded-lg shadow-md p-6 mb-6">
                        <h2
                            className="text-xl font-semibold mb-4"
                            style={{ color: colors.secondary }}
                        >
                            Get Started
                        </h2>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <button
                                onClick={createWallet}
                                disabled={wallet.isLoading}
                                className="flex items-center gap-3 p-4 border-2 border-dashed
                                    rounded-lg hover:opacity-80 transition-opacity disabled:opacity-50"
                                style={{
                                    borderColor: colors.accent,
                                    backgroundColor: 'rgba(60, 183, 184, 0.05)',
                                }}
                            >
                                <Plus
                                    className="w-6 h-6"
                                    style={{ color: colors.accent }}
                                />
                                <div className="text-left">
                                    <div
                                        className="font-medium"
                                        style={{ color: colors.accent }}
                                    >
                                        Create New Wallet
                                    </div>
                                    <div
                                        className="text-sm"
                                        style={{ color: colors.medium }}
                                    >
                                        Generate a new wallet
                                    </div>
                                </div>
                            </button>

                            <button
                                onClick={recoverWallet}
                                disabled={wallet.isLoading}
                                className="flex items-center gap-3 p-4 border-2 border-dashed
                                    rounded-lg hover:opacity-80 transition-opacity disabled:opacity-50"
                                style={{
                                    borderColor: colors.primary,
                                    backgroundColor: 'rgba(255, 110, 32, 0.05)',
                                }}
                            >
                                <Wallet
                                    className="w-6 h-6"
                                    style={{ color: colors.primary }}
                                />
                                <div className="text-left">
                                    <div
                                        className="font-medium"
                                        style={{ color: colors.primary }}
                                    >
                                        Recover Wallet
                                    </div>
                                    <div
                                        className="text-sm"
                                        style={{ color: colors.medium }}
                                    >
                                        Import from private key
                                    </div>
                                </div>
                            </button>
                        </div>
                    </div>
                )}

                {wallet.publicKey && (
                    <>
                        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
                            <h2
                                className="text-xl font-semibold mb-4"
                                style={{ color: colors.secondary }}
                            >
                                Balance
                            </h2>
                            <div
                                className="text-3xl font-bold"
                                style={{ color: colors.primary }}
                            >
                                {wallet.isLoading ? (
                                    <div className="flex items-center gap-2">
                                        <RefreshCw className="w-6 h-6 animate-spin" />
                                        Loading...
                                    </div>
                                ) : (
                                    `${wallet.balance} EFB`
                                )}
                            </div>
                        </div>

                        <div className="bg-white rounded-lg shadow-md overflow-hidden">
                            <div
                                className="border-b"
                                style={{ borderBottomColor: colors.light }}
                            >
                                <nav className="flex">
                                    {[
                                        {
                                            id: 'overview',
                                            label: 'Overview',
                                            icon: Wallet,
                                        },
                                        {
                                            id: 'send',
                                            label: 'Send',
                                            icon: ArrowUpRight,
                                        },
                                        {
                                            id: 'search',
                                            label: 'Search',
                                            icon: Search,
                                        },
                                    ].map(tab => (
                                        <button
                                            key={tab.id}
                                            onClick={() => setActiveTab(tab.id)}
                                            className="flex items-center gap-2 px-6 py-3 text-sm
                                                font-medium border-b-2 transition-colors"
                                            style={{
                                                borderBottomColor:
                                                    activeTab === tab.id
                                                        ? colors.accent
                                                        : 'transparent',
                                                color:
                                                    activeTab === tab.id
                                                        ? colors.accent
                                                        : colors.medium,
                                            }}
                                        >
                                            <tab.icon className="w-4 h-4" />
                                            {tab.label}
                                        </button>
                                    ))}
                                </nav>
                            </div>

                            <div className="p-6">
                                {activeTab === 'overview' && (
                                    <div className="space-y-4">
                                        <h3
                                            className="text-lg font-semibold"
                                            style={{ color: colors.secondary }}
                                        >
                                            Wallet Overview
                                        </h3>
                                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                            <div
                                                className="p-4 rounded-lg"
                                                style={{
                                                    backgroundColor:
                                                        'rgba(60, 183, 184, 0.1)',
                                                }}
                                            >
                                                <div
                                                    className="text-sm"
                                                    style={{
                                                        color: colors.accent,
                                                    }}
                                                >
                                                    Current Balance
                                                </div>
                                                <div
                                                    className="text-2xl font-bold"
                                                    style={{
                                                        color: colors.secondary,
                                                    }}
                                                >
                                                    {wallet.balance} EFB
                                                </div>
                                            </div>
                                            <div
                                                className="p-4 rounded-lg"
                                                style={{
                                                    backgroundColor:
                                                        'rgba(255, 110, 32, 0.1)',
                                                }}
                                            >
                                                <div
                                                    className="text-sm"
                                                    style={{
                                                        color: colors.primary,
                                                    }}
                                                >
                                                    Status
                                                </div>
                                                <div
                                                    className="text-lg font-semibold"
                                                    style={{
                                                        color: colors.secondary,
                                                    }}
                                                >
                                                    Active
                                                </div>
                                            </div>
                                            <div
                                                className="p-4 rounded-lg"
                                                style={{
                                                    backgroundColor:
                                                        'rgba(5, 91, 92, 0.1)',
                                                }}
                                            >
                                                <div
                                                    className="text-sm"
                                                    style={{
                                                        color: colors.medium,
                                                    }}
                                                >
                                                    Network
                                                </div>
                                                <div
                                                    className="text-lg font-semibold"
                                                    style={{
                                                        color: colors.secondary,
                                                    }}
                                                >
                                                    EF Blockchain
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                )}

                                {activeTab === 'send' && (
                                    <div className="space-y-4">
                                        <h3
                                            className="text-lg font-semibold"
                                            style={{ color: colors.secondary }}
                                        >
                                            Send Transaction
                                        </h3>
                                        <div className="space-y-4">
                                            <div>
                                                <label
                                                    className="block text-sm font-medium mb-2"
                                                    style={{
                                                        color: colors.medium,
                                                    }}
                                                >
                                                    Recipient Wallet Address
                                                </label>
                                                <input
                                                    type="text"
                                                    value={sendForm.toWallet}
                                                    onChange={e =>
                                                        setSendForm(prev => ({
                                                            ...prev,
                                                            toWallet:
                                                                e.target.value,
                                                        }))
                                                    }
                                                    placeholder="Enter recipient wallet address
                                                        (66+ characters)"
                                                    className="w-full px-3 py-2 border rounded-lg
                                                        focus:ring-2 focus:outline-none"
                                                    style={{
                                                        borderColor: '#E5E7EB',
                                                    }}
                                                    onFocus={e => {
                                                        e.target.style.borderColor =
                                                            colors.accent;
                                                        e.target.style.boxShadow = `0 0 0 2px rgba(60, 183, 184, 0.2)`;
                                                    }}
                                                    onBlur={e => {
                                                        e.target.style.borderColor =
                                                            '#E5E7EB';
                                                        e.target.style.boxShadow =
                                                            'none';
                                                    }}
                                                />
                                            </div>

                                            <div>
                                                <label
                                                    className="block text-sm font-medium mb-2"
                                                    style={{
                                                        color: colors.medium,
                                                    }}
                                                >
                                                    Amount (EFB)
                                                </label>
                                                <input
                                                    type="number"
                                                    value={sendForm.amount}
                                                    onChange={e =>
                                                        setSendForm(prev => ({
                                                            ...prev,
                                                            amount: e.target
                                                                .value,
                                                        }))
                                                    }
                                                    placeholder="Enter amount"
                                                    min="1"
                                                    className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:outline-none"
                                                    style={{
                                                        borderColor:
                                                            colors.light,
                                                    }}
                                                />
                                            </div>

                                            <button
                                                onClick={sendTransaction}
                                                disabled={
                                                    wallet.isLoading ||
                                                    !sendForm.toWallet ||
                                                    !sendForm.amount
                                                }
                                                className="w-full flex items-center justify-center gap-2 px-4 py-2 text-white rounded-lg hover:opacity-90 disabled:opacity-50 disabled:cursor-not-allowed transition-opacity"
                                                style={{
                                                    backgroundColor:
                                                        colors.primary,
                                                }}
                                            >
                                                {wallet.isLoading ? (
                                                    <RefreshCw className="w-4 h-4 animate-spin" />
                                                ) : (
                                                    <ArrowUpRight className="w-4 h-4" />
                                                )}
                                                Send Transaction
                                            </button>
                                        </div>
                                    </div>
                                )}

                                {activeTab === 'search' && (
                                    <div className="space-y-4">
                                        <h3
                                            className="text-lg font-semibold"
                                            style={{ color: colors.secondary }}
                                        >
                                            Search Transaction
                                        </h3>
                                        <div className="space-y-4">
                                            <div>
                                                <label
                                                    className="block text-sm font-medium mb-2"
                                                    style={{
                                                        color: colors.medium,
                                                    }}
                                                >
                                                    Transaction Hash
                                                </label>
                                                <div className="flex gap-2">
                                                    <input
                                                        type="text"
                                                        value={searchHash}
                                                        onChange={e =>
                                                            setSearchHash(
                                                                e.target.value,
                                                            )
                                                        }
                                                        placeholder="Enter transaction hash"
                                                        className="flex-1 px-3 py-2 border rounded-lg
                                                            focus:ring-2 focus:outline-none"
                                                        style={{
                                                            borderColor:
                                                                colors.light,
                                                        }}
                                                    />
                                                    <button
                                                        onClick={
                                                            searchTransaction
                                                        }
                                                        disabled={
                                                            !searchHash.trim()
                                                        }
                                                        className="px-4 py-2 text-white rounded-lg
                                                            hover:opacity-90 disabled:opacity-50
                                                            transition-opacity"
                                                        style={{
                                                            backgroundColor:
                                                                colors.accent,
                                                        }}
                                                    >
                                                        <Search className="w-4 h-4" />
                                                    </button>
                                                </div>
                                            </div>

                                            {searchResult && (
                                                <div
                                                    className="p-4 rounded-lg"
                                                    style={{
                                                        backgroundColor:
                                                            colors.light,
                                                    }}
                                                >
                                                    <h4
                                                        className="font-medium mb-2"
                                                        style={{
                                                            color: colors.secondary,
                                                        }}
                                                    >
                                                        Transaction Found:
                                                    </h4>
                                                    <pre
                                                        className="text-sm whitespace-pre-wrap overflow-auto"
                                                        style={{
                                                            color: colors.medium,
                                                        }}
                                                    >
                                                        {JSON.stringify(
                                                            searchResult,
                                                            null,
                                                            2,
                                                        )}
                                                    </pre>
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                )}
                            </div>
                        </div>
                    </>
                )}

                {error && (
                    <div
                        className="fixed bottom-4 right-4 border px-4 py-3 rounded-lg shadow-lg"
                        style={{
                            backgroundColor: 'rgba(255, 110, 32, 0.1)',
                            borderColor: colors.primary,
                            color: colors.secondary,
                        }}
                    >
                        {error}
                    </div>
                )}

                {success && (
                    <div
                        className="fixed bottom-4 right-4 border px-4 py-3 rounded-lg shadow-lg"
                        style={{
                            backgroundColor: 'rgba(60, 183, 184, 0.1)',
                            borderColor: colors.accent,
                            color: colors.secondary,
                        }}
                    >
                        {success}
                    </div>
                )}
            </div>
        </div>
    );
};

export default WalletPage;
