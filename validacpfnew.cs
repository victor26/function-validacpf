using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace validacpfnew
{
    public class validacpfnew
    {
        private readonly ILogger<validacpfnew> _logger;

        public validacpfnew(ILogger<validacpfnew> logger)
        {
            _logger = logger;
        }

        [Function("validacpfnew")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Verifica se o método da requisição é POST
            if (req.Method != HttpMethods.Post)
            {
                return new BadRequestObjectResult("Only POST requests are allowed.");
            }

            // Lê o corpo da requisição
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Desserializa o JSON para um objeto
            var cpfData = JsonSerializer.Deserialize<CpfRequest>(requestBody);

            // Verifica se o CPF foi enviado
            if (cpfData == null || string.IsNullOrEmpty(cpfData.Cpf))
            {
                return new BadRequestObjectResult("Incluir um CPF para validação");
            }

            // Valida o CPF
            bool isValid = ValidarCpf(cpfData.Cpf);

            if (isValid)
            {
                return new OkObjectResult("CPF Valido.");
            }
            else
            {
                return new BadRequestObjectResult("CPF Invalido");
            }
        }

        private bool ValidarCpf(string cpf)
        {
            // Remove caracteres não numéricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            // Verifica se o CPF tem 11 dígitos
            if (cpf.Length != 11)
            {
                return false;
            }

            // Verifica se todos os dígitos são iguais
            if (cpf.Distinct().Count() == 1)
            {
                return false;
            }

            // Calcula o primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (10 - i);
            }
            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            // Calcula o segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (11 - i);
            }
            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            // Verifica se os dígitos verificadores estão corretos
            return cpf.EndsWith(digito1.ToString() + digito2.ToString());
        }

        private class CpfRequest
        {
            [JsonPropertyName("cpf")]
            public string Cpf { get; set; }
        }
    }
}