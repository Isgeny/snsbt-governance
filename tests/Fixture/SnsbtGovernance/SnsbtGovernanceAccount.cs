namespace SnsbtGovernance.Tests.Fixture.SnsbtGovernance;

public class SnsbtGovernanceAccount
{
    private const string ScriptPath = "../../../../scripts/snsbt-governance.ride";

    public SnsbtGovernanceAccount(AssetId snsbtId, string gnsbtGovernanceAddress)
    {
        PrivateKey = PrivateNode.GenerateAccount();

        var scriptText = File.ReadAllText(ScriptPath)
            .Replace("8wUmN9Y15f3JR4KZfE81XLXpkdgwnqoBNG6NmocZpKQx", snsbtId)
            .Replace("3PMoqtw9NCk1JDrNq24Pji6xqtuG3PYRy8m", gnsbtGovernanceAddress);

        var scriptInfo = PrivateNode.Instance.CompileScript(scriptText);

        SetScriptTransactionBuilder
            .Params(scriptInfo.Script!)
            .GetSignedWith(PrivateKey)
            .BroadcastAndWait(PrivateNode.Instance);
    }

    public PrivateKey PrivateKey { get; }

    public Address Address => PrivateKey.GetAddress();

    public void SetData(ICollection<EntryData> entries) => DataTransactionBuilder.Params(entries)
        .GetSignedWith(PrivateKey)
        .BroadcastAndWait(PrivateNode.Instance);

    public string InvokeDeposit(PrivateKey callerAccount, ICollection<Amount> payment) => InvokeScriptTransactionBuilder
        .Params(Address, payment, new Call { Function = "deposit" })
        .GetSignedWith(callerAccount)
        .BroadcastAndWait(PrivateNode.Instance)
        .Transaction.Id!;

    public string CastVote(PrivateKey callerAccount, long proposalId, long option, ICollection<Amount>? payment = null) => InvokeScriptTransactionBuilder
        .Params(Address, payment ?? new List<Amount>(), new Call
        {
            Function = "castVote", Args = new List<CallArg>
            {
                new() { Type = CallArgType.Integer, Value = proposalId },
                new() { Type = CallArgType.Integer, Value = option },
            }
        })
        .GetSignedWith(callerAccount)
        .BroadcastAndWait(PrivateNode.Instance)
        .Transaction.Id!;

    public string InvokeWithdraw(PrivateKey callerAccount, ICollection<Amount>? payment = null) => InvokeScriptTransactionBuilder
        .Params(Address, payment ?? new List<Amount>(), new Call { Function = "withdraw" })
        .GetSignedWith(callerAccount)
        .BroadcastAndWait(PrivateNode.Instance)
        .Transaction.Id!;
}