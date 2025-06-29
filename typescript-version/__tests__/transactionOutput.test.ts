import { describe, test, expect, beforeAll } from '@jest/globals';
import TransactionOutput from '../src/lib/transactionOutput';
import Wallet from '../src/lib/wallet';

describe("TransactionOutput tests", () => {

    let alice: Wallet, bob: Wallet;

    beforeAll(() => {
        alice = new Wallet();
        bob = new Wallet();
    })

    test('Should be valid', () => {
        const txInput = new TransactionOutput({
            amount: 10,
            toAddress: alice.publicKey,
            tx: "abc"
        } as TransactionOutput)

        const valid = txInput.isValid();
        expect(valid.success).toBeTruthy();
    })
})