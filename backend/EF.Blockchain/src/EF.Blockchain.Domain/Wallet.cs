using NBitcoin;

namespace EF.Blockchain.Domain;

public class Wallet
{
    public string PrivateKey { get; private set; }
    public string PublicKey { get; private set; }

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
