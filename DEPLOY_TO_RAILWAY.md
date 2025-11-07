# 🚂 نشر التطبيق على Railway.app (مجاناً)

Railway.app يوفر خيار أفضل لأنه يدعم SQL Server و PostgreSQL.

## 📋 المتطلبات

1. حساب على [Railway.app](https://railway.app) (مجاني)
2. حساب GitHub
3. المستودع على GitHub

## 🎯 الخطوات

### 1. إعداد المستودع على GitHub

```bash
git init
git add .
git commit -m "Ready for Railway deployment"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
git push -u origin main
```

### 2. إنشاء حساب على Railway

1. اذهب إلى [railway.app](https://railway.app)
2. اضغط **"Start a New Project"**
3. اختر **"Deploy from GitHub repo"**
4. سجّل دخول بحساب GitHub
5. اربط المستودع

### 3. إضافة Web Service (بدون قاعدة بيانات - سنستخدم Somee)

1. في المشروع، اضغط **"+ New"**
2. اختر **"GitHub Repo"**
3. اختر المستودع `backend_v1_3`
4. Railway سيكتشف Dockerfile تلقائياً
5. في **Settings** → **Variables**، أضف:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:${PORT}
ConnectionStrings__Default=workstation id=waseelsy.mssql.somee.com;packet size=4096;user id=aca_SQLLogin_1;pwd=12345678;data source=waseelsy.mssql.somee.com;persist security info=False;initial catalog=waseelsy;TrustServerCertificate=True
App__SelfUrl=${RAILWAY_PUBLIC_DOMAIN}
OpenIddict__Applications__DeliveryApp_App__ClientId=DeliveryApp_App
OpenIddict__Applications__DeliveryApp_App__ClientSecret=YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP
OpenIddict__Applications__DeliveryApp_App__RootUrl=${RAILWAY_PUBLIC_DOMAIN}
StringEncryption__DefaultPassPhrase=YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP
JwtSettings__Issuer=${RAILWAY_PUBLIC_DOMAIN}
JwtSettings__Audience=${RAILWAY_PUBLIC_DOMAIN}
JwtSettings__SecretKey=YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP
SendPulse__ClientId=Abdalhadi453@gmail.com
SendPulse__ClientSecret=EcgCtgZcPn
SendPulse__FromEmail=noreply@waselsy.com
SendPulse__FromName=Waseel
```

### 5. تشغيل Migrations

1. في Web Service، اضغط **"Deployments"**
2. بعد اكتمال البناء، اضغط **"View Logs"**
3. أو استخدم Railway CLI:

```bash
# تثبيت Railway CLI
npm install -g @railway/cli

# تسجيل الدخول
railway login

# تشغيل migrations
railway run dotnet DeliveryApp.DbMigrator.dll
```

### 6. الوصول للتطبيق

Railway سيعطيك URL مثل:
- `https://deliveryapp-production.up.railway.app`
- `https://deliveryapp-production.up.railway.app/swagger`

## ⚠️ ملاحظات

- **Free Tier**: 500 ساعة/شهر مجاناً
- **Sleeping**: لا ينام تلقائياً (مثل Render)
- **Custom Domain**: يمكن إضافة domain مخصص مجاناً

## 🔄 بدائل

راجع `DEPLOY_TO_RENDER.md` للبدائل الأخرى.


