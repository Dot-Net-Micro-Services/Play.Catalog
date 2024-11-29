using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;
using Play.Catalog.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private const string AdminRole = "Admin";
        private readonly IRepository<Item> itemsRepository;
        private readonly IPublishEndpoint publishEndpoint;
        
        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
        {
            this.itemsRepository = itemsRepository;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [Authorize(Policy = Policies.Read)]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            IEnumerable<ItemDto> items = (await itemsRepository.GetAllAsync()).Select(item => item.AsDto());
            return Ok(items);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Policies.Read)]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            ItemDto item = (await itemsRepository.GetAsync(id)).AsDto();

            if (item is null)
            {
                return NotFound();
            }

            return item;
        }

        // POST /items
        [HttpPost]
        [Authorize(Policy = Policies.Write)]
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto itemToCreate)
        {
            Item item = new()
            {
                Name = itemToCreate.Name,
                Description = itemToCreate.Description,
                Price = itemToCreate.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };
            await itemsRepository.CreateAsync(item);
            await publishEndpoint.Publish(new CatalogItemCreated(
                item.Id, 
                item.Name, 
                item.Description,
                item.Price
            ));
            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = Policies.Write)]
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto itemToUpdate)
        {
            Item existingItem = await itemsRepository.GetAsync(id);
            if (existingItem is null)
            {
                return NotFound();
            }

            existingItem.Name = itemToUpdate.Name;
            existingItem.Description = itemToUpdate.Description;
            existingItem.Price = itemToUpdate.Price;

            await itemsRepository.UpdateAsync(existingItem);
            await publishEndpoint.Publish(new CatalogItemUpdated(
                existingItem.Id, 
                existingItem.Name, 
                existingItem.Description,
                existingItem.Price
            ));

            return NoContent();
        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.Write)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            Item existingItem = await itemsRepository.GetAsync(id);
            if (existingItem is null)
            {
                return NotFound();
            }

            await itemsRepository.DeleteAsync(existingItem.Id);
            await publishEndpoint.Publish(new CatalogItemDeleted(id));
            
            return NoContent();
        }
    }
}