using BackendServices;
using BackendServices.Models;
using BackendServices.Models.Inventory;
using BackendServices.Models.PrePurchase;
using GeographicLib;
using MongoDB.Bson;
using PrePurchase.Models;
using PrePurchase.Models.Inventory;
using PrePurchase.Models.PrePurchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Infrastructure.Unit.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace Infrastructure.Helpers
{
    internal static class ModelHelpers
    {
        public static Company Update(this Company company, CompanyUpdateModel updates)
        {
            company.Address = updates.Address ?? company.Address;
            company.CompanyName = updates.CompanyName ?? company.CompanyName;
            company.RegisterationNumber = updates.RegisterationNumber ?? company.RegisterationNumber;

            return company;
        }

        public static OrderItemDto DtoFromOrderItem(this OrderItemDto dto, OrderItem orderItem)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (orderItem == null) throw new ArgumentNullException(nameof(orderItem));

            dto.Id = orderItem.Id.ToString();
            dto.CreateDate = orderItem.CreateDate;
            dto.UpdateDate = orderItem.UpdateDate;
            dto.CreatedBy = orderItem.CreatedBy.ToString();
            dto.UpdatedBy = orderItem.UpdatedBy.ToString();
            dto.DeletedIndicator = orderItem.DeletedIndicator;
            dto.ShopId = orderItem.ShopId.ToString();
            dto.OrderID = orderItem.OrderID.ToString();
            dto.ProductID = orderItem.ProductID.ToString();
            dto.Quantity = orderItem.Quantity;
            dto.UnitPrice = orderItem.UnitPrice;

            return dto;
        }

        public static OrderItem DtoToOrderItem(this OrderItem orderItem, OrderItemDto dto)
        {
            if (orderItem == null) throw new ArgumentNullException(nameof(orderItem));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            orderItem.Id = ObjectId.Parse(dto.Id);
            orderItem.CreateDate = dto.CreateDate;
            orderItem.UpdateDate = dto.UpdateDate;
            orderItem.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            orderItem.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            orderItem.DeletedIndicator = dto.DeletedIndicator;
            orderItem.ShopId = ObjectId.Parse(dto.ShopId);
            orderItem.OrderID = ObjectId.Parse(dto.OrderID);
            orderItem.ProductID = ObjectId.Parse(dto.ProductID);
            orderItem.Quantity = dto.Quantity;
            orderItem.UnitPrice = dto.UnitPrice;

            return orderItem;
        }

        public static CategoryDto DtoFromCategory(this CategoryDto dto, Category category)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (category == null) throw new ArgumentNullException(nameof(category));

            dto.Id = category.Id.ToString();
            dto.CreateDate = category.CreateDate;
            dto.UpdateDate = category.UpdateDate;
            dto.CreatedBy = category.CreatedBy.ToString();
            dto.UpdatedBy = category.UpdatedBy.ToString();
            dto.DeletedIndicator = category.DeletedIndicator;
            dto.CategoryName = category.CategoryName;
            dto.ShopId = category.ShopId.ToString();
            dto.ParentCategoryId = category.ParentCategoryId.ToString();
            dto.SubcategoriesIds = category.SubcategoriesIds?.ConvertAll(id => id.ToString());
            dto.ProductsIds = category.ProductsIds?.ConvertAll(id => id.ToString());
            dto.Level = category.Level;

            return dto;
        }

        public static Category DtoToCategory(this Category category, CategoryDto dto)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            category.Id = ObjectId.Parse(dto.Id);
            category.CreateDate = dto.CreateDate;
            category.UpdateDate = dto.UpdateDate;
            category.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            category.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            category.DeletedIndicator = dto.DeletedIndicator;
            category.CategoryName = dto.CategoryName;
            category.ShopId = ObjectId.Parse(dto.ShopId);
            category.ParentCategoryId = ObjectId.Parse(dto.ParentCategoryId);
            category.SubcategoriesIds = dto.SubcategoriesIds?.ConvertAll(id => ObjectId.Parse(id));
            category.ProductsIds = dto.ProductsIds?.ConvertAll(id => ObjectId.Parse(id));
            category.Level = dto.Level;

            return category;
        }

        public static ShopDto DtoFromShop(this ShopDto dto, Shop shop)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (shop == null) throw new ArgumentNullException(nameof(shop));
            dto.Id = shop.Id.ToString();
            dto.CreatedBy = shop.CreatedBy.ToString();
            dto.CreatedDate = shop.CreatedDate;
            dto.UpdatedDate = shop.UpdatedDate;
            dto.UpdatedBy = shop.UpdatedBy.ToString();
            dto.DeletedIndicator = shop.DeletedIndicator;
            dto.Name = shop.Name;
            dto.RegisterationNumber = shop.RegisterationNumber;
            dto.Email = shop.Email;
            dto.ContactNumber = shop.ContactNumber;
            dto.QRCode = shop.QRCode;
            dto.Timestamp = shop.Timestamp;
            dto.LicenseExpiryDate = shop.LicenseExpiryDate;
            return dto;
        }

        public static Shop DtoToShop(this Shop shop, ShopDto dto)
        {
            if (shop == null) throw new ArgumentNullException(nameof(shop));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            shop.Id = ObjectId.Parse(dto.Id);
            shop.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            shop.CreatedDate = dto.CreatedDate;
            shop.UpdatedDate = dto.UpdatedDate;
            shop.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            shop.DeletedIndicator = dto.DeletedIndicator;
            shop.Name = dto.Name;
            shop.RegisterationNumber = dto.RegisterationNumber;
            shop.Email = dto.Email;
            shop.ContactNumber = dto.ContactNumber;
            shop.QRCode = dto.QRCode;
            shop.Timestamp = dto.Timestamp;
            shop.LicenseExpiryDate = dto.LicenseExpiryDate;
            return shop;
        }
        public static User DtoToUser(this User user, UserDto dto)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            user.Id = ObjectId.Parse(dto.Id);
            user.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            user.CreatedDate = dto.CreatedDate;
            user.UpdatedDate = dto.UpdatedDate;
            user.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            user.DeletedIndicator = dto.DeletedIndicator;
            user.Name = dto.Name;
            user.SurName = dto.SurName;
            user.UserName = dto.UserName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Email = dto.Email;
            user.Password = dto.Password;
            user.Role = dto.Role;
            foreach (var shopId in dto.ShopId)
            {
                user.ShopId.Add(ObjectId.Parse(shopId));
            }

            return user;
        }
        public static UserDto DtoFromUser(this UserDto dto, User shop)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (shop == null) throw new ArgumentNullException(nameof(shop));

            dto.Id = shop.Id.ToString();
            dto.CreatedBy = shop.CreatedBy.ToString();
            dto.CreatedDate = shop.CreatedDate;
            dto.UpdatedDate = shop.UpdatedDate;
            dto.UpdatedBy = shop.UpdatedBy.ToString();
            dto.DeletedIndicator = shop.DeletedIndicator;
            dto.Name = shop.Name;
            dto.SurName = shop.SurName;
            dto.UserName = shop.UserName;
            dto.PhoneNumber = shop.PhoneNumber;
            dto.Email = shop.Email;
            dto.Password = shop.Password;
            dto.Role = shop.Role;
            foreach (var shopId in shop.ShopId)
            {
                dto.ShopId.Add(shopId.ToString());
            }
            return dto;
        }

        public static ProductDto DtoFromProduct(this ProductDto dto, Product product)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (product == null) throw new ArgumentNullException(nameof(product));

            dto.Id = product.Id.ToString();
            dto.CreateDate = product.CreateDate;
            dto.UpdateDate = product.UpdateDate;
            dto.CreatedBy = product.CreatedBy.ToString();
            dto.UpdatedBy = product.UpdatedBy.ToString();
            dto.DeletedIndicator = product.DeletedIndicator;
            dto.ShopId = product.ShopId.ToString();
            dto.Name = product.Name;
            dto.Description = product.Description;
            dto.Price = product.Price;
            dto.Barcode = product.Barcode;
            dto.CategoryID = product.CategoryID.ToString();
            dto.SupplierID = product.SupplierID.ToString();
            dto.StockQuantity = product.StockQuantity;
            dto.ReorderLevel = product.ReorderLevel;
            dto.ReorderQuantity = product.ReorderQuantity;
            dto.BulkQuantity = product.BulkQuantity;
            dto.BulkUnit = product.BulkUnit;

            return dto;
        }

        public static Product DtoToProduct(this Product product, ProductDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (product == null) throw new ArgumentNullException(nameof(product));

            product ??= new Product();

            product.Id = ObjectId.Parse(dto.Id);
            product.CreateDate = dto.CreateDate;
            product.UpdateDate = dto.UpdateDate;
            product.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            product.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            product.DeletedIndicator = dto.DeletedIndicator;
            product.ShopId = ObjectId.Parse(dto.ShopId);
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Barcode = dto.Barcode;
            product.CategoryID = ObjectId.Parse(dto.CategoryID);
            product.SupplierID = ObjectId.Parse(dto.SupplierID);
            product.StockQuantity = dto.StockQuantity;
            product.ReorderLevel = dto.ReorderLevel;
            product.ReorderQuantity = dto.ReorderQuantity;
            product.BulkQuantity = dto.BulkQuantity;
            product.BulkUnit = dto.BulkUnit;

            return product;
        }
        public static RechargeDto DtoFromRecharge(this RechargeDto dto, Recharge recharge)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (recharge == null) throw new ArgumentNullException(nameof(recharge));

            dto.Id = recharge.Id.ToString();
            dto.CreatedDate = recharge.CreatedDate;
            dto.CreatedBy = recharge.CreatedBy.ToString();
            dto.UpdatedDate = recharge.UpdatedDate;
            dto.UpdatedBy = recharge.UpdatedBy.ToString();
            dto.DeletedIndicator = recharge.DeletedIndicator;
            dto.UserId = recharge.UserId.ToString();
            dto.Amount = recharge.Amount;
            dto.RechargeDate = recharge.RechargeDate;
            dto.Status = recharge.Status;
            dto.PaymentMethod = recharge.PaymentMethod;

            return dto;
        }

        public static Recharge DtoToRecharge(this Recharge recharge, RechargeDto dto)
        {
            if (recharge == null) throw new ArgumentNullException(nameof(recharge));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            recharge.Id = ObjectId.Parse(dto.Id);
            recharge.CreatedDate = dto.CreatedDate;
            recharge.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            recharge.UpdatedDate = dto.UpdatedDate;
            recharge.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            recharge.DeletedIndicator = dto.DeletedIndicator;
            recharge.UserId = ObjectId.Parse(dto.UserId);
            recharge.Amount = dto.Amount;
            recharge.RechargeDate = dto.RechargeDate;
            recharge.Status = dto.Status;
            recharge.PaymentMethod = dto.PaymentMethod;

            return recharge;
        }

        public static UserAccountDto DtoFromUserAccount(this UserAccountDto dto, UserAccount userAccount)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (userAccount == null) throw new ArgumentNullException(nameof(userAccount));

            dto.Id = userAccount.Id.ToString();
            dto.CreatedDate = userAccount.CreatedDate;
            dto.CreatedBy = userAccount.CreatedBy.ToString();
            dto.UpdatedDate = userAccount.UpdatedDate;
            dto.UpdatedBy = userAccount.UpdatedBy.ToString();
            dto.DeletedIndicator = userAccount.DeletedIndicator;
            dto.UserId = userAccount.UserId.ToString();
            dto.AmountBalance = userAccount.AmountBalance;
            dto.ItemsBalances = userAccount.ItemsBalances?.ConvertAll(ib => new ItemBalanceDto
            {
                Balance = ib.Balance,
                ItemId = ib.ItemId.ToString()
            });

            return dto;
        }

        public static UserAccount DtoToUserAccount(this UserAccount userAccount, UserAccountDto dto)
        {
            if (userAccount == null) throw new ArgumentNullException(nameof(userAccount));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            userAccount.Id = ObjectId.Parse(dto.Id);
            userAccount.CreatedDate = dto.CreatedDate;
            userAccount.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            userAccount.UpdatedDate = dto.UpdatedDate;
            userAccount.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            userAccount.DeletedIndicator = dto.DeletedIndicator;
            userAccount.UserId = ObjectId.Parse(dto.UserId);
            userAccount.AmountBalance = dto.AmountBalance;
            userAccount.ItemsBalances = dto.ItemsBalances?.ConvertAll(ibDto => new ItemBalance
            {
                Balance = ibDto.Balance,
                ItemId = ObjectId.Parse(ibDto.ItemId)
            });

            return userAccount;
        }

        public static PermissionDto DtoFromPermission(this PermissionDto dto, Permission permission)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (permission == null) throw new ArgumentNullException(nameof(permission));

            dto.Id = permission.Id.ToString();
            dto.CreateDate = permission.CreateDate;
            dto.UpdateDate = permission.UpdateDate;
            dto.CreatedBy = permission.CreatedBy.ToString();
            dto.UpdatedBy = permission.UpdatedBy.ToString();
            dto.DeletedIndicator = permission.DeletedIndicator;
            dto.ShopId = permission.ShopId.ToString();
            dto.PermissionName = permission.PermissionName;

            return dto;
        }

        public static Permission DtoToPermission(this Permission permission, PermissionDto dto)
        {
            if (permission == null) throw new ArgumentNullException(nameof(permission));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            permission.Id = ObjectId.Parse(dto.Id);
            permission.CreateDate = dto.CreateDate;
            permission.UpdateDate = dto.UpdateDate;
            permission.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            permission.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            permission.DeletedIndicator = dto.DeletedIndicator;
            permission.ShopId = ObjectId.Parse(dto.ShopId);
            permission.PermissionName = dto.PermissionName;

            return permission;
        }

        public static PurchaseOrderDto DtoFromPurchaseOrder(this PurchaseOrderDto dto, PurchaseOrder purchaseOrder)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (purchaseOrder == null) throw new ArgumentNullException(nameof(purchaseOrder));

            dto.Id = purchaseOrder.Id.ToString();
            dto.CreateDate = purchaseOrder.CreateDate;
            dto.UpdateDate = purchaseOrder.UpdateDate;
            dto.CreatedBy = purchaseOrder.CreatedBy.ToString();
            dto.UpdatedBy = purchaseOrder.UpdatedBy.ToString();
            dto.DeletedIndicator = purchaseOrder.DeletedIndicator;
            dto.ShopId = purchaseOrder.ShopId.ToString();
            dto.SupplierID = purchaseOrder.SupplierID.ToString();
            dto.PurchaseOrderNumber = purchaseOrder.PurchaseOrderNumber;
            dto.OrderDate = purchaseOrder.OrderDate;
            dto.DeliveryDate = purchaseOrder.DeliveryDate;
            dto.TotalCost = purchaseOrder.TotalCost;

            return dto;
        }

        public static PurchaseOrder DtoToPurchaseOrder(this PurchaseOrder purchaseOrder, PurchaseOrderDto dto)
        {
            if (purchaseOrder == null) throw new ArgumentNullException(nameof(purchaseOrder));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            purchaseOrder.Id = ObjectId.Parse(dto.Id);
            purchaseOrder.CreateDate = dto.CreateDate;
            purchaseOrder.UpdateDate = dto.UpdateDate;
            purchaseOrder.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            purchaseOrder.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            purchaseOrder.DeletedIndicator = dto.DeletedIndicator;
            purchaseOrder.ShopId = ObjectId.Parse(dto.ShopId);
            purchaseOrder.SupplierID = ObjectId.Parse(dto.SupplierID);
            purchaseOrder.PurchaseOrderNumber = dto.PurchaseOrderNumber;
            purchaseOrder.OrderDate = dto.OrderDate;
            purchaseOrder.DeliveryDate = dto.DeliveryDate;
            purchaseOrder.TotalCost = dto.TotalCost;

            return purchaseOrder;
        }

        public static RoleDto DtoFromRole(this RoleDto dto, Role role)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (role == null) throw new ArgumentNullException(nameof(role));

            dto.Id = role.Id.ToString();
            dto.ShopId = role.ShopId.ToString();
            dto.CreateDate = role.CreateDate;
            dto.UpdateDate = role.UpdateDate;
            dto.CreatedBy = role.CreatedBy.ToString();
            dto.UpdatedBy = role.UpdatedBy.ToString();
            dto.DeletedIndicator = role.DeletedIndicator;
            dto.RoleName = role.RoleName;
            dto.Description = role.Description;

            return dto;
        }

        public static Role DtoToRole(this Role role, RoleDto dto)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            role.Id = ObjectId.Parse(dto.Id);
            role.ShopId = ObjectId.Parse(dto.ShopId);
            role.CreateDate = dto.CreateDate;
            role.UpdateDate = dto.UpdateDate;
            role.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            role.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            role.DeletedIndicator = dto.DeletedIndicator;
            role.RoleName = dto.RoleName;
            role.Description = dto.Description;

            return role;
        }

        public static RolePermissionDto DtoFromRolePermission(this RolePermissionDto dto, RolePermission rolePermission)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (rolePermission == null) throw new ArgumentNullException(nameof(rolePermission));

            dto.Id = rolePermission.Id.ToString();
            dto.CreateDate = rolePermission.CreateDate;
            dto.UpdateDate = rolePermission.UpdateDate;
            dto.CreatedBy = rolePermission.CreatedBy.ToString();
            dto.UpdatedBy = rolePermission.UpdatedBy.ToString();
            dto.DeletedIndicator = rolePermission.DeletedIndicator;
            dto.RoleID = rolePermission.RoleID.ToString();
            dto.PermissionID = rolePermission.PermissionID.ToString();
            dto.ShopId = rolePermission.ShopId.ToString();

            return dto;
        }

        public static RolePermission DtoToRolePermission(this RolePermission rolePermission, RolePermissionDto dto)
        {
            if (rolePermission == null) throw new ArgumentNullException(nameof(rolePermission));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            rolePermission.Id = ObjectId.Parse(dto.Id);
            rolePermission.CreateDate = dto.CreateDate;
            rolePermission.UpdateDate = dto.UpdateDate;
            rolePermission.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            rolePermission.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            rolePermission.DeletedIndicator = dto.DeletedIndicator;
            rolePermission.RoleID = ObjectId.Parse(dto.RoleID);
            rolePermission.PermissionID = ObjectId.Parse(dto.PermissionID);
            rolePermission.ShopId = ObjectId.Parse(dto.ShopId);

            return rolePermission;
        }

        public static StockCountDto DtoFromStockCount(this StockCountDto dto, StockCount stockCount)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (stockCount == null) throw new ArgumentNullException(nameof(stockCount));

            dto.Id = stockCount.Id.ToString();
            dto.CreateDate = stockCount.CreateDate;
            dto.UpdateDate = stockCount.UpdateDate;
            dto.CreatedBy = stockCount.CreatedBy.ToString();
            dto.UpdatedBy = stockCount.UpdatedBy.ToString();
            dto.DeletedIndicator = stockCount.DeletedIndicator;
            dto.ShopId = stockCount.ShopId.ToString();
            dto.CountDate = stockCount.CountDate;
            dto.CountType = stockCount.CountType;
            dto.ProductID = stockCount.ProductID.ToString();
            dto.CountedQuantity = stockCount.CountedQuantity;

            return dto;
        }

        public static StockCount DtoToStockCount(this StockCount stockCount, StockCountDto dto)
        {
            if (stockCount == null) throw new ArgumentNullException(nameof(stockCount));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            stockCount.Id = ObjectId.Parse(dto.Id);
            stockCount.CreateDate = dto.CreateDate;
            stockCount.UpdateDate = dto.UpdateDate;
            stockCount.CreatedBy = ObjectId.Parse(dto.CreatedBy);
            stockCount.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
            stockCount.DeletedIndicator = dto.DeletedIndicator;
            stockCount.ShopId = ObjectId.Parse(dto.ShopId);
            stockCount.CountDate = dto.CountDate;
            stockCount.CountType = dto.CountType;
            stockCount.ProductID = ObjectId.Parse(dto.ProductID);
            stockCount.CountedQuantity = dto.CountedQuantity;

            return stockCount;
        }

        public static SupplierDto DtoFromSupplier(this SupplierDto dto, Supplier supplier)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (supplier == null) throw new ArgumentNullException(nameof(supplier));

            dto.Id = supplier.Id.ToString();
            dto.CreateDate = supplier.CreateDate;
            dto.UpdateDate = supplier.UpdateDate;
            dto.CreatedBy = supplier.CreatedBy.ToString();
            dto.UpdatedBy = supplier.UpdatedBy.ToString();
            dto.DeletedIndicator = supplier.DeletedIndicator;
            dto.ShopId = supplier.ShopId.ToString();
            dto.SupplierName = supplier.SupplierName;
            dto.ContactName = supplier.ContactName;
            dto.Email = supplier.Email;
            dto.Phone = supplier.Phone;
            dto.Address = supplier.Address;

            return dto;
        }

        public static Supplier DtoToSupplier(this Supplier supplier, SupplierDto dto)
        {
            if (supplier == null) throw new ArgumentNullException(nameof(supplier));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                supplier.Id = ObjectId.Parse(dto.Id);
                supplier.CreateDate = dto.CreateDate;
                supplier.UpdateDate = dto.UpdateDate;
                supplier.CreatedBy = ObjectId.Parse(dto.CreatedBy);
                supplier.UpdatedBy = ObjectId.Parse(dto.UpdatedBy);
                supplier.DeletedIndicator = dto.DeletedIndicator;
                supplier.ShopId = ObjectId.Parse(dto.ShopId);
                supplier.SupplierName = dto.SupplierName;
                supplier.ContactName = dto.ContactName;
                supplier.Email = dto.Email;
                supplier.Phone = dto.Phone;
                supplier.Address = dto.Address;
            }
            catch (FormatException ex)
            {
                // Handle invalid ObjectId format
                throw new ArgumentException("Invalid ObjectId format.", ex);
            }

            return supplier;
        }

        public static CompanyEmployee CreateCompanyEmployeeFrom(this EmployeeDetails employeeDetails, string department, string position, ObjectId companyId)
        {
            return new()
            {
                CreatedDate = employeeDetails.CreatedDate,
                CreatedBy = employeeDetails.CreatedBy,
                UpdatedDate = employeeDetails.UpdatedDate,
                UpdatedBy = employeeDetails.UpdatedBy,
                DeletedIndicator = employeeDetails.DeletedIndicator,
                Name = employeeDetails.Name,
                Surname = employeeDetails.Surname,
                NickName = employeeDetails.NickName,
                CellNumber = employeeDetails.CellNumber,
                IdNumber = employeeDetails.IDNumber,
                Email = employeeDetails.Email,
                TaxNumber = employeeDetails.TaxNumber,
                DateOfEmployment = employeeDetails.DateOfEmployment,
                EmployeeAddress = employeeDetails.EmployeeAddress,
                BankAccountInfo = employeeDetails.BankAccountInfo,
                EmployeeId = employeeDetails.EmployeeId,
                Department = department,
                CompanyId = companyId,
                Position = position
            };
        }

        public static Deduction ToDeduction(this DeductionModel model)
        {
            model = model with { TypeOfDeduction = model.TypeOfDeduction.Trim() };
            var (TypeOfDeduction, AmountToDeduct, AmountType) = model;

            return new Deduction
            {
                TypeOfDeduction = TypeOfDeduction,
                AmountToDeduct = AmountToDeduct,
                AmountType = AmountType
            };
        }

        public static Rate ToRate(this RateModel model)
        {
            model = model with { NameOfPosition = model.NameOfPosition.Trim() };

            var (NameOfPosition, StandardDaysRate, SaturdaysRate, SundaysRate, PublicHolidaysRate, DailyBonus, OverTimeRate) = model;

            return new Rate
            {
                NameOfPosition = NameOfPosition,
                StandardDaysRate = StandardDaysRate,
                SaturdaysRate = SaturdaysRate,
                SundaysRate = SundaysRate,
                PublicHolidaysRate = PublicHolidaysRate,
                DailyBonus = DailyBonus,
                OverTimeRate = OverTimeRate
            };
        }

        public static Shift ToShift(this ShiftModel model)
        {
            model = model with { Name = model.Name.Trim() };

            (string Name, TimeOnly ShiftStartTime, TimeOnly ShiftEndTime) = model;

            return new Shift
            {
                Name = Name,
                ShiftStartTime = ShiftStartTime,
                ShiftEndTime = ShiftEndTime
            };
        }

        public static Loan Update(this Loan loan, CompanyUpdateLoan model, IDateTimeProvider dateTimeProvider, string updatedby)
        {
            loan.LoanStatus = model.Status;
            loan.UpdatedBy = ObjectId.Parse(updatedby);
            if (model.Payment.HasValue)
            {
                loan.AmountPayed += model.Payment.Value;
                loan.LastPayment = model.Payment.Value;
                loan.LastPaymentDate = dateTimeProvider.Now;
            }

            return loan;
        }

        public static Leave Update(this Leave leave, QueryLeaveModel model)
        {
            leave.Status = (LeaveStatus)model.Status!;

            leave.Comment = model.Comment ?? leave.Comment;
            leave.DaysToTake = model.DaysToTake ?? leave.DaysToTake;

            return leave;
        }

        public static CompanyEmployeeProfile From(CompanyEmployee companyEmployee, IEnumerable<Company> companies)
        {
            return new()
            {
                Id = companyEmployee.Id.ToString(),
                Name = companyEmployee.Name,
                Surname = companyEmployee.Surname,
                NickName = companyEmployee.NickName,
                CellNumber = companyEmployee.CellNumber,
                EmployeeId = companyEmployee.EmployeeId,
                Department = companyEmployee.Department,
                Company = companies.First().ToCompanyProfile(),
                Position = companyEmployee.Position,
                WeekDays = companyEmployee.WeekDays,
                Roles = companyEmployee.Roles,
                Timestamp = companyEmployee.Timestamp
            };
        }

        public static Rate ToDataTransferObject(this Rate rate)
        {
            return new()
            {
                Id = rate.Id,
                NameOfPosition = rate.NameOfPosition,
                OverTimeRate = rate.OverTimeRate,
                PublicHolidaysRate = rate.PublicHolidaysRate,
                StandardDaysRate = rate.StandardDaysRate,
                SundaysRate = rate.SundaysRate,
                SaturdaysRate = rate.SaturdaysRate,
                DailyBonus = rate.DailyBonus

            };
        }

        public static Shift ToDataTransferObject(this Shift shift)
        {
            return new()
            {
                Id = shift.Id,
                ShiftStartTime = shift.ShiftStartTime,
                ShiftEndTime = shift.ShiftEndTime,
                Name = shift.Name
            };
        }

        public static ClockDataDto ToDataTransferObject(this ClockData clock)
        {
            return new()
            {
                Id = clock.Id,
                Amount = clock.Amount,
                ClockIn = clock.ClockIn,
                ClockOut = clock.ClockOut,
                OverTimeHours = clock.OverTimeHours,
                Rate = clock.Rate.ToDataTransferObject(),
                Shift = clock.Shift.ToDataTransferObject(),
                RateType = clock.RateType,
                TotalHours = clock.TotalHours,

                IsProcessed = clock.IsProcessed,
                IsAdminClocking = clock.IsAdminClocking,

                IsClockInAdjusted = clock.IsClockInAdjusted,
                IsClockOutAdjusted = clock.IsClockOutAdjusted,

                IsSickLeaveDays = clock.IsSickLeaveDays,
                IsAnnualLeaveDays = clock.IsAnnualLeaveDays,
                IsFamilyLeaveDays = clock.IsFamilyLeaveDays,

                OldClockInValue = clock.OldClockInValue,
                OldClockOutValue = clock.OldClockOutValue,

                RestTimes = clock.RestTimes,

            };
        }

        public static CompanyEmployeeDto ToDataTransferObject(this CompanyEmployee employee)
        {

            return new()
            {
                Id = employee.Id,
                Name = employee.Name,
                Surname = employee.Surname,
                NickName = employee.NickName,
                EmployeeId = employee.EmployeeId,
                Department = employee.Department,
                CompanyId = employee.CompanyId.ToString(),
                Position = employee.Position,
                Timestamp = employee.Timestamp
            };
        }

        public static TimeSummaryWithEmployeeDetails From(TimeSummary timeSummary, CompanyEmployee employees)
        {

            return new()
            {
                Id = timeSummary.Id,
                CompanyEmployee = employees.ToDataTransferObject(),
                Clocks = timeSummary.Clocks,//timeSummary.Clocks.Select(x => x.ToDataTransferObject()).ToList(),
                CompanyId = timeSummary.CompanyId,
                EndDate = timeSummary.EndDate,
                StartDate = timeSummary.StartDate
            };
        }

        public static IEnumerable<CompanyProfile> ToCompanyProfiles(this IEnumerable<Company> companies)
        {
            return companies.Select(x => x.ToCompanyProfile());
        }

        public static CompanyProfile ToCompanyProfile(this Company company)
        {
            return new()
            {
                Id = company.Id.ToString(),
                Address = company.Address,
                CompanyName = company.CompanyName,
                CellNumber = company.CellNumber,
                Email = company.Email,
                RegisterationNumber = company.RegisterationNumber
            };
        }

        public static double Distance(this Location position, Location position1)
        {
            Geodesic.WGS84.Inverse(position.Latitude, position.Longitude, position1.Latitude, position1.Longitude, out var distance);
            return distance;
        }

        public static bool WithInRadius(Location position1, Location position2, double radiusInMeters)
        {
            double distance = position1.Distance(position2);
            return distance <= radiusInMeters;
        }

        public static bool WithInRadius(Location position1, Location position2, double radiusInMeters, out double distance)
        {
            distance = position1.Distance(position2);
            return distance <= radiusInMeters;
        }

        public static List<ClockData> GetClocksInRange(this TimeSummary timeSummary, DateTime start, DateTime end)
        {
            List<ClockData> clocksInRange = timeSummary.Clocks
                .Where(clock => clock != null && clock.ClockIn >= start && clock.ClockIn <= end)
                .OrderBy(clock => clock.ClockIn)
                .ToList();

            List<ClockData> mergedClocks = new();

            if (!clocksInRange.Any()) return mergedClocks;

            ClockData currentClock = clocksInRange[0];
            foreach (var clock in clocksInRange.Skip(1))
            {
                if (clock.ClockIn <= currentClock.ClockOut)
                {
                    currentClock.ClockOut = clock.ClockOut > currentClock.ClockOut ? clock.ClockOut : currentClock.ClockOut;
                }
                else
                {
                    mergedClocks.Add(currentClock);
                    currentClock = clock;
                }
            }
            mergedClocks.Add(currentClock);

            return mergedClocks;
        }

        public static TimeSummary WithClocksInRange(this TimeSummary timeSummary, DateTime start, DateTime end)
        {
            return new()
            {
                Id = timeSummary.Id,
                EmployeeDetailsId = timeSummary.EmployeeDetailsId,
                EmployeeId = timeSummary.EmployeeId,
                CompanyId = timeSummary.CompanyId,
                StartDate = start,
                EndDate = end,
                Clocks = timeSummary.GetClocksInRange(start, end)
            };
        }

        public static Dictionary<TKey, TValue> MapUnique<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector) where TKey : notnull
        {

            Dictionary<TKey, TValue> result;

            if (values.TryGetNonEnumeratedCount(out var count))
            {
                result = new(count);
            }
            else
            {
                result = new();
            }

            foreach (var value in values)
            {
                var key = keySelector(value);

                if (result.ContainsKey(key))
                    continue;

                result[key] = value;
            }

            return result;
        }

        public static Dictionary<TKey, TOut> MapUnique<TKey, TValue, TOut>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector, Func<TKey, TValue, TOut> tranform) where TKey : notnull
        {

            Dictionary<TKey, TOut> result;

            if (values.TryGetNonEnumeratedCount(out var count))
            {
                result = new(count);
            }
            else
            {
                result = new();
            }

            foreach (var value in values)
            {
                var key = keySelector(value);

                if (result.ContainsKey(key))
                    continue;

                result[key] = tranform(key, value);
            }

            return result;
        }
    }

}
