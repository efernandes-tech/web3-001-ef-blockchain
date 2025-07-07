using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.Mocks;

public static class BlockchainMockFactory
{
    public const string MockedPrivateKey = "5976f176c2632c8406c8c614630d6c9f209bb8d04fd2d4499e37d45c676e8aa1";
    public const string MockedPublicKey = "02e11761fee94d9577794f9ae4ce4060af61ec0025871953ca4d249131f888b216";

    public static Domain.Blockchain CreateWithBlocks(int count, bool addExtraTx = true)
    {
        if (count < 1)
            throw new ArgumentException("Must create at least 1 block");

        var chain = new Domain.Blockchain(MockedPublicKey);

        for (int i = 1; i < count; i++)
        {
            var last = chain.GetLastBlock();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var index = chain.NextIndex;

            var transactionInput = new TransactionInput(
                fromAddress: MockedPublicKey,
                amount: 10
            );

            transactionInput.Sign(MockedPrivateKey);

            var transactionOutput = new TransactionOutput(
                toAddress: MockedPublicKey,
                amount: 10,
                tx: "abc"
            );

            var transaction = new Transaction(
                timestamp: timestamp,
                txInputs: new List<TransactionInput> { transactionInput },
                txOutputs: new List<TransactionOutput> { transactionOutput }
            );

            var transactionFee = new Transaction(
                type: TransactionType.FEE,
                txOutputs: new List<TransactionOutput> { transactionOutput });

            chain.Mempool.Add(transaction);

            var block = new Block(
                index,
                last.Hash,
                new List<Transaction> { transaction, transactionFee },
                timestamp,
                hash: null
            );

            block.Mine(
                chain.GetDifficulty(),
                miner: MockedPublicKey
            );

            chain.AddBlock(block);
        }

        if (addExtraTx)
        {
            var extraTransaction = new Transaction(
                txInputs: new List<TransactionInput> { new TransactionInput() });

            chain.Mempool.Add(extraTransaction);
        }

        return chain;
    }
}
