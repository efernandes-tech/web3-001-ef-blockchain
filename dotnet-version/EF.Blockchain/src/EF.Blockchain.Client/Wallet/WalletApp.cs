namespace EF.Blockchain.Client.Wallet;

public class WalletApp
{
    private readonly string _privateKey;

    public WalletApp(string privateKey)
    {
        _privateKey = privateKey;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Starting wallet...");

        Console.WriteLine("Logged as " + _privateKey);

        var wallet = new Domain.Wallet(_privateKey);

        Console.WriteLine("Private Key: " + wallet.PrivateKey);
        Console.WriteLine("Public Key: " + wallet.PublicKey);

        var otherWallet = new Domain.Wallet();

        Console.WriteLine("Private Key (otherWallet): " + otherWallet.PrivateKey);
        Console.WriteLine("Public Key (otherWallet): " + otherWallet.PublicKey);

        var transactionInput = new Domain.TransactionInput(
            fromAddress: wallet.PublicKey,
            amount: 10
        );
        transactionInput.Sign(wallet.PrivateKey);
        Console.WriteLine("Signature (otherWallet): " + transactionInput.Signature);

        while (true)
        {
            await Task.Delay(1000); // Simulate some work
        }
    }
}
