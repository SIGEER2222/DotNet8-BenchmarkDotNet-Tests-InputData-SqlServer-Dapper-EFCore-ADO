using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkingDapperEFCoreCRM.EFCore;
using BenchmarkingDapperEFCoreCRM.Tests;

public class Program
{
    public static void Main(string[] args)
    {
        var db = new CRMContext();
        db.BulkDelete(db.Contatos);
        db.BulkDelete(db.Empresas);
        var config = DefaultConfig.Instance
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        new BenchmarkSwitcher(new[] { typeof(CRMTests) }).Run(args, config);
    }
}