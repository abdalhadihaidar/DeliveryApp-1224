# Docker Troubleshooting Guide

## المشاكل الشائعة والحلول

### 1. SQL Server Container Unhealthy

**الأعراض:**
```
Container deliveryapp-sqlserver is unhealthy
```

**الأسباب المحتملة:**
- Healthcheck يفشل لأن `sqlcmd` غير موجود في المسار المتوقع
- SQL Server يحتاج وقت أطول للبدء

**الحل:**
تم تحديث healthcheck لاستخدام `pgrep` للتحقق من أن SQL Server process يعمل:

```yaml
healthcheck:
  test: ["CMD-SHELL", "pgrep -f sqlservr || exit 1"]
  interval: 10s
  timeout: 3s
  retries: 10
  start_period: 60s
```

### 2. Database Connection Failed

**الأعراض:**
```
Login failed for user 'sa'. Reason: Failed to open the explicitly specified database 'DeliveryApp'
```

**السبب:**
قاعدة البيانات لم يتم إنشاؤها بعد عندما يحاول التطبيق الاتصال.

**الحل:**
1. تأكد من أن DbMigrator اكتمل بنجاح:
   ```bash
   docker-compose logs dbmigrator
   ```
   
2. يجب أن ترى:
   ```
   Successfully completed all database migrations
   ```

3. إذا لم يكتمل، شغّله يدوياً:
   ```bash
   docker-compose run --rm dbmigrator
   ```

### 3. Network Packet Errors (17836)

**الأعراض:**
```
Error: 17836, Severity: 20, State: 17.
Length specified in network packet payload did not match number of bytes read
```

**السبب:**
عادة ما يكون بسبب healthcheck أو محاولات اتصال متعددة في نفس الوقت.

**الحل:**
- هذا الخطأ عادة غير ضار إذا كان SQL Server يعمل بشكل صحيح
- تأكد من أن healthcheck يعمل بشكل صحيح
- تأكد من أن التطبيق ينتظر حتى يكون SQL Server جاهزاً

### 4. DbMigrator لا يعمل

**الأعراض:**
- التطبيق لا يمكنه الاتصال بقاعدة البيانات
- DbMigrator container يخرج بسرعة

**الحل:**
1. تحقق من السجلات:
   ```bash
   docker-compose logs dbmigrator
   ```

2. تحقق من connection string في docker-compose.yml

3. شغّل DbMigrator يدوياً لرؤية الأخطاء:
   ```bash
   docker-compose run --rm dbmigrator
   ```

### 5. Web Application 500 Error

**الأعراض:**
- التطبيق يعمل لكن يعطي 500 error
- السجلات تظهر أخطاء في الاتصال بقاعدة البيانات

**الحل:**
1. تحقق من أن SQL Server healthy:
   ```bash
   docker-compose ps sqlserver
   ```

2. تحقق من أن DbMigrator اكتمل:
   ```bash
   docker-compose logs dbmigrator | grep "Successfully"
   ```

3. تحقق من سجلات التطبيق:
   ```bash
   docker-compose logs web --tail 100
   ```

4. تأكد من connection string في docker-compose.yml صحيح

### 6. Port Already in Use

**الأعراض:**
```
Error: bind: address already in use
```

**الحل:**
1. غيّر المنافذ في docker-compose.yml:
   ```yaml
   ports:
     - "5001:80"  # بدلاً من 5000:80
   ```

2. أو أوقف الخدمة التي تستخدم المنفذ:
   ```bash
   # Windows
   netstat -ano | findstr :5000
   taskkill /PID <PID> /F
   ```

### 7. Healthcheck لا يعمل

**الأعراض:**
- SQL Server يعمل لكن healthcheck يفشل دائماً

**الحل:**
استخدم healthcheck بديل:

```yaml
healthcheck:
  # الطريقة 1: التحقق من process
  test: ["CMD-SHELL", "pgrep -f sqlservr || exit 1"]
  
  # الطريقة 2: استخدام sqlcmd (إذا كان موجوداً)
  # test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123 -C -Q 'SELECT 1' || exit 1"]
  
  # الطريقة 3: التحقق من المنفذ (يتطلب nc)
  # test: ["CMD-SHELL", "nc -z localhost 1433 || exit 1"]
```

## أوامر مفيدة للتشخيص

```bash
# عرض حالة جميع الخدمات
docker-compose ps

# عرض سجلات خدمة محددة
docker-compose logs -f sqlserver
docker-compose logs -f dbmigrator
docker-compose logs -f web

# إعادة تشغيل خدمة
docker-compose restart web

# إعادة بناء صورة
docker-compose build --no-cache web

# حذف كل شيء والبدء من جديد
docker-compose down -v
docker-compose up -d --build

# الوصول إلى shell داخل container
docker-compose exec web bash
docker-compose exec sqlserver bash

# اختبار الاتصال بقاعدة البيانات من داخل container
docker-compose exec web dotnet ef dbcontext info
```

## التحقق من نجاح الإعداد

1. **SQL Server يعمل:**
   ```bash
   docker-compose ps sqlserver
   # يجب أن يكون STATUS: Up X minutes (healthy)
   ```

2. **DbMigrator اكتمل:**
   ```bash
   docker-compose logs dbmigrator | grep "Successfully"
   # يجب أن ترى: "Successfully completed all database migrations"
   ```

3. **Web Application يعمل:**
   - افتح: http://localhost:5000
   - يجب أن ترى صفحة التطبيق أو Swagger UI

4. **الاتصال بقاعدة البيانات:**
   ```bash
   docker-compose exec web dotnet ef dbcontext info
   ```

## نصائح إضافية

1. **في حالة فشل مستمر:**
   - احذف volumes: `docker-compose down -v`
   - أعد البناء: `docker-compose build --no-cache`
   - ابدأ من جديد: `docker-compose up -d`

2. **للإنتاج:**
   - استخدم كلمات مرور قوية
   - استخدم secrets management
   - فعّل HTTPS
   - استخدم قاعدة بيانات خارجية (غير Docker)

3. **للتطوير:**
   - استخدم docker-compose.override.yml
   - فعّل verbose logging
   - استخدم hot reload إذا أمكن


