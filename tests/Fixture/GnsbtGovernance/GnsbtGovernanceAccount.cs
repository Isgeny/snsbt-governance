namespace SnsbtGovernance.Tests.Fixture.GnsbtGovernance;

public class GnsbtGovernanceAccount
{
    public GnsbtGovernanceAccount()
    {
        PrivateKey = PrivateNode.GenerateAccount();
    }

    public PrivateKey PrivateKey { get; }

    public void SetData(ICollection<EntryData> entries) => DataTransactionBuilder.Params(entries)
        .GetSignedWith(PrivateKey)
        .BroadcastAndWait(PrivateNode.Instance);
}