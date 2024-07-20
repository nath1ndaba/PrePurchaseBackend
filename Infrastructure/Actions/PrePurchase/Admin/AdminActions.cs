using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BackendServices;
using BackendServices.Actions.PrePurchase.AdminPortal;
using BackendServices.Exceptions;
using BackendServices.Models;
using MongoDB.Bson;
using PrePurchase.Models;
using admin = PrePurchase.Models.PrePurchase;

namespace Infrastructure.Actions.PrePurchase.Admin;

public class AdminActions : IAdminActions
{
    private readonly IRepository<admin.Admin> _admin;
    private readonly IRepository<Address> _address;
    private readonly IPasswordManager _passwordManager;

    public AdminActions(IRepository<admin.Admin> admin, IRepository<Address> address, IPasswordManager passwordManager)
    {
        _admin = admin;
        _address = address;
        _passwordManager = passwordManager;
    }


    public async Task<Response> RegisterAdmin(AdminRegisterModel model, ObjectId createdBy)
    {
        admin.Admin admin = await _admin.FindOne(x =>
            x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant());

        if (admin is not null)
            throw new HttpResponseException(new Response(HttpStatusCode.Conflict,
                error: $@"An admin ""{model.Name}"" is already registered!"));

        var hash = await _passwordManager.Hash(model.Password);

        admin.Admin adminData = new()
        {
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = createdBy,
            DeletedIndicator = false,
            Name = model.Name,
            Surname = model.Surname,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Password = hash,
        };

        admin = await _admin.FindOne(x =>
           x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant());
        model.Address.AddressBelongsToId = admin.Id;

        await _admin.Insert(adminData);
        await _address.Insert(model.Address);
        return new Response<admin.Admin>(admin, HttpStatusCode.Created);
    }

    public async Task<Response> UpdateAdmin(AdminRegisterModel model, ObjectId updatedBy)
    {
        admin.Admin exists = await _admin.FindOne(x =>
            x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant());

        if (exists is null)
            throw new HttpResponseException(new Response(HttpStatusCode.Conflict,
                error: $@"An admin ""{model.Name}"" is not found!"));
        if (model.Password is not null)
        {
            var hash = await _passwordManager.Hash(model.Password);
            exists.Password = hash;
        }

        exists.UpdatedBy = updatedBy;
        exists.UpdatedDate = DateTime.UtcNow;
        exists.Name = model.Name ?? exists.Name;
        exists.Surname = model.Surname ?? exists.Surname;
        exists.PhoneNumber = model.PhoneNumber ?? exists.PhoneNumber;
        exists.Email = model.Email ?? exists.Email;

        await _admin.Update(exists.Id.ToString(), exists);
        await _address.Update(model.Address.Id.ToString(), model.Address);

        return new Response<admin.Admin>(exists, HttpStatusCode.Accepted);
    }

    public async Task<Response> GetAdmin(string adminId)
    {
        admin.Admin exists = await _admin.FindOne(x => x.Id == ObjectId.Parse(adminId));

        if (exists is null)
        {
            throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                error: $@"Admin provided not found!"));
        }

        return new Response<admin.Admin>(exists, HttpStatusCode.Found);
    }

    public async Task<Response> GetAdmins()
    {
        var admins = await _admin.Find(x => x.DeletedIndicator == false);

        if (admins is null)
        {
            throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@"Admins not found!"));
        }

        return new Response<IEnumerable<admin.Admin>>(admins, HttpStatusCode.Found);
    }

    public async Task<Response> ArchiveAdmin(string adminId, string updatedBy)
    {
        admin.Admin admin = await _admin.FindOne(x => x.Id == ObjectId.Parse(adminId));

        if (admin is null)
        {
            throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                error: $@"Admin provided not found!"));
        }
        admin.UpdatedDate = DateTime.UtcNow;
        admin.UpdatedBy = ObjectId.Parse(updatedBy);
        admin.DeletedIndicator = true;
        var results = await _admin.Update(adminId, admin);

        return new Response<admin.Admin>(admin, HttpStatusCode.OK);
    }

    public async Task<Response> RestoreAdmin(string adminId, string updatedBy)
    {
        admin.Admin admin = await _admin.FindOne(x => x.Id == ObjectId.Parse(adminId));

        if (admin is null)
        {
            throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                error: $@"Admin provided not found!"));
        }
        admin.UpdatedDate = DateTime.UtcNow;
        admin.UpdatedBy = ObjectId.Parse(updatedBy);
        admin.DeletedIndicator = false;
        var results = await _admin.Update(adminId, admin);

        return new Response<admin.Admin>(admin, HttpStatusCode.OK);
    }
}