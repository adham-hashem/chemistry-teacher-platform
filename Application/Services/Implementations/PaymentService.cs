using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Dtos.PaymentDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _paymobApiKey;
        private readonly string _paymobIntegrationId;
        private readonly string _paymobIframeId;
        private readonly string _paymobHmac;

        public PaymentService(
            HttpClient httpClient,
            IPaymentRepository paymentRepository,
            ISubscriptionRepository subscriptionRepository,
            UserManager<ApplicationUser> userManager)
        {
            _httpClient = httpClient;
            _paymentRepository = paymentRepository;
            _subscriptionRepository = subscriptionRepository;
            _userManager = userManager;
            _paymobApiKey = Environment.GetEnvironmentVariable("PAYMOB_API_KEY");
            _paymobIntegrationId = Environment.GetEnvironmentVariable("PAYMOB_INTEGRATION_ID");
            _paymobIframeId = Environment.GetEnvironmentVariable("PAYMOB_IFRAME_ID");
            _paymobHmac = Environment.GetEnvironmentVariable("PAYMOB_HMAC");
        }

        public async Task<PaymentInitiateResponseDto> InitiatePaymentAsync(PaymentInitiateRequestDto request, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId);
            if (subscription == null)
                throw new Exception("Subscription not found.");

            // Step 1: Authenticate with Paymob
            var authResponse = await AuthenticateAsync();
            var authToken = authResponse.Token;

            // Step 2: Create Order
            var orderResponse = await CreateOrderAsync(authToken, request.Amount, request.SubscriptionId.ToString());

            // Step 3: Generate Payment Token
            var paymentTokenResponse = await GeneratePaymentTokenAsync(authToken, orderResponse.OrderId, request.Amount, user);

            // Step 4: Store Payment
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                SubscriptionId = request.SubscriptionId,
                UserId = userId,
                Amount = request.Amount,
                Currency = "EGP",
                PaymentMethod = request.PaymentMethod,
                TransactionId = paymentTokenResponse.Token,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            await _paymentRepository.AddAsync(payment);

            return new PaymentInitiateResponseDto
            {
                PaymentToken = paymentTokenResponse.Token,
                IframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_paymobIframeId}?payment_token={paymentTokenResponse.Token}"
            };
        }

        public async Task ProcessPaymentCallbackAsync(PaymentCallbackDto callback)
        {
            var payment = await _paymentRepository.GetByTransactionIdAsync(callback.TransactionId);
            if (payment == null)
                throw new Exception("Payment not found.");

            payment.Status = callback.Success ? "Completed" : "Failed";
            await _paymentRepository.UpdateAsync(payment);

            if (callback.Success)
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(payment.SubscriptionId);
                if (subscription != null)
                {
                    subscription.IsActive = true; // Custom property to mark subscription as active
                    await _subscriptionRepository.UpdateAsync(subscription);
                }
            }
        }

        private async Task<PaymobAuthResponse> AuthenticateAsync()
        {
            var requestBody = new { api_key = _paymobApiKey };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://accept.paymob.com/api/auth/tokens", content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<PaymobAuthResponse>(await response.Content.ReadAsStringAsync());
        }

        private async Task<PaymobOrderResponse> CreateOrderAsync(string authToken, decimal amount, string subscriptionId)
        {
            var requestBody = new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = (int)(amount * 100),
                currency = "EGP",
                merchant_order_id = subscriptionId
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://accept.paymob.com/api/ecommerce/orders", content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<PaymobOrderResponse>(await response.Content.ReadAsStringAsync());
        }

        private async Task<PaymobPaymentTokenResponse> GeneratePaymentTokenAsync(string authToken, string orderId, decimal amount, ApplicationUser user)
        {
            var requestBody = new
            {
                auth_token = authToken,
                amount_cents = (int)(amount * 100),
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    email = user.Email,
                    first_name = user.FirstName,
                    last_name = user.LastName,
                    phone_number = user.PhoneNumber ?? "NA",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "NA",
                    country = "Egypt",
                    state = "NA"
                },
                currency = "EGP",
                integration_id = _paymobIntegrationId
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://accept.paymob.com/api/acceptance/payment_keys", content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<PaymobPaymentTokenResponse>(await response.Content.ReadAsStringAsync());
        }

        private class PaymobAuthResponse
        {
            public string Token { get; set; }
        }

        private class PaymobOrderResponse
        {
            public string OrderId { get; set; }
        }

        private class PaymobPaymentTokenResponse
        {
            public string Token { get; set; }
        }
    }
}
