namespace Project.Core.Enums
{
    public enum VerificationStatus
    {
        Pending,   // تم رفع الأوراق وبانتظار الموافقة
        Verified,  // تم تفعيل الحساب (يعمل بكامل الصلاحيات)
        Rejected,  // تم رفض الأوراق (يحتاج تعديل)
        Suspended, // تم إيقاف الحساب لمخالفة
        Approved   // (ممكن تكون زي Verified أو مرحلة مبدئية حسب البيزنس بتاعك)
    }


}
