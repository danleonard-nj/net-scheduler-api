namespace NetScheduler.Configuration.Attributes;

public class KeyVaultSecretAttribute : Attribute
{
    public string Secret { get; set; }

    public KeyVaultSecretAttribute(string secret)
    {
        Secret = secret;
    }
}
