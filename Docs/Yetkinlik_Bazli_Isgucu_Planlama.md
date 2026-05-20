# Yetkinlik Bazlı İşgücü Planlaması (Competency-Based Workforce Planning)
## Akademik Literatür Araştırması ve Uygulama Rehberi

Bu belge, **MroPlan (AI-Powered Aviation MRO & Workforce Planning)** projenizin akademik bitirme tezi, teknik raporları veya jüri sunumları için kaynak oluşturmak üzere hazırlanmış kapsamlı bir **yetkinlik bazlı işgücü planlama literatür araştırması ve matematiksel modelleme rehberidir**.

---

## 1. Giriş ve Teorik Altyapı (Theoretical Background)

### 1.1. Yetkinlik Bazlı İşgücü Planlaması Nedir?
Geleneksel işgücü planlama modelleri, personeli sadece **"mevcudiyet" (availability)** ve **"nicelik" (adet/saat)** üzerinden ele alırken; **Yetkinlik Bazlı İşgücü Planlaması (Competency-Based Workforce Planning - CBWP)**, işgücünü bireylerin sahip olduğu spesifik **Bilgi, Beceri ve Yetenekler (Knowledge, Skills, and Abilities - KSAs)** üzerinden optimize eder. 

Literatürde bu yaklaşım, organizasyonun stratejik hedefleri ile insan kaynağının kapasitesi arasında dinamik bir köprü kurmayı hedefler (Armstrong & Taylor, 2020).

| Boyut | Geleneksel İşgücü Planlaması | Yetkinlik Bazlı İşgücü Planlaması |
| :--- | :--- | :--- |
| **Odak Noktası** | İş Tanımı (Job Description) ve Pozisyonlar | Bireysel Yetkinlikler (Competencies) ve Beceriler |
| **Ölçüm Birimi** | Çalışılan Saat (FTE - Full-Time Equivalent) | Yetkinlik Seviyesi (Proficiency Levels: Seviye 1 - 5) |
| **Esneklik** | Düşük (Kişi sadece tanımlı rolü yapar) | Yüksek (Çapraz eğitimli personel farklı rollerde görev alabilir) |
| **Amaç** | Pozisyonları doldurmak ve bütçeyi korumak | Stratejik yetkinlik açıklarını kapamak ve esnekliği artırmak |

### 1.2. MRO (Bakım, Onarım ve Revizyon) Sektöründeki Kritik Önemi
Havacılık ve savunma sanayi gibi **yüksek güvenlik standartlarına (EASA/FAA Part-145)** tabi sektörlerde yetkinlik bazlı planlama bir tercih değil, yasal bir zorunluluktur. Bakım operasyonlarında insan hatasını (human error) minimize etmek, iş emirlerini sadece "müsait" olan değil, **o parça üzerinde sertifikalı ve yetkinliği tescillenmiş** teknisyenlere atamaktan geçer.

---

## 2. Literatürdeki Temel Yaklaşımlar ve Problemler

Akademik literatürde (Operations Research ve Industrial Engineering) konu üç ana problem etrafında şekillenir:

### A. Yetkinlik Sınırlandırılmış Personel Atama Problemi (Skill-Constrained Personnel Assignment Problem - SCPAP)
Bu problemde, belirli sürelerde ve belirli önceliklerde yapılması gereken görevler (iş emirleri) vardır. Her görevin gerektirdiği minimum bir yetkinlik seviyesi ($S_{req}$) mevcuttur. Amaç; işlerin zamanında bitmesini sağlarken, kısıtları ihlal etmeden en uygun personeli seçmektir.

### B. Çapraz Eğitim ve Çoklu Beceri Yönetimi (Cross-Training & Multi-Skilling)
Tek bir alanda uzmanlaşmış işgücü yerine, birden fazla alanda yetkinleşmiş (T-tipi veya M-tipi) personel yetiştirmek, sistemin darboğazlara karşı direncini artırır. Literatür, eğitim bütçesini optimize ederek hangi personelin hangi eğitime gönderilmesi gerektiğini matematiksel modellerle belirler (Firat & Hurkens, 2012).

### C. Öğrenme ve Unutma Dinamikleri (Learning & Forgetting Curves)
İnsan yetkinliği statik değildir. Wright'ın Öğrenme Eğrisi (Wright's Learning Curve) modeline göre; bir teknisyen belirli bir bakımı tekrarladıkça hızı artar (öğrenme katsayısı). Ancak, uzun süre o parçaya dokunmazsa yetkinliği körelir (unutma katsayısı). Gelişmiş planlama modelleri bu dinamikleri de hesaba katar.

---

## 3. Matematiksel Optimizasyon ve Formülasyon Örnekleri

Literatürde en sık kullanılan optimizasyon yöntemi **Karışık Tamsayılı Doğrusal Programlama (Mixed-Integer Linear Programming - MILP)** modelleridir.

### 3.1. Karar Değişkenleri (Decision Variables)
$$x_{p, k, t} = \begin{cases} 1, & p \text{ teknisyeni } t \text{ periyodunda } k \text{ görevine atanmışsa} \\ 0, & \text{aksi takdirde} \end{cases}$$

### 3.2. Amaç Fonksiyonu Örnekleri (Objective Functions)

1. **Toplam Gecikmenin (Downtime/Tardiness) Minimizasyonu:**
   $$\min Z = \sum_{k \in K} (T^{real}_k - T^{plan}_k)^+$$
   *Burada havacılıkta Turnaround Time (TAT) süresini en aza indirmek hedeflenir.*

2. **Yetkinlik Uyumunun Maksimizasyonu (Quality & Competency Matching):**
   $$\max Z = \sum_{p \in P} \sum_{k \in K} S_{p, j(k)} \cdot x_{p, k, t}$$
   *Burada $S_{p, j(k)}$, $p$ teknisyeninin $k$ görevinin gerektirdiği $j$ parçası üzerindeki yetkinlik seviyesidir. Amaç, en zor işlere en uzman personeli yerleştirmektir.*

### 3.3. Kritik Kısıt Denklemleri (Constraints)

*   **Yetkinlik Barajı Kısıtı (Skill Requirement Constraint):** Bir işe sadece o işin gerektirdiği asgari yetkinliğe sahip olanlar atanabilir.
    $$S_{p, j(k)} \ge S^{req}_k \cdot x_{p, k, t} \quad \forall p \in P, \forall k \in K, \forall t$$
*   **Tek Görev Kısıtı (Capacity/Single Task Constraint):** Bir personel aynı anda birden fazla işe atanamaz.
    $$\sum_{k \in K} x_{p, k, t} \le 1 \quad \forall p \in P, \forall t$$

---

## 4. Gerçek Dünya Endüstriyel Uygulama Örnekleri

### Örnek 1: Havacılık Bakım Hangarları (Lufthansa Technik & Air France Industries Örneği)
Havacılıkta "A-Check", "B-Check" veya motor revizyonları gibi büyük operasyonlar binlerce adam-saat iş yükü içerir.
*   **Uygulama:** Teknisyenler EASA Part-66 lisans sınıflarına (A, B1, B2, C) göre ayrılır. 
*   **Problemin Çözümü:** Geliştirilen karar destek sistemleri, uçak hangara girdiği anda iş paketlerini parçalar. "Motor Türbin Kanat Değişimi" gibi kritik bir işi sadece B1 lisansına sahip ve motor yetkinliği seviye 4-5 olan teknisyenlere atarken; daha basit panel sökme işlerini seviye 1-2 stajyerlere atayarak iş gücünü optimize eder.

### Örnek 2: Otomotiv Üretim Hatları (Toyota Üretim Sistemi - Kaizen ve Shojinka)
Toyota'da **"Shojinka" (Esnek İş Gücü)** kavramı, talep değişimlerine göre işçi sayısını ve yerleşimi dinamik olarak değiştirmeyi ifade eder.
*   **Uygulama:** Her atölyede bir **"Çapraz Yetkinlik Matrisi" (Polyvalence / Skills Matrix)** asılıdır.
*   **Problemin Çözümü:** Bir hatta darboğaz oluştuğunda, sistem otomatik olarak diğer hattan o görevin yetkinliğine sahip boşta olan işçiyi tespit edip oraya kaydırır. Böylece üretim bandı hiç durmaz.

---

## 5. MroPlan Projenizin Akademik Pozisyonlaması (Academic Alignment)

MroPlan kapsamında geliştirdiğiniz modeller ve mimari, yukarıda bahsedilen akademik literatürün **en modern yapay zeka entegrasyonlu (AI-Driven)** örneklerinden birini oluşturur. Jüriye sunarken projenizi şu akademik temellere dayandırabilirsiniz:

*   **Atölye Kapasite Modeli (Capacity & Workload Model):** Literatürdeki standart man-hour planlama teorisine dayanır. Günlük kullanılabilir kapasite formülünüzdeki verimlilik katsayısı ($\eta = 0.85$), havacılıktaki **"Tool time" (fiili çalışma süresi)** kayıplarını mükemmel şekilde modelleyen akademik bir kabuldür.
*   **Yapay Zeka Uyum Skoru (AI Utility Score - $U_{p, j}$):** Literatürdeki **Çok Kriterli Karar Verme (Multi-Criteria Decision Making - MCDM)** teorisinin pragmatik bir uyarlamasıdır. Sadece yetkinliği değil, aynı zamanda operasyonel uygunluğu (iş yükü dengesini) de maksimize ederek **darboğazları engeller (bottleneck prevention)**.
*   **Teknisyen Performans Katsayısı ($\theta_p$):** Yöneylem araştırmasında "stokastik işlem süreleri" (stochastic processing times) olarak bilinen kavramı adresler. Teknisyenin gerçek performansına göre ($\theta_p > 1.30 \rightarrow$ Gelişim Gerekli), sistemin otomatik olarak **"AI Eğitim Modülü"** üzerinden kişiye özel eğitim önermesi, literatürde **"Kanalize Edilmiş Beceri Gelişimi" (Targeted Skill Enhancement)** olarak adlandırılır.

---

## 6. Literatür Referansları (Akademik Kaynak Gösterimi)
Tezinizde veya sunumunuzda kullanabileceğiniz bazı temel akademik referanslar şunlardır:

1. **Armstrong, M., & Taylor, S. (2020).** *Armstrong's Handbook of Human Resource Management Practice*. Kogan Page Publishers. (Yetkinlik bazlı yönetim teorisi için).
2. **Firat, M., & Hurkens, C. A. (2012).** *An Improved MILP Formulation for the Skill-Constrained Personnel Assignment Problem*. Journal of Scheduling, 15(4), 423-437. (Matematiksel atama modeli için).
3. **Vidal, T., Crainic, T. G., Gendreau, M., & Prins, C. (2013).** *Heuristics for multi-attribute vehicle routing problems: A survey and synthesis*. European Journal of Operational Research. (Çok yetkinlikli teknisyen yönlendirme algoritmaları için).
4. **Bortolotti, T., Boscari, S., & Danese, P. (2015).** *Successful lean implementation: Organizational culture and soft lean practices*. International Journal of Production Economics. (Toyota Shojinka ve kültürel yetkinlik matrisleri için).
