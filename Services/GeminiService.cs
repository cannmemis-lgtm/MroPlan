using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MroPlan.Models;

namespace MroPlan.Services
{
    /// <summary>
    /// Google Gemini API (Free Tier) kullanarak akıllı yetkinlik ve eğitim
    /// planlaması yapar. Sesli komutları analiz ederek bağlamsal çıktılar üretir.
    /// </summary>
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger)
        {
            _http = http;
            _logger = logger;
            _apiKey = config["Gemini:ApiKey"] ?? "";
            _model = config["Gemini:Model"] ?? "gemini-1.5-flash";
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        /// <summary>
        /// Gemini API kullanarak yetkinlik açıklarını gidermek için akıllı yol haritası hazırlar.
        /// </summary>
        public async Task<List<GelisimYolHaritasi>?> GeneratePlanWithGeminiAsync(
            List<YetkinlikAcigi> gaps,
            List<Personel> personels,
            List<EgitimModulu> modules,
            List<Yetkinlik> yetkinlikler,
            Dictionary<int, int> workLoads,
            string userVoiceCommand = "")
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("Gemini API key tanımlı değil. Local algoritma kullanılacak.");
                return null;
            }

            try
            {
                // Bağlamsal veriyi metinleştir
                var gapsText = string.Join("\n", gaps.Select(g => $"- {g.AtolyeAdi}: {g.ParcaAdi} ({g.ParcaPN}) | Gerekli: {g.ToplamPersonel} Uzman, Mevcut: {g.MevcutSv5Sayisi} Uzman {(g.Kritik ? "[KRİTİK AÇIK]" : "")}"));
                
                var personelsText = string.Join("\n", personels.Select(p => {
                    var yList = yetkinlikler.Where(y => y.PersonelId == p.Id).Select(y => $"{y.ParcaPN}:SV{y.YetkinlikSeviyesi}");
                    workLoads.TryGetValue(p.Id, out int wCount);
                    return $"- ID: {p.Id} | {p.AdSoyad} | Atölye: {p.BakimGrubu?.GrupAdi ?? "Belirtilmemiş"} | Yetkinlikler: {(yList.Any() ? string.Join(", ", yList) : "Yok")} | Aktif İş Yükü: {wCount} kart";
                }));

                var modulesText = string.Join("\n", modules.Select(m => $"- ID: {m.Id} | Adı: {m.Ad} | İlgili Parça ID: {m.ParcaSablonuId} | Hedef Seviye: SV{m.HedefYetkinlikSeviyesi}"));

                // Sistem ve kullanıcı Promptları
                string systemInstruction = "Sen havacılık helikopter bakım üssünde kıdemli eğitim ve operasyon yöneticisisin. Sana verilen yetkinlik açıklarını kapatmak için en doğru personelleri seçmeli, eğitim atamaları yapmalı ve gelişim yolları oluşturmalısın. Sonucu mutlaka belirtilen JSON şemasında dönmelisin.";

                string prompt = $@"
Aşağıdaki yetkinlik açıklarını ve personel havuzunu analiz et.
Yöneticinin Sesli Komutu/Talebi: ""{(string.IsNullOrWhiteSpace(userVoiceCommand) ? "Tüm kritik açıkları kapatacak optimal bir plan hazırla." : userVoiceCommand)}""

[YETKİNLİK AÇIKLARI]
{gapsText}

[PERSONEL VE MEVCUT DURUMLARI]
{personelsText}

[KULLANILABİLİR EĞİTİM MODÜLLERİ]
{modulesText}

[PLANLAMA KURALLARI]
1. Seviye 5 (Uzman) olan birine yeni eğitim veya pratik atama.
2. İş yükü 3'ten fazla olan personellere yeni görev yığma.
3. Tercihen seviyesi yüksek olanları (Seviye 3 veya 4) seçerek hızlıca uzman yap (Verimlilik).
4. Her personel için oluşturduğun gelişim planının nedenini 'aiAciklamasi' alanında Türkçe olarak açıkla. Bu açıklama sesli asistan tarafından okunacağı için akıcı, profesyonel ve teşvik edici bir dille yazılmalıdır (max 2 cümle).

JSON formatında 'yolHaritalari' listesi dön.";

                // Gemini API Call
                var requestBody = new GeminiRequest();
                requestBody.Contents.Add(new Content
                {
                    Parts = new List<Part> { new Part { Text = prompt } }
                });

                requestBody.GenerationConfig.SystemInstruction = new Content
                {
                    Parts = new List<Part> { new Part { Text = systemInstruction } }
                };

                // JSON Schema Tanımlaması (Structured Outputs)
                requestBody.GenerationConfig.ResponseMimeType = "application/json";
                requestBody.GenerationConfig.ResponseSchema = GetJsonSchema();

                var url = $"models/{_model}:generateContent?key={_apiKey}";
                var response = await _http.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API hata döndü: {err}", err);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                var jsonText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrWhiteSpace(jsonText)) return null;

                var finalResult = JsonSerializer.Deserialize<GeminiPlanResult>(jsonText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return finalResult?.YolHaritalari;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini planlama servisi çağrısında hata");
                return null;
            }
        }

        private ResponseSchema GetJsonSchema()
        {
            // C# DTO'larına mükemmel eşleşen JSON Şeması
            return new ResponseSchema
            {
                Type = "object",
                Required = new List<string> { "yolHaritalari" },
                Properties = new Dictionary<string, SchemaProperty>
                {
                    {
                        "yolHaritalari", new SchemaProperty
                        {
                            Type = "array",
                            Description = "Personel gelişim yol haritaları listesi",
                            Items = new SchemaProperty
                            {
                                Type = "object",
                                Required = new List<string> { "personelId", "personelAdSoyad", "atolyeAdi", "skor", "aiAciklamasi", "adimlar" },
                                Properties = new Dictionary<string, SchemaProperty>
                                {
                                    { "personelId", new SchemaProperty { Type = "integer", Description = "Personelin veritabanındaki ID'si" } },
                                    { "personelAdSoyad", new SchemaProperty { Type = "string", Description = "Personelin adı soyadı" } },
                                    { "atolyeAdi", new SchemaProperty { Type = "string", Description = "Personelin atölye adı" } },
                                    { "skor", new SchemaProperty { Type = "number", Description = "Uyum skoru (0-100)" } },
                                    { "aiAciklamasi", new SchemaProperty { Type = "string", Description = "Bu personelin seçilme nedeni ve mentör notu (Türkçe)" } },
                                    {
                                        "adimlar", new SchemaProperty
                                        {
                                            Type = "array",
                                            Items = new SchemaProperty
                                            {
                                                Type = "object",
                                                Required = new List<string> { "tur", "baslik", "hedefSeviye" },
                                                Properties = new Dictionary<string, SchemaProperty>
                                                {
                                                    { "tur", new SchemaProperty { Type = "string", Description = "'Eğitim' veya 'Pratik Görev'" } },
                                                    { "baslik", new SchemaProperty { Type = "string", Description = "Eğitim veya görevin adı" } },
                                                    { "hedefSeviye", new SchemaProperty { Type = "integer", Description = "Hedef yetkinlik seviyesi (1-5)" } },
                                                    { "egitimModuluId", new SchemaProperty { Type = "integer", Description = "Varsa ilgili eğitim modülünün ID'si, yoksa null" } }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        // --- GEMINI REQUEST MODELLERİ ---
        private class GeminiRequest
        {
            [JsonPropertyName("contents")]
            public List<Content> Contents { get; set; } = new();

            [JsonPropertyName("generationConfig")]
            public GenerationConfig GenerationConfig { get; set; } = new();
        }

        private class Content
        {
            [JsonPropertyName("parts")]
            public List<Part> Parts { get; set; } = new();
        }

        private class Part
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = "";
        }

        private class GenerationConfig
        {
            [JsonPropertyName("responseMimeType")]
            public string ResponseMimeType { get; set; } = "application/json";

            [JsonPropertyName("responseSchema")]
            public ResponseSchema? ResponseSchema { get; set; }

            [JsonPropertyName("systemInstruction")]
            public Content? SystemInstruction { get; set; }
        }

        private class ResponseSchema
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "object";

            [JsonPropertyName("properties")]
            public Dictionary<string, SchemaProperty> Properties { get; set; } = new();

            [JsonPropertyName("required")]
            public List<string> Required { get; set; } = new();
        }

        private class SchemaProperty
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "";

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("items")]
            public SchemaProperty? Items { get; set; }

            [JsonPropertyName("properties")]
            public Dictionary<string, SchemaProperty>? Properties { get; set; }

            [JsonPropertyName("required")]
            public List<string>? Required { get; set; }
        }

        // --- GEMINI RESPONSE MODELLERİ ---
        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            [JsonPropertyName("content")]
            public Content? Content { get; set; }
        }

        private class GeminiPlanResult
        {
            [JsonPropertyName("yolHaritalari")]
            public List<GelisimYolHaritasi> YolHaritalari { get; set; } = new();
        }
    }
}
