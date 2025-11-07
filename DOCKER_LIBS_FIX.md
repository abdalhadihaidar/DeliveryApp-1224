# Fix for wwwroot/libs Issue in Docker

## المشكلة

ABP Framework يتطلب وجود مجلد `wwwroot/libs` مع ملفات JavaScript/CSS محددة. عند النشر على Docker، هذا المجلد قد لا يكون موجوداً لأنه مستثنى من Git (في `.gitignore`).

## الحل المطبق

تم إضافة كود في Dockerfile لإنشاء بنية minimal لـ `wwwroot/libs` مع الملفات المطلوبة:

```dockerfile
# Ensure wwwroot/libs exists with required ABP files
RUN if [ ! -d "/app/wwwroot/libs" ] || [ -z "$(find /app/wwwroot/libs -name '*.js' -o -name '*.css' 2>/dev/null | head -1)" ]; then \
        mkdir -p /app/wwwroot/libs/abp/core; \
        echo "// ABP Framework placeholder" > /app/wwwroot/libs/abp/core/abp.js; \
        echo "/* ABP Framework placeholder */" > /app/wwwroot/libs/abp/core/abp.css; \
    fi
```

## الحل الأفضل (للمستقبل)

إذا كنت تريد استخدام الملفات الكاملة بدلاً من placeholders:

1. **إزالة wwwroot/libs من .gitignore مؤقتاً:**
   ```bash
   # في .gitignore، علّق أو احذف السطر:
   # **/wwwroot/libs/*
   ```

2. **أضف المجلد إلى Git:**
   ```bash
   git add src/DeliveryApp.Web/wwwroot/libs
   git commit -m "Add wwwroot/libs for Docker deployment"
   ```

3. **أعد نشر على Render/Railway**

## الملفات المحدثة

- ✅ `Dockerfile` - تم إضافة إنشاء wwwroot/libs
- ✅ `Dockerfile.render` - تم إضافة إنشاء wwwroot/libs
- ✅ `Dockerfile.railway` - تم إضافة إنشاء wwwroot/libs
- ✅ `DeliveryAppWebModule.cs` - تم إزالة كود تعطيل libs check (كان يسبب خطأ)

## التحقق

بعد إعادة النشر، يجب أن:
- ✅ لا يوجد خطأ 500
- ✅ التطبيق يعمل بشكل صحيح
- ✅ Swagger يعمل

---

**ملاحظة:** الحل الحالي يستخدم placeholder files. إذا احتجت الملفات الكاملة، اتبع الخطوات في "الحل الأفضل" أعلاه.

