# Docker Build Fixes

## المشكلة التي تم إصلاحها

### الخطأ الأصلي
```
error MSB3202: The project file "/src/src/DeliveryApp.Web/DeliveryApp.Web.csproj" was not found.
error MSB3202: The project file "/src/test/DeliveryApp.Application.Tests/DeliveryApp.Application.Tests.csproj" was not found.
```

### السبب
كان Dockerfile يحاول عمل `dotnet restore "DeliveryApp.sln"` الذي يحتوي على جميع المشاريع في الحل، بما في ذلك مشاريع الاختبار والمشاريع الأخرى غير المنسوخة إلى Docker image.

### الحل
تم تغيير استراتيجية الـ restore لاستعادة كل مشروع على حدة بدلاً من استخدام `.sln`:

**قبل:**
```dockerfile
RUN dotnet restore "DeliveryApp.sln"
```

**بعد:**
```dockerfile
RUN dotnet restore "src/DeliveryApp.Domain.Shared/DeliveryApp.Domain.Shared.csproj" && \
    dotnet restore "src/DeliveryApp.Domain/DeliveryApp.Domain.csproj" && \
    dotnet restore "src/DeliveryApp.Application.Contracts/DeliveryApp.Application.Contracts.csproj" && \
    dotnet restore "src/DeliveryApp.Application/DeliveryApp.Application.csproj" && \
    dotnet restore "src/DeliveryApp.EntityFrameworkCore/DeliveryApp.EntityFrameworkCore.csproj" && \
    dotnet restore "src/DeliveryApp.HttpApi/DeliveryApp.HttpApi.csproj" && \
    dotnet restore "src/DeliveryApp.Web/DeliveryApp.Web.csproj"
```

## تحسينات إضافية

1. **إزالة `version` من docker-compose.yml** - تمت إزالتها لأنها أصبحت obsolete في Docker Compose الحديث
2. **ترتيب المشاريع حسب التبعيات** - يتم restore المشاريع بالترتيب الصحيح للتبعيات

## الملفات المعدلة

- ✅ `Dockerfile` - تم إصلاح restore
- ✅ `Dockerfile.DbMigrator` - تم إصلاح restore
- ✅ `docker-compose.yml` - تمت إزالة `version`

## كيفية الاختبار

```bash
# إعادة بناء الصور بدون cache
docker-compose build --no-cache

# تشغيل الخدمات
docker-compose up -d
```

## ملاحظات

- البناء قد يستغرق وقتاً أطول قليلاً لأننا نستعيد كل مشروع على حدة
- هذا الحل أكثر موثوقية لأنه لا يعتمد على `.sln` الذي قد يحتوي على مشاريع غير موجودة


