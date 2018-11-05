using System;
using System.Collections.Generic;
using GustafsGalleryStore.Helpers;
using Stripe;

namespace GustafsGalleryStore.Services
{
    public static class StripeHelper
    {

        public static StripeOutcome CreateCharge(string token, long id, string idType, int amount)
        {
            // Set your secret key: remember to change this to your live secret key in production
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.SetApiKey(MasterStrings.StripeSecretKey);

            var options = new StripeChargeCreateOptions
            {
                Amount = amount,
                Currency = "gbp",
                Description = "Charge for Order Id " + id.ToString(),
                SourceTokenOrExistingSourceId = token,
                Metadata = new Dictionary<String, String>() { { idType, id.ToString() } }

            };

            var service = new StripeChargeService();

            try
            {
                StripeCharge charge = service.Create(options);

                if (charge.Outcome.Reason == "approve_with_id" || charge.Outcome.Reason == "issuer_not_available" ||
                   charge.Outcome.Reason == "processing_error" || charge.Outcome.Reason == "reenter_transaction" ||
                   charge.Outcome.Reason == "try_again_later")
                {
                    charge = service.Create(options);
                }

                charge.Outcome.Id = charge.Id;

                return charge.Outcome;
            }
            catch (Exception ex)
            {
                return new StripeOutcome() { NetworkStatus = MasterStrings.StripeDeclined, Reason = ex.Message };
            }

        }

        public static StripeSource CreateSource(string token, int amount, string redirectUrl)
        {
            StripeConfiguration.SetApiKey(MasterStrings.StripeSecretKey);

            var sourceOptions = new StripeSourceCreateOptions
            {
                Amount = amount,
                Currency = "gbp",
                Type = "three_d_secure",
                ThreeDSecureCardOrSourceId = token,
                RedirectReturnUrl = redirectUrl
            };

            var sourceService = new StripeSourceService();

            try
            {
                StripeSource source = sourceService.Create(sourceOptions);

                return source;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public static StripeOutcome ChargeSource(string source, bool livemode, string client_secret, long id, string idType)
        {
            // Set your secret key: remember to change this to your live secret key in production
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.SetApiKey(MasterStrings.StripeSecretKey);

            var service = new StripeSourceService();
            StripeSource sourceService = service.Get(source);

            if (sourceService.Status == "chargeable")
            {

                var options = new StripeChargeCreateOptions
                {
                    Amount = sourceService.Amount,
                    Currency = sourceService.Currency,
                    SourceTokenOrExistingSourceId = source,
                    Description = "Charge for Order Id " + id.ToString(),
                    Metadata = new Dictionary<String, String>() { { idType, id.ToString() } }

                };

                var serviceCharge = new StripeChargeService();

                try
                {
                    StripeCharge charge = serviceCharge.Create(options);

                    if (charge.Outcome.Reason == "approve_with_id" || charge.Outcome.Reason == "issuer_not_available" ||
                       charge.Outcome.Reason == "processing_error" || charge.Outcome.Reason == "reenter_transaction" ||
                       charge.Outcome.Reason == "try_again_later")
                    {
                        charge = serviceCharge.Create(options);
                    }

                    charge.Outcome.Id = charge.Id;
                    return charge.Outcome;
                }
                catch (Exception ex)
                {
                    throw new StripeException(ex.Message);
                }
            }

            return new StripeOutcome() 
            { 
                NetworkStatus = MasterStrings.StripeNotSent,
                SellerMessage = "Card not yet chargeable.",
                Type = "pending"
            };
        }

        public static StripeRefund RefundCharge(string token, int amount)
        {
            // Set your secret key: remember to change this to your live secret key in production
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.SetApiKey(MasterStrings.StripeSecretKey);

            var refundOptions = new StripeRefundCreateOptions()
            {
                Amount = amount,
                Reason = "requested_by_customer"

            };
            var refundService = new StripeRefundService();
            StripeRefund refund = refundService.Create(token, refundOptions);

            return refund;

        }

        public static DeclineMessageViewModel DeclineMessage(string reason)
        {

            var model = new DeclineMessageViewModel();

            switch (reason)
            {
                case "call_issuer":
                case "generic_decline":
                case "fraudulent":
                case "do_not_try_again":
                case "do_not_honor":
                case "approve_with_id":
                case "card_velocity_exceeded":
                case "issuer_not_available":
                case "lost_card":
                case "merchant_blacklist":
                case "no_action_taken":
                case "not_permitted":
                case "pickup_card":
                case "processing_error":
                case "reenter_transaction":
                case "restricted_card":
                case "revocation_of_all_authorizations":
                case "revocation_of_authorization":
                case "security_violation":
                case "service_not_allowed":
                case "stolen_card":
                case "stop_payment_order":
                case "transaction_not_allowed":
                    model.StatusMessage += "The payment has been declined.<br/>Please contact your card issuer for more information.";
                    break;
                case "currency_not_supported":
                    model.StatusMessage += "This card does not support the specified currency.<br/>Please check with the issuer whether the card can be used for the type of currency specified.";
                    break;
                case "card_not_supported":
                    model.StatusMessage += "This card does not support this type of purchase.<br/>Please contact the card issuer to make sure the card can be used to make this type of purchase.";
                    break;
                case "duplicate_transaction":
                    model.StatusMessage += "The payment has been declined.<br/>Please contact your card issuer for more information.";
                    model.PaymentMessage = "A transaction with identical amount and credit card information was submitted very recently.<br/>Check to see if a recent payment already exists.";
                    break;
                case "expired_card":
                    model.StatusMessage += "The card has expired.  Please use another card.";
                    break;
                case "incorrect_number":
                case "invalid_number":
                    model.StatusMessage += "The card number is incorrect.";
                    break;
                case "incorrect_cvc":
                case "invalid_cvc":
                    model.StatusMessage += "The CVC number is incorrect.";
                    break;
                case "incorrect_zip":
                    model.StatusMessage += "The ZIP/postal code is incorrect.";
                    break;
                case "insufficient_funds":
                case "withdrawal_count_limit_exceeded":
                    model.StatusMessage += "The card has insufficient funds to complete the purchase.";
                    break;
                case "invalid_account":
                case "new_account_information_available":
                    model.StatusMessage += "The card, or account the card is connected to, is invalid.<br/>Please contact your card issuer to check that the card is working correctly.";
                    break;
                case "invalid_amount":
                    model.StatusMessage += "The payment amount is invalid, or exceeds the amount that is allowed.<br/>If the amount appears to be correct, please check with your card issuer that you can make purchases of this amount.";
                    break;
                case "invalid_expiry_year":
                    model.StatusMessage += "The expiration year invalid.";
                    break;
                case "testmode_decline":
                    model.StatusMessage += "A Stripe test card number was used.";
                    break;
                default:
                    model.StatusMessage += reason;
                    break;
            }

            return model;
        }

    }
}
