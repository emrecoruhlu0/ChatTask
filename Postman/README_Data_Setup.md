# ChatTask - Hazır Veri Oluşturma Rehberi

Bu rehber, ChatTask projesi için Postman üzerinden hazır veri oluşturma sürecini açıklar.

## 📋 İçindekiler

1. [Kurulum](#kurulum)
2. [Veri Oluşturma Sırası](#veri-oluşturma-sırası)
3. [Değişkenler](#değişkenler)
4. [Veri Tipleri](#veri-tipleri)
5. [Kullanım Örnekleri](#kullanım-örnekleri)

## 🚀 Kurulum

### 1. Postman Collection'ını İçe Aktarın

1. Postman'i açın
2. `Import` butonuna tıklayın
3. `ChatTask_Data_Setup_Collection.json` dosyasını seçin
4. Collection'ı içe aktarın

### 2. Environment Değişkenlerini Ayarlayın

Collection'da aşağıdaki değişkenler tanımlanmıştır:

```json
{
  "baseUrl": "http://localhost",
  "userServicePort": "5001",
  "chatServicePort": "5002",
  "taskServicePort": "5003",
  "authServicePort": "5000"
}
```

### 3. 🎲 Rastgele Veri Özelliği

Bu collection **her çalıştırıldığında farklı veriler oluşturur**:

- **Kullanıcı Adları**: Rastgele isimler + sayılar (örn: `alex123`, `nova456`)
- **Workspace Adları**: Rastgele proje isimleri (örn: `ChatTask Ana Proje 42`)
- **Channel Adları**: Rastgele kanal isimleri (örn: `genel67`, `chat89`)
- **Task Başlıkları**: Rastgele görev isimleri (örn: `API Geliştirme 23`)
- **Mesaj İçerikleri**: Rastgele mesaj kombinasyonları
- **Tarihler**: Rastgele due date'ler (1-30 gün arası)
- **Öncelikler**: Rastgele priority seviyeleri (High/Medium/Low)

## 📊 Veri Oluşturma Sırası

### 1. Kullanıcı İşlemleri

- ✅ Kullanıcı Kaydı - Admin
- ✅ Kullanıcı Kaydı - Developer 1
- ✅ Kullanıcı Kaydı - Developer 2
- ✅ Kullanıcı Kaydı - Designer
- ✅ Kullanıcı Kaydı - Tester

### 2. Workspace İşlemleri

- ✅ Workspace Oluştur - Ana Proje
- ✅ Workspace Oluştur - Test Projesi

### 3. Channel İşlemleri

- ✅ Channel Oluştur - Genel
- ✅ Channel Oluştur - Geliştirme
- ✅ Channel Oluştur - Tasarım
- ✅ Channel Oluştur - Test

### 4. Group İşlemleri

- ✅ Group Oluştur - Sprint 1
- ✅ Group Oluştur - Bug Fix

### 5. Direct Message İşlemleri

- ✅ Direct Message Oluştur - Admin-Dev1
- ✅ Direct Message Oluştur - Dev1-Dev2

### 6. Task İşlemleri

- ✅ Task Oluştur - Backend API
- ✅ Task Oluştur - Frontend UI
- ✅ Task Oluştur - Test Senaryoları
- ✅ Task Oluştur - Database Optimizasyonu

### 7. Mesaj İşlemleri

- ✅ Mesaj Gönder - Genel Kanal
- ✅ Mesaj Gönder - Geliştirme Kanalı
- ✅ Mesaj Gönder - Tasarım Kanalı
- ✅ Mesaj Gönder - Test Kanalı

## 🔧 Değişkenler

Collection'da kullanılan önemli değişkenler:

| Değişken                  | Açıklama                   | Örnek                                  |
| ------------------------- | -------------------------- | -------------------------------------- |
| `{{adminUserId}}`         | Admin kullanıcısının ID'si | `123e4567-e89b-12d3-a456-426614174000` |
| `{{dev1UserId}}`          | Developer 1'in ID'si       | `123e4567-e89b-12d3-a456-426614174001` |
| `{{dev2UserId}}`          | Developer 2'nin ID'si      | `123e4567-e89b-12d3-a456-426614174002` |
| `{{designerUserId}}`      | Designer'ın ID'si          | `123e4567-e89b-12d3-a456-426614174003` |
| `{{testerUserId}}`        | Tester'ın ID'si            | `123e4567-e89b-12d3-a456-426614174004` |
| `{{workspaceId}}`         | Workspace ID'si            | `123e4567-e89b-12d3-a456-426614174010` |
| `{{genelChannelId}}`      | Genel kanal ID'si          | `123e4567-e89b-12d3-a456-426614174020` |
| `{{gelistirmeChannelId}}` | Geliştirme kanal ID'si     | `123e4567-e89b-12d3-a456-426614174021` |
| `{{tasarimChannelId}}`    | Tasarım kanal ID'si        | `123e4567-e89b-12d3-a456-426614174022` |
| `{{testChannelId}}`       | Test kanal ID'si           | `123e4567-e89b-12d3-a456-426614174023` |

## 📝 Veri Tipleri

### Kullanıcı (User) - 🎲 Rastgele

```json
{
  "name": "alex123", // Rastgele: alex, sam, mike, jordan, taylor + sayı
  "email": "alex123@chattask.com",
  "password": "dev123"
}
```

**Rastgele Kullanıcı Adları:**

- Admin: `admin`, `administrator`, `superuser`, `root`, `manager` + sayı
- Developer: `alex`, `sam`, `mike`, `jordan`, `taylor`, `casey` + sayı
- Designer: `luna`, `nova`, `zen`, `pixel`, `art`, `creative` + sayı
- Tester: `test`, `qa`, `quality`, `bug`, `check`, `verify` + sayı

### Workspace

```json
{
  "name": "ChatTask Ana Proje",
  "description": "ChatTask uygulamasının ana geliştirme workspace'i",
  "createdById": "{{adminUserId}}"
}
```

### Channel

```json
{
  "name": "genel",
  "description": "Genel sohbet kanalı",
  "topic": "Herkes için genel konuşmalar",
  "channelPurpose": "General",
  "autoAddNewMembers": true,
  "createdById": "{{adminUserId}}",
  "initialMemberIds": ["{{dev1UserId}}", "{{dev2UserId}}"]
}
```

### Group

```json
{
  "name": "Sprint 1 - Temel Özellikler",
  "description": "İlk sprint için temel özellikler geliştirme grubu",
  "groupPurpose": "Project",
  "expiresAt": "2024-02-15T23:59:59Z",
  "createdById": "{{adminUserId}}",
  "initialMemberIds": ["{{dev1UserId}}", "{{dev2UserId}}"]
}
```

### Direct Message

```json
{
  "name": "Admin - Dev1 Özel",
  "description": "Admin ve Dev1 arasında özel konuşma",
  "participantIds": ["{{adminUserId}}", "{{dev1UserId}}"]
}
```

### Task

```json
{
  "title": "Backend API Geliştirme",
  "description": "ChatTask uygulaması için REST API'lerin geliştirilmesi",
  "dueDate": "2024-02-10T17:00:00Z",
  "status": "Pending",
  "priority": "High",
  "isPrivate": false,
  "createdById": "{{adminUserId}}",
  "assigneeIds": ["{{dev1UserId}}", "{{dev2UserId}}"],
  "channelId": "{{genelChannelId}}"
}
```

### Message

```json
{
  "senderId": "{{adminUserId}}",
  "content": "Merhaba ekip! Bugün ChatTask projesi için çalışmaya başlıyoruz. Herkese başarılar! 🚀"
}
```

## 🎯 Kullanım Örnekleri

### Senaryo 1: Yeni Bir Proje Ekibi Oluşturma

1. **Kullanıcıları Oluşturun**

   - Admin, Developer, Designer, Tester kullanıcılarını kaydedin

2. **Workspace Oluşturun**

   - Proje için workspace oluşturun

3. **Channel'ları Oluşturun**

   - Genel, Geliştirme, Tasarım, Test kanallarını oluşturun

4. **Task'ları Oluşturun**

   - Proje görevlerini oluşturun ve atayın

5. **Mesajları Gönderin**
   - Kanallara hoş geldin mesajları gönderin

### Senaryo 2: Sprint Planlama

1. **Group Oluşturun**

   - Sprint için geçici grup oluşturun

2. **Task'ları Oluşturun**

   - Sprint görevlerini oluşturun

3. **Direct Message'lar Oluşturun**
   - Ekip üyeleri arasında özel konuşmalar başlatın

## 🔍 Veri Görüntüleme

Collection'ın son bölümünde veri görüntüleme endpoint'leri bulunur:

- **Workspace'leri Getir**: Kullanıcının workspace'lerini listeler
- **Conversation'ları Getir**: Workspace'teki tüm conversation'ları listeler
- **Channel'ları Getir**: Sadece channel'ları listeler
- **Group'ları Getir**: Sadece group'ları listeler
- **Direct Message'ları Getir**: Sadece DM'leri listeler
- **TaskGroup'ları Getir**: Sadece task group'ları listeler

## ⚠️ Önemli Notlar

1. **Sıralama Önemli**: Veri oluşturma sırasına dikkat edin
2. **ID'leri Kaydedin**: Response'lardan dönen ID'leri değişken olarak kaydedin
3. **Hata Kontrolü**: Her request'ten sonra response'u kontrol edin
4. **Port Kontrolü**: Servislerin doğru portlarda çalıştığından emin olun
5. **🎲 Rastgele Veri**: Her çalıştırmada farklı veriler oluşur - bu normaldir!
6. **Pre-request Scripts**: Her request öncesi JavaScript ile rastgele veri oluşturulur

## 🔧 **Gelişmiş Test Script'leri**

### ✅ **Başarılı Durumlar (200 OK)**

- **Status kontrolü**: 200 OK beklenir
- **ID kaydetme**: Response'dan ID'yi alıp environment'a kaydeder
- **Console log**: İşlem durumunu konsola yazdırır

### 🔄 **Hata Durumları ve Otomatik Düzeltme**

- **409 Conflict**: Kullanıcı zaten varsa, mevcut kullanıcıyı bulup ID'sini kaydeder
- **400 Bad Request**: Workspace oluşturma başarısızsa, adminUserId'yi kontrol eder
- **404 Not Found**: Channel/Group oluşturma başarısızsa, workspaceId'yi kontrol eder
- **Otomatik düzeltme**: Eksik ID'leri otomatik olarak bulup environment'a kaydeder

### 🎯 **Akıllı Kullanıcı Bulma**

- **Admin**: `admin`, `administrator`, `superuser` içeren kullanıcıları bulur
- **Developer 1**: `alex`, `sam`, `mike`, `jordan` içeren kullanıcıları bulur
- **Developer 2**: `chris`, `pat`, `blake`, `morgan` içeren kullanıcıları bulur
- **Designer**: `luna`, `nova`, `zen`, `pixel` içeren kullanıcıları bulur
- **Tester**: `test`, `qa`, `quality`, `bug` içeren kullanıcıları bulur

## 🐛 Sorun Giderme

### Yaygın Hatalar

1. **404 Not Found**: Servis çalışmıyor olabilir
2. **500 Internal Server Error**: Database bağlantı sorunu
3. **400 Bad Request**: Eksik veya hatalı veri gönderimi
4. **409 Conflict**: Kullanıcı zaten var (otomatik düzeltilir)

### Çözümler

1. Servislerin çalıştığını kontrol edin
2. Database bağlantısını kontrol edin
3. Request body'lerini kontrol edin
4. Port numaralarını kontrol edin
5. **Otomatik düzeltme**: Script'ler hata durumlarında otomatik olarak eksik ID'leri bulur

## 📞 Destek

Sorun yaşarsanız:

1. Console log'larını kontrol edin
2. Database connection string'lerini kontrol edin
3. Servis endpoint'lerini kontrol edin

---

**Not**: Bu collection, ChatTask projesinin tüm veri tiplerini kapsamlı bir şekilde test etmek için tasarlanmıştır. Gerçek kullanımda, veri tiplerini ve içeriklerini projenizin ihtiyaçlarına göre özelleştirebilirsiniz.
