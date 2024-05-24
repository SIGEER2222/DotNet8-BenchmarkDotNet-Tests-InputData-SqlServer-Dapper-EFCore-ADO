$env:NumeroContatosPorCompanhia = "1"
$env:BaseSQLSugarConnectionString = "Data Source=D:\试验\Log\BaiAn.db;"
$env:BaseEFCoreConnectionString = "Data Source=D:\试验\Log\BaiAn.db;"
$env:BaseDapperConnectionString = "Data Source=D:\试验\Log\BaiAn.db;"
$env:BaseDapperContribConnectionString = "Data Source=D:\试验\Log\BaiAn.db;"
$env:BaseADOConnectionString = "Data Source=D:\试验\Log\BaiAn.db;"
dotnet run --filter * -c Release