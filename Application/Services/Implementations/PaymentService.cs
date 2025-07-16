using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IDiscountCodeService _discountCodeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _kashierMerchantId;
        private readonly string _kashierApiKey;
        private readonly bool _kashierTestMode;
        private readonly string _merchantRedirectUrl;
        private readonly string _serverWebhookUrl;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ISubscriptionRepository subscriptionRepository,
            IDiscountCodeService discountCodeService,
            UserManager<ApplicationUser> userManager)
        {
            _paymentRepository = paymentRepository;
            _subscriptionRepository = subscriptionRepository;
            _discountCodeService = discountCodeService;
            _userManager = userManager;
            _kashierMerchantId = Environment.GetEnvironmentVariable("KASHIER_MERCHANT_ID")
                ?? throw new Exception("Kashier Merchant ID not configured.");
            _kashierApiKey = Environment.GetEnvironmentVariable("KASHIER_API_KEY")
                ?? throw new Exception("Kashier API Key not configured.");
            _kashierTestMode = Environment.GetEnvironmentVariable("KASHIER_TEST_MODE") == "true";
            _merchantRedirectUrl = Environment.GetEnvironmentVariable("KASHIER_MERCHANT_REDIRECT_URL")
                ?? "https://mywebsite.com/redirect";
            _serverWebhookUrl = Environment.GetEnvironmentVariable("KASHIER_SERVER_WEBHOOK_URL")
                ?? "https://mywebsite.com/webhook";
        }

        public async Task<PaymentInitiateResponseDto> InitiatePaymentAsync(PaymentInitiateRequestDto request, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId);
            if (subscription == null)
                throw new Exception("Subscription not found.");

            // Apply discount if provided
            decimal finalAmount = request.Amount;
            string appliedDiscountCode = null;
            if (!string.IsNullOrEmpty(request.DiscountCode))
            {
                finalAmount = await _discountCodeService.ValidateAndApplyDiscountAsync(request.DiscountCode, request.Amount);
                appliedDiscountCode = request.DiscountCode;
            }

            // Generate order hash
            var paymentId = Guid.NewGuid().ToString();
            var orderHash = GenerateKashierOrderHash(finalAmount, "EGP", request.SubscriptionId.ToString());

            // Store Payment
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                SubscriptionId = request.SubscriptionId,
                UserId = userId,
                Amount = finalAmount,
                OriginalAmount = request.Amount,
                Currency = "EGP",
                PaymentMethod = request.PaymentMethod,
                TransactionId = paymentId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                DiscountCode = appliedDiscountCode
            };
            await _paymentRepository.AddAsync(payment);

            // Prepare iframe attributes
            var customerData = System.Text.Json.JsonSerializer.Serialize(new
            {
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phone = user.PhoneNumber ?? "NA"
            });

            return new PaymentInitiateResponseDto
            {
                PaymentToken = paymentId,
                IframeUrl = "https://payments.kashier.io/kashier-checkout.js",
                IframeAttributes = new
                {
                    data_amount = finalAmount.ToString(),
                    data_hash = orderHash,
                    data_currency = "EGP",
                    data_orderId = request.SubscriptionId.ToString(),
                    data_merchantId = _kashierMerchantId,
                    data_merchantRedirect = Uri.EscapeDataString(_merchantRedirectUrl),
                    data_serverWebhook = Uri.EscapeDataString(_serverWebhookUrl),
                    data_mode = _kashierTestMode ? "test" : "live",
                    data_metaData = System.Text.Json.JsonSerializer.Serialize(new { customKey = "subscription", subscriptionId = request.SubscriptionId }),
                    data_description = $"Subscription {request.SubscriptionId}",
                    data_allowedMethods = "card,bank_installments,wallet,fawry",
                    data_defaultMethod = "card",
                    data_redirectMethod = "get",
                    data_failureRedirect = "TRUE",
                    data_paymentRequestId = paymentId,
                    data_type = "external",
                    data_brandColor = "#2da44e",
                    data_display = "en",
                    data_manualCapture = "FALSE",
                    data_customer = customerData,
                    data_saveCard = "optional",
                    data_interactionSource = "ECOMMERCE",
                    data_enable3DS = "true"
                }
            };
        }

        public async Task ProcessPaymentCallbackAsync(PaymentCallbackDto callback)
        {
            var payment = await _paymentRepository.GetByTransactionIdAsync(callback.TransactionId);
            if (payment == null)
                throw new Exception("Payment not found.");

            // Validate signature
            if (!ValidateKashierSignature(callback))
                throw new Exception("Invalid callback signature.");

            payment.Status = callback.PaymentStatus.ToUpper() == "SUCCESS" ? "Completed" : "Failed";
            await _paymentRepository.UpdateAsync(payment);

            if (callback.PaymentStatus.ToUpper() == "SUCCESS")
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(payment.SubscriptionId);
                if (subscription != null)
                {
                    subscription.IsActive = true;
                    await _subscriptionRepository.UpdateAsync(subscription);
                }
            }
        }

        private string GenerateKashierOrderHash(decimal amount, string currency, string orderId)
        {
            var path = $"/?payment={_kashierMerchantId}.{orderId}.{amount}.{currency}";
            using (var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(_kashierApiKey)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.ASCII.GetBytes(path));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private bool ValidateKashierSignature(PaymentCallbackDto callback)
        {
            var path = "";
            var queryParams = new System.Collections.Specialized.NameValueCollection
            {
                { "paymentStatus", callback.PaymentStatus },
                { "cardDataToken", callback.CardDataToken ?? "" },
                { "maskedCard", callback.MaskedCard ?? "" },
                { "merchantOrderId", callback.MerchantOrderId ?? "" },
                { "orderId", callback.OrderId ?? "" },
                { "cardBrand", callback.CardBrand ?? "" },
                { "transactionId", callback.TransactionId ?? "" },
                { "currency", callback.Currency ?? "" }
            };

            foreach (var key in queryParams.AllKeys.OrderBy(k => k))
            {
                if (key == "signature" || key == "mode")
                    continue;
                path += $"{(path.Length > 0 ? "&" : "")}{key}={queryParams[key]}";
            }

            using (var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(_kashierApiKey)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.ASCII.GetBytes(path));
                var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return computedSignature == callback.Signature?.ToLower();
            }
        }
    }
}