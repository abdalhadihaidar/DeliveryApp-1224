# 🌐 Localization Guide - Delivery Fee Management System

## Overview

The delivery fee management system has been fully localized to support both **Arabic** and **English** languages across all components. This guide covers the localization implementation and how to manage translations.

## 🎯 Localization Coverage

### **✅ Dashboard (Angular)**
- Settings page with all delivery fee controls
- Delivery fee management component
- Real-time testing interface
- Error messages and notifications
- Form validation messages

### **✅ Mobile App (Flutter)**
- Delivery fee widget
- Delivery fee preview widget
- Service layer messages
- Error handling and user feedback
- All UI components and labels

### **✅ Backend (C#)**
- API response messages
- Error codes and descriptions
- System settings descriptions
- Validation messages

## 📁 Translation File Structure

### **Dashboard Translations**
```
delivery-dashboard/src/assets/translations/
├── ar.json          # Arabic translations
└── en.json          # English translations
```

### **Mobile App Translations**
```
frontend_v1_3/lib/services/
└── localization_service.dart    # Flutter localization service
```

## 🔧 Dashboard Localization

### **Translation Service**
The dashboard uses a custom `TranslationService` that:
- Loads translations from JSON files
- Supports nested keys with dot notation
- Handles RTL/LTR layout switching
- Provides fallback to key names

### **Usage in Components**
```typescript
// In component template
{{ translationService.translate('deliveryFeeManagement') }}

// With fallback
{{ translationService.translate('deliveryFeeManagement') || 'إدارة رسوم التوصيل' }}

// RTL support
[class.rtl]="translationService.isRTL()"
```

### **Translation Keys Added**

#### **New Delivery Fee Settings**
```json
{
  "inTownDistanceThreshold": "حد المسافة داخل المدينة",
  "inTownBaseFee": "الرسوم الأساسية داخل المدينة",
  "outOfTownBaseFee": "الرسوم الأساسية خارج المدينة",
  "outOfTownRatePerKm": "معدل الرسوم لكل كيلومتر خارج المدينة",
  "rushDeliveryFee": "رسوم التوصيل السريع",
  "minimumOrderAmount": "الحد الأدنى لمبلغ الطلب"
}
```

#### **Delivery Fee Management Component**
```json
{
  "deliveryFeeManagement": "إدارة رسوم التوصيل",
  "testCalculation": "اختبار الحساب",
  "calculationResult": "نتيجة الحساب",
  "finalDeliveryFee": "رسوم التوصيل النهائية",
  "baseFee": "الرسوم الأساسية",
  "cityType": "نوع المدينة",
  "inTown": "داخل المدينة",
  "outOfTown": "خارج المدينة",
  "freeDelivery": "توصيل مجاني",
  "detailedBreakdown": "التفصيل المفصل"
}
```

## 📱 Mobile App Localization

### **Localization Service**
The Flutter app uses a custom `LocalizationService` that:
- Supports Arabic and English
- Provides RTL/LTR layout support
- Handles parameter substitution
- Offers easy-to-use extension methods

### **Usage in Widgets**
```dart
// Using extension method
Text('deliveryOptions'.tr())

// With parameters
Text('welcomeMessage'.tr(args: {'name': userName}))

// RTL support
Directionality(
  textDirection: LocalizationService.isRTL ? TextDirection.rtl : TextDirection.ltr,
  child: widget,
)
```

### **Translation Categories**

#### **Delivery Fee Management**
```dart
'deliveryFeeManagement': 'إدارة رسوم التوصيل',
'deliveryOptions': 'خيارات التوصيل',
'standardDelivery': 'توصيل عادي',
'expressDelivery': 'توصيل سريع',
'freeDelivery': 'توصيل مجاني',
'calculate': 'حساب',
'loading': 'جاري التحميل...',
```

#### **Error Messages**
```dart
'errorCalculatingFee': 'خطأ في حساب الرسوم',
'errorLoadingOptions': 'خطأ في تحميل الخيارات',
'errorNetworkConnection': 'خطأ في الاتصال بالشبكة',
'retry': 'إعادة المحاولة',
'noOptionsAvailable': 'لا توجد خيارات متاحة',
```

## 🎨 RTL/LTR Support

### **Dashboard RTL Support**
```typescript
// CSS classes for RTL
.rtl {
  direction: rtl;
}

.rtl .header-content {
  flex-direction: row-reverse;
}

.rtl .toggle-item {
  flex-direction: row-reverse;
}
```

### **Mobile App RTL Support**
```dart
// RTL detection
bool get isRTL => LocalizationService.isRTL;

// Directionality widget
Directionality(
  textDirection: LocalizationService.isRTL ? TextDirection.rtl : TextDirection.ltr,
  child: content,
)
```

## 🔄 Language Switching

### **Dashboard Language Switching**
```typescript
// Switch language
this.translationService.setLanguage('ar'); // Arabic
this.translationService.setLanguage('en'); // English

// Get current language
const currentLang = this.translationService.getCurrentLanguage();
```

### **Mobile App Language Switching**
```dart
// Switch language
LocalizationService.setLanguageCode('ar'); // Arabic
LocalizationService.setLanguageCode('en'); // English

// Get current language
final currentLang = LocalizationService.currentLanguageCode;
```

## 📝 Adding New Translations

### **Dashboard (Angular)**

1. **Add to Arabic file** (`ar.json`):
```json
{
  "newTranslationKey": "الترجمة العربية الجديدة"
}
```

2. **Add to English file** (`en.json`):
```json
{
  "newTranslationKey": "New English Translation"
}
```

3. **Use in component**:
```typescript
{{ translationService.translate('newTranslationKey') }}
```

### **Mobile App (Flutter)**

1. **Add to localization service**:
```dart
static const Map<String, String> _arabicTranslations = {
  'newTranslationKey': 'الترجمة العربية الجديدة',
  // ... other translations
};

static const Map<String, String> _englishTranslations = {
  'newTranslationKey': 'New English Translation',
  // ... other translations
};
```

2. **Use in widget**:
```dart
Text('newTranslationKey'.tr())
```

## 🎯 Best Practices

### **1. Translation Key Naming**
- Use descriptive, hierarchical keys
- Use camelCase for consistency
- Group related keys together
- Use prefixes for categories (e.g., `deliveryFee_`, `error_`)

### **2. Text Content**
- Keep translations concise but clear
- Avoid hardcoded text in components
- Use placeholders for dynamic content
- Test with different text lengths

### **3. RTL Considerations**
- Test layouts in both directions
- Use flexible layouts (Flex, Grid)
- Consider icon and image mirroring
- Test with long Arabic text

### **4. Parameter Substitution**
```dart
// Good: Use parameters for dynamic content
'welcomeMessage'.tr(args: {'name': userName})

// Avoid: String concatenation
'Welcome ' + userName + '!'
```

## 🔍 Testing Localization

### **Dashboard Testing**
1. Switch between Arabic and English
2. Test RTL layout in Arabic
3. Verify all text is translated
4. Check form validation messages
5. Test error scenarios

### **Mobile App Testing**
1. Test language switching
2. Verify RTL layout
3. Test with different screen sizes
4. Check parameter substitution
5. Test offline scenarios

## 📊 Translation Coverage

### **Dashboard Components**
- ✅ Settings Page (100% translated)
- ✅ Delivery Fee Management (100% translated)
- ✅ Error Messages (100% translated)
- ✅ Form Validation (100% translated)
- ✅ Notifications (100% translated)

### **Mobile App Components**
- ✅ Delivery Fee Widget (100% translated)
- ✅ Delivery Fee Preview (100% translated)
- ✅ Service Layer (100% translated)
- ✅ Error Handling (100% translated)
- ✅ User Feedback (100% translated)

## 🚀 Future Enhancements

### **1. Advanced Features**
- Pluralization support
- Date/time formatting
- Number formatting
- Currency formatting
- Dynamic language loading

### **2. Translation Management**
- Translation file validation
- Missing key detection
- Translation completeness reports
- Automated translation suggestions

### **3. Performance Optimization**
- Lazy loading of translations
- Caching strategies
- Bundle size optimization
- Runtime language switching

## 📋 Maintenance Checklist

### **Regular Tasks**
- [ ] Review new features for translation needs
- [ ] Update translation files with new keys
- [ ] Test RTL/LTR layouts
- [ ] Verify parameter substitution
- [ ] Check error message translations

### **Release Tasks**
- [ ] Validate all translation files
- [ ] Test language switching
- [ ] Verify RTL layout
- [ ] Check mobile app translations
- [ ] Update documentation

## 🎯 Conclusion

The delivery fee management system is fully localized with:

- ✅ **Complete Arabic/English Support**
- ✅ **RTL/LTR Layout Support**
- ✅ **Comprehensive Translation Coverage**
- ✅ **Easy Translation Management**
- ✅ **Consistent User Experience**

The localization system is production-ready and provides a seamless multilingual experience for both administrators and end users.
