import TransactionInput from "./transactionInput";
import TransactionType from "../transactionType"
import Validation from "../validation";

/**
 * Mocked Transaction class
 */
export default class Transaction {
    type: TransactionType;
    timestamp: number;
    hash: string;
    to: string;
    txInput: TransactionInput

    constructor(tx?: Transaction) {
        this.type = tx?.type || TransactionType.REGULAR;
        this.timestamp = tx?.timestamp || Date.now();
        this.to = tx?.to || "carteiraTo";

        if (tx && tx.txInput)
            this.txInput = new TransactionInput(tx.txInput);
        else
            this.txInput = new TransactionInput();

        this.hash = tx?.hash || this.getHash();
    }

    getHash(): string {
        return "abc";
    }

    isValid(): Validation {
        if (!this.to) return new Validation(false, "Invalid mock transaction.");

        if (!this.txInput.isValid().success) return new Validation(false, "Invalid mock transaction.");

        return new Validation();
    }
}