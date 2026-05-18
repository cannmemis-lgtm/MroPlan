# MroPlan - Mezuniyet Sunumu Grafik ve Slayt Rehberi

Bu rehber, **MroPlan (Aviation MRO & AI-Powered Workforce Planning)** projenizin bitirme savunma sunumunda (slaytlarda) kullanabileceğiniz grafik yerleşimlerini, jüriye sunum taktiklerini ve her slayt için söylenecek **anahtar savunma argümanlarını** içerir.

---

## 🖼️ Sunum Slayt Yapısı ve Görsel Tasarım Rehberi

Sunumunuzda analitik yetenekleri göstermek için **4 temel slayt** kurgulamanız önerilir. Her bir slaydın detayları, jüriye hitap tarzı ve vurgulanacak teknik detaylar aşağıdadır:

---

### Slayt 1: Operasyonel Kontrol ve Karar Destek Paneli (Dashboard)
*   **Görsel Önerisi:** Dashboard ekranınızın tam ekran yüksek çözünürlüklü bir ekran görüntüsü veya ürettiğimiz `mro_plan_sunum_grafigi.png` görseli.
*   **Slayt Başlığı:** `Operasyonel Karar Destek ve Anlık Filo Takibi`
*   **Slayt İçeriği (Bullet Points):**
    *   **Gerçek Zamanlı KPI Yönetimi:** Toplam personel, aktif bakımlar, tamamlanan görevler ve bekleyen iş emirleri tek bir ekranda.
    *   **Hattat ve Lojistik Dengesi:** Geciken işlerin veya kritik bakımların otomatik renklendirilmesi.
    *   **Atölye Kapasite Hesaplama Formülü:** $Kapasite = Aktif\,Teknisyen \times 480\,dk \times \%85\,Verim$
*   **🗣️ Jüriye Söylenecek Anahtar Cümle (The Punchline):**
    > *"MroPlan sadece bir kayıt defteri değildir. Geliştirdiğimiz kontrol paneli, atölyelerdeki iş yükünü (man-hour) anlık kapasiteyle kıyaslayarak hangi departmanın aşırı yük altında olduğunu, hangisinin yedek kapasiteye sahip olduğunu anında analiz eden dinamik bir karar destek mekanizmasıdır."*

---

### Slayt 2: Atölye Bazlı İş Yükü ve Darboğaz Analizi
*   **Görsel Önerisi:** Atölye Doluluk Oranları (Circular Progress) ve Atölye Sütun Grafiği (Column Chart).
*   **Slayt Başlığı:** `Atölye Yük Dağılımları ve Kapasite Darboğazları`
*   **Slayt İçeriği (Bullet Points):**
    *   **Atölye Yoğunluk Kıyaslaması:** Motor, aviyonik, gövde ve pervaneler arasındaki iş yükü dengesizliğinin tespiti.
    *   **Kritik Yoğunluk Sınırı (%85+):** Kırmızı alarmlar ile atölye darboğazlarının iş dağıtımı yapılmadan önce önlenmesi.
    *   **Zaman Kayıplarının Engellenmesi:** Hazırlık ve işlem sürelerinin matematiksel entegrasyonu.
*   **🗣️ Jüriye Söylenecek Anahtar Cümle (The Punchline):**
    > *"Havacılıkta bakım süresinin uzaması, helikopterlerin yerde kalması demektir. Bu slaytta gördüğünüz atölye yoğunluk analizleri sayesinde, iş emirleri dağıtılmadan önce olası yığılmaları tespit ediyor ve iş gücünü dinamik olarak yeniden planlayabiliyoruz."*

---

### Slayt 3: Personel Verimliliği ve Standart Süre Sapma Analizi
*   **Görsel Önerisi:** Performans Analizi sayfasındaki katsayı KPI'ları ve Personel Verimlilik Sapma Grafiği (Scatter/Dağılım Grafiği).
*   **Slayt Başlığı:** `İş Gücü Verimliliği ve Standart Süre Performans Yönetimi`
*   **Slayt İçeriği (Bullet Points):**
    *   **Standart Süre Katsayısı (Coefficient):** Personelin işi bitirme süresinin, üreticinin belirlediği standart süreye oranı ($Gercek\,Sure / Standart\,Sure$).
    *   **Performans Kategorizasyonu:** Hızlı Teknisyenler (<0.90), Standartlar (0.90 - 1.20) ve Gelişim Gerekenler (>1.30).
    *   **Kişiselleştirilmiş Eğitim İhtiyacı:** Teknisyenlerin hangi spesifik parçalarda yavaş kaldığının tespiti.
*   **🗣️ Jüriye Söylenecek Anahtar Cümle (The Punchline):**
    > *"Performans analizi modülümüz, teknisyenlerimizin performansını subjektif değerlendirmelerle değil, tamamen veri odaklı olarak ölçer. Bir personelin hangi parça tipinde standart sürenin gerisinde kaldığını tespit ederek, doğrudan o alanda nokta atışı eğitimler almasını sağlıyoruz."*

---

### Slayt 4: Yapay Zeka Destekli Yetkinlik Matrisi ve Eğitim Planlama (AI Planner)
*   **Görsel Önerisi:** Yetkinlik Matrisi tablonuz ve AI eğitim öneri listesi görseli.
*   **Slayt Başlığı:** `Yapay Zeka Destekli Yetkinlik Gelişimi ve Operasyonel Köprü`
*   **Slayt İçeriği (Bullet Points):**
    *   **Siber Yetkinlik Matrisi:** Personel ve helikopter tiplerine göre yetkinlik derecelendirmesi.
    *   **AI Eğitim-Operasyon Köprüsü:** Bakım sıralarındaki gecikmeleri önlemek amacıyla yapay zekanın personel eğitim açıklarını otomatik kapatma algoritması.
    *   **Geleceğe Hazırlık:** Filoya yeni katılacak helikopter tiplerine göre ekibin yetkinlik simülasyonu.
*   **🗣️ Jüriye Söylenecek Anahtar Cümle (The Punchline):**
    > *"Sistemimizin en inovatif yönü, Eğitim ile Operasyon arasındaki köprüdür. Yapay zeka motorumuz, yaklaşan bakım takvimindeki helikopter tiplerini analiz eder, yetkin teknisyen açığı oluşacağını öngörür ve bu açığı kapatacak eğitim planlamasını otomatik olarak sunar. MroPlan ile bakım gecikmeleri tarih oluyor."*

---

## 🎯 Jüriden Gelebilecek Olası Sorular ve Savunma Cevapları

1.  **Soru:** *"Bu grafiklerdeki veriler nereden geliyor, veritabanı yapınız nasıl?"*
    *   **Cevap:** *"Tüm analizler Entity Framework Core aracılığıyla MS SQL / PostgreSQL veritabanımızdaki `BakimKontrolKayitlari`, `Personel` ve `BakimGruplari` tablolarından anlık çekiliyor. Veri bütünlüğünü korumak ve performansı artırmak için `AsNoTracking()` ve optimize edilmiş LINQ GroupBy sorguları kullandık."*

2.  **Soru:** *"Grafikler anlık olarak güncelleniyor mu?"*
    *   **Cevap:** *"Evet, Blazor Server/Interactive altyapımız sayesinde veritabanında bir iş tamamlandığı veya yeni bir iş emri atandığı anda, grafik bileşenlerimiz arka planda tetiklenerek herhangi bir sayfa yenilemesine gerek kalmadan anında güncellenmektedir."*

3.  **Soru:** *"Neden ApexCharts tercih ettiniz?"*
    *   **Cevap:** *"Hem mobil uyumlu (responsive) olması, hem de havacılık yazılımlarında aranan modern, koyu tema ve neon gösterge stilini (Endüstriyel Siber Estetik) en yüksek performans ve pürüzsüz animasyonlarla sunabilmesi nedeniyle ApexCharts kütüphanesini Blazor ile entegre ettik."*

---

## 💡 Sunum Slaytı Tasarım İpucu

Hazırladığımız **`mro_plan_sunum_grafigi.png`** görselini sunum slaytınızın arka planı veya ana görseli yapıp üzerine beyaz ve altın sarısı metinlerle başlıklar eklediğinizde, jürinin projeyi görür görmez profesyonel bir endüstriyel havacılık yazılımı olarak kabul etmesini sağlayacaksınız.
