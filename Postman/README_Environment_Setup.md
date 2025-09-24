# ChatTask Environment Kurulum Rehberi

Bu rehber, ChatTask projesi için Postman Environment kurulumunu açıklar.

## 🚀 Kurulum Adımları

### 1. Environment Dosyasını İçe Aktarın

1. Postman'i açın
2. **Environments** sekmesine gidin
3. **Import** butonuna tıklayın
4. `ChatTask_Local_Environment.json` dosyasını seçin
5. **Import** butonuna tıklayın

### 2. Environment'ı Seçin

1. Sağ üst köşedeki **Environment** dropdown'ına tıklayın
2. **"ChatTask Local"** environment'ını seçin

### 3. Environment Değişkenleri

#### **Temel Servis Ayarları:**

- `baseUrl`: `http://localhost`
- `userServicePort`: `5001`
- `chatServicePort`: `5002`
- `taskServicePort`: `5003`
- `authServicePort`: `5000`

#### **Otomatik Olarak Doldurulacak Değişkenler:**

- `adminUserId`: Test script'leri tarafından doldurulur
- `dev1UserId`: Test script'leri tarafından doldurulur
- `dev2UserId`: Test script'leri tarafından doldurulur
- `designerUserId`: Test script'leri tarafından doldurulur
- `testerUserId`: Test script'leri tarafından doldurulur
- `workspaceId`: Test script'leri tarafından doldurulur
- `genelChannelId`: Test script'leri tarafından doldurulur
- `gelistirmeChannelId`: Test script'leri tarafından doldurulur
- `tasarimChannelId`: Test script'leri tarafından doldurulur
- `testChannelId`: Test script'leri tarafından doldurulur

## 🔧 Environment Kontrolü

### Environment'ın Doğru Seçildiğini Kontrol Edin:

1. **Sağ üst köşede** "ChatTask Local" yazıyor olmalı
2. **Collection'ı çalıştırırken** environment seçili olmalı
3. **Request'lerde** `{{baseUrl}}`, `{{userServicePort}}` gibi değişkenler çalışmalı

### Test Etmek İçin:

1. Herhangi bir request'i açın
2. **Send** butonuna tıklamadan önce
3. **URL'de** `{{baseUrl}}:{{userServicePort}}` görünmeli
4. **Send** butonuna tıkladığınızda `http://localhost:5001` olmalı

## 🎯 Kullanım

### 1. Collection'ı Çalıştırın

1. **Collection'ı seçin**
2. **"Run Collection"** butonuna tıklayın
3. **Environment:** "ChatTask Local" seçili olduğundan emin olun
4. **Run** butonuna tıklayın

### 2. Otomatik ID Kaydetme

Test script'leri çalıştığında:

- Kullanıcı kayıtlarından sonra ID'ler otomatik kaydedilir
- Workspace oluşturulduktan sonra ID kaydedilir
- Channel'lar oluşturulduktan sonra ID'ler kaydedilir

### 3. Environment Değişkenlerini Görüntüleme

1. **Environments** sekmesine gidin
2. **"ChatTask Local"** environment'ını seçin
3. Test'ler çalıştıktan sonra ID'lerin doldurulduğunu görebilirsiniz

## ⚠️ Önemli Notlar

1. **Environment Seçimi**: Her zaman "ChatTask Local" environment'ını seçin
2. **Servis Portları**: Servislerin doğru portlarda çalıştığından emin olun
3. **ID'ler**: Test script'leri çalıştıktan sonra ID'ler otomatik doldurulur
4. **Sıralama**: Collection'ı sırayla çalıştırın (kullanıcılar → workspace → channel'lar)

## 🐛 Sorun Giderme

### Environment Seçili Değil

- Sağ üst köşede "No Environment" yazıyorsa
- Dropdown'dan "ChatTask Local" seçin

### Değişkenler Çalışmıyor

- URL'de `{{baseUrl}}` görünüyorsa
- Environment'ın doğru seçildiğini kontrol edin

### ID'ler Kaydedilmiyor

- Test script'lerinin çalıştığını kontrol edin
- Console'da hata mesajları var mı bakın

## 📞 Destek

Sorun yaşarsanız:

1. Environment'ın doğru seçildiğini kontrol edin
2. Servislerin çalıştığını kontrol edin
3. Test script'lerinin çalıştığını kontrol edin

---

**Not**: Bu environment, ChatTask projesinin tüm servisleriyle çalışmak için tasarlanmıştır. Gerçek kullanımda, servis URL'lerini ve port numaralarını projenizin ihtiyaçlarına göre özelleştirebilirsiniz.

