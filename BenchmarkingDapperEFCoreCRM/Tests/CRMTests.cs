using BenchmarkDotNet.Attributes;
using BenchmarkingDapperEFCoreCRM.EFCore;
using Bogus.DataSets;
using Bogus.Extensions.Brazil;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BenchmarkingDapperEFCoreCRM.Tests;

[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput, launchCount: 5)]
public class CRMTests
{
    [Params(1_0000)]
    public int NumberOfRecords { get; set; }

    private const int NumeroContatosPorCompanhia = 1;

    private int GetNumeroContatosPorCompanhia()
    {
        var envNumeroContatosPorCompanhia =
            Environment.GetEnvironmentVariable("NumeroContatosPorCompanhia");
        if (!String.IsNullOrWhiteSpace(envNumeroContatosPorCompanhia) &&
            int.TryParse(envNumeroContatosPorCompanhia, out int result))
            return result;
        return NumeroContatosPorCompanhia;
    }

    #region SQLSugar Tests

    #endregion

    #region EFcore ext
    PooledDbContextFactory<CRMContext>? factory;

    [IterationSetup(Target = nameof(InputDataWithEntityFrameworkCoreExt))]
    public void SetupEntityFrameworkCoreExt()
    {
        var options = new DbContextOptionsBuilder<CRMContext>()
            .UseSqlite(Configurations.BaseEFCore)
            .Options;
        factory = new PooledDbContextFactory<CRMContext>(options);

        _namesDataSetEF = new Name("pt_BR");
        _phonesDataSetEF = new PhoneNumbers("pt_BR");
        _addressesDataSetEF = new Address("pt_BR");
        _companiesDataSetEF = new Company("pt_BR");
        _numeroContatosPorCompanhiaEF = GetNumeroContatosPorCompanhia();
    }

    [Benchmark]
    public void InputDataWithEntityFrameworkCoreExt()
    {
        for (int k = 0; k < NumberOfRecords / 100; k++)
        {
            using (var context = factory.CreateDbContext())
            {
                var lstData = new List<EFCore.Empresa>();

                for (int j = 0; j < 100; j++)
                {
                    var empresa = new EFCore.Empresa()
                    {
                        Nome = _companiesDataSetEF!.CompanyName(),
                        CNPJ = _companiesDataSetEF!.Cnpj(includeFormatSymbols: false),
                        Cidade = _addressesDataSetEF!.City(),
                        Contatos = new List<EFCore.Contato>()
                    };
                    for (int i = 0; i < _numeroContatosPorCompanhiaEF; i++)
                    {
                        empresa.Contatos.Add(new EFCore.Contato()
                        {
                            Nome = _namesDataSetEF!.FullName(),
                            Telefone = _phonesDataSetEF!.PhoneNumber()
                        });
                    }
                    lstData.Add(empresa);
                }
                context.BulkInsert(lstData);
            }

        }
    }

    #endregion

    #region EFCore Tests

    private Name? _namesDataSetEF;
    private PhoneNumbers? _phonesDataSetEF;
    private Address? _addressesDataSetEF;
    private Company? _companiesDataSetEF;
    private int _numeroContatosPorCompanhiaEF;


    [IterationSetup(Target = nameof(InputDataWithEntityFrameworkCore))]
    public void SetupEntityFrameworkCore()
    {
        var options = new DbContextOptionsBuilder<CRMContext>()
           .UseSqlite(Configurations.BaseEFCore)
           .Options;
        factory = new PooledDbContextFactory<CRMContext>(options);
        _namesDataSetEF = new Name("pt_BR");
        _phonesDataSetEF = new PhoneNumbers("pt_BR");
        _addressesDataSetEF = new Address("pt_BR");
        _companiesDataSetEF = new Company("pt_BR");
        _numeroContatosPorCompanhiaEF = GetNumeroContatosPorCompanhia();
    }

    [Benchmark]
    public void InputDataWithEntityFrameworkCore()
    {
        for (int i = 0; i < NumberOfRecords; i++)
        {
            using (var context = factory.CreateDbContext())
            {
                var empresa = new EFCore.Empresa()
                {
                    Nome = _companiesDataSetEF!.CompanyName(),
                    CNPJ = _companiesDataSetEF!.Cnpj(includeFormatSymbols: false),
                    Cidade = _addressesDataSetEF!.City(),
                    Contatos = new List<EFCore.Contato>()
                };
                for (int j = 0; j < _numeroContatosPorCompanhiaEF; j++)
                {
                    empresa.Contatos.Add(new EFCore.Contato()
                    {
                        Nome = _namesDataSetEF!.FullName(),
                        Telefone = _phonesDataSetEF!.PhoneNumber()
                    });
                }
                context.SaveChanges();
            }
        }
    }

    [IterationCleanup(Target = nameof(InputDataWithEntityFrameworkCore))]
    public void CleanupEntityFrameworkCore()
    {
        factory = null;
    }

    #endregion

    #region Dapper Tests

    private SqliteConnection? _connectionDapper;
    private Name? _namesDataSetDapper;
    private PhoneNumbers? _phonesDataSetDapper;
    private Address? _addressesDataSetDapper;
    private Company? _companiesDataSetDapper;
    private int _numeroContatosPorCompanhiaDapper;

    [IterationSetup(Target = nameof(InputDataWithDapper))]
    public void SetupDapper()
    {
        _connectionDapper = new SqliteConnection(Configurations.BaseDapper);
        _namesDataSetDapper = new Name("pt_BR");
        _phonesDataSetDapper = new PhoneNumbers("pt_BR");
        _addressesDataSetDapper = new Address("pt_BR");
        _companiesDataSetDapper = new Company("pt_BR");
        _numeroContatosPorCompanhiaDapper = GetNumeroContatosPorCompanhia();
    }

    [Benchmark]
    public Dapper.Empresa InputDataWithDapper()
    {
        var empresa = new Dapper.Empresa()
        {
            Nome = _companiesDataSetDapper!.CompanyName(),
            CNPJ = _companiesDataSetDapper!.Cnpj(includeFormatSymbols: false),
            Cidade = _addressesDataSetDapper!.City()
        };

        _connectionDapper!.Open();
        var transaction = _connectionDapper.BeginTransaction();

        empresa.IdEmpresa = _connectionDapper.QuerySingle<int>(
      "INSERT INTO Empresas (CNPJ, Nome, Cidade) " +
      "VALUES (@CNPJ, @Nome, @Cidade);" + Environment.NewLine +
      "SELECT last_insert_rowid()", empresa, transaction);
        empresa.Contatos = new();
        for (int i = 0; i < _numeroContatosPorCompanhiaDapper; i++)
        {
            var contato = new Dapper.Contato()
            {
                IdEmpresa = empresa.IdEmpresa,
                Nome = _namesDataSetDapper!.FullName(),
                Telefone = _phonesDataSetDapper!.PhoneNumber()
            };
            contato.IdContato = _connectionDapper.QuerySingle<int>(
     "INSERT INTO Contatos (Nome, Telefone, IdEmpresa) " +
     "VALUES (@Nome, @Telefone, @IdEmpresa);" + Environment.NewLine +
     "SELECT last_insert_rowid()", contato, transaction);
            empresa.Contatos.Add(contato);
        }

        transaction.Commit();
        _connectionDapper.Close();

        return empresa;
    }

    [IterationCleanup(Target = nameof(InputDataWithDapper))]
    public void CleanupDapper()
    {
        _connectionDapper = null;
    }

    #endregion

    #region Dapper.Contrib Tests

    private SqliteConnection? _connectionDapperContrib;
    private Name? _namesDataSetDapperContrib;
    private PhoneNumbers? _phonesDataSetDapperContrib;
    private Address? _addressesDataSetDapperContrib;
    private Company? _companiesDataSetDapperContrib;
    private int _numeroContatosPorCompanhiaDapperContrib;

    [IterationSetup(Target = nameof(InputDataWithDapperContrib))]
    public void SetupDapperContrib()
    {
        _connectionDapperContrib = new SqliteConnection(Configurations.BaseDapperContrib);
        _namesDataSetDapperContrib = new Name("pt_BR");
        _phonesDataSetDapperContrib = new PhoneNumbers("pt_BR");
        _addressesDataSetDapperContrib = new Address("pt_BR");
        _companiesDataSetDapperContrib = new Company("pt_BR");
        _numeroContatosPorCompanhiaDapperContrib = GetNumeroContatosPorCompanhia();
    }

    [Benchmark]
    public void InputDataWithDapperContrib()
    {
        _connectionDapperContrib!.Open();
        var transaction = _connectionDapperContrib.BeginTransaction();

        for (int j = 0; j < NumberOfRecords; j++)
        {
            var empresa = new Dapper.Empresa()
            {
                Nome = _companiesDataSetDapperContrib!.CompanyName(),
                CNPJ = _companiesDataSetDapperContrib!.Cnpj(includeFormatSymbols: false),
                Cidade = _addressesDataSetDapperContrib!.City(),
                Contatos = new List<Dapper.Contato>()
            };

            _connectionDapperContrib.Insert<Dapper.Empresa>(empresa, transaction);

            for (int i = 0; i < _numeroContatosPorCompanhiaDapperContrib; i++)
            {
                var contato = new Dapper.Contato()
                {
                    IdEmpresa = empresa.IdEmpresa,
                    Nome = _namesDataSetDapperContrib!.FullName(),
                    Telefone = _phonesDataSetDapperContrib!.PhoneNumber()
                };
                _connectionDapperContrib.Insert<Dapper.Contato>(contato, transaction);
                empresa.Contatos.Add(contato);
            }
        }

        transaction.Commit();
        _connectionDapperContrib.Close();
    }

    [IterationCleanup(Target = nameof(InputDataWithDapperContrib))]
    public void CleanupDapperContrib()
    {
        _connectionDapperContrib = null;
    }

    #endregion

    #region ADO.NET Tests

    private SqliteConnection? _connectionADO;
    private Name? _namesDataSetADO;
    private PhoneNumbers? _phonesDataSetADO;
    private Address? _addressesDataSetADO;
    private Company? _companiesDataSetADO;
    private int _numeroContatosPorCompanhiaADO;

    [IterationSetup(Target = nameof(InputDataWithADO))]
    public void SetupADO()
    {
        _connectionADO = new SqliteConnection(Configurations.BaseADO);
        _namesDataSetADO = new Name("pt_BR");
        _phonesDataSetADO = new PhoneNumbers("pt_BR");
        _addressesDataSetADO = new Address("pt_BR");
        _companiesDataSetADO = new Company("pt_BR");
        _numeroContatosPorCompanhiaADO = GetNumeroContatosPorCompanhia();
    }

    [Benchmark]
    public void InputDataWithADO()
    {
        _connectionADO!.Open();
        var transaction = _connectionADO.BeginTransaction();

        for (int j = 0; j < NumberOfRecords; j++)
        {
            var empresa = new Dapper.Empresa()
            {
                Nome = _companiesDataSetADO!.CompanyName(),
                CNPJ = _companiesDataSetADO!.Cnpj(includeFormatSymbols: false),
                Cidade = _addressesDataSetADO!.City()
            };

            using var commandInsertEmpresa = new SqliteCommand(
            "INSERT INTO Empresas (CNPJ, Nome, Cidade) VALUES (@CNPJ, @Nome, @Cidade); " +
            "SELECT last_insert_rowid();",
            _connectionADO, transaction);
            commandInsertEmpresa.Parameters.AddWithValue("@CNPJ", empresa.CNPJ);
            commandInsertEmpresa.Parameters.AddWithValue("@Nome", empresa.Nome);
            commandInsertEmpresa.Parameters.AddWithValue("@Cidade", empresa.Cidade);
            empresa.IdEmpresa = (int)(long)commandInsertEmpresa.ExecuteScalar();

            for (int i = 0; i < _numeroContatosPorCompanhiaADO; i++)
            {
                var contato = new Dapper.Contato()
                {
                    IdEmpresa = empresa.IdEmpresa,
                    Nome = _namesDataSetADO!.FullName(),
                    Telefone = _phonesDataSetADO!.PhoneNumber()
                };
                using var commandInsertContato = new SqliteCommand(
                "INSERT INTO Contatos (Nome, Telefone, IdEmpresa) VALUES (@Nome, @Telefone, @IdEmpresa); " +
                "SELECT last_insert_rowid();",
                _connectionADO, transaction);
                commandInsertContato.Parameters.AddWithValue("@Nome", contato.Nome);
                commandInsertContato.Parameters.AddWithValue("@Telefone", contato.Telefone);
                commandInsertContato.Parameters.AddWithValue("@IdEmpresa", contato.IdEmpresa);
                contato.IdContato = (int)(long)commandInsertContato.ExecuteScalar();
            }
        }
        transaction.Commit();
        _connectionADO.Close();
    }

    [IterationCleanup(Target = nameof(InputDataWithADO))]
    public void CleanupADO()
    {
        _connectionADO = null;
    }

    #endregion
}