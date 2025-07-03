using EF.Blockchain.Domain;
using Flurl.Http;

namespace EF.Blockchain.Client.Wallet;

public class WalletApp
{
    private readonly string _blockchainServer;

    private string? _myWalletPublicKey;
    private string? _myWalletPrivateKey;

    public WalletApp(string blockchainServer)
    {
        _blockchainServer = blockchainServer;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Starting wallet...");

        while (true)
        {
            Console.Clear();

            if (!string.IsNullOrEmpty(_myWalletPublicKey))
                Console.WriteLine($"You are logged as {_myWalletPublicKey}");
            else
                Console.WriteLine("You aren't logged.");

            Console.WriteLine("1 - Create Wallet");
            Console.WriteLine("2 - Recover Wallet");
            Console.WriteLine("3 - Balance");
            Console.WriteLine("4 - Send Tx");
            Console.WriteLine("5 - Search Tx");
            Console.WriteLine("0 - Exit");

            Console.Write("Choose your option: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    CreateWallet();
                    break;

                case "2":
                    RecoverWallet();
                    break;

                case "3":
                    GetBalanceAsync();
                    break;

                case "4":
                    await SendTransaction();
                    break;

                case "5":
                    await SearchTransaction();
                    break;

                case "0":
                    Console.WriteLine("Bye!");
                    return;

                default:
                    Console.WriteLine("Wrong option!");
                    break;
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    private void CreateWallet()
    {
        Console.Clear();
        var wallet = new Domain.Wallet();
        Console.WriteLine("Your new wallet:");
        Console.WriteLine($"Private Key: {wallet.PrivateKey}");
        Console.WriteLine($"Public Key: {wallet.PublicKey}");

        _myWalletPublicKey = wallet.PublicKey;
        _myWalletPrivateKey = wallet.PrivateKey;
    }

    private void RecoverWallet()
    {
        Console.Clear();
        Console.Write("Enter your private key or WIF: ");
        var inputKey = Console.ReadLine() ?? string.Empty;

        try
        {
            var wallet = new Domain.Wallet(inputKey);
            Console.WriteLine("Your recovered wallet:");
            Console.WriteLine($"Private Key: {wallet.PrivateKey}");
            Console.WriteLine($"Public Key: {wallet.PublicKey}");

            _myWalletPublicKey = wallet.PublicKey;
            _myWalletPrivateKey = wallet.PrivateKey;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recovering wallet: {ex.Message}");
        }
    }

    private async Task GetBalanceAsync()
    {
        Console.Clear();

        if (string.IsNullOrEmpty(_myWalletPublicKey))
        {
            Console.WriteLine("You don't have a wallet yet.");
            return;
        }

        try
        {
            var walletData = await $"{_blockchainServer}/wallets/{_myWalletPublicKey}"
                .GetJsonAsync<WalletResponse>();

            Console.WriteLine($"Balance: {walletData.Balance}");
        }
        catch (FlurlHttpException ex)
        {
            var status = ex.Call?.Response?.StatusCode.ToString() ?? "No HTTP response";
            var error = await ex.GetResponseStringAsync();
            if (string.IsNullOrWhiteSpace(error)) error = ex.Message;

            Console.WriteLine($"Error ({status}): {error}");
        }
    }

    private async Task SendTransaction()
    {
        Console.Clear();

        if (string.IsNullOrEmpty(_myWalletPublicKey) || string.IsNullOrEmpty(_myWalletPrivateKey))
        {
            Console.WriteLine("You don't have a wallet yet.");
            return;
        }

        Console.WriteLine($"Your wallet is {_myWalletPublicKey}");

        Console.Write("To Wallet: ");
        var toWallet = Console.ReadLine() ?? string.Empty;

        if (toWallet.Length < 66)
        {
            Console.WriteLine("Invalid wallet.");
            return;
        }

        Console.Write("Amount: ");
        if (!int.TryParse(Console.ReadLine(), out var amount) || amount <= 0)
        {
            Console.WriteLine("Invalid amount.");
            return;
        }

        try
        {
            // Get wallet data
            var walletData = await $"{_blockchainServer}/wallets/{_myWalletPublicKey}"
                .GetJsonAsync<WalletResponse>();

            var balance = walletData.Balance;
            var fee = walletData.Fee;
            var utxos = walletData.Utxo;

            if (balance < amount + fee)
            {
                Console.WriteLine("Insufficient balance (tx + fee).");
                return;
            }

            // Build inputs from UTXOs
            var txInputs = utxos
                .Select(TransactionInput.FromTxo)
                .ToList();

            txInputs.ForEach(input => input.Sign(_myWalletPrivateKey));

            // Build outputs
            var txOutputs = new List<TransactionOutput>
            {
                new TransactionOutput(toAddress: toWallet, amount: amount)
            };

            // Change
            var remaining = balance - amount - fee;
            if (remaining > 0)
            {
                txOutputs.Add(new TransactionOutput(
                    toAddress: _myWalletPublicKey,
                    amount: remaining
                ));
            }

            // Build transaction
            var tx = new Transaction(
                type: TransactionType.REGULAR,
                timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                txInputs: txInputs,
                txOutputs: txOutputs
            );

            Console.WriteLine("Send a transaction...");

            var response = await $"{_blockchainServer}/transactions/"
                .PostJsonAsync(tx);

            var result = await response.ResponseMessage.Content.ReadAsStringAsync();

            Console.WriteLine("Transaction accepted. Waiting for miners!");
            Console.WriteLine(result);
        }
        catch (FlurlHttpException ex)
        {
            var status = ex.Call?.Response?.StatusCode.ToString() ?? "No HTTP response";
            var error = await ex.GetResponseStringAsync();

            if (string.IsNullOrWhiteSpace(error))
                error = ex.Message;

            Console.WriteLine($"Error ({status}): {error}");
        }
    }

    private async Task SearchTransaction()
    {
        Console.Clear();

        Console.Write("Your tx hash: ");
        var hash = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(hash))
        {
            Console.WriteLine("Invalid hash.");
            return;
        }

        try
        {
            var response = await $"{_blockchainServer}/transactions/{hash}"
                .GetAsync();

            var result = await response.ResponseMessage.Content.ReadAsStringAsync();

            Console.WriteLine("Transaction found:");
            Console.WriteLine(result);
        }
        catch (FlurlHttpException ex)
        {
            var status = ex.Call?.Response?.StatusCode.ToString() ?? "No HTTP response";
            var error = await ex.GetResponseStringAsync();

            if (string.IsNullOrWhiteSpace(error))
                error = ex.Message;

            Console.WriteLine($"Error ({status}): {error}");
        }
    }
}

public class WalletResponse
{
    public int Balance { get; set; }
    public int Fee { get; set; }
    public List<TransactionOutput> Utxo { get; set; } = new();
}
