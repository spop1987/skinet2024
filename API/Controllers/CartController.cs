using Core.Interfaces;
using Core.RedisDtos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class CartController(ICartService _cartService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<ShoppingCart>> GetCartbyId(string id)
        {
            var cart = await _cartService.GetCartAsync(id);
            return Ok(cart ?? new ShoppingCart{Id = id});
        }

        [HttpPost]
        public async Task<ActionResult<ShoppingCart>> UpdateCart(ShoppingCart cart)
        {
            var updatedCart = await _cartService.SetCartAsync(cart);

            if(updatedCart == null) return BadRequest("Problem with cart");

            return updatedCart;
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteCart(string id)
        {
            var result = await _cartService.DeleteCartAsync(id);

            if(!result) return BadRequest("Problem deleting cart");

            return Ok();
        }
    }
}