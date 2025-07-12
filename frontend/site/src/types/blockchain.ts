// Blockchain API Types

export interface BlockchainStatus {
    blocks: number;
    height: number;
    isValid: boolean;
    difficulty: number;
    mempool: number;
    lastBlockHash: string;
}

export interface TransactionInput {
    fromAddress: string;
    amount: number;
    signature: string;
    previousTx: string;
}

export interface TransactionOutput {
    toAddress: string;
    amount: number;
    tx: string;
}

export interface Transaction {
    type: number;
    timestamp: number;
    hash: string;
    txInputs: TransactionInput[];
    txOutputs: TransactionOutput[];
}

export interface BlockInfo {
    index: number;
    previousHash: string;
    difficulty: number;
    maxDifficulty: number;
    feePerTx: number;
    transactions: Transaction[];
}

export interface Block {
    index: number;
    previousHash: string;
    transactions: Transaction[];
    timestamp: number;
    hash: string;
    nonce: number;
    miner: string;
}

export interface WalletInfo {
    balance: number;
    fee: number;
    utxo: TransactionOutput[];
}

export interface ApiError {
    message: string;
    status: number;
}
