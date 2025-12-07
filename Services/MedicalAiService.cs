using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ClinicManagement.Services
{
    public class MedicalAiService
    {
        private readonly string _apiKey;
        private readonly HttpClient _http;

        public MedicalAiService(IConfiguration config)
        {
            _apiKey = config["OpenAI:ApiKey"];
            _http = new HttpClient();
        }

        public async Task<string> GeneratePatientSummary(string name, int age, string doctorNotes, string diagnosis)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a medical assistant that summarizes patient information."},
                    new { role = "user", content = $"Patient: {name}, Age: {age}, Diagnosis: {diagnosis}, Notes: {doctorNotes}" }
                }
            };

            var json = JsonConvert.SerializeObject(body);
            var response = await _http.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var result = await response.Content.ReadAsStringAsync();

            dynamic ai = JsonConvert.DeserializeObject(result);

            try
            {
                if (ai == null)
                    return "⚠ No response from AI.";
                if (ai.error != null)
                    return $"⚠ API Error: {ai.error.message}";
                if (ai.choices == null || ai.choices.Count == 0)
                    return "⚠ AI returned no choices.";
                if (ai.choices[0].message == null)
                    return "⚠ AI returned empty message.";

                return ai.choices[0].message.content.Value;
            }
            catch
            {
                return "⚠ Failed to parse AI response.";
            }
        }



        public async Task<string> ChatWithAI(string name, int age, string doctorNotes, string diagnosis, string userMessage)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
            new { role = "system", content = "You are a professional medical assistant. Provide clinically helpful answers, but do NOT make a final diagnosis or prescribe medication." },
            new { role = "user", content = $"Patient Name: {name}, Age: {age}, Diagnosis: {diagnosis}, Notes: {doctorNotes}" },
            new { role = "user", content = $"Doctor Question: {userMessage}" }
        }
            };

            var json = JsonConvert.SerializeObject(body);

            var response = await _http.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var result = await response.Content.ReadAsStringAsync();

            dynamic ai = JsonConvert.DeserializeObject(result);

            try
            {
                if (ai == null)
                    return "⚠ No response from AI.";
                if (ai.error != null)
                    return $"⚠ API Error: {ai.error.message}";
                if (ai.choices == null || ai.choices.Count == 0)
                    return "⚠ AI returned no choices.";
                if (ai.choices[0].message == null)
                    return "⚠ AI returned empty message.";

                return ai.choices[0].message.content.Value;
            }
            catch
            {
                return "⚠ Failed to parse AI response.";
            }
        }


    }
}
