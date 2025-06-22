namespace EF.Blockchain.Client.Wallet;

public class WalletApp
{
    private string? _myWalletPublicKey;
    private string? _myWalletPrivateKey;

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
                    GetBalance();
                    break;

                case "4":
                    SendTransaction();
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

    private void GetBalance()
    {
        Console.Clear();

        if (string.IsNullOrEmpty(_myWalletPublicKey))
        {
            Console.WriteLine("You don't have a wallet yet.");
            return;
        }

        Console.WriteLine("(TODO) Get balance via API");
    }

    private void SendTransaction()
    {
        Console.Clear();

        if (string.IsNullOrEmpty(_myWalletPublicKey))
        {
            Console.WriteLine("You don't have a wallet yet.");
            return;
        }

        Console.WriteLine("(TODO) Send transaction via API");
    }
}
