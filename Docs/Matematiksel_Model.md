# MroPlan - Matematiksel Modelleme ve Optimizasyon Formülasyonu

Bu belge, **MroPlan (AI-Powered Aviation MRO & Workforce Planning)** projenizin akademik bitirme tezi, sunum slaytları veya poster gösteriminde kullanılmak üzere hazırlanmış **resmi matematiksel ve optimizasyon formülasyonunu** içerir. 

Bu formülasyon, projenin arkasındaki yöneylem araştırması (operations research), kapasite planlama ve yapay zeka karar kısıtlarını matematiksel dille jüriye sunmanızı sağlar.

---

## 1. Atölye Kapasite ve İş Yükü Modellemesi (Workshop Capacity & Workload Model)

Her bir atölye grubu (Motor, Aviyonik vb.) için kapasite sınırları ve anlık iş yükleri man-hour (adam-dakika) cinsinden modellenmiştir.

### Tanımlamalar (Indices & Sets):
*   $G$: Sistemdeki tüm bakım atölyelerinin (gruplarının) kümesi, $g \in G$.
*   $P_g$: $g$ atölyesine bağlı teknik personel kümesi, $p \in P_g$.
*   $P_g^{aktif} \subseteq P_g$: İzinli olmayan ve aktif olarak çalışmaya hazır teknik personel kümesi.
*   $K_g$: $g$ atölyesine atanmış ve devam eden (aktif) bakım görevleri kümesi, $k \in K_g$.

### Kapasite Formülasyonu (Capacity Equation):
Her bir aktif teknisyenin günlük standart çalışma süresi $480$ dakikadır (8 saat). Havacılık güvenlik, hazırlık ve raporlama standartları göz önüne alınarak bir atölye verimlilik katsayısı ($\eta = 0.85$) tanımlanmıştır. 

$g$ atölyesinin günlük toplam kullanılabilir iş gücü kapasitesi ($C_g$):
$$C_g = |P_g^{aktif}| \times 480 \times \eta \quad (\text{adam-dakika})$$

### İş Yükü Formülasyonu (Workload Equation):
Her bir aktif bakım görevi $k$ için, parça şablonunda tanımlanmış standart işlem süresi ($S_k$) ve hazırlık süresi ($H_k$) bulunmaktadır. 

$g$ atölyesinin anlık toplam iş yükü ($W_g$):
$$W_g = \sum_{k \in K_g} (S_k + H_k) \quad (\text{dakika})$$

### Kapasite Kısıt Denklemi (Capacity Constraint):
Atölyelerin aşırı iş yükü altında ezilmesini önlemek amacıyla, anlık iş yükünün atölye kapasitesinin maksimum $\%150$ sınırını aşması engellenmiştir (Aşırı Kritik Yoğunluk Limiti):
$$W_g \le 1.5 \times C_g \quad \forall g \in G$$

---

## 2. Yapay Zeka Personel Seçimi ve Eğitim Atama Optimizasyonu (AI Training-to-Operation Optimization)

`GeminiService.cs` ve AI Planlayıcı tarafından, yetkinlik açığı bulunan bir $j$ parça şablonu için en uygun $p$ teknisyenini seçmek ve eğitim ataması yapmak üzere kullanılan çok kriterli karar destek fonksiyonudur.

### Parametreler ve Değişkenler:
*   $S_{p, j} \in \{1, 2, 3, 4, 5\}$: $p$ teknisyeninin $j$ parçası üzerindeki mevcut yetkinlik seviyesi.
*   $L_p \in \mathbb{N}_{\ge 0}$: $p$ teknisyeninin üzerindeki anlık aktif görev (iş yükü) sayısı.
*   $L_{max} = 3$: Bir teknisyenin üzerine alabileceği maksimum aktif iş yükü limiti.
*   $U_{p, j}$: $p$ teknisyeninin $j$ parçasının eğitimine atanması için hesaplanan **Uyum Skoru (Utility Score)**.

### Uyum Skoru Amaç Fonksiyonu (Utility / Score Function):
Sistem, yetkinlik seviyesi Uzmanlığa ($SV5$) en yakın olan ve iş yükü en az olan personeli önceliklendirir.
$$U_{p, j} = \alpha \cdot \left( \frac{S_{p, j}}{5} \right) + \beta \cdot \left( 1 - \frac{L_p}{L_{max}} \right)$$

Burada:
*   $\alpha$: Yetkinlik seviyesi ağırlığı (Örn: $0.70$). Hızlı uzmanlaştırma ve verimlilik hedeflenir.
*   $\beta$: İş yükü dengesi ağırlığı (Örn: $0.30$). Adil iş dağılımı ve darboğaz engelleme hedeflenir.
*   $\alpha + \beta = 1.0$

### Karar Kısıtları (Decision Constraints):
Yapay zeka planlama yaparken aşağıdaki kısıt denklemlerini uygulamak zorundadır (Gemini structured rules):
1.  **Uzmanlık Kısıtı (Expert Constraint):** Zaten Seviye 5 (Uzman) olan personele yeni eğitim veya pratik görev atanamaz:
    $$S_{p, j} < 5 \quad \forall p \in P_{secilen}$$
2.  **Aşırı Yük Kısıtı (Overload Constraint):** Aktif iş yükü sınırı aşan personele planlama yapılamaz:
    $$L_p \le L_{max} \quad \forall p \in P_{secilen}$$
3.  **Optimal Personel Seçim Kararı:**
    $$p^* = \arg\max_{p \in P_g} (U_{p, j})$$

---

## 3. Personel Bireysel Verimlilik ve Performans Katsayısı (Technician Performance Coefficient)

Performans analizi modülünde teknisyenlerin standart bakım sürelerinden ne kadar saptığını ölçen matematiksel formülasyondur.

### Tanımlamalar:
*   $I_p$: $p$ teknisyeni tarafından başarıyla tamamlanmış bakım görevleri kümesi.
*   $T^{plan}_i$: $i$ görevinin üretici tarafından tanımlanmış planlanan standart süresi.
*   $T^{real}_i$: $i$ görevinin teknisyen tarafından fiilen tamamlandığı gerçek süre.

### Bireysel Performans Katsayısı Formülü ($\theta_p$):
Teknisyenin tamamladığı tüm işlerdeki gerçek sürelerin standart sürelere oranının ortalamasıdır:
$$\theta_p = \frac{1}{|I_p|} \sum_{i \in I_p} \left( \frac{T^{real}_i}{T^{plan}_i} \right)$$

### Performans Yorumlama Kriterleri:
*   $\theta_p < 0.90$: **Hızlı / Üstün Performans** (İşler standart süreden daha kısa sürede, yüksek verimle bitirilmiştir).
*   $0.90 \le \theta_p \le 1.20$: **Standart Performans** (Beklenen süre aralığında uyumlu çalışma).
*   $\theta_p > 1.30$: **Gelişim Gerekli** (İşler standart sürenin çok üzerinde tamamlanmıştır. Personelin bu parça tipinde eğitime ihtiyacı vardır).
