# WpfTaskManager

تطبيق WPF كامل (MVP) بلغة C# لإدارة المهام اليومية.

## المزايا
- إضافة مهمة مع عنوان وملاحظات.
- عرض المهام في قائمة مع تاريخ الإضافة.
- تحديد مهمة وتبديل حالتها (مكتملة / غير مكتملة).
- حذف مهمة محددة.
- حذف جميع المهام المكتملة.
- عدادات فورية (إجمالي / مكتملة / متبقية).
- حفظ تلقائي للبيانات محليًا بصيغة JSON داخل مجلد المستخدم.

## هيكلة المشروع
- `WpfTaskManager/Models/TaskItem.cs`: نموذج بيانات المهمة.
- `WpfTaskManager/Services/JsonStorageService.cs`: التخزين المحلي وقراءة/كتابة JSON.
- `WpfTaskManager/ViewModels/*`: منطق MVVM والأوامر.
- `WpfTaskManager/MainWindow.xaml`: واجهة المستخدم.

## التشغيل
> يتطلب .NET SDK على ويندوز.

```bash
cd WpfTaskManager
dotnet restore
dotnet run
```

## ملاحظات تقنية
- يستخدم نمط MVVM.
- البيانات تحفظ في:
  - `%LOCALAPPDATA%/WpfTaskManager/tasks.json`
