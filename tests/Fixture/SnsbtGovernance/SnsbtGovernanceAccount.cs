namespace SnsbtGovernance.Tests.Fixture.SnsbtGovernance;

public class SnsbtGovernanceAccount
{
    private const string ScriptPath = "../../../../scripts/snsbt-governance.ride";

    public SnsbtGovernanceAccount(AssetId snsbtId)
    {
        PrivateKey = PrivateNode.GenerateAccount();

        var scriptText = File.ReadAllText(ScriptPath)
            .Replace("8wUmN9Y15f3JR4KZfE81XLXpkdgwnqoBNG6NmocZpKQx", snsbtId);

        var scriptInfo = PrivateNode.Instance.CompileScript(scriptText);

        SetScriptTransactionBuilder
            .Params(scriptInfo.Script!)
            .GetSignedWith(PrivateKey)
            .BroadcastAndWait(PrivateNode.Instance);
    }

    public PrivateKey PrivateKey { get; }

    public Address Address => PrivateKey.GetAddress();

    public string InvokeDeposit(PrivateKey callerAccount, ICollection<Amount> payment) => InvokeScriptTransactionBuilder
        .Params(Address, payment, new Call { Function = "deposit" })
        .GetSignedWith(callerAccount)
        .BroadcastAndWait(PrivateNode.Instance)
        .Transaction.Id!;
}