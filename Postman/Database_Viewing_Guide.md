# 🗄️ ChatTask Veritabanı Görüntüleme Rehberi

Bu rehber, ChatTask projesindeki SQL Server veritabanlarını görüntüleme yöntemlerini açıklar.

## 📊 Mevcut Veritabanları

ChatTask projesinde 2 ana veritabanı bulunmaktadır:

1. **ChatTask_UserService** - Kullanıcı yönetimi
2. **ChatTask_ChatService** - Sohbet ve workspace yönetimi

## 🔧 Veritabanı Görüntüleme Yöntemleri

### **Yöntem 1: Docker SQL Server Container'ı ile Komut Satırı**

#### Bağlantı Bilgileri:

- **Server**: localhost:1433
- **Username**: sa
- **Password**: YourStrong@Passw0rd
- **Trust Server Certificate**: Evet

#### Temel Komutlar:

```bash
# 1. Veritabanlarını listele
docker exec -it chattask-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "SELECT name FROM sys.databases"

# 2. UserService veritabanındaki tabloları listele
docker exec -it chattask-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "USE ChatTask_UserService; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"

# 3. ChatService veritabanındaki tabloları listele
docker exec -it chattask-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "USE ChatTask_ChatService; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"

# 4. Users tablosunun yapısını görüntüle
docker exec -it chattask-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "USE ChatTask_UserService; SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' ORDER BY ORDINAL_POSITION"

# 5. Chats tablosunun yapısını görüntüle
docker exec -it chattask-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "USE ChatTask_ChatService; SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Chats' ORDER BY ORDINAL_POSITION"
```

#### Veri Görüntüleme:

```bash
# Users tablosundaki verileri görüntüle
docker exec -it chattask-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "USE ChatTask_UserService; SELECT Id, Name, Email, Status, CreatedAt FROM Users"

# Chats tablosundaki verileri görüntüle
docker exec -it chattask-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "USE ChatTask_ChatService; SELECT Id, UserId, ToUserId, Message, Date, IsRead FROM Chats"
```

### **Yöntem 2: SQL Server Management Studio (SSMS)**

1. **SSMS'i indirin**: [Microsoft SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

2. **Bağlantı Ayarları**:

   - **Server name**: localhost,1433
   - **Authentication**: SQL Server Authentication
   - **Login**: sa
   - **Password**: YourStrong@Passw0rd
   - **Options** → **Connection Properties** → **Trust server certificate**: ✅

3. **Veritabanlarını Keşfedin**:
   - Object Explorer'da **Databases** klasörünü genişletin
   - **ChatTask_UserService** ve **ChatTask_ChatService** veritabanlarını görün

### **Yöntem 3: Azure Data Studio**

1. **Azure Data Studio'yu indirin**: [Azure Data Studio](https://docs.microsoft.com/en-us/sql/azure-data-studio/download-azure-data-studio)

2. **Bağlantı Ayarları** (SSMS ile aynı):
   - **Server**: localhost,1433
   - **Authentication**: SQL Login
   - **User name**: sa
   - **Password**: YourStrong@Passw0rd
   - **Trust server certificate**: ✅

### **Yöntem 4: Entity Framework Migration Dosyaları**

Migration dosyalarından tablo yapılarını görebilirsiniz:

- **ChatService**: `ChatTask.ChatService/Migrations/20250909130637_UnifiedMemberSystem.cs`
- **UserService**: `ChatTask.UserService/Migrations/` klasöründeki dosyalar

## 📋 Mevcut Tablo Yapıları

### **ChatTask_UserService.Users**

| Column       | Type             | Nullable | Max Length | Description        |
| ------------ | ---------------- | -------- | ---------- | ------------------ |
| Id           | uniqueidentifier | NO       | -          | Primary Key        |
| Name         | nvarchar         | NO       | 100        | Kullanıcı adı      |
| Email        | nvarchar         | NO       | 255        | E-posta adresi     |
| Avatar       | nvarchar         | NO       | 500        | Avatar URL         |
| Status       | nvarchar         | NO       | 20         | Kullanıcı durumu   |
| CreatedAt    | datetime2        | NO       | -          | Oluşturulma tarihi |
| UpdatedAt    | datetime2        | NO       | -          | Güncellenme tarihi |
| PasswordHash | nvarchar         | NO       | -1         | Şifre hash'i       |
| PasswordSalt | nvarchar         | NO       | -1         | Şifre salt'ı       |

### **ChatTask_ChatService.Chats**

| Column    | Type             | Nullable | Max Length | Description           |
| --------- | ---------------- | -------- | ---------- | --------------------- |
| Id        | uniqueidentifier | NO       | -          | Primary Key           |
| UserId    | uniqueidentifier | NO       | -          | Gönderen kullanıcı ID |
| ToUserId  | uniqueidentifier | NO       | -          | Alıcı kullanıcı ID    |
| Message   | nvarchar         | NO       | 1000       | Mesaj içeriği         |
| Date      | datetime2        | NO       | -          | Mesaj tarihi          |
| IsRead    | bit              | NO       | -          | Okundu mu?            |
| CreatedAt | datetime2        | NO       | -          | Oluşturulma tarihi    |

## 🔍 Yararlı SQL Sorguları

### **Veri Analizi Sorguları:**

```sql
-- En aktif kullanıcılar
SELECT u.Name, COUNT(c.Id) as MessageCount
FROM ChatTask_UserService.dbo.Users u
LEFT JOIN ChatTask_ChatService.dbo.Chats c ON u.Id = c.UserId
GROUP BY u.Name
ORDER BY MessageCount DESC;

-- Son 24 saatteki mesajlar
SELECT u.Name as Sender, u2.Name as Receiver, c.Message, c.Date
FROM ChatTask_ChatService.dbo.Chats c
JOIN ChatTask_UserService.dbo.Users u ON c.UserId = u.Id
JOIN ChatTask_UserService.dbo.Users u2 ON c.ToUserId = u2.Id
WHERE c.Date >= DATEADD(day, -1, GETDATE())
ORDER BY c.Date DESC;

-- Kullanıcı istatistikleri
SELECT
    COUNT(*) as TotalUsers,
    COUNT(CASE WHEN Status = 'online' THEN 1 END) as OnlineUsers,
    COUNT(CASE WHEN Status = 'offline' THEN 1 END) as OfflineUsers
FROM ChatTask_UserService.dbo.Users;
```

## 🛠️ Sorun Giderme

### **Bağlantı Sorunları:**

1. **SSL Certificate Error**:

   ```bash
   # -C parametresi ekleyin (Trust server certificate)
   docker exec -it chattask-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "SELECT 1"
   ```

2. **Container Çalışmıyor**:

   ```bash
   docker ps
   docker-compose up -d
   ```

3. **Port Çakışması**:
   ```bash
   netstat -an | findstr :1433
   ```

## 📊 Veritabanı Yönetimi

### **Backup Alma:**

```bash
docker exec -it chattask-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "BACKUP DATABASE ChatTask_UserService TO DISK = '/var/opt/mssql/backup/UserService.bak'"
```

### **Veritabanını Sıfırlama:**

```bash
# Migration'ları geri al
cd ChatTask.UserService
dotnet ef database drop
dotnet ef database update

cd ../ChatTask.ChatService
dotnet ef database drop
dotnet ef database update
```

---

**Not**: Bu rehber, ChatTask projesinin unified member sistemi ile güncel veritabanı yapısını yansıtmaktadır.
