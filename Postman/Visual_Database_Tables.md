# 🎨 ChatTask Görsel Veritabanı Tabloları

## 📊 Mevcut Tablo Yapıları (Görsel)

### **🗄️ ChatTask_UserService Database**

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                Users Table                                              │
├─────────────┬─────────────────┬─────────┬─────────────┬─────────────────────────────────┤
│ 🔑 Column   │ 📝 Type         │ ❌ Null │ 📏 Length   │ 📋 Description                  │
├─────────────┼─────────────────┼─────────┼─────────────┼─────────────────────────────────┤
│ Id          │ uniqueidentifier│ NO      │ -           │ Primary Key (GUID)              │
│ Name        │ nvarchar        │ NO      │ 100         │ Kullanıcı adı                   │
│ Avatar      │ nvarchar        │ NO      │ 500         │ Avatar URL                      │
│ Status      │ nvarchar        │ NO      │ 20          │ online/offline/busy             │
│ CreatedAt   │ datetime2       │ NO      │ -           │ Hesap oluşturulma tarihi        │
│ UpdatedAt   │ datetime2       │ NO      │ -           │ Son güncelleme tarihi           │
│ PasswordHash│ nvarchar        │ NO      │ -1          │ Şifre hash'i (güvenlik)         │
│ PasswordSalt│ nvarchar        │ NO      │ -1          │ Şifre salt'ı (güvenlik)         │
└─────────────┴─────────────────┴─────────┴─────────────┴─────────────────────────────────┘
```

**📊 Users Tablosu İstatistikleri:**
- **Toplam Kolon**: 8
- **Primary Key**: Id (uniqueidentifier)
- **Index**: Id (Unique)
- **Constraints**: Name, Avatar, Status, PasswordHash, PasswordSalt (NOT NULL)

---

### **🗄️ ChatTask_ChatService Database**

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                Chats Table                                              │
├─────────────┬─────────────────┬─────────┬─────────────┬─────────────────────────────────┤
│ 🔑 Column   │ 📝 Type         │ ❌ Null │ 📏 Length   │ 📋 Description                  │
├─────────────┼─────────────────┼─────────┼─────────────┼─────────────────────────────────┤
│ Id          │ uniqueidentifier│ NO      │ -           │ Primary Key (GUID)              │
│ UserId      │ uniqueidentifier│ NO      │ -           │ Gönderen kullanıcı ID           │
│ ToUserId    │ uniqueidentifier│ NO      │ -           │ Alıcı kullanıcı ID              │
│ Message     │ nvarchar        │ NO      │ 1000        │ Mesaj içeriği                   │
│ Date        │ datetime2       │ NO      │ -           │ Mesaj gönderilme tarihi         │
│ IsRead      │ bit             │ NO      │ -           │ Okundu mu? (true/false)         │
│ CreatedAt   │ datetime2       │ NO      │ -           │ Kayıt oluşturulma tarihi        │
└─────────────┴─────────────────┴─────────┴─────────────┴─────────────────────────────────┘
```

**📊 Chats Tablosu İstatistikleri:**
- **Toplam Kolon**: 7
- **Primary Key**: Id (uniqueidentifier)
- **Foreign Keys**: UserId, ToUserId (Users tablosuna referans)
- **Index**: Id (Unique)
- **Constraints**: Tüm kolonlar NOT NULL

---

## 🔗 Tablo İlişkileri (Görsel)

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              Database Relationships                                    │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                         │
│  ┌─────────────────┐                    ┌─────────────────┐                            │
│  │   Users Table   │                    │   Chats Table   │                            │
│  │                 │                    │                 │                            │
│  │ 🔑 Id (PK)      │◄───────────────────┤ UserId (FK)     │                            │
│  │ 👤 Name         │                    │                 │                            │
│  │ 🖼️ Avatar       │                    │ 🔑 Id (PK)      │                            │
│  │ 🟢 Status       │                    │                 │                            │
│  │ 📅 CreatedAt    │                    │ ToUserId (FK)   │◄──────────────────────────┐│
│  │ 🔄 UpdatedAt    │                    │                 │                           ││
│  │ 🔐 PasswordHash │                    │ 💬 Message      │                           ││
│  │ 🧂 PasswordSalt │                    │ 📅 Date         │                           ││
│  └─────────────────┘                    │ ✅ IsRead       │                           ││
│                                         │ 📅 CreatedAt    │                           ││
│                                         └─────────────────┘                           ││
│                                                                                       ││
│                                         ┌─────────────────┐                           ││
│                                         │   Users Table   │                           ││
│                                         │                 │                           ││
│                                         │ 🔑 Id (PK)      │◄──────────────────────────┘│
│                                         │ 👤 Name         │                            │
│                                         │ 🖼️ Avatar       │                            │
│                                         │ 🟢 Status       │                            │
│                                         │ 📅 CreatedAt    │                            │
│                                         │ 🔄 UpdatedAt    │                            │
│                                         │ 🔐 PasswordHash │                            │
│                                         │ 🧂 PasswordSalt │                            │
│                                         └─────────────────┘                            │
│                                                                                         │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

**🔗 İlişki Açıklamaları:**
- **Users → Chats (UserId)**: Bir kullanıcı birden fazla mesaj gönderebilir (1:N)
- **Users → Chats (ToUserId)**: Bir kullanıcı birden fazla mesaj alabilir (1:N)
- **Chats.UserId**: Users.Id'ye referans (Foreign Key)
- **Chats.ToUserId**: Users.Id'ye referans (Foreign Key)

---

## 📈 Veri Örnekleri (Görsel)

### **Users Tablosu Örnek Veriler:**

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                Users Table Data                                        │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                         │
│ ┌─────────────────────────────────────────────────────────────────────────────────────┐ │
│ │ Id                                   │ Name      │ Email              │ Status    │ │
│ ├─────────────────────────────────────────────────────────────────────────────────────┤ │
│ │ 123e4567-e89b-12d3-a456-426614174000│ John Doe  │ john@example.com   │ online    │ │
│ │ 987fcdeb-51a2-43d1-9f12-345678901234│ Jane Smith│ jane@example.com   │ offline   │ │
│ │ 456789ab-cdef-1234-5678-901234567890│ Bob Wilson│ bob@example.com    │ busy      │ │
│ └─────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                         │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

### **Chats Tablosu Örnek Veriler:**

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                Chats Table Data                                        │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                         │
│ ┌─────────────────────────────────────────────────────────────────────────────────────┐ │
│ │ Id                                   │ UserId   │ ToUserId │ Message        │ Date  │ │
│ ├─────────────────────────────────────────────────────────────────────────────────────┤ │
│ │ 111e4567-e89b-12d3-a456-426614174000│ 123e4567 │ 987fcdeb │ "Hello!"       │ 2024- │ │
│ │ 222e4567-e89b-12d3-a456-426614174000│ 987fcdeb │ 123e4567 │ "Hi there!"    │ 2024- │ │
│ │ 333e4567-e89b-12d3-a456-426614174000│ 456789ab │ 123e4567 │ "How are you?" │ 2024- │ │
│ └─────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                         │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🎨 Görsel Veritabanı Araçları

### **1. 🌐 Web Tabanlı (Adminer)**
```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              Adminer Web Interface                                     │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                         │
│  🌐 http://localhost:8080                                                               │
│                                                                                         │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│  │ Server: sqlserver                                                               │   │
│  │ Username: sa                                                                    │   │
│  │ Password: YourStrong@Passw0rd                                                   │   │
│  │ Database: ChatTask_UserService / ChatTask_ChatService                          │   │
│  └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                         │
│  📊 Özellikler:                                                                         │
│  • Tablo yapılarını görsel olarak görüntüleme                                          │
│  • Veri sorgulama ve düzenleme                                                          │
│  • İstatistikler ve raporlar                                                            │
│  • Renkli tablo görünümleri                                                             │
│                                                                                         │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

### **2. 🖥️ Desktop (SSMS)**
```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                        SQL Server Management Studio                                    │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                         │
│  📥 İndirme: https://aka.ms/ssmsfullsetup                                              │
│                                                                                         │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│  │ Server: localhost,1433                                                          │   │
│  │ Authentication: SQL Server Authentication                                       │   │
│  │ Login: sa                                                                       │   │
│  │ Password: YourStrong@Passw0rd                                                   │   │
│  │ Trust server certificate: ✅                                                    │   │
│  └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                         │
│  🎨 Görsel Özellikler:                                                                  │
│  • Object Explorer ile tablo navigasyonu                                               │
│  • Table Designer ile tablo yapısını düzenleme                                         │
│  • Database Diagrams ile ER diagramları                                                │
│  • Query Results ile grid görünümü                                                     │
│  • Syntax Highlighting ile renkli SQL kodları                                          │
│                                                                                         │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🚀 Hızlı Başlangıç

### **En Görsel Yöntem (SSMS):**
1. **SSMS İndirin**: https://aka.ms/ssmsfullsetup
2. **Bağlanın**: localhost,1433
3. **Object Explorer'da**:
   - 📁 Databases → ChatTask_UserService → 📋 Tables → Users
   - 📁 Databases → ChatTask_ChatService → 📋 Tables → Chats
4. **Tablo Tasarımcısını Açın**: Sağ tık → Design

### **Web Tabanlı Hızlı Yöntem (Adminer):**
1. **Adminer'ı Başlatın**: `docker-compose up -d adminer`
2. **Tarayıcıda Açın**: http://localhost:8080
3. **Bağlanın**: sqlserver, sa, YourStrong@Passw0rd
4. **Veritabanını Seçin**: ChatTask_UserService veya ChatTask_ChatService

---

**💡 İpucu**: En görsel deneyim için SSMS kullanın, hızlı erişim için Adminer'ı tercih edin!
