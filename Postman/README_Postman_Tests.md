# ChatTask Microservices - Postman Test Collection

Bu Postman collection'ı, ChatTask mikroservis mimarisindeki tüm endpoint'leri ve DTO'ları test etmek için hazırlanmıştır.

## 📋 İçerik

### 🔧 **Test Kategorileri:**

1. **UserService Tests**

   - Kullanıcı kaydı (Register User)
   - Tüm kullanıcıları listele (Get All Users)
   - ID ile kullanıcı getir (Get User by ID)

2. **ChatService Tests**

   - Workspace oluştur (Create Workspace)
   - Workspace conversation'larını getir (Get Workspace Conversations)
   - Channel oluştur (Create Channel)
   - Mesaj gönder (Send Message)
   - Mesajları getir (Get Messages)

3. **TaskService Tests**

   - Task oluştur (Create Task)
   - Tüm task'ları listele (Get All Tasks)
   - ID ile task getir (Get Task by ID)
   - Task güncelle (Update Task)
   - Task durumu güncelle (Update Task Status)
   - Kullanıcıya göre task'ları getir (Get Tasks by User ID)
   - Task'ı kullanıcılara ata (Assign Task to Users)

4. **Enum Validation Tests**

   - TaskStatus enum doğrulama (Validate TaskStatus Enum)
   - TaskPriority enum doğrulama (Validate TaskPriority Enum)
   - ConversationType enum doğrulama (Validate ConversationType Enum)
   - MemberRole enum doğrulama (Validate MemberRole Enum)

5. **Integration Tests**
   - Tam workflow testi (Full Workflow Test)

## 🚀 Kullanım

### 1. **Postman'e Import Etme:**

```
File → Import → ChatTask_Microservices_Complete_Tests.json
```

### 2. **Environment Variables:**

Collection'da şu değişkenler tanımlı:

- `userServiceUrl`: http://localhost:5001
- `chatServiceUrl`: http://localhost:5002
- `taskServiceUrl`: http://localhost:5004
- `authServiceUrl`: http://localhost:5003

### 3. **Test Çalıştırma:**

- **Tek test**: Herhangi bir test'e tıklayıp "Send" butonuna basın
- **Tüm testler**: Collection'ın üzerine sağ tıklayıp "Run collection" seçin

## 🎯 Test Özellikleri

### ✅ **Otomatik Rastgele Veri Üretimi:**

- Her test çalıştırıldığında farklı rastgele veriler kullanılır
- `{{$randomFullName}}`, `{{$randomEmail}}`, `{{$randomPassword}}` gibi Postman dinamik değişkenleri
- Her test benzersiz verilerle çalışır

### ✅ **Kapsamlı Response Validation:**

- **Status Code Kontrolü**: 200, 201 gibi başarılı kodlar
- **DTO Structure Validation**: Tüm gerekli alanların varlığı
- **Data Type Validation**: GUID formatı, enum değerleri
- **Business Logic Validation**: ID'lerin eşleşmesi, tarih kontrolleri

### ✅ **Test Dependencies:**

- Testler birbirine bağımlı çalışır
- İlk test (Register User) çalıştıktan sonra `testUserId` global değişken olarak saklanır
- Sonraki testler bu ID'yi kullanır

## 📊 Test Sonuçları

### **Başarılı Test Örneği:**

```javascript
✅ Status code is 200
✅ Response has user data
✅ User ID is valid GUID
✅ User is active
```

### **Test Assertions:**

- **GUID Validation**: `pm.expect(id).to.match(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)`
- **Enum Validation**:
  - TaskStatus: `pm.expect([1, 2, 3, 4, 5]).to.include(status)` (Pending=1, InProgress=2, Completed=3, Cancelled=4, OnHold=5)
  - TaskPriority: `pm.expect([1, 2, 3, 4]).to.include(priority)` (Low=1, Medium=2, High=3, Critical=4)
  - ConversationType: `pm.expect([1, 2, 3, 4]).to.include(type)` (Channel=1, Group=2, DirectMessage=3, TaskGroup=4)
- **Property Existence**: `pm.expect(response).to.have.property('id')`
- **Array Validation**: `pm.expect(response).to.be.an('array')`

## 🔄 Test Sırası

**Önerilen çalıştırma sırası:**

1. UserService Tests → Register User
2. ChatService Tests → Create Workspace
3. ChatService Tests → Create Channel
4. ChatService Tests → Send Message
5. TaskService Tests → Create Task
6. Integration Tests → Full Workflow Test

## 🐛 Troubleshooting

### **Yaygın Hatalar:**

1. **Connection Refused (ECONNREFUSED)**

   - Servislerin çalıştığından emin olun: `docker ps`
   - Port'ların doğru olduğunu kontrol edin

2. **401 Unauthorized**

   - Authentication geçici olarak devre dışı
   - Eğer hala alıyorsanız, servisleri yeniden başlatın

3. **404 Not Found**

   - Endpoint URL'lerini kontrol edin
   - Route'ların doğru tanımlandığından emin olun

4. **Validation Errors**
   - Request body'deki veri tiplerini kontrol edin
   - Enum değerlerinin doğru olduğundan emin olun

## 📈 Test Coverage

### **DTO Coverage:**

- ✅ RegisterDto
- ✅ CreateWorkspaceDto
- ✅ CreateConversationDto
- ✅ SendMessageDto
- ✅ CreateTaskDto
- ✅ UpdateTaskDto

### **Endpoint Coverage:**

- ✅ UserService: 3/3 endpoints
- ✅ ChatService: 5/5 endpoints
- ✅ TaskService: 7/7 endpoints
- ✅ Enum Validation: 4/4 enum types

### **Response Validation:**

- ✅ UserDto
- ✅ WorkspaceDto
- ✅ ConversationDto
- ✅ MessageDto
- ✅ TaskDto

## 🎉 Sonuç

Bu collection ile mikroservis mimarinizin tüm bileşenlerini test edebilir, DTO'ların doğru çalıştığını doğrulayabilir ve servisler arası iletişimi kontrol edebilirsiniz.

**Not**: Testler süre kontrolü yapmaz, sadece response'ları ve DTO yapılarını doğrular.
