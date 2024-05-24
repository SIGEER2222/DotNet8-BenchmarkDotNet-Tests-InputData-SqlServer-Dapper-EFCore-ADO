$env:NumeroContatosPorCompanhia = "1"
$env:BaseSQLSugarConnectionString = "Data Source=D:\桌面\DotNet8-BenchmarkDotNet-Tests-InputData-SqlServer-Dapper-EFCore-ADO\BenchmarkingDapperEFCoreCRM\DB\BaiAn.db;"
$env:BaseEFCoreConnectionString = "Data Source=D:\桌面\DotNet8-BenchmarkDotNet-Tests-InputData-SqlServer-Dapper-EFCore-ADO\BenchmarkingDapperEFCoreCRM\DB\BaiAn.db;"
$env:BaseDapperConnectionString = "Data Source=D:\桌面\DotNet8-BenchmarkDotNet-Tests-InputData-SqlServer-Dapper-EFCore-ADO\BenchmarkingDapperEFCoreCRM\DB\BaiAn.db;"
$env:BaseDapperContribConnectionString = "Data Source=D:\桌面\DotNet8-BenchmarkDotNet-Tests-InputData-SqlServer-Dapper-EFCore-ADO\BenchmarkingDapperEFCoreCRM\DB\BaiAn.db;"
$env:BaseADOConnectionString = "Data Source=D:\桌面\DotNet8-BenchmarkDotNet-Tests-InputData-SqlServer-Dapper-EFCore-ADO\BenchmarkingDapperEFCoreCRM\DB\BaiAn.db;"
dotnet run --filter * -c Release