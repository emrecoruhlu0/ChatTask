# ChatTask Veritabanları Veri Temizleme PowerShell Scripti
# Bu script tüm veritabanlarındaki verileri temizler

param(
    [string]$ServerInstance = "127.0.0.1,1433",
    [string]$Username = "sa",
    [string]$Password = "YourStrong@Passw0rd",
    [switch]$SkipLocalDB = $false
)

Write-Host "ChatTask Veritabanları Veri Temizleme Başlatılıyor..." -ForegroundColor Green

# SQL Server veritabanları
$databases = @(
    "ChatTask_UserService",
    "ChatTask_TaskService"
)

# LocalDB veritabanı (isteğe bağlı)
if (-not $SkipLocalDB) {
    $databases += "ChatTask_ChatService_Dev"
}

foreach ($database in $databases) {
    Write-Host "`n$database veritabanı temizleniyor..." -ForegroundColor Yellow
    
    try {
        if ($database -eq "ChatTask_ChatService") {
            # LocalDB için farklı connection string
            $connectionString = "Server=(localdb)\mssqllocaldb;Database=$database;Trusted_Connection=True;MultipleActiveResultSets=true"
            $sqlcmd = "sqlcmd -S `"(localdb)\mssqllocaldb`" -d `"$database`" -E"
        }
        else {
            # SQL Server için
            $connectionString = "Server=$ServerInstance;Database=$database;User Id=$Username;Password=$Password;TrustServerCertificate=true;"
            $sqlcmd = "sqlcmd -S `"$ServerInstance`" -d `"$database`" -U `"$Username`" -P `"$Password`""
        }
        
        # Veri temizleme komutları
        $clearScript = @"
-- Foreign key constraint'leri geçici olarak devre dışı bırak
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- Tüm tablolardaki verileri temizle
EXEC sp_MSforeachtable "DELETE FROM ?"

-- Foreign key constraint'leri tekrar aktif et
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"

-- Identity sütunlarını sıfırla (eğer varsa)
EXEC sp_MSforeachtable "DBCC CHECKIDENT ('?', RESEED, 0)"

PRINT '$database veritabanı temizlendi'
"@
        
        # Scripti geçici dosyaya yaz
        $tempFile = [System.IO.Path]::GetTempFileName() + ".sql"
        $clearScript | Out-File -FilePath $tempFile -Encoding UTF8
        
        # SQL komutunu çalıştır
        $command = "$sqlcmd -i `"$tempFile`""
        Invoke-Expression $command
        
        # Geçici dosyayı sil
        Remove-Item $tempFile -Force
        
        Write-Host "$database veritabanı başarıyla temizlendi!" -ForegroundColor Green
        
    }
    catch {
        Write-Host "Hata: $database veritabanı temizlenirken hata oluştu: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nTüm veritabanları temizleme işlemi tamamlandı!" -ForegroundColor Green
Write-Host "`nKullanım örnekleri:" -ForegroundColor Cyan
Write-Host "  .\Clear-DatabaseData.ps1                                    # Tüm veritabanları (LocalDB dahil)"
Write-Host "  .\Clear-DatabaseData.ps1 -SkipLocalDB                      # Sadece SQL Server veritabanları"
Write-Host "  .\Clear-DatabaseData.ps1 -ServerInstance 'localhost,1433'  # Farklı server"
