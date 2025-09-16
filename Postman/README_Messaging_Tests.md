# ChatTask Messaging Tests

Bu dokümantasyon, ChatTask mikroservisleri için geliştirilmiş kapsamlı mesajlaşma testlerini açıklar.

## Test Koleksiyonları

### 1. ChatTask_Microservices_Complete_Tests.json

Ana test koleksiyonu - tüm mikroservislerin temel işlevlerini test eder.

**Eklenen Mesajlaşma Testleri:**

- ✅ **Create Direct Message** - Direct Message oluşturma
- ✅ **Send Message to DM** - DM'e mesaj gönderme
- ✅ **Create Message Thread** - Mesaj thread'i oluşturma
- ✅ **Edit Message** - Mesaj düzenleme
- ✅ **Mark Message as Read** - Mesajı okundu işaretleme

### 2. ChatTask_Advanced_Messaging_Tests.json

Gelişmiş mesajlaşma özelliklerini test eden ayrı koleksiyon.

**Test Kategorileri:**

#### Direct Message Tests

- Create Direct Message
- Send Message to DM
- Get DM Messages

#### Group Message Tests

- Create Group
- Send Message to Group

#### Message Thread Tests

- Create Message Thread
- Get Message Threads

#### Message Management Tests

- Edit Message
- Delete Message
- Mark Message as Read

#### Conversation Member Tests

- Get Conversation Members
- Add Member to Conversation
- Remove Member from Conversation

#### Advanced Message Features

- Search Messages
- Get Unread Message Count
- Mark All Messages as Read

## Test Özellikleri

### 🔍 **Kapsamlı Validasyon**

- Status code kontrolü (200, 201)
- Response data yapısı kontrolü
- GUID formatı doğrulaması
- Enum değerleri kontrolü
- Timestamp validasyonu

### 🔗 **Test Bağımlılıkları**

- Testler birbirine bağımlı olarak çalışır
- Önceki testlerden alınan ID'ler sonraki testlerde kullanılır
- Global değişkenler ile test verileri paylaşılır

### 📊 **Test Verileri**

- Random data generation
- Dynamic test data
- Realistic test scenarios

## Kullanım

### Postman'da İçe Aktarma

1. Postman'ı açın
2. "Import" butonuna tıklayın
3. JSON dosyalarını seçin
4. Environment variables'ları ayarlayın

### Environment Variables

```json
{
  "userServiceUrl": "http://localhost:5001",
  "chatServiceUrl": "http://localhost:5002",
  "taskServiceUrl": "http://localhost:5004",
  "authServiceUrl": "http://localhost:5003"
}
```

### Test Çalıştırma

1. Environment'ı seçin
2. Collection'ı çalıştırın
3. Test sonuçlarını kontrol edin

## Test Senaryoları

### Temel Mesajlaşma Akışı

1. **User Registration** - Kullanıcı kaydı
2. **Workspace Creation** - Workspace oluşturma
3. **Channel Creation** - Kanal oluşturma
4. **Message Sending** - Mesaj gönderme
5. **Message Retrieval** - Mesaj alma

### Gelişmiş Mesajlaşma Akışı

1. **Direct Message Creation** - DM oluşturma
2. **Group Creation** - Grup oluşturma
3. **Thread Creation** - Thread oluşturma
4. **Message Management** - Mesaj yönetimi
5. **Member Management** - Üye yönetimi

## Test Sonuçları

### Başarılı Test Kriterleri

- ✅ Tüm HTTP status kodları doğru
- ✅ Response data yapısı geçerli
- ✅ GUID formatları doğru
- ✅ Enum değerleri geçerli
- ✅ Timestamp'ler güncel

### Hata Durumları

- ❌ 400 Bad Request - Geçersiz veri
- ❌ 401 Unauthorized - Yetki hatası
- ❌ 404 Not Found - Kaynak bulunamadı
- ❌ 500 Internal Server Error - Sunucu hatası

## Geliştirme Notları

### Test Güncellemeleri

- Yeni endpoint'ler eklendiğinde testler güncellenmelidir
- API değişikliklerinde test senaryoları revize edilmelidir
- Yeni özellikler için testler eklenmelidir

### Performans Testleri

- Büyük veri setleri ile test edilmelidir
- Concurrent request'ler test edilmelidir
- Response time'lar kontrol edilmelidir

## Sorun Giderme

### Yaygın Hatalar

1. **Connection Refused** - Servisler çalışmıyor
2. **Timeout** - Servis yanıt vermiyor
3. **Validation Error** - Geçersiz test verisi
4. **Dependency Error** - Önceki test başarısız

### Çözümler

1. Servisleri başlatın: `docker-compose up`
2. Environment variables'ları kontrol edin
3. Test verilerini doğrulayın
4. Test sırasını kontrol edin

## Katkıda Bulunma

### Yeni Test Ekleme

1. Mevcut test yapısını takip edin
2. Comprehensive validation ekleyin
3. Error handling test edin
4. Documentation güncelleyin

### Test İyileştirme

1. Performance optimizasyonu
2. Test coverage artırma
3. Error scenario ekleme
4. User experience test etme

---

**Not:** Bu testler ChatTask mikroservislerinin mesajlaşma özelliklerini kapsamlı şekilde test eder. Yeni özellikler eklendiğinde testlerin güncellenmesi önerilir.
