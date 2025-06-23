using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.Mocks;

public static class TransactionMockFactory
{
    public const string MockedPrivateKey = "5976f176c2632c8406c8c614630d6c9f209bb8d04fd2d4499e37d45c676e8aa1";
    public const string MockedPublicKey = "02e11761fee94d9577794f9ae4ce4060af61ec0025871953ca4d249131f888b216";

    public const string MockedPublicKeyTo = "02b92d6cdef01b4c1438214ac0aa68f552f9dad1221ebd270133d172f5b0a15e5d";

    public static Transaction Create(
        TransactionType? type = null,
        long? timestamp = null,
        TransactionInput? transactionInput = null,
        string? hash = null,
        string? to = null)
    {
        TransactionInput txInput = null;

        if (transactionInput != null)
        {
            txInput = transactionInput.Amount == 0
                ? new TransactionInput(
                    fromAddress: MockedPublicKey,
                    amount: 1000)
                : transactionInput;

            txInput.Sign(MockedPrivateKey);
        }

        var tx = new Transaction(
            type: type ?? TransactionType.REGULAR,
            timestamp: timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            txInput: txInput,
            to: to ?? MockedPublicKeyTo
        );

        if (hash != null)
        {
            // Use reflection to change private/internal state
            typeof(Transaction)
                .GetProperty(nameof(Transaction.Hash))!
                .SetValue(tx, hash);
        }

        return tx;
    }

    public static Transaction CreateInvalidTxInput() =>
        Create(transactionInput: new TransactionInput(amount: -1));

    public static Transaction CreateInvalidHash() =>
        Create(transactionInput: new TransactionInput(), hash: "wrong-hash");
}
