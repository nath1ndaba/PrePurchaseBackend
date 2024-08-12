using BackendServices;
using BackendServices.Actions.PrePurchase;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PrePurchase.Models;
using PrePurchase.Models.PrePurchase;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.PrePurchase;

public class ShopActions : IShopActions
{
    private readonly IRepository<Shop> _shop;
    private readonly IRepository<Address> _address;
    private readonly IPasswordManager _passwordManager;
    private readonly ILogger<ShopActions> _logger;

    public ShopActions(IRepository<Shop> shop, IRepository<Address> address, IPasswordManager passwordManager, ILogger<ShopActions> logger)
    {
        _shop = shop ?? throw new ArgumentNullException(nameof(shop));
        _address = address ?? throw new ArgumentNullException(nameof(address));
        _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Response> RegisterShop(ShopDto model, string createdBy)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        _logger.LogInformation($"Registering shop with email: {model.Email}");

        Shop shop = await _shop.FindOne(x =>
            x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant());

        if (shop != null)
        {
            _logger.LogWarning($"A Shop with email {model.Email} is already registered.");
            throw new HttpResponseException(new Response(HttpStatusCode.Conflict,
                error: $@"A Shop ""{model.Name}"" is already registered!"));
        }

        // Generate and hash QR Code
        model.QRCode = $"{model.Email}{model.Name}{model.ContactNumber}{model.Id}";
        model.QRCode = await _passwordManager.Hash(model.QRCode);

        model.Id = ObjectId.GenerateNewId().ToString();
        model.CreatedBy = createdBy;
        model.UpdatedBy = createdBy;
        model.CreatedDate = DateTime.UtcNow;
        model.UpdatedDate = DateTime.UtcNow;
        model.DeletedIndicator = false;

        var newShop = new Shop();
        newShop.DtoToShop(model);

        await _shop.Insert(newShop);

        shop = await _shop.FindOne(x =>
            x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant());

        if (shop == null)
        {
            _logger.LogError($"Shop with email {model.Email} could not be created.");
            throw new HttpResponseException(new Response(HttpStatusCode.InternalServerError, error: "Failed to create shop."));
        }

        if (model.Address != null)
        {
            model.Address.AddressBelongsToId = shop.Id;
            await _address.Insert(model.Address);
        }

        return new Response<ShopDto>(model, HttpStatusCode.Created);
    }

    public async Task<Response> UpdateShop(ShopDto model, string updatedBy)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        _logger.LogInformation($"Updating shop with email: {model.Email}");

        Shop exists = await _shop.FindOne(x =>
            x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant());

        if (exists == null)
        {
            _logger.LogWarning($"A Shop with email {model.Email} was not found.");
            throw new HttpResponseException(new Response(HttpStatusCode.Conflict,
                error: $@"A Shop ""{model.Name}"" is not found!"));
        }

        UpdateShopDetails(exists, model, ObjectId.Parse(updatedBy));
        await _shop.Update(exists.Id.ToString(), exists);

        var shopAddress = await _address.FindOne(x => x.AddressBelongsToId == exists.Id);
        if (shopAddress == null && model.Address != null)
        {
            model.Address.AddressBelongsToId = exists.Id;
            await _address.Insert(model.Address);
        }
        else if (model.Address != null)
        {
            await _address.Update(shopAddress.Id.ToString(), model.Address);
        }

        return new Response<ShopDto>(model, HttpStatusCode.Accepted);
    }

    public async Task<Response> GetShop(string shopId)
    {
        if (string.IsNullOrEmpty(shopId))
            throw new ArgumentNullException(nameof(shopId));

        _logger.LogInformation($"Retrieving shop with ID: {shopId}");

        Shop exists = await _shop.FindOne(x => x.Id == ObjectId.Parse(shopId));

        if (exists == null)
        {
            _logger.LogWarning($"Shop with ID {shopId} not found.");
            throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                error: $@"Shop provided not found!"));
        }

        return new Response<Shop>(exists, HttpStatusCode.Found);
    }

    public async Task<Response> GetShops()
    {
        _logger.LogInformation("Retrieving all shops");

        var shops = await _shop.Find(x => x.DeletedIndicator == false) ?? new List<Shop>();
        List<ShopDto> shopsDto = new List<ShopDto>();
        foreach (var shop in shops)
        {
            var shopDto = new ShopDto();
            shopDto.DtoFromShop(shop);
            shopsDto.Add(shopDto);
        }

        return new Response<IEnumerable<ShopDto>>(shopsDto, HttpStatusCode.Found);
    }

    public async Task<Response> ArchiveShop(string shopId, string updatedBy)
    {
        if (string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(updatedBy))
            throw new ArgumentNullException(nameof(shopId), "Parameters cannot be null or empty.");

        _logger.LogInformation($"Archiving shop with ID: {shopId}");

        Shop shop = await _shop.FindOne(x => x.Id == ObjectId.Parse(shopId));

        if (shop == null)
        {
            _logger.LogWarning($"Shop with ID {shopId} not found.");
            throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                error: $@"Shop provided not found!"));
        }

        UpdateShopStatus(shop, updatedBy, true);
        await _shop.Update(shopId, shop);

        return new Response<Shop>(shop, HttpStatusCode.OK);
    }

    public async Task<Response> RestoreShop(string shopId, string updatedBy)
    {
        if (string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(updatedBy))
            throw new ArgumentNullException(nameof(shopId), "Parameters cannot be null or empty.");

        _logger.LogInformation($"Restoring shop with ID: {shopId}");

        Shop shop = await _shop.FindOne(x => x.Id == ObjectId.Parse(shopId));

        if (shop == null)
        {
            _logger.LogWarning($"Shop with ID {shopId} not found.");
            throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                error: $@"Shop provided not found!"));
        }

        UpdateShopStatus(shop, updatedBy, false);
        await _shop.Update(shopId, shop);

        return new Response<Shop>(shop, HttpStatusCode.OK);
    }

    private void UpdateShopDetails(Shop shop, ShopDto model, ObjectId updatedBy)
    {
        shop.UpdatedBy = updatedBy;
        shop.UpdatedDate = DateTime.UtcNow;
        shop.Name = model.Name ?? shop.Name;
        shop.Email = model.Email ?? shop.Email;
        shop.ContactNumber = model.ContactNumber ?? shop.ContactNumber;
        shop.LicenseExpiryDate = model.LicenseExpiryDate ?? shop.LicenseExpiryDate;
        shop.RegisterationNumber = model.RegisterationNumber ?? shop.RegisterationNumber;
    }

    private void UpdateShopStatus(Shop shop, string updatedBy, bool deletedIndicator)
    {
        shop.UpdatedBy = ObjectId.Parse(updatedBy);
        shop.UpdatedDate = DateTime.UtcNow;
        shop.DeletedIndicator = deletedIndicator;
    }
}
