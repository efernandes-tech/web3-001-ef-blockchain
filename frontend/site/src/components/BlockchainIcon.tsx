import type { CSSProperties } from 'react';

const BlockchainIcon: React.FC<{
    className?: string;
    style?: CSSProperties;
}> = ({ className = 'w-8 h-8', style }) => (
    <img
        src="/ef-blockchain.svg"
        alt="EF Blockchain"
        className={className}
        style={style}
    />
);

export default BlockchainIcon;
