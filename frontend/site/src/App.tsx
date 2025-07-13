import { Activity, Home, Menu, Wallet, X } from 'lucide-react';
import React from 'react';
import {
    BrowserRouter,
    Link,
    Route,
    Routes,
    useLocation,
} from 'react-router-dom';
import BlockchainIcon from './components/BlockchainIcon';
import { colors } from './config/consts';
import './index.css';
import DashboardPage from './pages/DashboardPage';
import MinerLogsPage from './pages/MinerLogsPage';
import WalletPage from './pages/WalletPage';

const Navigation: React.FC = () => {
    const location = useLocation();
    const [isMobileMenuOpen, setIsMobileMenuOpen] = React.useState(false);

    const navItems = [
        { path: '/', label: 'Dashboard', icon: Home },
        { path: '/wallet', label: 'Wallet', icon: Wallet },
        { path: '/miner-logs', label: 'Miner Logs', icon: Activity },
    ];

    const isActive = (path: string) => location.pathname === path;

    return (
        <nav
            className="shadow-lg border-b"
            style={{
                backgroundColor: colors.secondary,
                borderBottomColor: colors.medium,
            }}
        >
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between h-16">
                    <div className="flex items-center">
                        <Link to="/" className="flex items-center space-x-2">
                            <BlockchainIcon
                                className="w-10 h-10"
                                style={{ color: colors.primary }}
                            />
                            <span className="text-xl text-white font-bold hidden sm:block">
                                EF Blockchain
                            </span>
                        </Link>
                    </div>
                    <div className="hidden md:flex items-center space-x-8">
                        {navItems.map(item => (
                            <Link
                                key={item.path}
                                to={item.path}
                                className={`flex items-center space-x-2 px-3 py-2
                                    rounded-md text-sm font-medium transition-colors ${
                                        isActive(item.path)
                                            ? 'text-white'
                                            : 'text-gray-300 hover:text-white'
                                    }`}
                                style={{
                                    backgroundColor: isActive(item.path)
                                        ? colors.accent
                                        : 'transparent',
                                }}
                                onMouseEnter={e => {
                                    if (!isActive(item.path)) {
                                        e.currentTarget.style.backgroundColor =
                                            colors.medium;
                                    }
                                }}
                                onMouseLeave={e => {
                                    if (!isActive(item.path)) {
                                        e.currentTarget.style.backgroundColor =
                                            'transparent';
                                    }
                                }}
                            >
                                <item.icon className="w-4 h-4" />
                                <span>{item.label}</span>
                            </Link>
                        ))}
                    </div>
                    <div className="md:hidden flex items-center">
                        <button
                            onClick={() =>
                                setIsMobileMenuOpen(!isMobileMenuOpen)
                            }
                            className="text-gray-300 hover:text-white
                                focus:outline-none focus:text-white p-2"
                        >
                            {isMobileMenuOpen ? (
                                <X className="w-6 h-6" />
                            ) : (
                                <Menu className="w-6 h-6" />
                            )}
                        </button>
                    </div>
                </div>
                {isMobileMenuOpen && (
                    <div className="md:hidden border-t border-gray-200">
                        <div className="pt-2 pb-3 space-y-1">
                            {navItems.map(item => (
                                <Link
                                    key={item.path}
                                    to={item.path}
                                    onClick={() => setIsMobileMenuOpen(false)}
                                    className={`flex items-center space-x-2 px-3 py-2
                                        text-base font-medium transition-colors ${
                                            isActive(item.path)
                                                ? 'text-white border-l-4'
                                                : 'text-gray-300 hover:text-white'
                                        }`}
                                    style={{
                                        backgroundColor: isActive(item.path)
                                            ? colors.accent
                                            : 'transparent',
                                        borderLeftColor: isActive(item.path)
                                            ? colors.primary
                                            : 'transparent',
                                    }}
                                >
                                    <item.icon className="w-5 h-5" />
                                    <span>{item.label}</span>
                                </Link>
                            ))}
                        </div>
                    </div>
                )}
            </div>
        </nav>
    );
};

const App: React.FC = () => {
    return (
        <BrowserRouter>
            <div
                className="App min-h-screen"
                style={{ backgroundColor: colors.light }}
            >
                <Navigation />

                <main className="flex-1">
                    <Routes>
                        <Route path="/" element={<DashboardPage />} />
                        <Route path="/wallet" element={<WalletPage />} />
                        <Route path="/miner-logs" element={<MinerLogsPage />} />
                        <Route
                            path="*"
                            element={
                                <div
                                    className="flex items-center justify-center
                                        min-h-screen"
                                >
                                    <div className="text-center">
                                        <h1
                                            className="text-4xl font-bold mb-4"
                                            style={{ color: colors.secondary }}
                                        >
                                            404
                                        </h1>
                                        <p
                                            className="mb-4"
                                            style={{ color: colors.medium }}
                                        >
                                            Page not found
                                        </p>
                                        <Link
                                            to="/"
                                            className="underline hover:no-underline
                                                transition-colors"
                                            style={{
                                                color: colors.accent,
                                            }}
                                            onMouseEnter={e => {
                                                e.currentTarget.style.color =
                                                    colors.primary;
                                            }}
                                            onMouseLeave={e => {
                                                e.currentTarget.style.color =
                                                    colors.accent;
                                            }}
                                        >
                                            Go back to Dashboard
                                        </Link>
                                    </div>
                                </div>
                            }
                        />
                    </Routes>
                </main>
            </div>
        </BrowserRouter>
    );
};

export default App;
