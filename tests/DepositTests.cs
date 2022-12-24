using SnsbtGovernance.Tests.Fixture;
using SnsbtGovernance.Tests.Fixture.GnsbtGovernance;
using SnsbtGovernance.Tests.Fixture.Snsbt;
using SnsbtGovernance.Tests.Fixture.SnsbtGovernance;

namespace SnsbtGovernance.Tests;

public class DepositTests
{
    private readonly SnsbtAccount _snsbtAccount;
    private readonly SnsbtGovernanceAccount _snsbtGovernanceAccount;

    public DepositTests()
    {
        _snsbtAccount = new SnsbtAccount();
        var gnsbtGovernanceAccount = new GnsbtGovernanceAccount();
        _snsbtGovernanceAccount = new SnsbtGovernanceAccount(_snsbtAccount.SnsbtId, gnsbtGovernanceAccount.PrivateKey.GetAddress());
    }

    [Fact]
    public void Invoke_FromAdminAccount_ThrowException()
    {
        _snsbtAccount.FaucetSnsbt(_snsbtGovernanceAccount.PrivateKey, 1_000000);

        var invoke = () => _snsbtGovernanceAccount.InvokeDeposit(_snsbtGovernanceAccount.PrivateKey, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        invoke.Should().Throw<Exception>().WithMessage("*Access denied");
    }

    [Fact]
    public void Invoke_WithoutPayments_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();

        var invoke = () => _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount>());

        invoke.Should().Throw<Exception>().WithMessage("*Only one sNSBT payment is allowed");
    }

    [Fact]
    public void Invoke_WithWrongPayment_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();

        var invoke = () => _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { Value = 1_00000000 } });

        invoke.Should().Throw<Exception>().WithMessage("*Only sNSBT allowed");
    }

    [Fact]
    public void Invoke_WithMultiplePayments_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 1_000000);

        var invoke = () => _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount>
        {
            new() { Value = 1_00000000 },
            new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 }
        });

        invoke.Should().Throw<Exception>().WithMessage("*Only one sNSBT payment is allowed");
    }

    [Fact]
    public void Invoke_SingleDeposit_Success()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 1_000000);

        var transactionId = _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        using (new AssertionScope())
        {
            transactionId.Should().NotBeEmpty();

            PrivateNode.Instance.GetData(_snsbtGovernanceAccount.Address).Should().BeEquivalentTo(new List<EntryData>
            {
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account.GetAddress()}",
                    Value = 1_000000,
                },
            });
        }
    }

    [Fact]
    public void Invoke_DepositTwice_Success()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 1_000000);

        var transaction1Id = _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 700000 } });
        var transaction2Id = _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 100000 } });

        using (new AssertionScope())
        {
            transaction1Id.Should().NotBeEmpty();
            transaction2Id.Should().NotBeEmpty();

            PrivateNode.Instance.GetData(_snsbtGovernanceAccount.Address).Should().BeEquivalentTo(new List<EntryData>
            {
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account.GetAddress()}",
                    Value = 800000,
                },
            });
        }
    }

    [Fact]
    public void Invoke_DepositFromTwoAccounts_Success()
    {
        var account1 = PrivateNode.GenerateAccount();
        var account2 = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account1, 1_000000);
        _snsbtAccount.FaucetSnsbt(account2, 3_000000);

        var transaction1Id = _snsbtGovernanceAccount.InvokeDeposit(account1, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });
        var transaction2Id = _snsbtGovernanceAccount.InvokeDeposit(account2, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 2_900000 } });

        using (new AssertionScope())
        {
            transaction1Id.Should().NotBeEmpty();
            transaction2Id.Should().NotBeEmpty();

            PrivateNode.Instance.GetData(_snsbtGovernanceAccount.Address).Should().BeEquivalentTo(new List<EntryData>
            {
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account1.GetAddress()}",
                    Value = 1_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account2.GetAddress()}",
                    Value = 2_900000,
                },
            });
        }
    }
}