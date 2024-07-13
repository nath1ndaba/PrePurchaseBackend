using System;
using System.Net;
using System.Threading.Tasks;
using BackendServices;
using BackendServices.Actions.PrePurchase.AdminPortal;
using BackendServices.Exceptions;
using BackendServices.Models;
using MongoDB.Bson;
using admin = PrePurchase.Models.PrePurchase;

namespace Infrastructure.Actions.PrePurchase.Admin;

public class RegisterAdminActions : IRegisterAdminActions
{
    private readonly IRepository<admin.Admin> _admin;
    private readonly IPasswordManager _passwordManager;

    public RegisterAdminActions(IRepository<admin.Admin> admin, IPasswordManager passwordManager)
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
}