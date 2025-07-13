export interface WalletData {
    balance: number;
    fee: number;
    utxo: TransactionOutput[];
}

export interface TransactionOutput {
    toAddress: string;
    amount: number;
    txHash: string;
}

export interface WalletState {
    publicKey: string | null;
    privateKey: string | null;
    balance: number;
    isLoading: boolean;
}
