# 🚀 نشر التطبيق على Render.com (مجاناً)

## 📋 المتطلبات

1. حساب على [Render.com](https://render.com) (مجاني)
2. حساب GitHub (لربط المستودع)
3. المستودع موجود على GitHub

## 🎯 الخطوات

### 1. إعداد المستودع على GitHub

إذا لم يكن المشروع على GitHub بعد:

```bash
# في مجلد backend_v1_3
git init
git add .
git commit -m "Initial commit - Ready for Render deployment"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
git push -u origin main
```

### 2. إنشاء حساب على Render

1. اذهب إلى [render.com](https://render.com)
2. سجّل حساب جديد (يمكن استخدام GitHub)
3. اربط حساب GitHub

### 3. إنشاء قاعدة بيانات PostgreSQL

**ملاحظة مهمة:** Render لا يوفر SQL Server مجاناً، لكن يوفر PostgreSQL. ستحتاج لتعديل Connection String.

1. في Dashboard، اضغط **"New +"** → **"PostgreSQL"**
2. اختر:
   - **Name**: `deliveryapp-db`
   - **Database**: `DeliveryApp`
   - **User**: `deliveryapp_user`
   - **Region**: اختر الأقرب لك
   - **Plan**: **Free**
3. اضغط **"Create Database"**
4. بعد الإنشاء، انسخ **Internal Database URL** (ستحتاجه لاحقاً)

### 4. نشر التطبيق

#### الطريقة 1: استخدام render.yaml (موصى به)

1. في Dashboard، اضغط **"New +"** → **"Web Service"**
2. اربط المستودع GitHub الخاص بك
3. اختر المستودع `backend_v1_3`
4. في الإعدادات:
   - **Name**: `deliveryapp-web`
   - **Environment**: `Docker`
   - **Dockerfile Path**: `Dockerfile.render`
   - **Docker Context**: `.`
   - **Plan**: **Free**
5. في **Environment Variables**، أضف:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:10000
   ```
6. أضف **Database URL** من الخطوة 3:
   ```
   ConnectionStrings__Default=<Internal Database URL من PostgreSQL>
   ```
7. اضغط **"Create Web Service"**

#### الطريقة 2: إعداد يدوي

1. في Dashboard، اضغط **"New +"** → **"Web Service"**
2. اربط المستودع GitHub
3. اختر المستودع
4. الإعدادات:
   - **Name**: `deliveryapp-web`
   - **Environment**: `Docker`
   - **Dockerfile Path**: `Dockerfile.render`
   - **Docker Context**: `.`
   - **Plan**: **Free**
5. **Environment Variables**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:10000
   ConnectionStrings__Default=<PostgreSQL Connection String>
   App__SelfUrl=https://deliveryapp-web.onrender.com
   OpenIddict__Applications__DeliveryApp_App__ClientId=DeliveryApp_App
   OpenIddict__Applications__DeliveryApp_App__ClientSecret=YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP
   OpenIddict__Applications__DeliveryApp_App__RootUrl=https://deliveryapp-web.onrender.com
   StringEncryption__DefaultPassPhrase=YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP
   JwtSettings__Issuer=https://deliveryapp-web.onrender.com
   JwtSettings__Audience=https://deliveryapp-web.onrender.com
   JwtSettings__SecretKey=YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP
   SendPulse__ClientId=Abdalhadi453@gmail.com
   SendPulse__ClientSecret=EcgCtgZcPn
   SendPulse__FromEmail=noreply@waselsy.com
   SendPulse__FromName=Waseel
   ```
6. اضغط **"Create Web Service"**

### 5. تشغيل Database Migrations

بعد نشر التطبيق، ستحتاج لتشغيل migrations:

1. في Dashboard، اذهب إلى Web Service
2. اضغط **"Shell"** (أو استخدم Render CLI)
3. شغّل:
   ```bash
   # ستحتاج لتثبيت .NET SDK أولاً
   # أو استخدم Docker container
   ```

**أو** استخدم Render CLI:

```bash
# تثبيت Render CLI
npm install -g render-cli

# تسجيل الدخول
render login

# تشغيل migrations
render exec deliveryapp-web -- dotnet DeliveryApp.DbMigrator.dll
```

### 6. الوصول للتطبيق

بعد النشر، ستحصل على URL مثل:
- `https://deliveryapp-web.onrender.com`
- `https://deliveryapp-web.onrender.com/swagger`

## ⚠️ ملاحظات مهمة

### 1. PostgreSQL بدلاً من SQL Server

Render لا يوفر SQL Server مجاناً. ستحتاج لتعديل:
- Connection String لاستخدام PostgreSQL
- قد تحتاج لتعديل بعض الكود إذا كان يستخدم SQL Server-specific features

### 2. Free Tier Limitations

- **Sleeping**: التطبيق ينام بعد 15 دقيقة من عدم الاستخدام
- **Cold Start**: أول طلب بعد النوم قد يستغرق 30-60 ثانية
- **Build Time**: قد يستغرق البناء 10-15 دقيقة

### 3. Database Migrations

ستحتاج لتشغيل migrations يدوياً بعد النشر الأول.

## 🔄 بدائل أخرى

### Railway.app (موصى به أيضاً)

1. اذهب إلى [railway.app](https://railway.app)
2. سجّل حساب (مجاني)
3. اربط GitHub
4. أنشئ **New Project** → **Deploy from GitHub repo**
5. أضف **PostgreSQL** service
6. أضف **Web Service** من Dockerfile

### Fly.io

1. اذهب إلى [fly.io](https://fly.io)
2. سجّل حساب
3. ثبت Fly CLI
4. شغّل:
   ```bash
   fly launch
   fly deploy
   ```

## 📚 موارد إضافية

- [Render Documentation](https://render.com/docs)
- [Railway Documentation](https://docs.railway.app)
- [Fly.io Documentation](https://fly.io/docs)

## 🆘 استكشاف الأخطاء

### المشكلة: Build Failed
- تحقق من Dockerfile path
- تحقق من أن جميع الملفات موجودة في المستودع

### المشكلة: Database Connection Failed
- تحقق من Connection String
- تأكد من استخدام Internal Database URL (ليس External)

### المشكلة: Application Crashes
- تحقق من Logs في Render Dashboard
- تأكد من Environment Variables

---

**ملاحظة:** إذا كنت تريد استخدام SQL Server، قد تحتاج لخدمة مدفوعة أو استخدام Azure SQL Database (لديه free tier محدود).


