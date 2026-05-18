// Browser Notification API — Blazor JSInterop bridge

window.bildirimler = {
    izinDurumu: () => {
        if (!("Notification" in window)) return "desteklenmiyor";
        return Notification.permission;
    },

    izinIste: async () => {
        if (!("Notification" in window)) return "desteklenmiyor";
        if (Notification.permission === "granted") return "granted";
        return await Notification.requestPermission();
    },

    goster: (baslik, mesaj, turu) => {
        if (!("Notification" in window) || Notification.permission !== "granted") return;

        const ikonlar = {
            "Atama":         "📋",
            "Gecikme":       "⏰",
            "KapasiteAsimi": "🔴",
            "SistemBilgisi": "ℹ️"
        };

        const ikon = ikonlar[turu] ?? "🔔";

        const n = new Notification(`${ikon} ${baslik}`, {
            body: mesaj,
            icon: "/favicon.png",
            badge: "/favicon.png",
            tag: `mroPlan-${turu}`,   // aynı tür üst üste gelince günceller, biriktirmez
            renotify: true
        });

        // 6 saniye sonra otomatik kapat
        setTimeout(() => n.close(), 6000);
    }
};
