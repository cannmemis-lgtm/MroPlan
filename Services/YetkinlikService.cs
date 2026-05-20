using Microsoft.EntityFrameworkCore;
using MroPlan.Data;
using MroPlan.Models;
using MroPlan.Models.Enums;

namespace MroPlan.Services
{
    public class YetkinlikService(
        IDbContextFactory<ApplicationDbContext> factory,
        BildirimServisi bildirimServisi,
        GeminiService geminiService) : IYetkinlikService
    {
        public async Task<List<Yetkinlik>> GetYetkinliklerAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.Yetkinlikler
                .Include(y => y.Personel).ThenInclude(p => p!.BakimGrubu)
                .Include(y => y.ParcaSablonu).ThenInclude(s => s!.BakimGrubu)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<BakimGrubu>> GetBakimGruplariAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.BakimGruplari.AsNoTracking().ToListAsync();
        }

        public async Task KartTamamlandiAsync(int personelId, int parcaSablonuId, int kartId)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            await using var tx = await ctx.Database.BeginTransactionAsync();

            var personel = await ctx.Personeller.FindAsync(personelId);
            var parca = await ctx.ParcaSablonlari.FindAsync(parcaSablonuId);
            if (personel == null || parca == null) return;

            var yetkinlik = await ctx.Yetkinlikler
                .FirstOrDefaultAsync(y => y.PersonelId == personelId && y.ParcaSablonuId == parcaSablonuId);

            if (yetkinlik == null)
            {
                yetkinlik = new Yetkinlik
                {
                    PersonelId = personelId,
                    ParcaSablonuId = parcaSablonuId,
                    SicilNo = personel.SicilNo,
                    ParcaPN = parca.ParcaPN,
                    YetkinlikSeviyesi = 1,
                    TamamlananKartSayisi = 0
                };
                ctx.Yetkinlikler.Add(yetkinlik);
            }

            yetkinlik.TamamlananKartSayisi++;

            // SV yükseltme kontrolü
            if (SvEsikler.SvYukseltilebilir(yetkinlik.YetkinlikSeviyesi, yetkinlik.TamamlananKartSayisi))
            {
                yetkinlik.YetkinlikSeviyesi++;

                await ctx.SaveChangesAsync();
                await tx.CommitAsync();

                var svMesaj = yetkinlik.YetkinlikSeviyesi == 5
                    ? $"{personel.AdSoyad} artık {parca.ParcaAdi} için tam yetkin (SV5)!"
                    : $"{personel.AdSoyad}, {parca.ParcaAdi} için SV{yetkinlik.YetkinlikSeviyesi} seviyesine ulaştı.";

                bildirimServisi.Ekle(new Bildirim
                {
                    Baslik = yetkinlik.YetkinlikSeviyesi == 5 ? "Tam Yetkinlik Kazanıldı!" : "SV Artışı",
                    Mesaj = svMesaj,
                    Turu = BildirimTuru.SistemBilgisi,
                    Seviye = yetkinlik.YetkinlikSeviyesi == 5 ? BildirimSeviye.Kritik : BildirimSeviye.Uyari
                });
                return;
            }

            // Sonraki SV'ye 1 kart kaldıysa uyarı gönder
            var sonrakiEsik = SvEsikler.KumulatifEsik(yetkinlik.YetkinlikSeviyesi) + SvEsikler.SonrakiEsik(yetkinlik.YetkinlikSeviyesi);
            bool birKartKaldi = yetkinlik.YetkinlikSeviyesi < 5 && sonrakiEsik - yetkinlik.TamamlananKartSayisi == 1;

            await ctx.SaveChangesAsync();
            await tx.CommitAsync();

            if (birKartKaldi)
                bildirimServisi.Ekle(new Bildirim
                {
                    Baslik = "SV Artışına 1 Kart Kaldı",
                    Mesaj = $"{personel.AdSoyad}, {parca.ParcaAdi} için SV{yetkinlik.YetkinlikSeviyesi + 1}'e 1 kart kaldı.",
                    Turu = BildirimTuru.SistemBilgisi,
                    Seviye = BildirimSeviye.Uyari
                });
        }

        public async Task GelistirmeModunaAlAsync(BakimKontrolKaydi kayit, int gelistirmePersonelId)
        {
            await using var ctx = await factory.CreateDbContextAsync();

            var dbKayit = await ctx.BakimKontrolKayitlari.FindAsync(kayit.Id);
            if (dbKayit == null) return;

            dbKayit.GelistirmeModu = true;
            dbKayit.GelistirmePersonelId = gelistirmePersonelId;
            dbKayit.GelistirmeBaslangic = DateTime.UtcNow;

            await ctx.SaveChangesAsync();

            var personel = await ctx.Personeller.FindAsync(gelistirmePersonelId);
            bildirimServisi.Ekle(new Bildirim
            {
                Baslik = "Geliştirme Moduna Alındı",
                Mesaj = $"{personel?.AdSoyad ?? "Personel"} geliştirme modunda çalışmaya başladı.",
                Turu = BildirimTuru.Atama,
                Seviye = BildirimSeviye.Bilgi
            });
        }


        public async Task<List<BakimKontrolKaydi>> GetAktifGelistirmelerAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.BakimKontrolKayitlari
                .Include(k => k.GelistirmePersonel)
                .Include(k => k.ParcaSablonu)
                .Include(k => k.BakimPlani)
                .Where(k => k.GelistirmeModu && k.Durum != "Tamamlandi")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<YetkinlikAcigi>> GetAtolyelYetkinlikAciklariAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();

            // SQL tarafında gruplama yaparak sadece ihtiyacımız olan sonuçları çekiyoruz
            var acikData = await ctx.BakimGruplari
                .AsNoTracking()
                .Select(g => new
                {
                    GrupId = g.Id,
                    g.GrupAdi,
                    g.AtolyeKodu,
                    Personeller = ctx.Personeller
                        .Where(p => p.BakimGrubuId == g.Id || p.AtolyeKodu == g.AtolyeKodu)
                        .Select(p => p.Id).Distinct().ToList(),
                    Parcalar = ctx.ParcaSablonlari.Where(p => p.BakimGrubuId == g.Id).Select(p => new { p.Id, p.ParcaAdi, p.ParcaPN }).ToList()
                })
                .ToListAsync();

            var yetkinlikler = await ctx.Yetkinlikler
                .Where(y => y.YetkinlikSeviyesi >= 5)
                .Select(y => new { y.PersonelId, y.ParcaSablonuId })
                .AsNoTracking()
                .ToListAsync();

            var yetkinlikSet = yetkinlikler.Select(y => (y.PersonelId, y.ParcaSablonuId)).ToHashSet();

            List<YetkinlikAcigi> aciklar = [];

            foreach (var grup in acikData)
            {
                if (grup.Personeller.Count == 0) continue;

                foreach (var parca in grup.Parcalar)
                {
                    var sv5Sayisi = grup.Personeller.Count(pid => yetkinlikSet.Contains((pid, parca.Id)));

                    if (sv5Sayisi < grup.Personeller.Count)
                    {
                        aciklar.Add(new YetkinlikAcigi(
                            grup.GrupAdi,
                            parca.ParcaAdi,
                            parca.ParcaPN ?? "",
                            sv5Sayisi,
                            grup.Personeller.Count,
                            sv5Sayisi == 0 // Hiç yetkin personel yoksa KRİTİK
                        ));
                    }
                }
            }

            return [.. aciklar.OrderByDescending(a => a.Kritik).ThenBy(a => a.MevcutSv5Sayisi)];
        }

        // --- EĞİTİM METOTLARI ---

        public async Task<List<EgitimModulu>> GetEgitimModulleriAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var list = await ctx.EgitimModulleri.Include(e => e.ParcaSablonu).AsNoTracking().ToListAsync();
            if (list.Count == 0)
            {
                // Seed data if empty
                var p1 = await ctx.ParcaSablonlari.FirstOrDefaultAsync();
                if (p1 != null)
                {
                    var demo = new EgitimModulu { Ad = "Temel Bakım Eğitimi", Kategori = "Genel", HedefYetkinlikSeviyesi = 1, ParcaSablonuId = p1.Id, Aciklama = "Genel bakım prensipleri." };
                    ctx.EgitimModulleri.Add(demo);
                    await ctx.SaveChangesAsync();
                    list.Add(demo);
                }
            }
            return list;
        }

        public async Task<List<PersonelEgitim>> GetPersonelEgitimleriAsync(int personelId)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.PersonelEgitimleri
                .Include(pe => pe.EgitimModulu)
                .Where(pe => pe.PersonelId == personelId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task EgitimiTamamlaAsync(int personelId, int egitimId)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var egitim = await ctx.EgitimModulleri.FindAsync(egitimId);
            if (egitim == null) return;

            var kayit = await ctx.PersonelEgitimleri
                .FirstOrDefaultAsync(pe => pe.PersonelId == personelId && pe.EgitimModuluId == egitimId);

            if (kayit == null)
            {
                kayit = new PersonelEgitim { PersonelId = personelId, EgitimModuluId = egitimId };
                ctx.PersonelEgitimleri.Add(kayit);
            }

            kayit.Tamamlandi = true;
            // PlanlananBitis varsa onu kullan (test/otomatik), yoksa şimdiki zaman
            kayit.TamamlanmaTarihi = kayit.PlanlananBitis?.ToUniversalTime() ?? DateTime.UtcNow;
            kayit.IlerlemeYuzdesi = 100;

            // Yetkinlik seviyesini güncelle (Eğer eğitim bir parça ile ilişkiliyse)
            if (egitim.ParcaSablonuId.HasValue)
            {
                var ytk = await ctx.Yetkinlikler.FirstOrDefaultAsync(y => y.PersonelId == personelId && y.ParcaSablonuId == egitim.ParcaSablonuId);
                if (ytk != null)
                {
                    if (ytk.YetkinlikSeviyesi < egitim.HedefYetkinlikSeviyesi)
                    {
                        int eskiSv = ytk.YetkinlikSeviyesi;
                        ytk.YetkinlikSeviyesi = egitim.HedefYetkinlikSeviyesi;
                        int yeniBaseline = SvEsikler.KumulatifEsik(egitim.HedefYetkinlikSeviyesi);
                        if (ytk.TamamlananKartSayisi < yeniBaseline)
                            ytk.TamamlananKartSayisi = yeniBaseline;
                        // Sertifika otomatik tanımla
                        ytk.SertifikaTarihi  = kayit.TamamlanmaTarihi;
                        ytk.SertifikaBelgeNo = $"CERT-{ytk.SicilNo}-SV{egitim.HedefYetkinlikSeviyesi}-{(ytk.ParcaPN ?? "PARCA").Replace(" ", "")}-{kayit.TamamlanmaTarihi.Year}";
                        ctx.YetkinlikGecmisleri.Add(new YetkinlikGecmisi
                        {
                            YetkinlikId      = ytk.Id,
                            SicilNo          = ytk.SicilNo,
                            ParcaPN          = ytk.ParcaPN,
                            EskiSeviye       = eskiSv,
                            YeniSeviye       = egitim.HedefYetkinlikSeviyesi,
                            IslemYapanSicil  = "sistem",
                            IslemTarihi      = DateTime.UtcNow,
                            IslemNotu        = $"Eğitim tamamlandı: {egitim.Ad}"
                        });
                    }
                }
                else
                {
                    // Yeni yetkinlik kaydı
                    var personel = await ctx.Personeller.FindAsync(personelId);
                    var parca = await ctx.ParcaSablonlari.FindAsync(egitim.ParcaSablonuId.Value);
                    if (personel != null && parca != null)
                    {
                        int hedefSv = egitim.HedefYetkinlikSeviyesi;
                        string belgeNo = $"CERT-{personel.SicilNo}-SV{hedefSv}-{(parca.ParcaPN ?? "PARCA").Replace(" ", "")}-{kayit.TamamlanmaTarihi.Year}";
                        ctx.Yetkinlikler.Add(new Yetkinlik
                        {
                            PersonelId           = personelId,
                            ParcaSablonuId       = egitim.ParcaSablonuId.Value,
                            SicilNo              = personel.SicilNo,
                            ParcaPN              = parca.ParcaPN,
                            YetkinlikSeviyesi    = hedefSv,
                            TamamlananKartSayisi = SvEsikler.KumulatifEsik(hedefSv),
                            SertifikaTarihi      = kayit.TamamlanmaTarihi,
                            SertifikaBelgeNo     = belgeNo
                        });
                        await ctx.SaveChangesAsync();
                        var yeniYtk = await ctx.Yetkinlikler.FirstOrDefaultAsync(y =>
                            y.PersonelId == personelId && y.ParcaSablonuId == egitim.ParcaSablonuId.Value);
                        if (yeniYtk != null)
                        {
                            ctx.YetkinlikGecmisleri.Add(new YetkinlikGecmisi
                            {
                                YetkinlikId     = yeniYtk.Id,
                                SicilNo         = yeniYtk.SicilNo,
                                ParcaPN         = yeniYtk.ParcaPN,
                                EskiSeviye      = 0,
                                YeniSeviye      = hedefSv,
                                IslemYapanSicil = "sistem",
                                IslemTarihi     = DateTime.UtcNow,
                                IslemNotu       = $"Eğitim tamamlandı: {egitim.Ad}"
                            });
                        }
                    }
                }
            }

            await ctx.SaveChangesAsync();

            // Aktif eğitim kalmadıysa personeli Aktif'e çek
            bool kalanEgitimVar = await ctx.PersonelEgitimleri
                .AnyAsync(pe => pe.PersonelId == personelId && !pe.Tamamlandi);
            if (!kalanEgitimVar)
            {
                var personelEgitimde = await ctx.Personeller.FindAsync(personelId);
                if (personelEgitimde != null && personelEgitimde.Durum == nameof(PersonelDurumu.Egitimde))
                    personelEgitimde.Durum = nameof(PersonelDurumu.Aktif);
                await ctx.SaveChangesAsync();
            }

            bildirimServisi.Ekle(new Bildirim
            {
                Baslik = "Eğitim Tamamlandı",
                Mesaj = $"{egitim.Ad} başarısıyla bitirildi. Yetkinlikleriniz güncellendi.",
                Turu = BildirimTuru.SistemBilgisi,
                Seviye = BildirimSeviye.Bilgi
            });
        }

        public async Task<List<EgitimOnerisi>> GetOnerilenEgitimlerAsync(int personelId)
        {
            await using var ctx = await factory.CreateDbContextAsync();

            var personel = await ctx.Personeller
                .Include(p => p.BakimGrubu)
                .FirstOrDefaultAsync(p => p.Id == personelId);
            
            if (personel == null) return [];

            // 1. Mevcut Atölye Açıkları
            var tumAciklar = await GetAtolyelYetkinlikAciklariAsync();
            var personelAtolyeAciklari = tumAciklar
                .Where(a => a.AtolyeAdi == personel.BakimGrubu?.GrupAdi)
                .ToDictionary(a => (a.ParcaAdi, a.ParcaPN), a => a);

            // 2. Bekleyen (atanmamış) işler
            var bekleyenIsler = await ctx.BakimKontrolKayitlari
                .Include(k => k.ParcaSablonu)
                .Where(k => k.Durum == nameof(BakimDurumu.Beklemede) && k.AtananPersonelId == null)
                .ToListAsync();

            var isSayilari = bekleyenIsler
                .GroupBy(k => k.ParcaSablonuId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 3. Mevcut eğitimler ve tamamlananlar
            var tumModuller = await ctx.EgitimModulleri
                .Include(e => e.ParcaSablonu)
                .ToListAsync();
            
            var tamamlananIdler = await ctx.PersonelEgitimleri
                .Where(pe => pe.PersonelId == personelId && pe.Tamamlandi)
                .Select(pe => pe.EgitimModuluId)
                .ToHashSetAsync();

            // 4. Mevcut yetkinlik seviyeleri
            var yetkinlikler = await ctx.Yetkinlikler
                .Where(y => y.PersonelId == personelId)
                .ToDictionaryAsync(y => y.ParcaSablonuId, y => y.YetkinlikSeviyesi);

            List<EgitimOnerisi> oneriler = [];

            foreach (var modul in tumModuller.Where(m => !tamamlananIdler.Contains(m.Id)))
            {
                if (!modul.ParcaSablonuId.HasValue) continue;
                
                var parcaId = modul.ParcaSablonuId.Value;
                var parca = modul.ParcaSablonu!;
                isSayilari.TryGetValue(parcaId, out var isSayisi);
                
                // Atölye bazlı kritiklik kontrolü
                bool atolyeKritik = personelAtolyeAciklari.TryGetValue((parca.ParcaAdi, parca.ParcaPN ?? ""), out var acik) && acik.Kritik;
                
                // Kişisel gelişim potansiyeli
                yetkinlikler.TryGetValue(parcaId, out var mevcutSv);
                bool svYukseltir = modul.HedefYetkinlikSeviyesi > mevcutSv;

                if (atolyeKritik || isSayisi > 0 || svYukseltir)
                {
                    string neden = "";
                    if (atolyeKritik) 
                        neden = $"Atölyenizde {parca.ParcaAdi} için SV5 yetkinliği olan personel bulunmuyor! ";
                    else if (isSayisi > 0)
                        neden = $"{parca.ParcaAdi} için bekleyen {isSayisi} iş emri var. ";
                    
                    if (svYukseltir)
                        neden += $"Bu eğitim yetkinliğinizi SV{modul.HedefYetkinlikSeviyesi} seviyesine yükseltecektir.";

                    oneriler.Add(new EgitimOnerisi(
                        modul,
                        neden,
                        isSayisi,
                        atolyeKritik || (isSayisi >= 3)
                    ));
                }
            }

            return [.. oneriler.OrderByDescending(o => o.Kritik).ThenByDescending(o => o.AcikIsSayisi)];
        }
        public async Task UpsertAsync(Yetkinlik yetkinlik, string guncelleyenSicil = "sistem")
        {
            await using var ctx = await factory.CreateDbContextAsync();
            await using var tx = await ctx.Database.BeginTransactionAsync();

            yetkinlik.GuncellenmeTarihi = DateTime.UtcNow;
            yetkinlik.GuncelleyenSicil = guncelleyenSicil;

            var existing = await ctx.Yetkinlikler
                .FirstOrDefaultAsync(y => y.PersonelId == yetkinlik.PersonelId && y.ParcaSablonuId == yetkinlik.ParcaSablonuId);

            if (existing == null)
            {
                ctx.Yetkinlikler.Add(yetkinlik);
                await ctx.SaveChangesAsync();

                ctx.YetkinlikGecmisleri.Add(new YetkinlikGecmisi
                {
                    YetkinlikId = yetkinlik.Id,
                    SicilNo = yetkinlik.SicilNo,
                    ParcaPN = yetkinlik.ParcaPN ?? "",
                    EskiSeviye = 0,
                    YeniSeviye = yetkinlik.YetkinlikSeviyesi,
                    IslemYapanSicil = guncelleyenSicil,
                    IslemTarihi = DateTime.UtcNow,
                    IslemNotu = "İlk kayıt"
                });
            }
            else
            {
                var eskiSeviye = existing.YetkinlikSeviyesi;
                existing.YetkinlikSeviyesi = yetkinlik.YetkinlikSeviyesi;
                existing.Aciklama = yetkinlik.Aciklama;
                existing.GuncellenmeTarihi = DateTime.UtcNow;
                existing.GuncelleyenSicil = guncelleyenSicil;

                if (eskiSeviye != yetkinlik.YetkinlikSeviyesi)
                {
                    ctx.YetkinlikGecmisleri.Add(new YetkinlikGecmisi
                    {
                        YetkinlikId = existing.Id,
                        SicilNo = existing.SicilNo,
                        ParcaPN = existing.ParcaPN ?? "",
                        EskiSeviye = eskiSeviye,
                        YeniSeviye = yetkinlik.YetkinlikSeviyesi,
                        IslemYapanSicil = guncelleyenSicil,
                        IslemTarihi = DateTime.UtcNow,
                        IslemNotu = $"Seviye güncellendi: {eskiSeviye} → {yetkinlik.YetkinlikSeviyesi}"
                    });
                }
            }

            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task DeleteAsync(int id, string guncelleyenSicil = "sistem")
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var ytk = await ctx.Yetkinlikler.FindAsync(id);
            if (ytk == null) return;

            ctx.YetkinlikGecmisleri.Add(new YetkinlikGecmisi
            {
                YetkinlikId = ytk.Id,
                SicilNo = ytk.SicilNo,
                ParcaPN = ytk.ParcaPN ?? "",
                EskiSeviye = ytk.YetkinlikSeviyesi,
                YeniSeviye = 0,
                IslemYapanSicil = guncelleyenSicil,
                IslemTarihi = DateTime.UtcNow,
                IslemNotu = "Kayıt silindi"
            });

            ctx.Yetkinlikler.Remove(ytk);
            await ctx.SaveChangesAsync();
        }

        public async Task<List<YetkinlikGecmisi>> GetYetkinlikGecmisiAsync(string sicilNo)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.YetkinlikGecmisleri
                .Where(g => g.SicilNo == sicilNo)
                .OrderByDescending(g => g.IslemTarihi)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<GelisimYolHaritasi>> GenerateRoadmapAsync(int atolyeId, List<YetkinlikHedefi> hedefler)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            
            // 1. O atölyedeki personelleri getir
            var personeller = await ctx.Personeller
                .Where(p => p.BakimGrubuId == atolyeId && p.Durum == nameof(PersonelDurumu.Aktif))
                .ToListAsync();

            if (!personeller.Any()) return new List<GelisimYolHaritasi>();

            // 2. Personellerin mevcut yetkinliklerini çek
            var personelIdler = personeller.Select(p => p.Id).ToList();
            var mevcutYetkinlikler = await ctx.Yetkinlikler
                .Where(y => personelIdler.Contains(y.PersonelId))
                .ToListAsync();

            // 3. Personellerin üzerindeki anlık iş yükünü (Bekleyen + Devam Eden) çek
            var isYukleri = await ctx.BakimKontrolKayitlari
                .Where(k => k.AtananPersonelId.HasValue && personelIdler.Contains(k.AtananPersonelId.Value) && k.Durum != nameof(BakimDurumu.Tamamlandi))
                .GroupBy(k => k.AtananPersonelId!.Value)
                .Select(g => new { PersonelId = g.Key, IsSayisi = g.Count() })
                .ToDictionaryAsync(x => x.PersonelId, x => x.IsSayisi);

            var egitimModulleri = await ctx.EgitimModulleri.ToListAsync();

            var yolHaritasi = new Dictionary<int, GelisimYolHaritasi>();

            foreach (var hedef in hedefler)
            {
                var acikSayisi = hedef.HedefUzmanSayisi - hedef.MevcutUzmanSayisi;
                if (acikSayisi <= 0) continue;

                // 4. Adayları Skorla (Recommendation / Heuristic Fitness Function)
                var adaylar = personeller.Select(p => 
                {
                    var ytk = mevcutYetkinlikler.FirstOrDefault(y => y.PersonelId == p.Id && y.ParcaSablonuId == hedef.ParcaSablonuId);
                    int mevcutSv = ytk?.YetkinlikSeviyesi ?? 0;
                    
                    // Eğer zaten SV5 ise aday olamaz (açığı kapatmaz, zaten uzmandır)
                    if (mevcutSv >= 5) return null;

                    isYukleri.TryGetValue(p.Id, out int isYuku);

                    // Puanlama Mantığı (Akıllı Tavsiye):
                    // SV Seviyesi yüksek olanlar avantajlı (+20 puan / SV)
                    // İş yükü az olanlar avantajlı (-5 puan / İş)
                    double skor = (mevcutSv * 20) - (isYuku * 5) + 50; // Taban Puan 50
                    if (skor < 1) skor = 1;

                    return new { Personel = p, MevcutSv = mevcutSv, Skor = skor };
                })
                .Where(a => a != null)
                .OrderByDescending(a => a!.Skor)
                .ToList();

                // 5. İhtiyaç duyulan sayıda en iyi adayı seç (En Yüksek Skorlu Adaylar)
                var secilenAdaylar = adaylar.Take(acikSayisi).ToList();

                foreach (var aday in secilenAdaylar)
                {
                    if (!yolHaritasi.TryGetValue(aday!.Personel.Id, out var harita))
                    {
                        harita = new GelisimYolHaritasi
                        {
                            PersonelId = aday.Personel.Id,
                            PersonelAdSoyad = aday.Personel.AdSoyad,
                            Skor = Math.Round(aday.Skor, 1)
                        };
                        yolHaritasi[aday.Personel.Id] = harita;
                    }

                    // 6. Yol Haritası Adımlarını Oluştur
                    // Kişi SV kaçta ise oradan başlayarak hedef SV'ye kadar eğitim ve görev adımları ata
                    int adimAyi = 1;
                    int anlikSv = aday.MevcutSv;

                    // Varsa bu parçanın teorik eğitimlerini ekle
                    var ilgiliEgitimler = egitimModulleri
                        .Where(e => e.ParcaSablonuId == hedef.ParcaSablonuId && e.HedefYetkinlikSeviyesi > anlikSv)
                        .OrderBy(e => e.HedefYetkinlikSeviyesi);
                        
                    foreach(var egitim in ilgiliEgitimler)
                    {
                        harita.Adimlar.Add(new YolHaritasiAdimi
                        {
                            Tur = "Eğitim",
                            Baslik = egitim.Ad,
                            TahminiTamamlanma = DateTime.UtcNow.AddMonths(adimAyi++),
                            HedefSeviye = egitim.HedefYetkinlikSeviyesi
                        });
                        anlikSv = Math.Max(anlikSv, egitim.HedefYetkinlikSeviyesi);
                    }

                    // Uzman olana kadar pratik (Kritik Görev) adımları ekle
                    while (anlikSv < 5)
                    {
                        anlikSv++;
                        harita.Adimlar.Add(new YolHaritasiAdimi
                        {
                            Tur = "Kritik Görev",
                            Baslik = $"{hedef.ParcaAdi} - OJT ve Pratik Görev (Seviye {anlikSv})",
                            TahminiTamamlanma = DateTime.UtcNow.AddMonths(adimAyi++),
                            HedefSeviye = anlikSv
                        });
                    }
                }
            }

            return yolHaritasi.Values.OrderByDescending(h => h.Skor).ToList();
        }

        public async Task<List<GelisimYolHaritasi>> GeneratePlanForGapsAsync(List<YetkinlikAcigi> kritikAciklar, string userVoiceCommand = "")
        {
            await using var ctx = await factory.CreateDbContextAsync();

            // Load all required data upfront
            var tumPersoneller = await ctx.Personeller
                .Include(p => p.BakimGrubu)
                .Where(p => p.Durum == nameof(PersonelDurumu.Aktif))
                .ToListAsync();

            var tumYetkinlikler = await ctx.Yetkinlikler.ToListAsync();

            var isYukleri = await ctx.BakimKontrolKayitlari
                .Where(k => k.AtananPersonelId.HasValue && k.Durum != nameof(BakimDurumu.Tamamlandi))
                .GroupBy(k => k.AtananPersonelId!.Value)
                .Select(g => new { PersonelId = g.Key, IsSayisi = g.Count() })
                .ToDictionaryAsync(x => x.PersonelId, x => x.IsSayisi);

            var egitimModulleri = await ctx.EgitimModulleri.ToListAsync();
            var parcalar = await ctx.ParcaSablonlari.ToListAsync();
            var gruplar = await ctx.BakimGruplari.ToListAsync();

            if (geminiService.IsConfigured)
            {
                var geminiPlan = await geminiService.GeneratePlanWithGeminiAsync(
                    kritikAciklar, tumPersoneller, egitimModulleri, tumYetkinlikler, isYukleri, userVoiceCommand);

                if (geminiPlan != null && geminiPlan.Count > 0)
                {
                    return geminiPlan;
                }
            }

            var yolHaritasi = new Dictionary<int, GelisimYolHaritasi>();

            foreach (var acik in kritikAciklar)
            {
                // Find the ParcaSablonu by PN
                var parca = parcalar.FirstOrDefault(p => p.ParcaPN == acik.ParcaPN);
                if (parca == null) continue;

                // Find the group for this gap
                var grup = gruplar.FirstOrDefault(g => string.Equals(g.GrupAdi, acik.AtolyeAdi, StringComparison.OrdinalIgnoreCase));
                if (grup == null) continue;

                // Personel in this workshop
                var personeller = tumPersoneller
                    .Where(p => p.BakimGrubuId == grup.Id ||
                                (!string.IsNullOrEmpty(grup.AtolyeKodu) && p.AtolyeKodu == grup.AtolyeKodu))
                    .ToList();
                if (!personeller.Any()) continue;

                // How many more experts needed (at least 1)
                int acikSayisi = Math.Max(1, acik.ToplamPersonel - acik.MevcutSv5Sayisi);

                // Score candidates
                var adaylar = personeller
                    .Select(p =>
                    {
                        var ytk = tumYetkinlikler.FirstOrDefault(y => y.PersonelId == p.Id && y.ParcaSablonuId == parca.Id);
                        int sv = ytk?.YetkinlikSeviyesi ?? 0;
                        if (sv >= 5) return null;
                        isYukleri.TryGetValue(p.Id, out int isYuku);
                        double skor = (sv * 20) - (isYuku * 5) + 50;
                        if (skor < 1) skor = 1;
                        return new { Personel = p, MevcutSv = sv, Skor = skor };
                    })
                    .Where(a => a != null)
                    .OrderByDescending(a => a!.Skor)
                    .Take(acikSayisi)
                    .ToList();

                foreach (var aday in adaylar)
                {
                    if (!yolHaritasi.TryGetValue(aday!.Personel.Id, out var harita))
                    {
                        harita = new GelisimYolHaritasi
                        {
                            PersonelId = aday.Personel.Id,
                            PersonelAdSoyad = aday.Personel.AdSoyad,
                            AtolyeAdi = acik.AtolyeAdi,
                            Skor = Math.Round(aday.Skor, 1)
                        };
                        yolHaritasi[aday.Personel.Id] = harita;
                    }

                    int adimAyi = harita.Adimlar.Count + 1;
                    int anlikSv = aday.MevcutSv;

                    // Training modules for this part above current SV
                    var ilgiliEgitimler = egitimModulleri
                        .Where(e => e.ParcaSablonuId == parca.Id && e.HedefYetkinlikSeviyesi > anlikSv)
                        .OrderBy(e => e.HedefYetkinlikSeviyesi);

                    foreach (var egitim in ilgiliEgitimler)
                    {
                        harita.Adimlar.Add(new YolHaritasiAdimi
                        {
                            Tur = "Eğitim",
                            Baslik = egitim.Ad,
                            TahminiTamamlanma = DateTime.UtcNow.AddMonths(adimAyi++),
                            HedefSeviye = egitim.HedefYetkinlikSeviyesi,
                            EgitimModuluId = egitim.Id
                        });
                        anlikSv = Math.Max(anlikSv, egitim.HedefYetkinlikSeviyesi);
                    }

                    // Practical OJT steps up to SV5
                    while (anlikSv < 5)
                    {
                        anlikSv++;
                        harita.Adimlar.Add(new YolHaritasiAdimi
                        {
                            Tur = "Pratik Görev",
                            Baslik = $"{acik.ParcaAdi} — OJT Seviye {anlikSv}",
                            TahminiTamamlanma = DateTime.UtcNow.AddMonths(adimAyi++),
                            HedefSeviye = anlikSv,
                            EgitimModuluId = null
                        });
                    }
                }
            }

            return yolHaritasi.Values.OrderBy(h => h.AtolyeAdi).ThenByDescending(h => h.Skor).ToList();
        }

        public async Task<int> PersonelEgitimlerOnayla(
            List<(int PersonelId, int EgitimModuluId)> atamalar,
            Dictionary<(int, int), DateTime>? tahminiTarihler = null)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            int kayitSayisi = 0;

            foreach (var (personelId, egitimModuluId) in atamalar)
            {
                var zatenVar = await ctx.PersonelEgitimleri
                    .AnyAsync(pe => pe.PersonelId == personelId && pe.EgitimModuluId == egitimModuluId);
                if (zatenVar) continue;

                // Eğitim süresini mevcut SV'ye göre hesapla
                var modul = await ctx.EgitimModulleri.FindAsync(egitimModuluId);
                var ytkSure = modul?.ParcaSablonuId.HasValue == true
                    ? await ctx.Yetkinlikler.FirstOrDefaultAsync(y =>
                        y.PersonelId == personelId && y.ParcaSablonuId == modul.ParcaSablonuId.Value)
                    : null;
                int mevcutSvSure = ytkSure?.YetkinlikSeviyesi ?? 0;
                int surGun = SvEsikler.EgitimSuresiGun(mevcutSvSure);

                ctx.PersonelEgitimleri.Add(new PersonelEgitim
                {
                    PersonelId = personelId,
                    EgitimModuluId = egitimModuluId,
                    IlerlemeYuzdesi = 0,
                    Tamamlandi = false,
                    TamamlanmaTarihi = DateTime.MinValue,
                    PlanlananBaslangic = DateTime.UtcNow,
                    PlanlananBitis = tahminiTarihler?.GetValueOrDefault((personelId, egitimModuluId))
                                     ?? DateTime.UtcNow.AddDays(surGun)
                });

                // Personel durumunu Egitimde yap
                var personel = await ctx.Personeller.FindAsync(personelId);
                if (personel != null && personel.Durum == nameof(PersonelDurumu.Aktif))
                    personel.Durum = nameof(PersonelDurumu.Egitimde);

                kayitSayisi++;
            }

            if (kayitSayisi > 0)
            {
                await ctx.SaveChangesAsync();
                bildirimServisi.Ekle(new Bildirim
                {
                    Baslik = "Eğitim Planı Onaylandı",
                    Mesaj = $"{kayitSayisi} personel eğitim ataması oluşturuldu.",
                    Turu = BildirimTuru.SistemBilgisi,
                    Seviye = BildirimSeviye.Bilgi
                });
            }

            return kayitSayisi;
        }

        public async Task<List<KapsamaAnalizi>> GetKapsamaAnaliziAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var gruplar      = await ctx.BakimGruplari.AsNoTracking().ToListAsync();
            var personeller  = await ctx.Personeller.AsNoTracking().ToListAsync();
            var parcalar     = await ctx.ParcaSablonlari.AsNoTracking().ToListAsync();
            var yetkinlikler = await ctx.Yetkinlikler
                .Select(y => new { y.PersonelId, y.ParcaSablonuId, y.YetkinlikSeviyesi })
                .AsNoTracking().ToListAsync();

            var result = new List<KapsamaAnalizi>();
            foreach (var g in gruplar)
            {
                var pIds         = personeller.Where(p => p.BakimGrubuId == g.Id).Select(p => p.Id).ToHashSet();
                int toplamPersonel = pIds.Count;
                var gParcalar    = parcalar.Where(p => p.BakimGrubuId == g.Id);

                foreach (var p in gParcalar)
                {
                    var band  = yetkinlikler.Where(y => pIds.Contains(y.PersonelId) && y.ParcaSablonuId == p.Id).ToList();
                    int sv1   = band.Count(y => y.YetkinlikSeviyesi == 1);
                    int sv2   = band.Count(y => y.YetkinlikSeviyesi == 2);
                    int sv3p  = band.Count(y => y.YetkinlikSeviyesi >= 3);
                    int sv4p  = band.Count(y => y.YetkinlikSeviyesi >= 4);
                    bool pipeline = (sv1 + sv2) > 0;
                    double oran   = toplamPersonel > 0 ? (double)sv3p / toplamPersonel : 0;

                    string risk = sv3p == 0 && !pipeline  ? "ACİL"
                                : sv3p == 0               ? "KRİTİK"
                                : sv3p == 1 && oran < 0.15 && !pipeline ? "KRİTİK"
                                : sv3p == 1               ? "YÜKSEK"
                                : sv3p == 2 && oran < 0.40 ? "ORTA"
                                : sv4p == 0               ? "İYİ"
                                :                           "TAMAM";

                    result.Add(new KapsamaAnalizi(
                        g.GrupAdi, p.ParcaAdi, p.ParcaPN ?? "",
                        p.HeliTipi ?? "", toplamPersonel,
                        sv1, sv2, sv3p, sv4p, pipeline, risk));
                }
            }

            return [.. result.OrderBy(r => r.RiskSeviyesi switch {
                "ACİL"   => 0, "KRİTİK" => 1, "YÜKSEK" => 2,
                "ORTA"   => 3, "İYİ"    => 4, _         => 5 })];
        }

        public async Task<List<EgitimCizelgesiItem>> GetEgitimCizelgesiAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var kayitlar = await ctx.PersonelEgitimleri
                .Include(pe => pe.Personel).ThenInclude(p => p!.BakimGrubu)
                .Include(pe => pe.EgitimModulu).ThenInclude(e => e!.ParcaSablonu)
                .AsNoTracking().ToListAsync();

            var yetkinlikler = await ctx.Yetkinlikler.AsNoTracking().ToListAsync();
            var bugun = DateTime.UtcNow;

            return kayitlar.Select(pe =>
            {
                var ytk = yetkinlikler.FirstOrDefault(y =>
                    y.PersonelId == pe.PersonelId &&
                    y.ParcaSablonuId == pe.EgitimModulu!.ParcaSablonuId);
                int hedefSv   = pe.EgitimModulu?.HedefYetkinlikSeviyesi ?? 2;
                // Tamamlanan eğitimlerde yetkinlik zaten hedefSv'ye yükseldi;
                // geçmiş gösterimi için "başladığı" seviyeyi hedefSv-1 olarak hesapla
                int mevcutSv  = pe.Tamamlandi
                    ? Math.Max(0, hedefSv - 1)
                    : (ytk?.YetkinlikSeviyesi ?? 0);

                // Kümülatif eşik dahil doğru kalan kart hesabı
                int threshold = SvEsikler.KumulatifEsik(mevcutSv) + SvEsikler.SonrakiEsik(mevcutSv);
                int done      = ytk?.TamamlananKartSayisi ?? 0;
                int kalanKart = Math.Max(1, threshold - done);

                DateTime bas  = pe.PlanlananBaslangic ?? bugun;
                DateTime bit  = pe.PlanlananBitis     ?? bugun.AddDays(SvEsikler.EgitimSuresiGun(mevcutSv));
                string renk   = hedefSv switch { 1 => "#a855f7", 2 => "#3B82F6", 3 => "#F59E0B", 4 => "#10B981", _ => "#FACC15" };

                // Gerçek zamanlı ilerleme yüzdesi (YetkinlikSayfasi ile aynı formül)
                int esik     = SvEsikler.SonrakiEsik(mevcutSv);
                int baseline = SvEsikler.KumulatifEsik(mevcutSv);
                int pct      = pe.Tamamlandi ? 100
                               : esik > 0 && esik != int.MaxValue
                                 ? Math.Min(100, Math.Max(0, (done - baseline) * 100 / esik))
                                 : 0;

                return new EgitimCizelgesiItem(
                    pe.PersonelId,
                    pe.Personel?.AdSoyad ?? "?",
                    pe.Personel?.BakimGrubu?.GrupAdi ?? "",
                    pe.EgitimModulu?.Ad ?? "",
                    pe.EgitimModulu?.ParcaSablonu?.ParcaAdi ?? "",
                    hedefSv,
                    pe.EgitimModuluId,
                    ToMs(bas), ToMs(bit),
                    pe.Tamamlandi, pct, renk,
                    pe.Tamamlandi ? pe.TamamlanmaTarihi : null,
                    mevcutSv);
            }).ToList();

            static long ToMs(DateTime dt) =>
                new DateTimeOffset(dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime())
                    .ToUnixTimeMilliseconds();
        }

        public async Task<EgitimModulu> EnsureModulAsync(int parcaId, int hedefSv)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var modul = await ctx.EgitimModulleri
                .FirstOrDefaultAsync(m => m.ParcaSablonuId == parcaId && m.HedefYetkinlikSeviyesi == hedefSv);
            if (modul == null)
            {
                var parca = await ctx.ParcaSablonlari.FindAsync(parcaId);
                modul = new EgitimModulu
                {
                    Ad                     = $"{parca?.ParcaAdi ?? "Parça"} - SV{hedefSv} Eğitimi",
                    HedefYetkinlikSeviyesi = hedefSv,
                    ParcaSablonuId         = parcaId,
                    Kategori               = "OJT",
                    Aciklama               = $"Otomatik oluşturuldu: SV{hedefSv - 1} → SV{hedefSv} geçiş eğitimi"
                };
                ctx.EgitimModulleri.Add(modul);
                await ctx.SaveChangesAsync();
            }
            return modul;
        }

        public async Task<HashSet<(int PersonelId, int ParcaSablonuId)>> GetAktifEgitimParcalariAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var kayitlar = await ctx.PersonelEgitimleri
                .Include(pe => pe.EgitimModulu)
                .Where(pe => !pe.Tamamlandi && pe.EgitimModulu!.ParcaSablonuId.HasValue)
                .Select(pe => new { pe.PersonelId, pe.EgitimModulu!.ParcaSablonuId })
                .AsNoTracking()
                .ToListAsync();
            return kayitlar.Select(x => (x.PersonelId, x.ParcaSablonuId!.Value)).ToHashSet();
        }

        public async Task AtamaKaldirAsync(int personelId, int egitimModuluId)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var kayit = await ctx.PersonelEgitimleri
                .FirstOrDefaultAsync(pe => pe.PersonelId == personelId && pe.EgitimModuluId == egitimModuluId);
            if (kayit != null)
            {
                ctx.PersonelEgitimleri.Remove(kayit);
                await ctx.SaveChangesAsync();

                // Aktif eğitim kalmadıysa personeli Aktif'e çek
                bool kalanEgitimVar = await ctx.PersonelEgitimleri
                    .AnyAsync(pe => pe.PersonelId == personelId && !pe.Tamamlandi);
                if (!kalanEgitimVar)
                {
                    var personel = await ctx.Personeller.FindAsync(personelId);
                    if (personel != null && personel.Durum == nameof(PersonelDurumu.Egitimde))
                        personel.Durum = nameof(PersonelDurumu.Aktif);
                    await ctx.SaveChangesAsync();
                }
            }
        }
    }
}
