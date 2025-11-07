# Docker Setup Guide - DeliveryApp Backend

## 📋 المتطلبات

- Docker Desktop (Windows/Mac) أو Docker Engine (Linux)
- Docker Compose
- على الأقل 4GB من الذاكرة المتاحة

## 🚀 التشغيل السريع

### 1. تشغيل جميع الخدمات (SQL Server + Migrator + Web)

```bash
docker-compose up -d
```

### 2. عرض السجلات

```bash
# جميع الخدمات
docker-compose logs -f

# خدمة محددة
docker-compose logs -f web
docker-compose logs -f dbmigrator
docker-compose logs -f sqlserver
```

### 3. إيقاف الخدمات

```bash
docker-compose down
```

### 4. إيقاف الخدمات مع حذف البيانات

```bash
docker-compose down -v
```

## 🔧 الإعدادات

### تغيير كلمة مرور قاعدة البيانات

قم بتعديل `SA_PASSWORD` في ملف `docker-compose.yml`:

```yaml
sqlserver:
  environment:
    - SA_PASSWORD=YourNewStrongPassword123
```

### استخدام متغيرات البيئة

1. انسخ ملف `.env.docker.example` إلى `.env.docker`
2. قم بتعديل القيم حسب احتياجاتك
3. استخدم الأمر:

```bash
docker-compose --env-file .env.docker up -d
```

### إعدادات التطوير

1. انسخ `docker-compose.override.yml.example` إلى `docker-compose.override.yml`
2. قم بتعديل الإعدادات حسب احتياجاتك
3. سيتم تحميل الملف تلقائياً عند استخدام `docker-compose up`

## 📦 الخدمات المتوفرة

### 1. SQL Server
- **Port**: 1433
- **Username**: sa
- **Password**: (كما هو محدد في docker-compose.yml)
- **Database**: DeliveryApp (سيتم إنشاؤها تلقائياً)

### 2. Database Migrator
- يعمل تلقائياً بعد تشغيل SQL Server
- يقوم بتطبيق الهجرات وإنشاء قاعدة البيانات

### 3. Web Application
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001 (يتطلب شهادة SSL)
- **Swagger**: http://localhost:5000/swagger

## 🔍 الأوامر المفيدة

### إعادة بناء الصور

```bash
docker-compose build --no-cache
```

### إعادة بناء خدمة محددة

```bash
docker-compose build --no-cache web
```

### تشغيل أوامر داخل الحاوية

```bash
# الوصول إلى shell في حاوية web
docker-compose exec web bash

# تشغيل أمر migrator يدوياً
docker-compose run --rm dbmigrator
```

### عرض حالة الخدمات

```bash
docker-compose ps
```

### إعادة تشغيل خدمة

```bash
docker-compose restart web
```

## 🗄️ إدارة قاعدة البيانات

### الاتصال بقاعدة البيانات من خارج Docker

استخدم أي أداة مثل:
- SQL Server Management Studio (SSMS)
- Azure Data Studio
- DBeaver

**Connection String:**
```
Server=localhost,1433
Database=DeliveryApp
User Id=sa
Password=YourStrong@Password123
TrustServerCertificate=True
```

### نسخ احتياطي لقاعدة البيانات

```bash
# إنشاء نسخة احتياطية
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Password123 \
  -Q "BACKUP DATABASE DeliveryApp TO DISK='/var/opt/mssql/backup/DeliveryApp.bak'"
```

## 🐛 استكشاف الأخطاء

### المشكلة: لا يمكن الاتصال بقاعدة البيانات

**الحل:**
1. تأكد من أن SQL Server يعمل: `docker-compose ps`
2. تحقق من السجلات: `docker-compose logs sqlserver`
3. انتظر حتى تكتمل health check (30 ثانية)

### المشكلة: فشل Migrator

**الحل:**
1. تحقق من السجلات: `docker-compose logs dbmigrator`
2. تأكد من أن SQL Server جاهز: `docker-compose ps sqlserver`
3. شغّل Migrator يدوياً: `docker-compose run --rm dbmigrator`

### المشكلة: التطبيق لا يعمل

**الحل:**
1. تحقق من السجلات: `docker-compose logs web`
2. تأكد من أن Migrator اكتمل بنجاح
3. تحقق من متغيرات البيئة: `docker-compose config`

### المشكلة: مشاكل في الصلاحيات

**الحل:**
على Linux/Mac، قد تحتاج لتعديل صلاحيات المجلدات:

```bash
sudo chown -R $USER:$USER ./logs
sudo chown -R $USER:$USER ./wwwroot/uploads
```

## 📝 ملاحظات مهمة

1. **الشهادات (Certificates)**: إذا كنت تحتاج لشهادة OpenIddict (`openiddict.pfx`)، قم بإضافتها إلى مجلد `src/DeliveryApp.Web/` قبل البناء.

2. **الملفات المرفوعة**: المجلد `wwwroot/uploads` يتم مشاركته كـ volume لضمان استمرارية الملفات المرفوعة.

3. **السجلات**: المجلد `logs` يتم مشاركته كـ volume لسهولة الوصول إلى السجلات.

4. **الأمان**: في بيئة الإنتاج، يجب:
   - تغيير كلمات المرور الافتراضية
   - استخدام secrets management
   - تفعيل HTTPS بشكل صحيح
   - تأمين الاتصال بقاعدة البيانات

## 🔐 الإنتاج

للإنتاج، يجب:

1. استخدام Docker secrets أو متغيرات بيئة آمنة
2. تفعيل HTTPS مع شهادات صحيحة
3. استخدام قاعدة بيانات خارجية (غير Docker)
4. إعداد monitoring و logging مناسب
5. استخدام reverse proxy (nginx/traefik)

## 📚 موارد إضافية

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [SQL Server on Linux](https://docs.microsoft.com/en-us/sql/linux/)


