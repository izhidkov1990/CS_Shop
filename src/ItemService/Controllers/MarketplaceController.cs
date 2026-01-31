using ItemService.Exceptions;
using ItemService.Models;
using ItemService.Models.Dto;
using ItemService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ItemService.Controllers
{
    [Route("market")]
    [ApiController]
    public class MarketplaceController : ControllerBase
    {
        private const int DefaultAppId = 730;
        private const string DefaultContextId = "2";
        private const int MaxPageSize = 100;

        private readonly IMarketplaceService _marketplaceService;
        private readonly ILogger<MarketplaceController> _logger;

        public MarketplaceController(IMarketplaceService marketplaceService, ILogger<MarketplaceController> logger)
        {
            _marketplaceService = marketplaceService;
            _logger = logger;
        }

        [HttpGet("listings")]
        public async Task<ActionResult<IEnumerable<ListingResponse>>> GetListings([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
        {
            take = Math.Clamp(take, 1, MaxPageSize);
            skip = Math.Max(0, skip);

            var listings = await _marketplaceService.GetActiveListingsAsync(skip, take, cancellationToken);
            return Ok(listings.Select(MapListing));
        }

        [HttpGet("listings/{id:guid}")]
        public async Task<ActionResult<ListingResponse>> GetListingById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var listing = await _marketplaceService.GetListingAsync(id, cancellationToken);
            if (listing == null)
            {
                return NotFound();
            }

            return Ok(MapListing(listing));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("listings")]
        public async Task<ActionResult<ListingResponse>> CreateListing([FromBody] CreateListingRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var steamId = GetSteamId();
                var appId = request.AppId ?? DefaultAppId;
                var contextId = string.IsNullOrWhiteSpace(request.ContextId) ? DefaultContextId : request.ContextId;

                var listing = await _marketplaceService.CreateListingAsync(steamId, appId, contextId, request, cancellationToken);
                return Ok(MapListing(listing));
            }
            catch (MarketplaceException ex)
            {
                _logger.LogWarning(ex, "Marketplace error during listing creation");
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during listing creation");
                return BadRequest("Failed to create listing.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("listings/{id:guid}")]
        public async Task<IActionResult> CancelListing([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var steamId = GetSteamId();
                await _marketplaceService.CancelListingAsync(id, steamId, cancellationToken);
                return NoContent();
            }
            catch (MarketplaceException ex)
            {
                _logger.LogWarning(ex, "Marketplace error during listing cancel");
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during listing cancel");
                return BadRequest("Failed to cancel listing.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("cart/{listingId:guid}")]
        public async Task<ActionResult<CartItemResponse>> AddToCart([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            try
            {
                var steamId = GetSteamId();
                var cartItem = await _marketplaceService.AddToCartAsync(steamId, listingId, cancellationToken);
                return Ok(MapCartItem(cartItem));
            }
            catch (MarketplaceException ex)
            {
                _logger.LogWarning(ex, "Marketplace error during add to cart");
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during add to cart");
                return BadRequest("Failed to add item to cart.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("cart")]
        public async Task<ActionResult<IEnumerable<CartItemResponse>>> GetCart(CancellationToken cancellationToken)
        {
            var steamId = GetSteamId();
            var cartItems = await _marketplaceService.GetCartAsync(steamId, cancellationToken);
            return Ok(cartItems.Select(MapCartItem));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("cart/{listingId:guid}")]
        public async Task<IActionResult> RemoveFromCart([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            var steamId = GetSteamId();
            await _marketplaceService.RemoveFromCartAsync(steamId, listingId, cancellationToken);
            return NoContent();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutResponse>> Checkout(CancellationToken cancellationToken)
        {
            try
            {
                var steamId = GetSteamId();
                var purchases = await _marketplaceService.CheckoutAsync(steamId, cancellationToken);
                var response = new CheckoutResponse
                {
                    Purchases = purchases.Select(MapPurchase).ToList()
                };
                response.Total = response.Purchases.Sum(p => p.Price);
                if (response.Purchases.Count > 0)
                {
                    response.Currency = response.Purchases[0].Currency;
                }

                return Ok(response);
            }
            catch (MarketplaceException ex)
            {
                _logger.LogWarning(ex, "Marketplace error during checkout");
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during checkout");
                return BadRequest("Checkout failed.");
            }
        }

        private string GetSteamId()
        {
            var steamId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(steamId))
            {
                throw new MarketplaceException("User steamId not found in token.", 401);
            }

            return steamId;
        }

        private static ListingResponse MapListing(MarketListing listing)
        {
            return new ListingResponse
            {
                Id = listing.Id,
                SellerSteamId = listing.SellerSteamId,
                AppId = listing.AppId,
                ContextId = listing.ContextId,
                AssetId = listing.AssetId,
                ClassId = listing.ClassId,
                InstanceId = listing.InstanceId,
                Amount = listing.Amount,
                Name = listing.Name,
                MarketHashName = listing.MarketHashName,
                ItemType = listing.ItemType,
                IconUrl = listing.IconUrl,
                Price = listing.Price,
                Currency = listing.Currency,
                Status = listing.Status,
                CreatedAt = listing.CreatedAt,
                SoldAt = listing.SoldAt
            };
        }

        private static CartItemResponse MapCartItem(CartItem cartItem)
        {
            return new CartItemResponse
            {
                Id = cartItem.Id,
                ListingId = cartItem.ListingId,
                Quantity = cartItem.Quantity,
                CreatedAt = cartItem.CreatedAt,
                Listing = cartItem.Listing == null ? new ListingResponse() : MapListing(cartItem.Listing)
            };
        }

        private static PurchaseResponse MapPurchase(Purchase purchase)
        {
            return new PurchaseResponse
            {
                Id = purchase.Id,
                ListingId = purchase.ListingId,
                Price = purchase.Price,
                Currency = purchase.Currency,
                PurchasedAt = purchase.PurchasedAt,
                SellerSteamId = purchase.SellerSteamId,
                BuyerSteamId = purchase.BuyerSteamId,
                ItemName = purchase.ItemName,
                MarketHashName = purchase.MarketHashName,
                ItemType = purchase.ItemType,
                IconUrl = purchase.IconUrl,
                AppId = purchase.AppId,
                ContextId = purchase.ContextId,
                AssetId = purchase.AssetId,
                ClassId = purchase.ClassId,
                InstanceId = purchase.InstanceId,
                Amount = purchase.Amount
            };
        }
    }
}
