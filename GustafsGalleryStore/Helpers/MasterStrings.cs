using System;
namespace GustafsGalleryStore.Helpers
{
    public static class MasterStrings
    {
        // Email Strings
        public const string AdminEmail = "seanpconkie@yahoo.com";
        public const string Header = "<!DOCTYPE html><html lang='en'><head><style>body,h1,h2,h3,h4,h5,h6 {font-family: 'Lato', sans-serif;} body, html {height: 100%;color: #777;line-height: 1.8;} .w3-button:hover{color:#000!important;background-color:#ccc!important} .w3-button{border:none;display:inline-block;padding:8px 16px;vertical-align:middle;overflow:hidden;text-decoration:none;color:inherit;background-color:inherit;text-align:center;cursor:pointer;white-space:nowrap} .w3-round,.w3-round-medium{border-radius:4px} .w3-dark-grey,.w3-hover-dark-grey:hover,.w3-dark-gray,.w3-hover-dark-gray:hover{color:#fff!important;background-color:#616161!important} .w3-table,.w3-table-all{border-collapse:collapse;border-spacing:0;width:100%;display:table}.w3-table-all{border:1px solid #ccc} </style></head>";
        public const string SpamMessage = "<p>Please could you check your emails (including your Spam box) once you have placed your order, as we may have to contact you regarding your order. This is especially important for items requiring personalisation.</p>";

        public const string CustomerNewOrder = "<h4>Thanks you for your purchase!</h4><hr><p>Hi {0},<br/>We are processing your order. We will notify you when it has been sent.</p>";
        public const string StoreNewOrder = "<h4>You've had a new order!</h4><hr><p>Hi {0},<br/>you've received a new order.</p>";

        public const string AwaitingStockMessage = "<p>Hi {0},<br/>Some of the items in your order are out of stock. We will notify you when they are back in stock and your order has been sent.</p>";
        public const string OrderDispatchedMessage = "<p>Hi {0},<br/>Your order has been dispatched.</p><p>View your order details to see postage details and track your package.</p>";
        public const string OrderCancelledMessage = "<p>Hi {0},<br/>Your order number {1} has been cancelled.</p>";
        public const string OrderReturnMessage = "<p>Hi {0},<br/>Your return has been completed.</p>";

        public const string CustomerCancellation = "<p>Hi {0},<br/>Thank you for notifying us of your cancellation.  We'll be in touch shortly to confirm when this has been completed.  Any payments made will be refunded as soon as possible.</p>";
        public const string StoreCancellation = "<p>Hi {0},<br/>An order has been cancelled.</p>";

        public const string CustomerReturn = "<p>Hi {0},<br/>Thank you for notifying us of your return.  We'll be in touch shortly to confirm more details on making your return.</p>";
        public const string StoreReturn = "<p>Hi {0},<br/>A return has been requested.</p>";

        public const string CustomerOrderLink = "<p><a href='https://gustafsgallery.co.uk/Orders/ViewOrder?id={0}' class='w3-button w3-dark-grey w3-round'>View Order</a></p>";
        public const string StoreOrderLink = "<p><a href='https://gustafsgallery.co.uk/ManageOrders/ViewOrder?id={0}' class='w3-button w3-dark-grey w3-round'>View Order</a></p>";

        public const string OrderSummaryStart = "<h4>Order Summary</h4><hr><table class='w3-table' style='width: 100%'><tbody>";
        public const string OrderSummaryFinish = "</tbody></table>";

        public const string StoreName = "Gustaf";

        // User Roles
        public const string StaffRole = "IsStaff";

        // Order Status
        public const string Basket = "Basket";
        public const string AwaitingReturn = "Awaiting Return";
        public const string AwaitingStock = "Awaiting Stock";
        public const string CancellationCompleted = "Cancellation Completed";
        public const string OrderCancelled = "Order Cancelled";
        public const string OrderDispatched = "Order Dispatched";
        public const string OrderPlaced = "Order Placed";
        public const string OrderReturned = "Order Returned";
        public const string ReturnCompleted = "Return Completed";

        // Payment States
        public const string ThreeDSecureRequired = "required";
        public const string ThreeDSecureRecommended = "recommended";

        public const string StripeDeclined = "declined_by_network";
        public const string StripeApproved = "approved_by_network";
        public const string StripeNotSent = "not_sent_to_network";
        public const string StripeReversed = "reversed_after_approval";
        public const string StripeResultPending = "pending";
        public const string StripeResultCancelled = "cancelled";
        public const string StripeResultFailed = "failed";
        public const string StripeResultSucceeded = "succeeded";

        public const string PayPalResultFailed = "failed";
        public const string PayPalResultApproved = "approved";
        public const string PayPalResultCompleted = "completed";
        public const string PayPalResultPending = "pending";
        public const string PayPalResultCancelled = "cancelled";

        // Keys
        public const string PayPalClientId = "";
        public const string PayPalSecretKey = "";
        public const string AWSAccessKeyId = "";
        public const string AWSSecretAccessKey = "";
        public const string StripeAPIKey = "";
        public const string FacebookApiId = "";
        public const string FacebookSecretKey = "";
        public const string TwitterApiId = "";
        public const string TwitterSecrectKey = "";
        public const string GoogleApiId = "";
        public const string GoogleSecretKey = "";
        public const string MicrosoftApiId = "";
        public const string MicrosoftSecretKey = "";
        public const string InstagramApiId = "";
        public const string InstagramSecretKey = "";
        public const string YahooApiId = "";
        public const string YahooSecretKey = "";
        public const string LinkedInApiId = "";
        public const string LinkedInSecretKey = "";
        public const string GitHubApiId = "";
        public const string GithubSecretKey = "";
    }
}
