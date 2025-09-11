# ChatTask API Testing Guide

Bu rehber, ChatTask projesinin unified member sistemi ile güncellenmiş API testlerini nasıl çalıştıracağınızı açıklar.

## 📋 Gereksinimler

- Postman Desktop App (v10.0.0+)
- ChatTask servisleri çalışır durumda olmalı:
  - UserService (Port 5001)
  - ChatService (Port 5002)
  - AuthService (Port 5003)
  - TaskService (Port 5004) - Opsiyonel

## 🚀 Kurulum

### 1. Postman Collection'ı İçe Aktarın

1. Postman'i açın
2. `Import` butonuna tıklayın
3. `ChatTask_Unified_Member_System_Tests.json` dosyasını seçin
4. Collection'ı içe aktarın

### 2. Environment'ı Ayarlayın

1. Postman'de `Environments` sekmesine gidin
2. `Import` butonuna tıklayın
3. `ChatTask_Unified_Environment.json` dosyasını seçin
4. Environment'ı içe aktarın
5. Environment'ı aktif hale getirin (sağ üst köşeden seçin)

### 3. Servisleri Başlatın

```bash
# Docker ile tüm servisleri başlatın
docker-compose up -d

# Veya manuel olarak
dotnet run --project ChatTask.UserService
dotnet run --project ChatTask.ChatService
dotnet run --project ChatTask.AuthService
```

## 🧪 Test Senaryoları

### 🔐 Authentication & User Management

- **Register User**: Benzersiz kullanıcı kaydı
- **Login User**: JWT token ile giriş
- **Get All Users**: Kullanıcı listesi

### 🏢 Workspace Management

- **Create Workspace**: Yeni workspace oluşturma
- **Get All Workspaces**: Workspace listesi
- **Add Member to Workspace**: Workspace'e üye ekleme

### 💬 Conversation Management

- **Create Channel**: Kanal oluşturma
- **Create Group**: Grup oluşturma
- **Create Direct Message**: Direkt mesaj oluşturma
- **Get All Conversations**: Konuşma listesi
- **Get Conversations by Type**: Tip bazlı filtreleme

### 👥 Member Management

- **Add Member to Channel**: Kanala üye ekleme
- **Remove Member from Channel**: Kanaldan üye çıkarma

### 📝 Messaging

- **Send Message to Channel**: Kanala mesaj gönderme
- **Get Messages from Channel**: Kanal mesajlarını alma

### 🧪 Advanced Tests

- **Test Member ID Hashing**: ID hashleme testi
- **Test Multiple Member Roles**: Farklı roller testi

## 🎯 Test Çalıştırma

### Tekil Test Çalıştırma

1. Collection'da istediğiniz testi seçin
2. `Send` butonuna tıklayın
3. Response ve test sonuçlarını kontrol edin

### Tüm Testleri Çalıştırma

1. Collection'ın yanındaki `...` menüsüne tıklayın
2. `Run collection` seçin
3. Test sırasını kontrol edin (önemli!)
4. `Run ChatTask Unified Member System Tests` butonuna tıklayın

### Test Sırası

Testler şu sırayla çalıştırılmalıdır:

1. **Authentication & User Management** (1-3)
2. **Workspace Management** (4-6)
3. **Conversation Management** (7-11)
4. **Member Management** (12-13)
5. **Messaging** (14-15)
6. **Advanced Tests** (16-17)

## 🔍 Test Sonuçları

### Başarılı Test Göstergeleri

- ✅ Yeşil tik işareti
- Response status: 200
- Response time < 5000ms
- Gerekli alanlar mevcut

### Hata Durumları

- ❌ Kırmızı X işareti
- Response status: 4xx/5xx
- Console'da hata mesajları

## 🐛 Sorun Giderme

### Yaygın Hatalar

#### 1. Connection Refused

```
Error: connect ECONNREFUSED 127.0.0.1:5001
```

**Çözüm**: Servislerin çalıştığından emin olun

```bash
docker ps
# veya
netstat -an | findstr :5001
```

#### 2. 401 Unauthorized

```
Error: 401 Unauthorized
```

**Çözüm**:

- Login testinin başarılı olduğundan emin olun
- Access token'ın environment'da kayıtlı olduğunu kontrol edin

#### 3. 404 Not Found

```
Error: 404 Not Found
```

**Çözüm**:

- API endpoint'lerinin doğru olduğunu kontrol edin
- Workspace/Channel ID'lerinin environment'da kayıtlı olduğunu kontrol edin

#### 4. 500 Internal Server Error

```
Error: 500 Internal Server Error
```

**Çözüm**:

- Veritabanı bağlantısını kontrol edin
- Migration'ların uygulandığından emin olun
- Log dosyalarını kontrol edin

### Debug İpuçları

1. **Console Logs**: Her test console'da detaylı loglar bırakır
2. **Environment Variables**: Test sırasında değişkenlerin güncellendiğini kontrol edin
3. **Response Body**: Hata durumlarında response body'yi inceleyin
4. **Pre-request Scripts**: Otomatik veri oluşturma scriptlerini kontrol edin

## 📊 Test Metrikleri

### Performans Beklentileri

- **Response Time**: < 2000ms (normal), < 5000ms (kabul edilebilir)
- **Success Rate**: %100
- **Memory Usage**: Stabil

### Test Coverage

- ✅ Authentication
- ✅ User Management
- ✅ Workspace Management
- ✅ Conversation Management
- ✅ Member Management
- ✅ Messaging
- ✅ ID Hashing System
- ✅ Role Management

## 🔄 Güncellemeler

Bu test suite'i aşağıdaki durumlarda güncellenmelidir:

1. Yeni API endpoint'leri eklendiğinde
2. Mevcut endpoint'ler değiştirildiğinde
3. Yeni DTO'lar eklendiğinde
4. Member sistemi değişikliklerinde

## 📞 Destek

Sorunlar için:

1. Console loglarını kontrol edin
2. Environment değişkenlerini doğrulayın
3. Servis durumlarını kontrol edin
4. Bu rehberi tekrar okuyun

---

**Not**: Bu test suite, ChatTask'ın unified member sistemi ile tam uyumludur ve production-ready API'ları test etmek için tasarlanmıştır.
