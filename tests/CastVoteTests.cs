using SnsbtGovernance.Tests.Fixture;
using SnsbtGovernance.Tests.Fixture.GnsbtGovernance;
using SnsbtGovernance.Tests.Fixture.Snsbt;
using SnsbtGovernance.Tests.Fixture.SnsbtGovernance;

namespace SnsbtGovernance.Tests;

public class CastVoteTests
{
    private readonly SnsbtAccount _snsbtAccount;
    private readonly GnsbtGovernanceAccount _gnsbtGovernanceAccount;
    private readonly SnsbtGovernanceAccount _snsbtGovernanceAccount;

    public CastVoteTests()
    {
        _snsbtAccount = new SnsbtAccount();
        _gnsbtGovernanceAccount = new GnsbtGovernanceAccount();
        _snsbtGovernanceAccount = new SnsbtGovernanceAccount(_snsbtAccount, _gnsbtGovernanceAccount.PrivateKey.GetAddress());
    }

    [Fact]
    public void Invoke_FromAdminAccount_ThrowException()
    {
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

        var castVoteTransaction = InvokeScriptTransactionBuilder
            .Params(_snsbtGovernanceAccount.Address, new List<Amount>(), new Call
            {
                Function = "castVote", Args = new List<CallArg>
                {
                    new() { Type = CallArgType.Integer, Value = 8L },
                    new() { Type = CallArgType.Integer, Value = 1L },
                }
            })
            .SetFee(0_00900000)
            .GetUnsigned();

        var invoke = () => castVoteTransaction.GetSignedWith(_snsbtGovernanceAccount.PrivateKey, _snsbtAccount.PrivateKey).BroadcastAndWait(PrivateNode.Instance);

        invoke.Should().Throw<Exception>().WithMessage("*Access denied");
    }

    [Fact]
    public void Invoke_WithPayment_ThrowException()
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

        var invoke = () => _snsbtGovernanceAccount.CastVote(account, 8, 1, new List<Amount> { new() { Value = 1_00000000 } });

        invoke.Should().Throw<Exception>().WithMessage("*Payments are prohibited");
    }

    [Fact]
    public void Invoke_NotDeposited_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();

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

        var invoke = () => _snsbtGovernanceAccount.CastVote(account, 8, 1);

        invoke.Should().Throw<Exception>().WithMessage($"*Key '%s%s__deposit__{account.GetAddress()}' is not exist");
    }

    [Fact]
    public void Invoke_VotingIsCanceled_ThrowException()
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
                Value = "%b%d%d%d%b%d%b__false__0__0__0__false__0__true",
            },
            new StringEntry
            {
                Key = "%s%d__proposalData__8",
                Value = $"%s%s%s%s%s%d%d%d%s%d%s__6iPF8FLp2X5jSCn74U6jrtHEPKAnvMnNFfbzzS7eUJEj__IDEA__3P88qk1KzF1BKjD7fC7LjNVAKM4ezff5WE6__" +
                        $"368La1qZAv72FseGrkQA8Vh65Yb9XUHgN5yKobbjUUN9jB7XEK7dRE1siDsSZFHtvocKoF1Pwz89h7q6nSkvwtja__5c3z79TsQa8vka25CQoPd6B8RzrKFTSdvPPXdenH7EdQ__1671819140106__{start}__{end}____" +
                        $"1434237513036__NO:YES",
            },
        });

        var invoke = () => _snsbtGovernanceAccount.CastVote(account, 8, 1);

        invoke.Should().Throw<Exception>().WithMessage("*Voting is canceled by team");
    }

    [Fact]
    public void Invoke_VotingNotStarted_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 1_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        var start = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds();
        var end = DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds();

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

        var invoke = () => _snsbtGovernanceAccount.CastVote(account, 8, 1);

        invoke.Should().Throw<Exception>().WithMessage("*Voting not started yet");
    }

    [Fact]
    public void Invoke_VotingFinished_ThrowException()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 1_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        var start = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeMilliseconds();
        var end = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds();

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

        var invoke = () => _snsbtGovernanceAccount.CastVote(account, 8, 1);

        invoke.Should().Throw<Exception>().WithMessage("*Voting already finished");
    }

    [Fact]
    public void Invoke_NotEnoughChoices_ThrowException()
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
                        $"1434237513036__YES",
            },
        });

        var invoke = () => _snsbtGovernanceAccount.CastVote(account, 8, 1);

        invoke.Should().Throw<Exception>().WithMessage("*Too few choices to vote");
    }

    [Fact]
    public void Invoke_UnknownChoice_ThrowException()
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

        var invoke = () => _snsbtGovernanceAccount.CastVote(account, 8, 3);

        invoke.Should().Throw<Exception>().WithMessage("*Unknown choice! Must be 0..2");
    }

    [Fact]
    public void Invoke_FirstVote_Success()
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

        var transactionId = _snsbtGovernanceAccount.CastVote(account, 8, 1);

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
                new IntegerEntry
                {
                    Key = $"%s%d%s__votesByUser__8__{account.GetAddress()}",
                    Value = 1_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__optionByUser__8__{account.GetAddress()}",
                    Value = 1,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__releaseTime__{account.GetAddress()}",
                    Value = end,
                },
                new IntegerEntry
                {
                    Key = "%s%d%d__votesByOption__8__1",
                    Value = 1_000000,
                },
            }, options => options.ComparingByValue<EntryData>());
        }
    }

    [Fact]
    public void Invoke_AbstainVote_Success()
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

        var transactionId = _snsbtGovernanceAccount.CastVote(account, 8, 2);

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
                new IntegerEntry
                {
                    Key = $"%s%d%s__votesByUser__8__{account.GetAddress()}",
                    Value = 1_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__optionByUser__8__{account.GetAddress()}",
                    Value = 2,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__releaseTime__{account.GetAddress()}",
                    Value = end,
                },
                new IntegerEntry
                {
                    Key = "%s%d%d__votesByOption__8__2",
                    Value = 1_000000,
                },
            }, options => options.ComparingByValue<EntryData>());
        }
    }

    [Fact]
    public void Invoke_SecondVoteAnotherOption_Success()
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

        var transactionId = _snsbtGovernanceAccount.CastVote(account, 8, 0);

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
                new IntegerEntry
                {
                    Key = $"%s%d%s__votesByUser__8__{account.GetAddress()}",
                    Value = 1_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__optionByUser__8__{account.GetAddress()}",
                    Value = 0,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__releaseTime__{account.GetAddress()}",
                    Value = end,
                },
                new IntegerEntry
                {
                    Key = "%s%d%d__votesByOption__8__0",
                    Value = 1_000000,
                },
                new IntegerEntry
                {
                    Key = "%s%d%d__votesByOption__8__1",
                    Value = 0,
                },
            }, options => options.ComparingByValue<EntryData>());
        }
    }

    [Fact]
    public void Invoke_SecondVoteSameOptionDepositedMore_Success()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 3_000000);
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
        _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 2_000000 } });

        var transactionId = _snsbtGovernanceAccount.CastVote(account, 8, 1);

        using (new AssertionScope())
        {
            transactionId.Should().NotBeEmpty();

            PrivateNode.Instance.GetData(_snsbtGovernanceAccount.Address).Should().BeEquivalentTo(new List<EntryData>
            {
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account.GetAddress()}",
                    Value = 3_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__votesByUser__8__{account.GetAddress()}",
                    Value = 3_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__optionByUser__8__{account.GetAddress()}",
                    Value = 1,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__releaseTime__{account.GetAddress()}",
                    Value = end,
                },
                new IntegerEntry
                {
                    Key = "%s%d%d__votesByOption__8__1",
                    Value = 3_000000,
                },
            }, options => options.ComparingByValue<EntryData>());
        }
    }

    [Fact]
    public void Invoke_SecondVoteAnotherOptionDepositedMore_Success()
    {
        var account = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account, 3_000000);
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
        _snsbtGovernanceAccount.InvokeDeposit(account, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 2_000000 } });

        var transactionId = _snsbtGovernanceAccount.CastVote(account, 8, 0);

        using (new AssertionScope())
        {
            transactionId.Should().NotBeEmpty();

            PrivateNode.Instance.GetData(_snsbtGovernanceAccount.Address).Should().BeEquivalentTo(new List<EntryData>
            {
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account.GetAddress()}",
                    Value = 3_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__votesByUser__8__{account.GetAddress()}",
                    Value = 3_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__optionByUser__8__{account.GetAddress()}",
                    Value = 0,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__releaseTime__{account.GetAddress()}",
                    Value = end,
                },
                new IntegerEntry
                {
                    Key = "%s%d%d__votesByOption__8__0",
                    Value = 3_000000,
                },
                new IntegerEntry
                {
                    Key = "%s%d%d__votesByOption__8__1",
                    Value = 0,
                },
            }, options => options.ComparingByValue<EntryData>());
        }
    }

    [Fact]
    public void Invoke_ThreeVoters_Success()
    {
        var account1 = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account1, 1_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account1, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 1_000000 } });

        var account2 = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account2, 3_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account2, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 3_000000 } });

        var account3 = PrivateNode.GenerateAccount();
        _snsbtAccount.FaucetSnsbt(account3, 4_000000);
        _snsbtGovernanceAccount.InvokeDeposit(account3, new List<Amount> { new() { AssetId = _snsbtAccount.SnsbtId, Value = 4_000000 } });

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

        var transaction1Id = _snsbtGovernanceAccount.CastVote(account1, 8, 0);
        var transaction2Id = _snsbtGovernanceAccount.CastVote(account2, 8, 1);
        var transaction3Id = _snsbtGovernanceAccount.CastVote(account3, 8, 1);

        using (new AssertionScope())
        {
            transaction1Id.Should().NotBeEmpty();
            transaction2Id.Should().NotBeEmpty();
            transaction3Id.Should().NotBeEmpty();

            PrivateNode.Instance.GetData(_snsbtGovernanceAccount.Address).Should().BeEquivalentTo(new List<EntryData>
            {
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account1.GetAddress()}",
                    Value = 1_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__votesByUser__8__{account1.GetAddress()}",
                    Value = 1_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__optionByUser__8__{account1.GetAddress()}",
                    Value = 0,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__releaseTime__{account1.GetAddress()}",
                    Value = end,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account2.GetAddress()}",
                    Value = 3_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__votesByUser__8__{account2.GetAddress()}",
                    Value = 3_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__optionByUser__8__{account2.GetAddress()}",
                    Value = 1,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__releaseTime__{account2.GetAddress()}",
                    Value = end,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__deposit__{account3.GetAddress()}",
                    Value = 4_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__votesByUser__8__{account3.GetAddress()}",
                    Value = 4_000000,
                },
                new IntegerEntry
                {
                    Key = $"%s%d%s__optionByUser__8__{account3.GetAddress()}",
                    Value = 1,
                },
                new IntegerEntry
                {
                    Key = $"%s%s__releaseTime__{account3.GetAddress()}",
                    Value = end,
                },
                new IntegerEntry
                {
                    Key = "%s%d%d__votesByOption__8__0",
                    Value = 1_000000,
                },
                new IntegerEntry
                {
                    Key = "%s%d%d__votesByOption__8__1",
                    Value = 7_000000,
                },
            }, options => options.ComparingByValue<EntryData>());
        }
    }
}