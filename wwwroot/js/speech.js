window.MroSpeech = {
    recognition: null,
    dotNetRef: null,

    initRecognition: function (dotNetRef) {
        window.MroSpeech.dotNetRef = dotNetRef;
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            console.warn("Web Speech Recognition API is not supported in this browser.");
            return false;
        }

        const rec = new SpeechRecognition();
        rec.lang = 'tr-TR';
        rec.continuous = false;
        rec.interimResults = false;

        rec.onstart = () => {
            if (window.MroSpeech.dotNetRef) {
                window.MroSpeech.dotNetRef.invokeMethodAsync('OnSpeechStart');
            }
        };

        rec.onresult = (event) => {
            const transcript = event.results[0][0].transcript;
            if (window.MroSpeech.dotNetRef) {
                window.MroSpeech.dotNetRef.invokeMethodAsync('OnSpeechResult', transcript);
            }
        };

        rec.onerror = (event) => {
            console.error("Speech recognition error", event.error);
            if (window.MroSpeech.dotNetRef) {
                window.MroSpeech.dotNetRef.invokeMethodAsync('OnSpeechError', event.error);
            }
        };

        rec.onend = () => {
            if (window.MroSpeech.dotNetRef) {
                window.MroSpeech.dotNetRef.invokeMethodAsync('OnSpeechEnd');
            }
        };

        window.MroSpeech.recognition = rec;
        return true;
    },

    startListening: function () {
        if (window.MroSpeech.recognition) {
            try {
                window.MroSpeech.recognition.start();
            } catch (e) {
                console.error("Start listening error:", e);
            }
        }
    },

    stopListening: function () {
        if (window.MroSpeech.recognition) {
            window.MroSpeech.recognition.stop();
        }
    },

    speak: function (text) {
        if ('speechSynthesis' in window) {
            // Devam eden konuşmaları iptal et
            window.speechSynthesis.cancel();

            const utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = 'tr-TR';
            
            // Türkçe ses bulmaya çalış
            const voices = window.speechSynthesis.getVoices();
            const trVoice = voices.find(v => v.lang.startsWith('tr') || v.lang === 'tr-TR');
            if (trVoice) {
                utterance.voice = trVoice;
            }
            
            utterance.pitch = 1.05; // Cyberpunk/Asistan hissi için hafif tizleştirildi
            utterance.rate = 1.05;  // Akıcı bir konuşma hızı
            
            window.speechSynthesis.speak(utterance);
        } else {
            console.error("Speech Synthesis is not supported in this browser.");
        }
    },

    stopSpeaking: function () {
        if ('speechSynthesis' in window) {
            window.speechSynthesis.cancel();
        }
    }
};
