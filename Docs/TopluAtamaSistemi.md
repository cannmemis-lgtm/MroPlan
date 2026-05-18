# Toplu Personel Atama Planlama Sistemi

## 1. Genel Bakış

MroPlan'ın Toplu Atama Planlama modülü; atölye yöneticilerine, bekleyen bakım işlerini tek seferde inceleyerek personel atamalarını optimize edilmiş biçimde gerçekleştirme imkânı sunar. Sistem, atama kararlarını otomatikleştirmek yerine karar vericiye **önceliklendirme bilgisi** ve **uygunluk analizi** sağlayarak insan gözetiminde yarı-otomatik planlama yapar.

---

## 2. Veri Modeli

### 2.1 İlgili Tablolar

| Tablo | Alan | Açıklama |
|---|---|---|
| `BakimKontrolKayitlari` | `AtananPersonelId` | Atanan teknisyen (FK → Personeller) |
| `BakimKontrolKayitlari` | `SupervisorPersonelId` | Gözetimci personel (FK → Personeller) |
| `BakimKontrolKayitlari` | `EgitimModu` | `true` ise performans hesabına dahil edilmez |
| `BakimKontrolKayitlari` | `TamamlanmaTarihi` | Gerçek bitiş tarihi (atama tarihi `IslemTarihi`'nden ayrı tutulur) |
| `ParcaSablonlari` | `IslemSure` | Tahmini işlem süresi (dakika) |
| `ParcaSablonlari` | `IslemTuru` | `Acil`, `Kritik`, `Rutin` |
| `Yetkinlikler` | `YetkinlikSeviyesi` | 1–5 arası integer; personelin parça bazında yetkinlik düzeyi |
| `Personeller` | `AtolyeKodu` | Personelin çalıştığı atölye |
| `Personeller` | `IzinBaslangic`, `IzinBitis` | İzin aralığı; sistem bu tarihler içinde kişiyi uygun saymaz |

### 2.2 Kapasite Hesabı

Sistem, gün içi net vardiya süresini sabit bir sabit olarak tanımlar:

```
NetVardiyaSüresi = 8.0 saat  (mola dahil değil)
```

Her personel için kalan kapasitesi şu formülle hesaplanır:

```
KalanKapasite(personel) =
    NetVardiyaSüresi
    − Σ (mevcut DevamEdiyor işlerin IslemSure / 60)
    − Σ (bu planda atanmış işlerin IslemSure / 60)
```

---

## 3. Kısıt Katmanları

### 3.1 Zorunlu Kısıtlar (Hard Constraints)

Aşağıdaki koşullardan herhangi birini sağlamayan personel **kesinlikle atanamaz**:

| # | Kısıt | Açıklama |
|---|---|---|
| HC-1 | **Aktif Durum** | `Personel.Durum == "Aktif"` olmalıdır. |
| HC-2 | **İzin Kontrolü** | Planlama günü `IzinBaslangic ≤ bugün ≤ IzinBitis` aralığında ise kişi atanamaz. |
| HC-3 | **Atölye Uyumu** | Personelin `AtolyeKodu`, işin ait olduğu `BakimGrubu.AtolyeKodu` ile eşleşmelidir. |
| HC-4 | **Yetkinlik (Sertifika)** | `Yetkinlikler` tablosunda `SicilNo + ParcaPN` çifti için kayıt bulunmalıdır. |
| HC-5 | **Kapasite** | `KalanKapasite ≥ IslemSure / 60` sağlanmalıdır; yani iş sığacak kadar serbest saat olmalıdır. |

### 3.2 Gözetimci Kısıtı (Soft Constraint)

Yetkinlik seviyesi **1 veya 2** olan bir personele iş atandığında, aynı atölyeden en az **seviye 3** yetkinliğe sahip bir gözetimci (supervisor) belirlenmesi gerekir. Gözetimci ataması zorunlu değildir; ancak atanmamış olması risk skorunu yükseltir.

---

## 4. Puanlama Algoritması

Her iş ve her personel çifti için iki bağımsız puan hesaplanır.

### 4.1 Aciliyet Puanı (0 – 110)

İşin ne kadar öncelikli olduğunu gösterir; **işe** özgüdür, personelden bağımsızdır.

```
AciliyetPuanı =
    IslemTuruPuanı
    + min(GünFarkı × 5, 50)
    [max: 110]
```

| `IslemTuru` | Puan |
|---|---|
| `Acil` | 60 |
| `Kritik` | 50 |
| `Rutin` | 20 |
| Diğer | 10 |

**GünFarkı**: İşin sisteme kayıt tarihi (`IslemTarihi`) ile bugün arasındaki gün sayısı. Her geçen gün için +5 puan, en fazla +50.

> **Örnek:** Bir "Kritik" iş 8 gün önce açılmışsa → 50 + min(8×5, 50) = 50 + 40 = **90 puan**.

### 4.2 Uyum Puanı (0 – 110)

Belirli bir personelin belirli bir işe ne kadar uygun olduğunu ölçer. Yalnızca zorunlu kısıtları geçen personel için hesaplanır.

```
UyumPuanı =
    YetkinlikSeviyesiPuanı   (20–60)
    + AtölyeEşleşmePuanı     (0 veya 30)
    + DüşükYükBonusu         (0, 10 veya 20)
    [max: 110]
```

| Yetkinlik Seviyesi | Puan |
|---|---|
| 5 | 60 |
| 4 | 50 |
| 3 | 40 |
| 2 | 30 |
| 1 | 20 |

| Koşul | Puan |
|---|---|
| `Personel.AtolyeKodu == İş.AtolyeKodu` | +30 |
| Aktif iş sayısı = 0 | +20 |
| Aktif iş sayısı = 1 | +10 |
| Aktif iş sayısı ≥ 2 | +0 |

> **Toplam maksimum uyum puanı:** 60 + 30 + 20 = **110**

---

## 5. Risk Skoru

Plan onaylanmadan önce hesaplanan risk skoru, yöneticiye genel plan kalitesi hakkında özet bir gösterge sunar.

```
RiskSkoru =
    (AtanmamışİşSayısı × 15)
    + (Seviye1veya2AtamasıSayısı × 10)
```

| Aralık | Değerlendirme |
|---|---|
| 0 | Düşük Risk |
| 1 – 39 | Orta Risk |
| 40+ | Yüksek Risk |

---

## 6. Otomatik Dağıtım Algoritması

`OtomatikDağıt` işlevi aşağıdaki adımları sırayla uygular:

1. **Sıralama:** Bekleyen tüm işler `AciliyetPuanı` değerine göre **azalan** sırada işlenir. Böylece en kritik işler önce atanır.

2. **Personel Seçimi:** Her iş için zorunlu kısıtları geçen (`UygunMu = true`) personel listesi hesaplanır. Liste `UyumPuanı` değerine göre azalan sırada sıralanır; en yüksek puanlı kişi seçilir.

3. **Kapasite Güncelleme:** Atama gerçekleştiğinde seçilen personelin `PlanKapasitesi` anında güncellenir. Bu güncelleme, aynı kişi için sonraki işlerin kapasite kontrolünü etkiler.

4. **Gözetimci Atama:** Seçilen personelin yetkinlik seviyesi ≤ 2 ise, aynı atölyeden seviye ≥ 3 olan ve en yüksek yetkinliğe sahip personel gözetimci olarak otomatik atanır.

5. **Atanamayan İşler:** Hiçbir uygun personel bulunamayan işler `null` olarak bırakılır ve risk skoruna yansır.

```
Girdi  : Bekleyen işler listesi, personel + yetkinlik veritabanı
Çıktı  : _planAtamalari  (isId → personelId?)
         _planSupervisorlari (isId → supervisorId?)
Karmaşıklık: O(İş × Personel)
```

---

## 7. Eğitim Modu

Plan onaylanmadan önce **Eğitim Modu** açık bırakılabilir. Bu modda onaylanan atamalarda `BakimKontrolKaydi.EgitimModu = true` olarak kaydedilir. Performans metrikleri hesaplanırken bu işler filtrelenerek çıkarılır; böylece stajyer veya yeni personelin iş yüküne dahil edilmesi diğer çalışanların istatistiklerini bozmaz.

---

## 8. Onay Akışı

```
[Toplu Atama Planı butonu]
        ↓
[İki panelli plan ekranı]
  ├─ Sol: İş tablosu (aciliyete göre sıralı, kişi seçimi)
  └─ Sağ: Personel kapasite kartları (anlık güncelleme)
        ↓
[Planı Onayla → Özet Dialog]
  ├─ Risk skoru gösterimi
  ├─ Atanacak işler listesi
  ├─ Atanamamış işler uyarısı
  └─ Gözetimci eksikliği uyarısı
        ↓
[Onayla ve Uygula]
  → Veritabanına toplu yazım (SaveChangesAsync)
  → Durum: Beklemede → DevamEdiyor
  → IslemTarihi = UTC şimdiki zaman
```

---

## 9. Özet Tablo

| Özellik | Değer |
|---|---|
| Net vardiya süresi | 8 saat |
| Aciliyet puanı aralığı | 0 – 110 |
| Uyum puanı aralığı | 0 – 110 |
| Zorunlu kısıt sayısı | 5 |
| Gözetimci eşiği | Yetkinlik seviyesi ≤ 2 |
| Risk skoru bileşenleri | Atanmamış iş × 15, Düşük seviye × 10 |
| Veritabanı altyapısı | PostgreSQL + EF Core 8 |
| Arayüz | Blazor Server + MudBlazor |
