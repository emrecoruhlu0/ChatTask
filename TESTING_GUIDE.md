# 🧪 ChatTask New API Testing Guide

Bu rehber, yeni Conversation ve TaskGroup yapısının Postman ile nasıl test edileceğini açıklar.

## 📋 Ön Gereksinimler

### 1. Projeleri Çalıştır

```bash
# UserService
cd ChatTask.UserService
dotnet run --urls="https://localhost:7001"

# AuthService
cd ChatTask.AuthService
dotnet run --urls="https://localhost:7002"

# ChatService
cd ChatTask.ChatService
dotnet run --urls="https://localhost:7003"

# TaskService
cd ChatTask.TaskService
dotnet run --urls="https://localhost:7004"
```

### 2. Veritabanı Migration'ları

```bash
# ChatService için
cd ChatTask.ChatService
dotnet ef database update

# TaskService için
cd ChatTask.TaskService
dotnet ef database update
```

## 🚀 Postman Collection Kurulumu

### 1. Collection'ı İçe Aktar

- Postman'de `Import` butonuna tıkla
- `ChatTask_New_API_Collection.json` dosyasını seç
- Collection başarıyla yüklendiğinde "ChatTask New API Collection" görünecek

### 2. Environment Variables

Collection'da şu değişkenler otomatik olarak tanımlanmış:

- `userBase`: https://localhost:7001 (UserService)
- `authBase`: https://localhost:7002 (AuthService)
- `chatBase`: https://localhost:7003 (ChatService)
- `taskBase`: https://localhost:7004 (TaskService)

## 🔐 Test Senaryoları

### Senaryo 1: Temel Kullanıcı İşlemleri

1. **Register User** - Yeni kullanıcı oluştur
2. **Login** - Kullanıcı girişi yap ve token al

### Senaryo 2: Workspace Yönetimi

1. **Create Workspace** - Test şirketi oluştur

### Senaryo 3: Conversation Yönetimi

1. **Create Channel** - #general kanalı oluştur
2. **Create Group** - Marketing Team grubu oluştur
3. **Create Direct Message** - DM oluştur
4. **Get All Conversations** - Tüm conversation'ları listele
5. **Get Conversations by Type** - Sadece channel'ları listele

### Senaryo 4: Mesajlaşma

1. **Send Message to Channel** - Kanal'a mesaj gönder
2. **Get Messages from Channel** - Kanal mesajlarını getir

### Senaryo 5: Görev Yönetimi

1. **Create Task Group** - Sprint 1 görev grubu oluştur
2. **Assign Task to Group** - Gruba görev ata
3. **Create Individual Task** - Bireysel görev oluştur
4. **Assign Task to Multiple Users** - Görevi birden fazla kişiye ata
5. **Get All Tasks** - Tüm görevleri listele
6. **Get Task Groups** - Görev gruplarını listele
7. **Get Tasks by User** - Kullanıcının görevlerini getir

## 📊 Test Sonuçları Kontrolü

### ✅ Başarılı Test İşaretleri

- HTTP Status: 200 (OK)
- Response body'de beklenen veriler
- Environment variables otomatik güncellenmiş
- Console'da log mesajları

### ❌ Hata Durumları

- HTTP Status: 400 (Bad Request) - Validasyon hatası
- HTTP Status: 401 (Unauthorized) - Token eksik/geçersiz
- HTTP Status: 404 (Not Found) - Kaynak bulunamadı
- HTTP Status: 500 (Internal Server Error) - Sunucu hatası

## 🔍 Debug İpuçları

### 1. Console Logları

Her request'te Postman Console'da detaylı log'lar görünecek:

```
Generated username: User_1703123456789_123
User registered: User_1703123456789_123
User ID: 12345678-1234-5678-9012-123456789abc
Login successful, token saved
```

### 2. Environment Variables

Test sırasında otomatik olarak güncellenen değişkenler:

- `currentUsername`: Oluşturulan kullanıcı adı
- `currentUserId`: Kullanıcı ID'si
- `accessToken`: JWT token
- `workspaceId`: Workspace ID'si
- `channelId`: Channel ID'si
- `groupId`: Group ID'si
- `taskGroupId`: Task Group ID'si

### 3. Response Validation

Her response'da şunları kontrol et:

- Status code doğru mu?
- Response body'de beklenen alanlar var mı?
- ID'ler GUID formatında mı?
- Tarih alanları UTC formatında mı?

## 🚨 Yaygın Hatalar ve Çözümleri

### Hata: "Connection refused"

**Çözüm**: İlgili servisin çalıştığından emin ol

```bash
# Servis durumunu kontrol et
netstat -an | findstr :7001
netstat -an | findstr :7002
netstat -an | findstr :7003
netstat -an | findstr :7004
```

### Hata: "Unauthorized"

**Çözüm**: Token'ın geçerli olduğundan emin ol

- Login request'ini tekrar çalıştır
- Token'ın expire olmadığını kontrol et

### Hata: "Database connection failed"

**Çözüm**: Connection string'leri kontrol et

- `appsettings.json` dosyalarında doğru connection string'ler var mı?
- SQL Server çalışıyor mu?
- Migration'lar çalıştırıldı mı?

## 📈 Performance Test

### Response Time Kontrolü

Collection'da global test script'i her response'da 2 saniyeden az olmasını kontrol eder:

```javascript
pm.test("Response time is less than 2000ms", function () {
  pm.expect(pm.response.responseTime).to.be.below(2000);
});
```

### Load Test

Aynı anda birden fazla request göndererek:

- Concurrent user handling
- Database connection pooling
- Memory usage

## 🔗 SignalR Test

### WebSocket Bağlantısı

1. **Join Conversation** request'ini çalıştır
2. Browser DevTools'da Network tab'ında WebSocket bağlantısını kontrol et
3. Real-time mesaj gönderimi test et

## 📝 Test Raporu

Test sonuçlarını şu formatta kaydet:

```markdown
# Test Raporu - [Tarih]

## ✅ Başarılı Testler

- [ ] Authentication (2/2)
- [ ] Workspace Management (1/1)
- [ ] Conversation Management (5/5)
- [ ] Messaging (2/2)
- [ ] Task Management (7/7)

## ❌ Başarısız Testler

- [ ] Hata detayları

## 📊 Performans

- Ortalama Response Time: \_\_\_ms
- En Yavaş Request: \_\_\_ms
- En Hızlı Request: \_\_\_ms

## 🐛 Bulunan Bug'lar

- [ ] Bug açıklaması
- [ ] Reproduce steps
```

## 🎯 Sonraki Adımlar

1. **Frontend Integration**: API'leri frontend'de kullan
2. **Real-time Features**: SignalR ile real-time özellikleri test et
3. **Load Testing**: JMeter veya Artillery ile yük testi yap
4. **Security Testing**: JWT token'ları ve authorization'ları test et
5. **Integration Testing**: Tüm servislerin birlikte çalışmasını test et

---

**Not**: Bu rehber sürekli güncellenmektedir. Yeni özellikler eklendikçe test senaryoları da güncellenecektir.
