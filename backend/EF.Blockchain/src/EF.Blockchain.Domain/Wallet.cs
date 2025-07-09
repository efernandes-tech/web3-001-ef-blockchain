using NBitcoin;

namespace EF.Blockchain.Domain;

/// <summary>
/// Represents a wallet with a public and private key used to sign and verify blockchain transactions.
/// </summary>
public class Wallet
{
    /// <summary>
    /// The private key in hexadecimal format. Used to sign transactions.
    /// </summary>
    public string PrivateKey { get; private set; }

    /// <summary>
    /// The public key in hexadecimal format. Used to verify ownership and generate the wallet address.
    /// </summary>
    public string PublicKey { get; private set; }

    /// <summary>
    /// Initializes a new wallet.
    /// Can use a raw private key (64 hex chars), a WIF string, or generate a new key if none is provided.
    /// </summary>
    /// <param name="wifOrPrivateKey">
    /// Optional string representing a raw private key (64 hex characters) or a WIF-encoded private key.
    /// If null, a new random key is generated.
    /// </param>
    public Wallet(string? wifOrPrivateKey = null)
    {
        Key key;

        if (!string.IsNullOrEmpty(wifOrPrivateKey))
        {
            if (wifOrPrivateKey.Length == 64)
            {
                // From raw private key hex
                var bytes = Convert.FromHexString(wifOrPrivateKey);
                key = new Key(bytes);
            }
            else
            {
                // From WIF
                key = Key.Parse(wifOrPrivateKey, Network.Main);
            }
        }
        else
        {
            // Random new key
            key = new Key();
        }

        PrivateKey = key.ToHex();
        PublicKey = key.PubKey.ToHex();
    }
}
