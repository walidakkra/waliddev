# WpfTaskManager (Transport Cost Tracker)

تطبيق WPF بلغة C# لاحتساب كلفة التنقل اليومية حسب نوع اليوم:
- يوم كامل: 80 درهم (قابلة للتعديل من الإعدادات)
- نصف يوم: 40 درهم (قابلة للتعديل من الإعدادات)

## المزايا
- إضافة سجل تنقل جديد (الاسم، التاريخ، نوع اليوم، ملاحظات).
- احتساب الكلفة تلقائياً حسب نوع اليوم.
- بحث مباشر بالاسم.
- شاشة إعدادات سريعة لتعديل كلفة يوم كامل / نصف يوم.
- عرض إجماليات فورية:
  - مجموع كلفة الأيام الكاملة.
  - مجموع كلفة أنصاف الأيام.
  - الإجمالي العام بالدرهم.
- حذف سجل محدد.
- حفظ تلقائي للبيانات والإعدادات بصيغة JSON.

## هيكلة المشروع
- `WpfTaskManager/Models/TransportRecord.cs`: نموذج سجل التنقل.
- `WpfTaskManager/Models/AppSettings.cs`: إعدادات الكلفة.
- `WpfTaskManager/Models/AppData.cs`: كائن بيانات التخزين.
- `WpfTaskManager/Services/JsonStorageService.cs`: التخزين المحلي وقراءة/كتابة JSON.
- `WpfTaskManager/ViewModels/MainViewModel.cs`: منطق MVVM للإضافة/البحث/الاحتساب.
- `WpfTaskManager/MainWindow.xaml`: واجهة المستخدم.

## التشغيل
> يتطلب .NET SDK على ويندوز.

```bash
cd WpfTaskManager
dotnet restore
dotnet run
```

## مسار حفظ البيانات
- `%LOCALAPPDATA%/WpfTaskManager/transport-data.json`
