namespace SnsbtGovernance.Tests.Fixture.Snsbt;

public class SnsbtAccount
{
    private const string ScriptPath = "../../../Fixture/Snsbt/snsbt.ride";

    public SnsbtAccount()
    {
        PrivateKey = PrivateNode.GenerateAccount();

        SnsbtId = AssetId.As(IssueTransactionBuilder
            .Params("sNSBT", 1_000000, 6)
            .GetSignedWith(PrivateKey)
            .BroadcastAndWait(PrivateNode.Instance)
            .Transaction.Id!.Encoded);

        var scriptText = File.ReadAllText(ScriptPath)
            .Replace("8wUmN9Y15f3JR4KZfE81XLXpkdgwnqoBNG6NmocZpKQx", SnsbtId);

        var scriptInfo = PrivateNode.Instance.CompileScript(scriptText);

        SetScriptTransactionBuilder
            .Params(scriptInfo.Script!)
            .GetSignedWith(PrivateKey)
            .BroadcastAndWait(PrivateNode.Instance);
    }

    public PrivateKey PrivateKey { get; }

    public AssetId SnsbtId { get; }

    public void FaucetSnsbt(PrivateKey caller, long quantity) => InvokeScriptTransactionBuilder
        .Params(PrivateKey.GetAddress(), new Call { Function = "faucet", Args = new List<CallArg> { new() { Type = CallArgType.Integer, Value = quantity } } })
        .GetSignedWith(caller)
        .BroadcastAndWait(PrivateNode.Instance);
}