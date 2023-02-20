using SnsbtGovernance.Tests.Fixture;
using SnsbtGovernance.Tests.Fixture.GnsbtGovernance;
using SnsbtGovernance.Tests.Fixture.Snsbt;
using SnsbtGovernance.Tests.Fixture.SnsbtGovernance;

namespace SnsbtGovernance.Tests;

public class VerifierTests
{
    private readonly SnsbtAccount _snsbtAccount;
    private readonly SnsbtGovernanceAccount _snsbtGovernanceAccount;

    public VerifierTests()
    {
        _snsbtAccount = new SnsbtAccount();
        var gnsbtGovernanceAccount = new GnsbtGovernanceAccount();
        _snsbtGovernanceAccount = new SnsbtGovernanceAccount(_snsbtAccount, gnsbtGovernanceAccount.PrivateKey.GetAddress());
    }

    [Fact]
    public void OutgoingTransactionBySingleProof_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();

        Transaction faucetTransaction = InvokeScriptTransactionBuilder
            .Params(_snsbtAccount.PrivateKey.GetAddress(), new Call { Function = "faucet", Args = new List<CallArg> { new() { Type = CallArgType.Integer, Value = 1_000000L } } })
            .SetFee(0_00900000L)
            .GetUnsigned();

        faucetTransaction.GetSignedWith(_snsbtGovernanceAccount.PrivateKey, _snsbtAccount.PrivateKey).BroadcastAndWait(PrivateNode.Instance);

        var transaction = TransferTransactionBuilder
            .Params(account.GetAddress(), 1_000000L, _snsbtAccount.SnsbtId)
            .SetFee(0_00900000L)
            .GetSignedWith(_snsbtGovernanceAccount.PrivateKey);

        var invoke = () => transaction.BroadcastAndWait(PrivateNode.Instance);
        
        invoke.Should().Throw<Exception>().WithMessage("*Transaction is not allowed by account-script");
    }
}