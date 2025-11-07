# 🐳 Docker Quick Start Guide

## البدء السريع

### Windows
```bash
docker-start.bat
```

### Linux/Mac
```bash
chmod +x docker-start.sh
./docker-start.sh
```

### أو يدوياً
```bash
docker-compose up -d
```

## 📋 الملفات المهمة

- `Dockerfile` - صورة Docker للتطبيق الرئيسي
- `Dockerfile.DbMigrator` - صورة Docker لـ Database Migrator
- `docker-compose.yml` - إعدادات Docker Compose الأساسية
- `docker-compose.prod.yml` - إعدادات الإنتاج
- `.dockerignore` - الملفات المستثناة من البناء

## 🔧 الأوامر الأساسية

### تشغيل التطبيق
```bash
docker-compose up -d
```

### عرض السجلات
```bash
docker-compose logs -f web
```

### إيقاف التطبيق
```bash
docker-compose down
```

### إعادة بناء الصور
```bash
docker-compose build --no-cache
```

### إعادة تشغيل خدمة
```bash
docker-compose restart web
```

## 🌐 الوصول للتطبيق

- **Web App**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **SQL Server**: localhost:1433

## ⚙️ الإعدادات

### تغيير كلمة مرور قاعدة البيانات

عدّل في `docker-compose.yml`:
```yaml
sqlserver:
  environment:
    - SA_PASSWORD=YourNewPassword123
```

### استخدام متغيرات البيئة

1. أنشئ ملف `.env.docker` (انظر `env.docker.example`)
2. شغّل: `docker-compose --env-file .env.docker up -d`

## 📚 للمزيد من التفاصيل

راجع `DOCKER_README.md` للدليل الكامل.


