using SnsbtGovernance.Tests.Fixture;
using SnsbtGovernance.Tests.Fixture.GnsbtGovernance;
using SnsbtGovernance.Tests.Fixture.Snsbt;
using SnsbtGovernance.Tests.Fixture.SnsbtGovernance;

namespace SnsbtGovernance.Tests;

public class WithdrawTests
{
    private readonly SnsbtAccount _snsbtAccount;
    private readonly GnsbtGovernanceAccount _gnsbtGovernanceAccount;
    private readonly SnsbtGovernanceAccount _snsbtGovernanceAccount;

    public WithdrawTests()
    {
        _snsbtAccount = new SnsbtAccount();
        _gnsbtGovernanceAccount = new GnsbtGovernanceAccount();
        _snsbtGovernanceAccount = new SnsbtGovernanceAccount(_snsbtAccount.SnsbtId, _gnsbtGovernanceAccount.PrivateKey.GetAddress());
    }

    [Fact]
    public void Invoke_FromAdminAccount_ThrowException()
    {
        _snsbtAccount.FaucetSnsbt(_snsbtGovernanceAccount.PrivateKey, 1_000000);

        var invoke = () => _snsbtGovernanceAccount.InvokeWithdraw(_snsbtGovernanceAccount.PrivateKey);

        invoke.Should().Throw<Exception>().WithMessage("*Access denied");
    }

    [Fact]
    public void Invoke_WithPayment_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();

        var invoke = () => _snsbtGovernanceAccount.InvokeWithdraw(account, new List<Amount> { new() { Value = 1_00000000 } });

        invoke.Should().Throw<Exception>().WithMessage("*Payments are prohibited");
    }

    [Fact]
    public void Invoke_NotDepositedBefore_OneAccount_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();

        var invoke = () => _snsbtGovernanceAccount.InvokeWithdraw(account);

        invoke.Should().Throw<Exception>().WithMessage($"*Key '%s%s__deposit__{account.GetAddress()}' is not exist");
    }

    [Fact]
    public void Invoke_NotDepositedBefore_TwoAccounts_ThrowException()
    {
        var account1 = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account1, 1_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account1, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        var account2 = PrivateNode.GenerateAccount();

        var invoke = () => _snsbtGovernanceAccount.InvokeWithdraw(account2);

        invoke.Should().Throw<Exception>().WithMessage($"*Key '%s%s__deposit__{account2.GetAddress()}' is not exist");
    }

    [Fact]
    public void Invoke_InVotingProcess_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 1_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        var start = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds();
        var end = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds();

        _gnsbtGovernanceAccount.SetData(new List<EntryData>
        {
            new StringEntry
            {
                Key = "%s%d__proposalStatusData__8",
                Value = "%b%d%d%d%b%d%b__false__0__0__0__false__0__false",
            },
            new StringEntry
            {
                Key = "%s%d__proposalData__8",
                Value = $"%s%s%s%s%s%d%d%d%s%d%s__6iPF8FLp2X5jSCn74U6jrtHEPKAnvMnNFfbzzS7eUJEj__IDEA__3P88qk1KzF1BKjD7fC7LjNVAKM4ezff5WE6__" +
                        $"368La1qZAv72FseGrkQA8Vh65Yb9XUHgN5yKobbjUUN9jB7XEK7dRE1siDsSZFHtvocKoF1Pwz89h7q6nSkvwtja__5c3z79TsQa8vka25CQoPd6B8RzrKFTSdvPPXdenH7EdQ__1671819140106__{start}__{end}____" +
                        $"1434237513036__NO:YES",
            },
        });

        _snsbtGovernanceAccount.CastVote(account, 8, 1);

        var invoke = () => _snsbtGovernanceAccount.InvokeWithdraw(account);

        invoke.Should().Throw<Exception>().WithMessage($"*Your sNSBT are taking part in voting, cannot unstake until {end}");
    }

    [Fact]
    public void Invoke_OneAccountDeposited_Success()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 1_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        var transactionId = _snsbtGovernanceAccount.InvokeWithdraw(account);

        using (new AssertionScope())
        {
            transactionId.Should().NotBeEmpty();

            PrivateNode.Instance.GetData(_snsbtGovernanceAccount.Address).Should().BeEmpty();
        }
    }

    [Fact]
    public void Invoke_TwoAccountsDeposited_Success()
    {
        var account1 = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account1, 3_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account1, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 2_900000 } });

        var account2 = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account2, 1_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account2, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        var transactionId = _snsbtGovernanceAccount.InvokeWithdraw(account2);

        using (new AssertionScope())
        {
            transactionId.Should().NotBeEmpty();

            PrivateNode.Instance.GetData(_snsbtGovernanceAccount.Address).Should().BeEquivalentTo(new List<EntryData>
            {
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account1.GetAddress()}",
                    Value = 2_900000,
                },
            });
        }
    }

    [Fact]
    public void Invoke_VotingIsOver_Success()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 1_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        var end = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds();

        _snsbtGovernanceAccount.SetData(new List<EntryData>
        {
            new IntegerEntry
            {
                Key = $"%s%s__releaseTime__{account.GetAddress()}",
                Value = end,
            },
        });

        var transactionId = _snsbtGovernanceAccount.InvokeWithdraw(account);

        using (new AssertionScope())
        {
            transactionId.Should().NotBeEmpty();

            PrivateNode.Instance.GetData(_snsbtGovernanceAccount.Address).Should().BeEquivalentTo(new List<EntryData>
            {
                new IntegerEntry
                {
                    Key = $"%s%s__releaseTime__{account.GetAddress()}",
                    Value = end,
                },
            });
        }
    }
}