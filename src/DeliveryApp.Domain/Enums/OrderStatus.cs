namespace DeliveryApp.Domain.Enums
{
    // Represents the lifecycle status of an order (Arabic and English display names)
    public enum OrderStatus
    {
        // 0 - Order placed, awaiting restaurant acceptance
        Pending = 0,              // معلق

        // 1 - Order accepted by restaurant and being prepared
        Confirmed = 1,            // مؤكد
        Preparing = 2,            // جاري التحضير

        // 3 - Order ready at restaurant and waiting to be assigned to a courier
        ReadyForDelivery = 3,     // تم إرسال الطلبية للتوصيل

        // 4 - Awaiting courier acceptance
        WaitingCourier = 4,       // معلق عند التوصيل

        // 5 - Courier picked up order and is on the way
        OutForDelivery = 5,       // في الطريق للتوصيل
        Delivering = 6,           // جاري التوصيل

        // 7 - Order delivered to customer
        Delivered = 7,            // تم التوصيل

        // 8 - Order cancelled by restaurant or customer
        Cancelled = 8            // ملغي
    }
}
