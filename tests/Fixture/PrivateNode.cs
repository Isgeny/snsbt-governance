﻿namespace SnsbtGovernance.Tests.Fixture;

public static class PrivateNode
{
    private static readonly PrivateKey MainAccount;

    public const byte ChainId = (byte)'R';
    public static readonly Node Instance;

    static PrivateNode()
    {
        MainAccount = PrivateKey.FromSeed("waves private node seed with waves tokens");
        Instance = new Node("http://localhost:6869");
        try
        {
            Instance.GetHeight();
        }
        catch (Exception)
        {
            throw new Exception("Run the following docker image: https://github.com/wavesplatform/Waves/tree/HEAD/docker");
        }
    }

    public static PrivateKey GenerateAccount()
    {
        var seed = Crypto.GenerateRandomSeedPhrase();
        var privateKey = PrivateKey.FromSeed(seed);

        TransferTransactionBuilder
            .Params(privateKey.GetAddress(), 100_00000000)
            .GetSignedWith(MainAccount)
            .BroadcastAndWait(Instance);

        return privateKey;
    }

    public static Address GetAddress(this PrivateKey privateKey) => Address.FromPublicKey(ChainId, privateKey.PublicKey);

    public static TransactionInfo BroadcastAndWait(this Transaction transaction, Node node) => node.WaitForTransaction(node.Broadcast(transaction).Id);

    public static Transaction GetSignedWith(this Transaction transaction, PrivateKey privateKey1, PrivateKey privateKey2)
    {
        transaction.SenderPublicKey = privateKey1.PublicKey;
        
        var transactionBinarySerializerFactory = new TransactionBinarySerializerFactory();
        var bodyBytes = transactionBinarySerializerFactory.GetFor(transaction).Serialize(transaction);
        transaction.Id = Base58s.As(Crypto.CalculateBlake2bHash(bodyBytes));
        transaction.Proofs.Add(new Base58s(privateKey1.Sign(bodyBytes)));
        transaction.Proofs.Add(new Base58s(privateKey2.Sign(bodyBytes)));
        return transaction;
    }
}