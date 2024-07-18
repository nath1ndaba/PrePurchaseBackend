using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BackendServices;
using BackendServices.Actions.PrePurchase.AdminPortal;
using BackendServices.Exceptions;
using BackendServices.Models;
using MongoDB.Bson;
using admin = PrePurchase.Models.PrePurchase;

namespace Infrastructure.Actions.PrePurchase.Admin;

public class AdminActions : IAdminActions
{
    private readonly IRepository<admin.Admin> _admin;
    private readonly IPasswordManager _passwordManager;

    public AdminActions(IRepository<admin.Admin> admin, IPasswordManager passwordManager)
    {
        _admin = admin;
        _passwordManager = passwordManager;
    }


    public async Task<Response> Register(AdminRegisterModel model, ObjectId createdBy)
    {
        admin.Admin exists = await _admin.FindOne(x =>
            x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant());

        if (exists is not null)
            throw new HttpResponseException(new Response(HttpStatusCode.Conflict,
                error: $@"An admin ""{model.Name}"" is already registered!"));

        var hash = await _passwordManager.Hash(model.Password);

        admin.Admin admin = new()
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

        await _admin.Insert(admin);

        return new Response<admin.Admin>(admin, HttpStatusCode.Created);
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