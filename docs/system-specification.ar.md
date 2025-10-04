# المواصفات النظامية (العربية)

## 1) الهدف والنطاق
يوثّق هذا المستند متطلبات DeliveryApp الوظيفية وغير الوظيفية، وبنية النظام، ونموذج البيانات، والتكاملات، وتدفّقات التشغيل. الفئة المستهدفة: المهندسون ومالكو المنتج وأصحاب المصلحة.

## 2) المتطلبات الوظيفية
- إدارة المستخدمين: تسجيل/تسجيل دخول، أدوار (admin/manager/restaurant_owner/delivery/customer)، ملفات شخصية
- إدارة المطاعم: إنشاء/تحديث المطاعم، الفئات، القوائم، العروض الخاصة
- الطلبات: تصفّح القوائم، إنشاء طلب، إضافة عناصر/خيارات، تتبّع الحالة
- المدفوعات: بطاقات Stripe وحسابات متصلة/تحويلات؛ الدفع عند التسليم (COD)
- الإعلانات: طلبات إعلان من المالكين (AdRequest) ومراجعتها والموافقة عليها (Advertisement)
- الإشعارات: البريد (SendPulse) والدفعية (Firebase)
- الدردشة/الدعم: جلسات ورسائل بين الإدارة والعملاء/المندوبين
- التقارير: الأداء التشغيلي والمالي

## 3) متطلبات غير وظيفية
- الأمان: OIDC/OAuth2 عبر OpenIddict، تفويض بالأدوار، إدارة أسرار، HTTPS، سجلات تدقيق
- الأداء: قاعدة بيانات قابلة للتوسّع، وظائف خلفية، ذاكرة تخزين مؤقت (Redis اختياري)
- الاعتمادية: محاولات إعادة للاتصالات الخارجية، Webhooks عديمة الأثر، سجلات عبر Serilog
- المراقبة: سجلات منظّمة، نقاط صحّة (للإضافة)، Telemetry (للإضافة)

## 4) البنية العامة
```mermaid
flowchart LR
  Client[ويب/بليزر/جوال] -->|HTTPS| WebApp[DeliveryApp.Web]
  Client -->|HTTPS| ApiHost[DeliveryApp.HttpApi.Host]
  subgraph طبقة التطبيق
    AppServices[خدمات التطبيق]
  end
  WebApp <-->|وحدات ABP| AppServices
  ApiHost <-->|وحدات ABP| AppServices
  AppServices --> Db[(SQL Server)]
  AppServices --> Stripe[(Stripe API)]
  AppServices --> SendPulse[(SendPulse API)]
  AppServices --> Firebase[(Firebase Admin)]
  Redis[(Redis)] --- AppServices
```

## 5) نموذج المجال (مختصر)
```mermaid
classDiagram
  class Restaurant {+Guid Id
    +string Name
    +decimal? CommissionRate
    +Address Address
    +List~MenuItem~ Menu
    +Guid? CategoryId}
  class MealCategory {+Guid Id +string Name +Guid RestaurantId}
  class Order {+Guid Id +Guid UserId +Guid RestaurantId
    +List~OrderItem~ Items +string Status +string PaymentStatus}
  class PaymentTransaction {+int Id +Guid OrderId +string PaymentIntentId +decimal Amount +string Status}
  class Advertisement {+Guid Id +Guid? RestaurantId +Guid? CreatedById}
  class AdRequest {+Guid Id +Guid RestaurantId +Guid? ReviewedById +Guid? AdvertisementId}
  class ChatSession {+Guid Id +string DeliveryId +string CustomerId +bool IsActive}
  class ChatMessage {+Guid Id +Guid SessionId +string SenderId +string Content}

  Restaurant --> MealCategory
  Restaurant --> Order
  Order --> OrderItem
  Order --> PaymentTransaction
  AdRequest --> Advertisement
  ChatSession --> ChatMessage
```

## 6) التدفقات الرئيسية

### 6.1 التسجيل وتسجيل الدخول (OpenIddict)
```mermaid
sequenceDiagram
  participant U as مستخدم
  participant W as ويب/واجهة برمجية
  participant O as OpenIddict
  participant DB as قاعدة البيانات
  U->>W: طلب تسجيل/تسجيل دخول
  W->>O: تفويض/رمز وصول
  O->>DB: التحقق من الاعتمادات
  O-->>W: رموز الوصول/الهوية
  W-->>U: جلسة مصادقة
```

### 6.2 إنشاء طلب والدفع (Stripe)
```mermaid
sequenceDiagram
  participant C as عميل
  participant API as API
  participant OS as خدمة الطلب
  participant PS as خدمة الدفع Stripe
  participant ST as Stripe
  participant DB as قاعدة البيانات
  C->>API: إنشاء طلب
  API->>OS: حفظ الطلب (Pending Payment)
  OS->>DB: إدراج الطلب
  C->>API: الدفع (المبلغ/الطريقة)
  API->>PS: إنشاء PaymentIntent
  PS->>ST: إنشاء PaymentIntent
  ST-->>PS: ClientSecret/Status
  PS->>DB: حفظ PaymentTransaction
  API-->>C: ClientSecret
  ST-->>API: Webhook (succeeded/failed)
  API->>PS: HandleWebhook
  PS->>DB: تحديث حالة الدفع/الطلب
```

### 6.3 الدفع عند التسليم (COD)
```mermaid
sequenceDiagram
  participant C as عميل
  participant API as API
  participant OS as خدمة الطلب
  participant DB as قاعدة البيانات
  C->>API: إنشاء طلب (COD)
  API->>OS: حفظ الطلب (PaymentStatus=Pending)
  OS->>DB: إدراج الطلب
  Note over API,DB: التسوية لاحقاً عبر كيان CODTransaction
```

### 6.4 سير عمل طلب الإعلان
```mermaid
sequenceDiagram
  participant RO as مالك مطعم
  participant API as API
  participant ADS as خدمة AdRequest
  participant ADM as مشرف
  participant DB as قاعدة البيانات
  RO->>API: تقديم AdRequest
  API->>ADS: تحقق/حفظ
  ADS->>DB: إدراج AdRequest
  ADM->>API: مراجعة/موافقة
  API->>ADS: موافقة
  ADS->>DB: ربط Advertisement
```

### 6.5 الإشعارات (SendPulse/Firebase)
```mermaid
sequenceDiagram
  participant APP as خدمة التطبيق
  participant SP as SendPulse
  participant FB as Firebase
  participant DB as قاعدة البيانات
  APP->>SP: إرسال بريد (رمز وصول)
  APP->>FB: إرسال إشعار فوري
  APP->>DB: تتبّع الحالة
```

## 7) اصطلاحات الـ API
- REST ضمن `DeliveryApp.HttpApi` و`DeliveryApp.Web`
- التحكّم بالأدوار عبر السمات `[Authorize(Roles = ...)]`
- Webhook لStripe مع تحقق التوقيع

## 8) الإعدادات
- `appsettings.*.json` لكل مشروع وبيئة
- الأسرار عبر متغيرات البيئة أو مدير أسرار

## 9) النشر
- سكربتات Windows/IIS واستضافة مشتركة متوفرة
- استخدم DbMigrator للهجرات وتعبئة البيانات

## 10) المخاطر والضوابط
- تدوير الأسرار، التحقق من المُدخلات، تحديد المعدل لمحاولات تسجيل الدخول والدفع، تفعيل نقاط الصحّة
