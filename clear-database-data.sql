-- ChatTask Veritabanları Veri Temizleme Scripti
-- Bu script tüm tablolardaki verileri temizler, tabloları silmez

-- ChatTask_UserService veritabanı
USE [ChatTask_UserService]
GO

-- Foreign key constraint'leri geçici olarak devre dışı bırak
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
GO

-- Tüm tablolardaki verileri temizle
EXEC sp_MSforeachtable "DELETE FROM ?"
GO

-- Foreign key constraint'leri tekrar aktif et
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"
GO

-- Identity sütunlarını sıfırla (eğer varsa)
EXEC sp_MSforeachtable "DBCC CHECKIDENT ('?', RESEED, 0)"
GO

PRINT 'ChatTask_UserService veritabanı temizlendi'
GO

-- ChatTask_TaskService veritabanı
USE [ChatTask_TaskService]
GO

-- Foreign key constraint'leri geçici olarak devre dışı bırak
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
GO

-- Tüm tablolardaki verileri temizle
EXEC sp_MSforeachtable "DELETE FROM ?"
GO

-- Foreign key constraint'leri tekrar aktif et
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"
GO

-- Identity sütunlarını sıfırla (eğer varsa)
EXEC sp_MSforeachtable "DBCC CHECKIDENT ('?', RESEED, 0)"
GO

PRINT 'ChatTask_TaskService veritabanı temizlendi'
GO

-- ChatTask_ChatService veritabanı (LocalDB)
USE [ChatTask_ChatService]
GO

-- Foreign key constraint'leri geçici olarak devre dışı bırak
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
GO

-- Tüm tablolardaki verileri temizle
EXEC sp_MSforeachtable "DELETE FROM ?"
GO

-- Foreign key constraint'leri tekrar aktif et
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"
GO

-- Identity sütunlarını sıfırla (eğer varsa)
EXEC sp_MSforeachtable "DBCC CHECKIDENT ('?', RESEED, 0)"
GO

PRINT 'ChatTask_ChatService veritabanı temizlendi'
GO

PRINT 'Tüm veritabanları başarıyla temizlendi!'
