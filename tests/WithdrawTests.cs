using FluentAssertions.Execution;
using SnsbtGovernance.Tests.Fixture;
using SnsbtGovernance.Tests.Fixture.Snsbt;
using SnsbtGovernance.Tests.Fixture.SnsbtGovernance;

namespace SnsbtGovernance.Tests;

public class WithdrawTests
{
    private readonly SnsbtAccount _snsbtAccount;
    private readonly SnsbtGovernanceAccount _snsbtGovernanceAccount;

    public WithdrawTests()
    {
        _snsbtAccount = new SnsbtAccount();
        _snsbtGovernanceAccount = new SnsbtGovernanceAccount(_snsbtAccount.SnsbtId);
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

        invoke.Should().Throw<Exception>().WithMessage($"*Key '%s%s__{account.GetAddress()}__deposit' is not exist");
    }

    [Fact]
    public void Invoke_NotDepositedBefore_TwoAccounts_ThrowException()
    {
        var account1 = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account1, 1_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account1, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        var account2 = PrivateNode.GenerateAccount();

        var invoke = () => _snsbtGovernanceAccount.InvokeWithdraw(account2);

        invoke.Should().Throw<Exception>().WithMessage($"*Key '%s%s__{account2.GetAddress()}__deposit' is not exist");
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
                    Key = $"%s%s__{account1.GetAddress()}__deposit",
                    Value = 2_900000,
                },
            });
        }
    }
}