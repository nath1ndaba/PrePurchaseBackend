using BackendServices;
using BackendServices.Actions;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SOG.Models;
using MongoDB.Bson;
using System;
using PrePurchase.Models;
using System.Security.Claims;
using System.Linq;

namespace Infrastructure.Actions
{
#nullable enable
    internal class SimunyeActions : ISimunyeActions
    {

        private readonly IRepository<SOGDepartments> _departments;
        private readonly IRepository<SOGPositions> _positions;
        private readonly IRepository<SOGMembers> _members;
        private readonly IRepository<SOGIzifo> _izifo;
        private readonly IRepository<SOGContribution> _contribution;


        private readonly IPasswordManager _passwordManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUpdateBuilderProvider _updateBuilderProvider;
        private readonly ILogger<SimunyeActions> _logger;


        public SimunyeActions(
             IRepository<SOGDepartments> departments
            , IRepository<SOGPositions> positions
            , IRepository<SOGMembers> membersInfo
            , IRepository<SOGIzifo> isifo
            , IRepository<SOGContribution> contribution

            , IPasswordManager passwordManager
            , IDateTimeProvider dateTimeProvider
            , IUpdateBuilderProvider updateBuilderProvider
            , ILogger<SimunyeActions> logger)
        {
            _departments = departments;
            _positions = positions;
            _members = membersInfo;
            _izifo = isifo;
            _contribution = contribution;

            _dateTimeProvider = dateTimeProvider;
            _updateBuilderProvider = updateBuilderProvider;
            _passwordManager = passwordManager;
            _logger = logger;
        }

        public async Task<Response> Login(MemberLogin model)
        {
            SOGMembers user = await _members.FindOne(x => x.Login.ContactNumber == model.ContactNumber);

            if (user is null) throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Irongo iNombolo Oyifakile!"));

            bool isMatch = await _passwordManager.IsMatch(model.Password, user.Login.Password);

            if (!isMatch) throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Irongo iPassword Oyifakile!"));

            return new Response<SOGMembers>(user);
        }


        public async Task<Response> AddDepartments(string createdBy, string updatedBy, SOGDepartments model)
        {
            model.Department = model.Department.ToLower().Trim();
            SOGDepartments exists = await _departments.FindOne(x => x.Department == model.Department);
            if (exists is not null)
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: "Department already exists!"));

            model.CreatedBy = ObjectId.Parse(createdBy);
            model.UpdatedBy = ObjectId.Parse(updatedBy);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedIndicator = false;

            await _departments.Insert(model);

            return new Response<SOGDepartments>(model, HttpStatusCode.Created);
        }

        public async Task<Response> AddPositions(string createdBy, string updatedBy, SOGPositions model)
        {
            model.Position = model.Position.ToLower().Trim();
            SOGPositions exists = await _positions.FindOne(x => x.Position == model.Position);

            if (exists is not null)
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@" ""{model.Position}"" already exists!"));

            model.CreatedBy = ObjectId.Parse(createdBy);
            model.UpdatedBy = ObjectId.Parse(updatedBy);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedIndicator = false;

            await _positions.Insert(model);

            return new Response<SOGPositions>(model, HttpStatusCode.Created);
        }

        public async Task<Response> AddMember(string createdBy, string updatedBy, SOGMembers model)
        {
            SOGMembers? exists = await _members.FindOne(x => x.Login.ContactNumber == model.Login.ContactNumber);
            if (exists is not null)
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@"A User with Number ""{model.Login.ContactNumber}"" already exists!"));

            model.Login.Password = await _passwordManager.Hash(model.Login.Password);
            model.CreatedBy = ObjectId.Parse(createdBy);
            model.UpdatedBy = ObjectId.Parse(updatedBy);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;

            model.DeletedIndicator = false;
            model.IsDeceased = false;
            await _members.Insert(model);
            return new Response<SOGMembers>(model, HttpStatusCode.Created);
        }

        public async Task<Response> AddIsifo(string createdBy, string updatedBy, SOGIzifo model, string role, string? companyId = null)
        {
            Response response = await GetLoggedMember(role, companyId);
            if (response is not Response<SOGMembers> member)
                throw new HttpResponseException(response);

            SOGIzifo deceased = await _izifo.FindOne(x => x.DeaceasedName.Contains(model.DeaceasedName.ToLower().Trim()) && x.DeaceasedSurname.Contains(model.DeaceasedSurname.ToLower().Trim()));
            if (deceased is not null)
            {
                SOGMembers deaseasedCreator = await _members.FindOne(x => x.Id == deceased.CreatedBy);
                if (deaseasedCreator is null) throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "mmhm khona okungahambanga kahle"));

                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: $@" Inkokheli u""{deaseasedCreator.Ibizo}"" usemufakile!"));
            }

            if (model.IsMember)
            {
                SOGMembers deaseased = await _members.FindOne(x => x.Id == model.IsMemberId);
                deaseased.DeletedIndicator = true;
                deaseased.IsDeceased = true;
                await _members.Update(deaseased.Id.ToString(), deaseased);

            }
            model.CreatedBy = ObjectId.Parse(createdBy);
            model.UpdatedBy = ObjectId.Parse(updatedBy);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedIndicator = false;
            model.IsAllPaid = false;
            await _izifo.Insert(model);

            return new Response<SOGIzifo>(model, HttpStatusCode.Created);
        }

        public async Task<Response> AddContribution(string createdBy, string updatedBy, SOGContribution model, string role, string? memberId = null)
        {
            SOGIzifo? ofileyo = await _izifo.FindOne(x => x.Id == model.IsifoID);
            if (ofileyo is null) throw new HttpResponseException("Lo mufi akaka Regist-wa");


            SOGMembers? member = await _members.FindOne(x => x.Id == model.ContributerId);
            if (member is null) throw new HttpResponseException("Le member ayika Regist-wa");


            var contributer = await _contribution.FindOne(x => x.IsifoID == model.IsifoID && x.ContributerId == member.Id);
            if (contributer is not null)
            {
                SOGMembers? admin = await _members.FindOne(x => x.Id == contributer.UpdatedBy);
                throw new HttpResponseException($"Le member seyifakile imali, imali yakhe ibhalwe ngu {admin.Ibizo}");
            }

            model.CreatedBy = ObjectId.Parse(createdBy);
            model.UpdatedBy = ObjectId.Parse(updatedBy);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedIndicator = false;
            await _contribution.Insert(model);

            IEnumerable<SOGMembers> totalMember = await _members.Find(_ => true);
            IEnumerable<SOGContribution> totalContributions = await _contribution.Find(_ => true);
            if (totalMember?.Count() == totalContributions.Count())
            {
                ofileyo.IsAllPaid = true;
                ofileyo.UpdatedBy = ObjectId.Parse(updatedBy);
                ofileyo.UpdatedDate = DateTime.UtcNow;
                await _izifo.Update(ofileyo.Id.ToString(), ofileyo);
            }

            return new Response<SOGContribution>(model, HttpStatusCode.Created);
        }


        public async Task<Response> UpdateDepartment(string updatedBy, SOGDepartments model)
        {
            SOGDepartments exists = await _departments.FindOne(x => x.Id == model.Id);
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" ""{model.Department}"" does not exists!"));

            exists.Department = model.Department;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGDepartments memberInfo = await _departments.Update(exists.Id.ToString(), exists);
            return new Response<SOGDepartments>(memberInfo, HttpStatusCode.Accepted);
        }

        public async Task<Response> UpdatePosition(string updatedBy, SOGPositions model)
        {
            SOGPositions exists = await _positions.FindOne(x => x.Id == model.Id);
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" ""{model.Position}"" does not exists!"));

            exists.Position = model.Position;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGPositions memberInfo = await _positions.Update(exists.Id.ToString(), exists);
            return new Response<SOGPositions>(memberInfo, HttpStatusCode.Accepted);
        }

        public async Task<Response> UpdateMember(string updatedBy, SOGMembers model)
        {
            SOGMembers exists = await _members.FindOne(x => x.Id == model.Id);
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" ""{model.Ibizo}"" does not exists!"));

            exists.Login.ContactNumber = model.Login.ContactNumber;
            exists.PositionId = model.PositionId;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGMembers memberInfo = await _members.Update(exists.Id.ToString(), exists);
            return new Response<SOGMembers>(memberInfo, HttpStatusCode.Accepted);
        }

        public async Task<Response> UpdateIsifo(string updatedBy, SOGIzifo model)
        {
            SOGIzifo exists = await _izifo.FindOne(x => x.Id == model.Id);
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" ""{model.DeaceasedName}"" does not exists!"));

            exists.DeaceasedName = model.DeaceasedName;
            exists.DeaceasedSurname = model.DeaceasedSurname;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGIzifo izifo = await _izifo.Update(exists.Id.ToString(), exists);
            return new Response<SOGIzifo>(izifo, HttpStatusCode.Accepted);
        }

        public async Task<Response> UpdateContribution(string updatedBy, SOGContribution model)
        {
            SOGContribution exists = await _contribution.FindOne(x => x.Id == model.Id);
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" This contribution does not exists!"));

            exists.ContributedAmount = model.ContributedAmount;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGContribution contribution = await _contribution.Update(exists.Id.ToString(), exists);
            return new Response<SOGContribution>(contribution, HttpStatusCode.Accepted);
        }


        public async Task<Response> GetDepartments(string role, string? MemberId = null)
        {

            Response response = await GetLoggedMember(role, MemberId);
            if (response is not Response<SOGMembers> memberResponse)
                throw new HttpResponseException(response);

            IEnumerable<SOGDepartments> departments = await _departments.Find(x => x.DeletedIndicator == false);

            return new Response<IEnumerable<SOGDepartments>>(departments);
        }

        public async Task<Response> GetPositions(string role, string? MemberId = null)
        {

            Response response = await GetLoggedMember(role, MemberId);
            if (response is not Response<SOGMembers> companyResponse)
                throw new HttpResponseException(response);

            IEnumerable<SOGPositions> positions = await _positions.Find(x => x.DeletedIndicator == false);

            return new Response<IEnumerable<SOGPositions>>(positions);
        }

        public async Task<Response> GetMembers(string role, string? MemberId = null)
        {

            Response response = await GetLoggedMember(role, MemberId);
            if (response is not Response<SOGMembers> companyResponse)
                throw new HttpResponseException(response);

            IEnumerable<SOGMembers> members = await _members.Find(x => x.DeletedIndicator == false);

            return new Response<IEnumerable<SOGMembers>>(members);
        }

        public async Task<Response> GetIzifo(string role, string? MemberId = null)
        {

            Response response = await GetLoggedMember(role, MemberId);
            if (response is not Response<SOGMembers> companyResponse)
                throw new HttpResponseException(response);

            IEnumerable<SOGIzifo> ListOfIzifo = await _izifo.Find(x => x.DeletedIndicator == false);

            return new Response<IEnumerable<SOGIzifo>>(ListOfIzifo);
        }

        public async Task<Response> GetContributions(string role, string isifoId, string? MemberId = null)
        {

            Response response = await GetLoggedMember(role, MemberId);
            if (response is not Response<SOGMembers> companyResponse)
                throw new HttpResponseException(response);

            IEnumerable<SOGContribution> ListOfIzifo = await _contribution.Find(x => x.DeletedIndicator == false && x.IsifoID == ObjectId.Parse(isifoId));
            List<SOGContributedMember> contributedMembers = new();
            foreach (SOGContribution contribution in ListOfIzifo)
            {
                SOGMembers members = await _members.FindOne(x => x.Id == contribution.ContributerId);
                SOGContributedMember contributedMember = new()
                {
                    Name = members.Ibizo,
                    Surname = members.Isibongo,
                    Amount = contribution.ContributedAmount
                };
                contributedMembers.Add(contributedMember);
            }
            return new Response<IEnumerable<SOGContributedMember>>(contributedMembers);
        }


        public async Task<Response> SoftDeleteDepartment(string deletedBy, string id)
        {
            SOGDepartments exists = await _departments.FindOne(x => x.Id == ObjectId.Parse(id));
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" Department does not exists!"));

            exists.DeletedIndicator = true;
            exists.UpdatedBy = ObjectId.Parse(deletedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGDepartments memberInfo = await _departments.Update(exists.Id.ToString(), exists);
            return new Response<SOGDepartments>(memberInfo, HttpStatusCode.Accepted);
        }

        public async Task<Response> SoftDeletePosition(string updatedBy, string id)
        {
            SOGPositions exists = await _positions.FindOne(x => x.Id == ObjectId.Parse(id));
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" Position does not exists!"));

            exists.DeletedIndicator = true;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGPositions memberInfo = await _positions.Update(exists.Id.ToString(), exists);
            return new Response<SOGPositions>(memberInfo, HttpStatusCode.Accepted);
        }

        public async Task<Response> SoftDeleteMember(string updatedBy, string id)
        {
            SOGMembers exists = await _members.FindOne(x => x.Id == ObjectId.Parse(id));
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" Member does not exists!"));

            exists.DeletedIndicator = true;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGMembers memberInfo = await _members.Update(exists.Id.ToString(), exists);
            return new Response<SOGMembers>(memberInfo, HttpStatusCode.Accepted);
        }

        public async Task<Response> SoftDeleteIsifo(string updatedBy, string id)
        {
            SOGIzifo exists = await _izifo.FindOne(x => x.Id == ObjectId.Parse(id));
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" ""Lesifo asikho kuSOG"));

            exists.DeletedIndicator = true;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGIzifo izifo = await _izifo.Update(exists.Id.ToString(), exists);
            return new Response<SOGIzifo>(izifo, HttpStatusCode.Accepted);
        }

        public async Task<Response> SoftDeleteContribution(string updatedBy, string id)
        {
            SOGContribution exists = await _contribution.FindOne(x => x.Id == ObjectId.Parse(id));
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" This contribution does not exists!"));

            exists.DeletedIndicator = true;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            SOGContribution contribution = await _contribution.Update(exists.Id.ToString(), exists);
            return new Response<SOGContribution>(contribution, HttpStatusCode.Accepted);
        }






        public async Task<Response> Get(string role, string? companyId = null)
        {
            var response = await GetLoggedMember(role, companyId);
            if (response is not Response<SOGMembers> companyResponse)
                throw new HttpResponseException(response);

            var company = companyResponse.Data!;
            return new Response<SOGMembers>(company);
        }

        public async Task<Response> GetLoggedMember(string role, string? id = null)
        {
            async Task<SOGMembers> Data()
                => await _members.FindById(id);

#nullable disable
            Response response;



            switch (role)
            {
                case AuthRoles.Manager:
                    if (id is null)
                        response = new(error: "Company id is not specified!");
                    else
                        response = new Response<SOGMembers>() { Data = await Data() };
                    break;

                case AuthRoles.Owner:
                    response = new Response<SOGMembers>() { Data = await Data() };
                    break;
                default:
                    response = new Response(HttpStatusCode.Unauthorized, error: "You don't have access to this resource!");
                    break;
            }

            return response;

        }
    }
}
