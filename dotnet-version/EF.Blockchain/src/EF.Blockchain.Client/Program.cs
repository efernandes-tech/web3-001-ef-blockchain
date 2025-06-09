using EF.Blockchain.Client.Miner;

var miner = MinerAppFactory.Create();
await miner.RunAsync();
