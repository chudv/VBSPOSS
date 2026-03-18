using AutoMapper;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using VBSPOSS.Constants;
using VBSPOSS.Controllers;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Services.Implements
{
    public class AdministrationService : IAdministrationService
    {
        /// <summary>
        /// Defines the _dbContext.
        /// </summary>
        private readonly ApplicationDbContext _dbContext;

        private readonly IMapper _mapper;

        private readonly ILogger<AdministrationService> _logger;

        private readonly UserManager<IdentityUser> _userManager;


        public AdministrationService(ApplicationDbContext dbContext, IMapper mapper, ILogger<AdministrationService> logger, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleCode"></param>
        /// <returns></returns>
        public List<MenuRoleView> GetMenuByRole(string roleCode)
        {
            var menus = new List<MenuRoleView>();

            if (!string.IsNullOrEmpty(roleCode))
            {
                List<MenuRoleView> lstData = _dbContext.MenuRoleViews.Where(w => w.RoleCode == roleCode).OrderBy(o => o.GroupOrder).ToList();

                foreach (MenuRoleView menu in lstData)
                {
                    if (menu.Status == StatusValue.ACTIVE.Value && menu.Permission == StatusValue.ACTIVE.Value)
                    {
                        MenuRoleView menuView = new MenuRoleView();
                        menuView.Id = menu.Id;
                        menuView.Text = menu.Text;
                        menuView.Controller = menu.Controller;
                        menuView.Action = menu.Action;
                        menuView.Activeli = menu.Activeli;
                        menuView.Area = menu.Area;
                        menuView.Status = menu.Status;
                        menuView.HasChild = menu.HasChild;
                        menuView.IsParent = menu.IsParent;
                        menuView.IsSelect = menu.IsSelect;
                        menuView.ParentId = menu.ParentId;
                        menuView.ImageClass = menu.ImageClass;

                        menus.Add(menuView);
                    }

                }
            }
            return menus.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="menuId"></param>
        /// <returns></returns>
        //public int GetPermitOrUserGroup(string groupCode, string actionName, string controllerName, int menuId)
        //{
        //    var menu = _dbContext.MenuViews.Where(w => w.GroupCode == groupCode
        //    && w.Id == menuId && w.Controller == controllerName && w.Action == actionName).FirstOrDefault();
        //    if (menu == null)
        //    {
        //        return PermitValue.NOT_ALLOW;
        //    }
        //    else
        //    {
        //        return menu.Permit;
        //    }
        //}




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<MenuRoleView> GetMenuRoleViews()
        {
            var lstMenuRoles = _dbContext.MenuRoleViews.ToList();
            return lstMenuRoles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public MenuRoleView GetMenuRoleById(int roleId)
        {
            var menuRoleView = _dbContext.MenuRoleViews.Where(w => w.Id == roleId).FirstOrDefault();
            return menuRoleView;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuId"></param>
        /// <param name="groupUserCode"></param>
        /// <returns></returns>
        public MenuRoleView GetMenuRoleById(int menuId, string groupUserCode)
        {
            var menuRoleView = _dbContext.MenuRoleViews.Where(w => w.Id == menuId && w.RoleCode == groupUserCode).
                OrderBy(o => o.RoleCode).ThenByDescending(o => o.Status).FirstOrDefault();
            return menuRoleView;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grantType"></param>
        /// <param name="grade"></param>
        /// <returns></returns>
        public List<Role> GetRoles(int grantType, int grade)
        {
            List<Role> lstUserGroups;
            if (grantType == 0)
            {
                lstUserGroups = _dbContext.Roles.ToList();
            }
            else
            {
                lstUserGroups = _dbContext.Roles.Where(w => w.Grade <= grade && w.GrantType == grantType).ToList();
            }

            return lstUserGroups;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Menu> GetMenus()
        {
            var lstMenus = _dbContext.Menus.ToList();
            return lstMenus;
        }

        public List<PermissionModel> GetPermissions()
        {
            var lstPermission = _dbContext.Permissions.ToList();
            List<PermissionModel> lstPermissionModel = new List<PermissionModel>();
            for (int i = 0; i < lstPermission.Count; i++)
            {
                var _permissionModel = _mapper.Map<PermissionModel>(lstPermission[i]);
                _permissionModel.StatusDesc = _permissionModel.Status == StatusValue.ACTIVE.Value ? StatusValue.ACTIVE.Description : StatusValue.CLOSED.Description;
                _permissionModel.StatusFlag = _permissionModel.Status == StatusValue.ACTIVE.Value ? true : false;
                lstPermissionModel.Add(_permissionModel);
            }
            return lstPermissionModel;
        }

        public List<MenuRoleView> GetMenusForAddNew(string roleCode)
        {
            var lstExistsMenuId = _dbContext.MenuRoleViews.Where(w => w.RoleCode == roleCode).Select(s => s.Id).ToList();
            var lstAddMenus = _dbContext.Menus.Where(w => !lstExistsMenuId.Contains(w.Id)).ToList();
            List<MenuRoleView> result = new List<MenuRoleView>();
            foreach (Menu menu in lstAddMenus)
            {
                var addMenu = _mapper.Map<Menu, MenuRoleView>(menu);
                result.Add(addMenu);
            }
            return result;
        }

        /// <summary>
        /// The UpdateMenuRole.
        /// </summary>
        /// <param name="menuRole">The menuRole<see cref="MenuRole"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int UpdateMenuRole(MenuRole menuRole)
        {
            var updateMenuRole = _dbContext.MenuRoles.Where(w => w.Id == menuRole.Id).FirstOrDefault();
            if (updateMenuRole != null)
            {
                //updateMenuRole.Permit = menuRole.Permit;
                _dbContext.MenuRoles.Update(updateMenuRole);
                var result = _dbContext.SaveChanges();
                return result;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// The CreateMenuRole.
        /// </summary>
        /// <param name="menuRole">The menuRole<see cref="MenuRole"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CreateMenuRole(MenuRole menuRole)
        {
            var addMenuRole = _dbContext.MenuRoles.Where(w => w.RoleCode == menuRole.RoleCode && w.MenuId == menuRole.MenuId).FirstOrDefault();
            if (addMenuRole != null)
            {
                //addMenuRole.Permit = menuRole.Permit;
                var result = _dbContext.MenuRoles.Update(addMenuRole);
                _dbContext.SaveChanges();
                return result.Entity.Id;
            }
            else
            {
                var result = _dbContext.MenuRoles.Add(menuRole);
                _dbContext.SaveChanges();
                return result.Entity.Id;
            }
        }

        /// <summary>
        /// Hàm thực hiện cập nhật dữ liệu phân quyền cho nhóm người dùng. Bảng dữ liệu MenuRoles
        /// </summary>
        /// <param name="model">Model MenuRoles</param>
        /// <param name="pUserName">Người dùng thực hiện</param>
        /// <returns>Id bản ghi bảng cập nhật MenuRoles</returns>
        public int UpdateMenuRole(MenuRole model, string pUserName)
        {
            int iSaveChanges = 0, iResultId = 0;
            try
            {
                DateTime currentDateVal = DateTime.Now;
                var objMenuRole = _dbContext.MenuRoles.Where(w => w.RoleCode == model.RoleCode && w.MenuId == model.MenuId)
                                            .OrderBy(o => o.RoleCode).ThenByDescending(o => o.Status).FirstOrDefault();
                if (objMenuRole != null && objMenuRole.Id != 0)
                {
                    //objMenuRole.Permit = model.Permit;
                    objMenuRole.Status = model.Status;
                    //_dbContext.Entry(objMenuRole).Property(x => x.Permit).IsModified = true;
                    _dbContext.Entry(objMenuRole).Property(x => x.Status).IsModified = true;
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = objMenuRole.Id;
                }
                else
                {
                    MenuRole objModelMenuRole = new MenuRole();
                    objModelMenuRole.Id = model.Id;
                    objModelMenuRole.RoleCode = model.RoleCode;
                    objModelMenuRole.MenuId = model.MenuId;
                    //objModelMenuRole.Permit = model.Permit;
                    objModelMenuRole.Status = model.Status;
                    _dbContext.MenuRoles.Add(objModelMenuRole);
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = objModelMenuRole.Id;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iResultId;
        }

        /// <summary>
        /// The DeleteMenuRole.
        /// </summary>
        /// <param name="menuRole">The menuRole<see cref="MenuRole"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int DeleteMenuRole(MenuRole menuRole)
        {
            var deleteMenuRole = _dbContext.MenuRoles.Where(w => w.Id == menuRole.Id).FirstOrDefault();
            if (deleteMenuRole != null)
            {
                _dbContext.MenuRoles.Remove(deleteMenuRole);
                var result = _dbContext.SaveChanges();
                return result;
            }
            else
            {
                return 0;
            }
        }

        public int DeleteMenuRole(int pMenuId, string pGroupUser)
        {
            var deleteMenuRole = _dbContext.MenuRoles.Where(w => w.MenuId == pMenuId && w.RoleCode == pGroupUser)
                                            .OrderBy(o => o.RoleCode).ThenByDescending(o => o.Status).FirstOrDefault();
            if (deleteMenuRole != null)
            {
                _dbContext.MenuRoles.Remove(deleteMenuRole);
                var result = _dbContext.SaveChanges();
                return result;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// The CreateMenu.
        /// </summary>
        /// <param name="entry">The entry<see cref="Menu"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CreateMenu(Menu entry)
        {
            var addedEntry = _dbContext.Menus.Add(entry);
            _dbContext.SaveChanges();
            return addedEntry.Entity.Id;
        }

        /// <summary>
        /// The UpdateMenu.
        /// </summary>
        /// <param name="entry">The entry<see cref="Menu"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int UpdateMenu(Menu entry)
        {
            int result = 0;
            var dbEntry = _dbContext.Menus.Where(w => w.Id == entry.Id).FirstOrDefault();
            if (dbEntry != null)
            {
                dbEntry.Action = entry.Action;
                dbEntry.ActiveLi = entry.ActiveLi;
                dbEntry.Area = entry.Area;
                dbEntry.Controller = entry.Controller;
                dbEntry.GroupOrder = entry.GroupOrder;
                dbEntry.HasChild = entry.HasChild;
                dbEntry.ParentId = entry.ParentId;
                dbEntry.Status = entry.Status;
                dbEntry.IsSelect = entry.IsSelect;
                dbEntry.Text = entry.Text;
                dbEntry.ImageClass = entry.ImageClass;
                dbEntry.IsParent = entry.IsParent;
                dbEntry.ModifiedBy = "system";
                dbEntry.ModifiedDate = DateTime.Now;
                _dbContext.Menus.Update(dbEntry);
                result = _dbContext.SaveChanges();
            }
            return result;
        }

        /// <summary>
        /// The DeleteMenu.
        /// </summary>
        /// <param name="entry">The entry<see cref="Menu"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int DeleteMenu(Menu entry)
        {
            int result = 0;
            var dbEntry = _dbContext.Menus.Where(w => w.Id == entry.Id).FirstOrDefault();
            if (dbEntry != null)
            {
                _dbContext.Menus.Remove(dbEntry);
                result = _dbContext.SaveChanges();
            }
            return result;
        }

        /// <summary>
        /// The CreateUserGroup.
        /// </summary>
        /// <param name="entry">The entry<see cref="Role"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CreateUserGroup(Role entry)
        {
            var addedEntry = _dbContext.Roles.Add(entry);
            _dbContext.SaveChanges();
            return addedEntry.Entity.Id;
        }

        /// <summary>
        /// The UpdateUserGroup.
        /// </summary>
        /// <param name="entry">The entry<see cref="Role"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int UpdateUserGroup(Role entry)
        {
            int result = 0;
            var dbEntry = _dbContext.Roles.Where(w => w.Id == entry.Id).FirstOrDefault();
            if (dbEntry != null)
            {
                dbEntry.RoleCode = entry.RoleCode;
                dbEntry.RoleName = entry.RoleName;
                dbEntry.Grade = entry.Grade;
                dbEntry.Status = entry.Status;
                dbEntry.GrantType = entry.GrantType;
                //dbEntry.UserRole = entry.UserRole;
                _dbContext.Roles.Update(dbEntry);
                // cap nhat lai bang MenuRoles
                result = _dbContext.SaveChanges();
            }
            return result;
        }

        /// <summary>
        /// The DeleteUserGroup.
        /// </summary>
        /// <param name="entry">The entry<see cref="Role"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int DeleteUserGroup(Role entry)
        {
            int result = 0;
            var dbEntry = _dbContext.Roles.Where(w => w.Id == entry.Id).FirstOrDefault();
            if (dbEntry != null)
            {
                _dbContext.Roles.Remove(dbEntry);
                result = _dbContext.SaveChanges();
            }
            return result;
        }

        public List<Role> GetUserRoles(string userName)
        {
            //var _user = _dbContext.Users.Where(w => w.UserName == userName).FirstOrDefault();
            //if (_user != null)
            //{
            //    var _userGroup = _dbContext.Roles.Where(w => w.RoleCode == _user.GroupCode).FirstOrDefault();
            //    return _userGroup;
            //}
            //else
            //{
            //    return null;
            //}
            return null;

        }


        /// <summary>
        /// The CreateMenu.
        /// </summary>
        /// <param name="entry">The entry<see cref="Menu"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CreatePermission(Permission entry)
        {
            var addedEntry = _dbContext.Permissions.Add(entry);
            _dbContext.SaveChanges();
            return addedEntry.Entity.Id;
        }

        /// <summary>
        /// The UpdateMenu.
        /// </summary>
        /// <param name="entry">The entry<see cref="Menu"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int UpdatePermission(Permission entry)
        {
            int result = 0;
            var _dbEntry = _dbContext.Permissions.Where(w => w.Id == entry.Id).FirstOrDefault();
            if (_dbEntry != null)
            {
                _dbEntry.Code = entry.Code;
                _dbEntry.Description = entry.Description;
                _dbEntry.Status = entry.Status;
                _dbContext.Permissions.Update(_dbEntry);
                result = _dbContext.SaveChanges();
            }
            return result;
        }

        // StaffView


        /// <summary>
        /// The DeleteMenu.
        /// </summary>
        /// <param name="entry">The entry<see cref="Menu"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int DeletePermission(Permission entry)
        {
            int result = 0;
            var _dbEntry = _dbContext.Permissions.Where(w => w.Id == entry.Id).FirstOrDefault();
            if (_dbEntry != null)
            {
                _dbContext.Permissions.Remove(_dbEntry);
                result = _dbContext.SaveChanges();
            }
            return result;
        }

        ///// <summary>
        ///// The GetUserInfor.
        ///// </summary>
        ///// <param name="posCode">The posCode<see cref="string"/>.</param>
        ///// <param name="department">The department<see cref="string"/>.</param>
        ///// <param name="position">The position<see cref="string"/>.</param>
        ///// <param name="birthDayFromDate">The birthDayFromDate<see cref="string"/>.</param>
        ///// <param name="birthDayToDate">The birthDayToDate<see cref="string"/>.</param>
        ///// <param name="fullName">The fullName<see cref="string"/>.</param>
        ///// <param name="sex">The sex<see cref="string"/>.</param>
        ///// <param name="userName">The userName<see cref="string"/>.</param>
        ///// <param name="userGroup">The userGroup<see cref="string"/>.</param>
        ///// <returns>The <see cref="List{UserInforView}"/>.</returns>
        //public List<UserInforView> GetUserInfor(string posCode, string department, string position, DateTime birthDayFromDate, DateTime birthDayToDate, string fullName, string sex, string userName, string userGroup)
        //{
        //    return _dbContext.UserInforViews.Where(w =>
        //    (string.IsNullOrEmpty(posCode) || posCode == "000100" || w.Organization == posCode)
        //    && (string.IsNullOrEmpty(department) || w.Department == department)
        //    && (string.IsNullOrEmpty(position) || w.Position == position)
        //    && (string.IsNullOrEmpty(fullName) || w.FullName.Contains(fullName))
        //    && (string.IsNullOrEmpty(sex) || w.Sex == sex)
        //    && (string.IsNullOrEmpty(userName) || w.UserName == userName)
        //    && (string.IsNullOrEmpty(userGroup) || w.UserGroup == userGroup)
        //    && (birthDayFromDate == null || w.Birthday >= birthDayFromDate)
        //    && (birthDayToDate == null || w.Birthday <= birthDayToDate)
        //    ).ToList();
        //}

        ///// <summary>
        ///// The GetUserById.
        ///// </summary>
        ///// <param name="userId">The userId<see cref="int"/>.</param>
        ///// <returns>The <see cref="UserInforViewModel"/>.</returns>
        //public UserInforViewModel GetUserById(string userId)
        //{
        //    UserInforView userInfor = _dbContext.UserInforViews.Where(w => w.UserId == userId).FirstOrDefault();
        //    return _mapper.Map<UserInforView, UserInforViewModel>(userInfor);
        //}

        //public UserInforViewModel GetUserByUserName(string userName)
        //{
        //    UserInforView userInfor = _dbContext.UserInforViews.Where(w => w.UserName == userName).FirstOrDefault();
        //    return _mapper.Map<UserInforView, UserInforViewModel>(userInfor);
        //}

        ///// <summary>
        ///// The GetStaffList.
        ///// </summary>
        ///// <param name="posCode">The posCode<see cref="string"/>.</param>
        ///// <returns>The <see cref="List{StaffView}"/>.</returns>
        //public List<StaffView> GetStaffList(string posCode)
        //{
        //    return _dbContext.StaffViews.Where(w => w.MaDonVi == posCode).ToList();
        //}

        ///// <summary>
        ///// The GetStaffById.
        ///// </summary>
        ///// <param name="staffId">The staffId<see cref="string"/>.</param>
        ///// <returns>The <see cref="StaffView"/>.</returns>
        //public StaffView GetStaffById(string staffId)
        //{
        //    return _dbContext.StaffViews.Where(w => w.IdCanBo == staffId).FirstOrDefault();
        //}

        ///// <summary>
        ///// The CreateUser.
        ///// </summary>
        ///// <param name="userInfor">The userInfor<see cref="UserInforViewModel"/>.</param>
        ///// <param name="createdBy">The createdBy<see cref="string"/>.</param>
        ///// <returns>The <see cref="int"/>.</returns>
        //public async Task<int> CreateUser(UserInforViewModel userInfor, string createdBy)
        //{

        //    bool postClaimStatus = true;
        //    bool postRoleStatus = false;
        //    bool postUserStatus = false;

        //    if (userInfor.SSOStatus != StatusValues._ACTIVE)
        //    {
        //        // Khoi tao user tren SSO
        //        User addUser = new User();

        //        addUser.userName = userInfor.UserName;
        //        addUser.email = userInfor.Email;
        //        addUser.displayName = userInfor.FullName;
        //        addUser.phoneNumber = userInfor.Mobile;
        //        //addUser.lockoutEnd = new DateTime(2050, 1, 1);
        //        //addUser.lockoutEnabled = true;
        //        addUser.lockoutEnabled = false;
        //        addUser.userType = Constants.UserTypes.STAFF;

        //        var result = await _apiAdminServices.PostUsersAsync(addUser);
        //        postUserStatus = result != null;

        //        // set password
        //        var changePasswordResult = await _apiAdminServices.PostChangePasswordByUserId(result.id, userInfor.Password);
        //        postUserStatus = changePasswordResult;
        //    }
        //    else
        //    {
        //        postUserStatus = true;
        //    }


        //    // Check Claim va Role tren SSO
        //    var user = await _apiAdminServices.GetUsersAsync(userInfor.UserName);
        //    string userId = user.First().id;
        //    List<UserClaims> userClaims = await _apiAdminServices.GetClaimsByUserIdAsync(userId);
        //    List<ClaimDictionary> lstClaimDict = _dbContext.ClaimDictionaries.ToList();
        //    var userGroup = _dbContext.UserGroups.Where(w => w.GroupCode == userInfor.UserGroup).FirstOrDefault();

        //    // check claim
        //    List<UserClaims> addClaims = new List<UserClaims>();
        //    foreach (ClaimDictionary claim in lstClaimDict)
        //    {
        //        var isExists = userClaims.Where(w => w.claimType == claim.ClaimType).FirstOrDefault() != null;
        //        if (!isExists)
        //        {
        //            // add claim here
        //            if (claim.ClaimType.Equals("user_type"))
        //                addClaims.Add(new UserClaims
        //                {
        //                    userId = userId,
        //                    claimType = claim.ClaimType,
        //                    claimValue = UserTypes.STAFF
        //                });
        //            else if (claim.ClaimType.Equals("pos_code"))
        //                addClaims.Add(new UserClaims
        //                {
        //                    userId = userId,
        //                    claimType = claim.ClaimType,
        //                    claimValue = userInfor.Organization
        //                });
        //            else if (claim.ClaimType.Equals("phone_number"))
        //                addClaims.Add(new UserClaims
        //                {
        //                    userId = userId,
        //                    claimType = claim.ClaimType,
        //                    claimValue = userInfor.Mobile
        //                });
        //            else if (claim.ClaimType.Equals("family_name"))
        //                addClaims.Add(new UserClaims
        //                {
        //                    userId = userId,
        //                    claimType = claim.ClaimType,
        //                    claimValue = userInfor.FullName
        //                });
        //            else if (claim.ClaimType.Equals("user_grade"))
        //                addClaims.Add(new UserClaims
        //                {
        //                    userId = userId,
        //                    claimType = claim.ClaimType,
        //                    claimValue = userGroup.Grade.ToString()
        //                });
        //            else if (claim.ClaimType.Equals("user_group"))
        //                addClaims.Add(new UserClaims
        //                {
        //                    userId = userId,
        //                    claimType = claim.ClaimType,
        //                    claimValue = userGroup.GroupCode
        //                });
        //        }
        //    }
        //    if (addClaims != null && addClaims.Count > 0)
        //    {
        //        foreach (UserClaims userClaimTmp in addClaims)
        //        {
        //            var postClaimStatusTemp = await _apiAdminServices.PostClaimByUserId(userClaimTmp);
        //            postClaimStatus = postClaimStatus && postClaimStatusTemp;
        //        }
        //    }


        //    // check role
        //    List<Role> userRoles = await _apiAdminServices.GetRolesByUserIdAsync(userId);
        //    bool isRoleExists = false;
        //    foreach (Role userRole in userRoles)
        //    {
        //        if (userRole.name == userGroup.UserRole)
        //        {
        //            isRoleExists = true;
        //            break;
        //        }
        //    }
        //    if (!isRoleExists)
        //    {
        //        List<Role> lstRoleDict = await _apiAdminServices.GetRolesAsync();
        //        Role ssoRole = lstRoleDict.Where(w => w.name == userGroup.UserRole).FirstOrDefault();

        //        if (ssoRole != null)
        //        {
        //            UserRoles addRole = new UserRoles();
        //            addRole.userId = userId;
        //            addRole.roleId = ssoRole.id;
        //            postRoleStatus = await _apiAdminServices.PostRoleByUserId(addRole);
        //        }
        //        else
        //        {
        //            postRoleStatus = false;
        //        }

        //    }
        //    else
        //    {
        //        postRoleStatus = true;
        //    }

        //    var status = 0;

        //    if (postUserStatus && postClaimStatus && postRoleStatus)
        //    {
        //        var existsUser = _dbContext.UserInfors.Where(w => w.UserName == userInfor.UserName).FirstOrDefault();
        //        if (existsUser != null)
        //        {
        //            existsUser.FKDocumentId = userInfor.FKDocumentId;
        //            existsUser.UserGroup = userInfor.UserGroup;
        //            existsUser.Status = StatusValues._ACTIVE;
        //            existsUser.ModifiedBy = createdBy;
        //            existsUser.ModifiedDate = DateTime.Now;
        //            existsUser.UserId = userId;
        //            var result = _dbContext.UserInfors.Update(existsUser);
        //            _dbContext.SaveChanges();
        //            status = result.Entity.Id;
        //        }
        //        else
        //        {
        //            UserInfor newUserInfor = new UserInfor();
        //            newUserInfor.UserName = userInfor.UserName;
        //            newUserInfor.UserGroup = userInfor.UserGroup;
        //            newUserInfor.Status = StatusValues._ACTIVE;
        //            newUserInfor.FKDocumentId = userInfor.FKDocumentId;
        //            newUserInfor.CreatedBy = createdBy;
        //            newUserInfor.CreatedDate = DateTime.Now;
        //            newUserInfor.UserId = userId;
        //            var result = _dbContext.UserInfors.Add(newUserInfor);
        //            _dbContext.SaveChanges();
        //            status = result.Entity.Id;
        //        }
        //    }

        //    return status;
        //}
        //public List<UserType> GetUserTypeList()
        //{

        //    var lstUserTypes = _dbContext.UserTypes.ToList();
        //    return lstUserTypes;

        //}

        //public List<UserRole> GetUserRoleList()
        //{
        //    try
        //    {
        //        var lstRoles = _dbContext.UserRoles.ToList();
        //        return lstRoles;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public bool CheckRoleForUser(string userId, string roleCode)
        //{
        //    var userInfor = _dbContext.UserInfors.Where(w => w.UserId == userId).FirstOrDefault();
        //    if (userInfor == null)
        //        return false;
        //    var userGroup = _dbContext.UserGroups.Where(w => w.GroupCode == userInfor.UserGroup).FirstOrDefault();
        //    if (userGroup == null)
        //        return false;
        //    return roleCode == userGroup.UserRole;
        //}

        public List<MenuRolesViewModel> GetMenuRoles(string roleCode, string roleName, int menuId, string menuText, int flagCall)
        {
            var answer = new List<MenuRolesViewModel>();
            try
            {
                var _lstMenuRole = _dbContext.MenuRoleViews.OrderBy(o => o.RoleCode).ThenBy(o => o.GroupOrder).ToList();
                for (int i = 0; i < _lstMenuRole.Count; i++)
                {
                    var _menuRolesViewModelItem = _mapper.Map<MenuRolesViewModel>(_lstMenuRole[i]);
                    _menuRolesViewModelItem.Order = i + 1;
                    _menuRolesViewModelItem.ViewPermissionDesc = _menuRolesViewModelItem.ViewPermissionFlag == PermissionStatus.ALLOW ? "X" : string.Empty;
                    _menuRolesViewModelItem.CreatePermissionDesc = _menuRolesViewModelItem.CreatePermissionFlag == PermissionStatus.ALLOW ? "X" : string.Empty;
                    _menuRolesViewModelItem.EditPermissionDesc = _menuRolesViewModelItem.EditPermissionFlag == PermissionStatus.ALLOW ? "X" : string.Empty;
                    _menuRolesViewModelItem.DeletePermissionDesc = _menuRolesViewModelItem.DeletePermissionFlag == PermissionStatus.ALLOW ? "X" : string.Empty;
                    _menuRolesViewModelItem.AuthorizationPermissionDesc = _menuRolesViewModelItem.AuthorizationPermissionFlag == PermissionStatus.ALLOW ? "X" : string.Empty;
                    _menuRolesViewModelItem.ReportPermissionDesc = _menuRolesViewModelItem.ReportPermissionFlag == PermissionStatus.ALLOW ? "X" : string.Empty;
                    //_menuRolesViewModelItem.CurrentPermissionDesc = "Xem, Xóa, Tạo tờ trình";
                    _menuRolesViewModelItem.StatusDesc = _menuRolesViewModelItem.Status == StatusValue.ACTIVE.Value ? StatusValue.ACTIVE.Description : StatusValue.CLOSED.Description;
                    if (!string.IsNullOrEmpty(roleCode) && _lstMenuRole[i].RoleCode == roleCode && menuId != 0 && _lstMenuRole[i].Id == menuId)
                    {
                        answer.Add(_menuRolesViewModelItem);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(roleCode) && _lstMenuRole[i].RoleCode == roleCode && menuId == 0)
                        {
                            answer.Add(_menuRolesViewModelItem);
                        }
                        else if (string.IsNullOrEmpty(roleCode) && menuId == 0)
                        {
                            answer.Add(_menuRolesViewModelItem);
                        }
                    }
                }
                return answer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int BatchUpdateRole(BatchAuthorizationModel batchUpdateData)
        {
            //-- Cập nhật vào bảng MenuRole và bảng RolePermission
            var _roleCode = batchUpdateData.RoleCode;
            var _lstMenuRoleModel = batchUpdateData.MenuRolesList;

            List<MenuRole> _lstAddMenuRole = new List<MenuRole>();
            List<MenuRole> _lstUpdateMenuRole = new List<MenuRole>();

            List<RolePermission> _lstAddRolePermission = new List<RolePermission>();
            List<RolePermission> _lstUpdateRolePermission = new List<RolePermission>();


            if (_lstMenuRoleModel.Count > 0)
            {

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {

                        for (int i = 0; i < _lstMenuRoleModel.Count; i++)
                        {
                            // Cập nhật bảng menuRole
                            var _menuRole = _dbContext.MenuRoles.Where(w => w.RoleCode == _roleCode && w.MenuId == _lstMenuRoleModel[i].Id).FirstOrDefault();
                            if (_menuRole != null)
                            {
                                _menuRole.Status = _lstMenuRoleModel[i].ViewPermissionFlag;
                                _lstUpdateMenuRole.Add(_menuRole);

                            }
                            else
                            {
                                _menuRole = new MenuRole();
                                _menuRole.RoleCode = _roleCode;
                                _menuRole.MenuId = _lstMenuRoleModel[i].Id;
                                _menuRole.Status = _lstMenuRoleModel[i].ViewPermissionFlag;
                                _menuRole.CreatedBy = "system";
                                _menuRole.CreatedDate = DateTime.Now;
                                _lstAddMenuRole.Add(_menuRole);
                            }

                            // Cập nhật bảng RolePermission
                            var _viewRolePermission = _dbContext.RolePermissions.Where(w => w.RoleCode == _roleCode && w.PermissionCode == PermissionValue.VIEW
                            && w.MenuId == _lstMenuRoleModel[i].Id).FirstOrDefault();

                            if (_viewRolePermission != null)
                            {
                                _viewRolePermission.Status = _lstMenuRoleModel[i].ViewPermissionFlag;
                                _lstUpdateRolePermission.Add(_viewRolePermission);
                            }
                            else
                            {
                                _viewRolePermission = new RolePermission();
                                _viewRolePermission.RoleCode = _roleCode;
                                _viewRolePermission.PermissionCode = PermissionValue.VIEW;
                                _viewRolePermission.MenuId = _lstMenuRoleModel[i].Id;
                                _viewRolePermission.Status = _lstMenuRoleModel[i].ViewPermissionFlag;
                                _lstAddRolePermission.Add(_viewRolePermission);
                            }

                            var _createRolePermission = _dbContext.RolePermissions.Where(w => w.RoleCode == _roleCode && w.PermissionCode == PermissionValue.CREATE
                            && w.MenuId == _lstMenuRoleModel[i].Id).FirstOrDefault();

                            if (_createRolePermission != null)
                            {
                                _createRolePermission.Status = _lstMenuRoleModel[i].CreatePermissionFlag;
                                _lstUpdateRolePermission.Add(_createRolePermission);
                            }
                            else
                            {
                                _createRolePermission = new RolePermission();
                                _createRolePermission.RoleCode = _roleCode;
                                _createRolePermission.PermissionCode = PermissionValue.CREATE;
                                _createRolePermission.MenuId = _lstMenuRoleModel[i].Id;
                                _createRolePermission.Status = _lstMenuRoleModel[i].CreatePermissionFlag;
                                _lstAddRolePermission.Add(_createRolePermission);
                            }

                            var _updateRolePermission = _dbContext.RolePermissions.Where(w => w.RoleCode == _roleCode && w.PermissionCode == PermissionValue.UPDATE
                            && w.MenuId == _lstMenuRoleModel[i].Id).FirstOrDefault();

                            if (_updateRolePermission != null)
                            {
                                _updateRolePermission.Status = _lstMenuRoleModel[i].EditPermissionFlag;
                                _lstUpdateRolePermission.Add(_updateRolePermission);
                            }
                            else
                            {
                                _updateRolePermission = new RolePermission();
                                _updateRolePermission.RoleCode = _roleCode;
                                _updateRolePermission.PermissionCode = PermissionValue.UPDATE;
                                _updateRolePermission.MenuId = _lstMenuRoleModel[i].Id;
                                _updateRolePermission.Status = _lstMenuRoleModel[i].EditPermissionFlag;
                                _lstAddRolePermission.Add(_updateRolePermission);
                            }

                            var _deleteRolePermission = _dbContext.RolePermissions.Where(w => w.RoleCode == _roleCode && w.PermissionCode == PermissionValue.DELETE
                            && w.MenuId == _lstMenuRoleModel[i].Id).FirstOrDefault();

                            if (_deleteRolePermission != null)
                            {
                                _deleteRolePermission.Status = _lstMenuRoleModel[i].DeletePermissionFlag;
                                _lstUpdateRolePermission.Add(_deleteRolePermission);
                            }
                            else
                            {
                                _deleteRolePermission = new RolePermission();
                                _deleteRolePermission.RoleCode = _roleCode;
                                _deleteRolePermission.PermissionCode = PermissionValue.DELETE;
                                _deleteRolePermission.MenuId = _lstMenuRoleModel[i].Id;
                                _deleteRolePermission.Status = _lstMenuRoleModel[i].DeletePermissionFlag;
                                _lstAddRolePermission.Add(_deleteRolePermission);
                            }

                            var _authRolePermission = _dbContext.RolePermissions.Where(w => w.RoleCode == _roleCode && w.PermissionCode == PermissionValue.AUTHORIZATION
                            && w.MenuId == _lstMenuRoleModel[i].Id).FirstOrDefault();


                            if (_authRolePermission != null)
                            {
                                _authRolePermission.Status = _lstMenuRoleModel[i].AuthorizationPermissionFlag;
                                _lstUpdateRolePermission.Add(_authRolePermission);
                            }
                            else
                            {
                                _authRolePermission = new RolePermission();
                                _authRolePermission.RoleCode = _roleCode;
                                _authRolePermission.PermissionCode = PermissionValue.AUTHORIZATION;
                                _authRolePermission.MenuId = _lstMenuRoleModel[i].Id;
                                _authRolePermission.Status = _lstMenuRoleModel[i].AuthorizationPermissionFlag;
                                _lstAddRolePermission.Add(_authRolePermission);
                            }

                            var _reportRolePermission = _dbContext.RolePermissions.Where(w => w.RoleCode == _roleCode && w.PermissionCode == PermissionValue.REPORT
                            && w.MenuId == _lstMenuRoleModel[i].Id).FirstOrDefault();

                            if (_reportRolePermission != null)
                            {
                                _reportRolePermission.Status = _lstMenuRoleModel[i].ReportPermissionFlag;
                                _lstUpdateRolePermission.Add(_reportRolePermission);
                            }
                            else
                            {
                                _reportRolePermission = new RolePermission();
                                _reportRolePermission.RoleCode = _roleCode;
                                _reportRolePermission.PermissionCode = PermissionValue.REPORT;
                                _reportRolePermission.MenuId = _lstMenuRoleModel[i].Id;
                                _reportRolePermission.Status = _lstMenuRoleModel[i].ReportPermissionFlag;
                                _lstAddRolePermission.Add(_reportRolePermission);
                            }

                        }
                        if (_lstUpdateMenuRole.Count > 0)
                        {
                            _dbContext.MenuRoles.UpdateRange(_lstUpdateMenuRole);
                        }

                        if (_lstAddMenuRole.Count > 0)
                        {
                            _dbContext.MenuRoles.AddRange(_lstAddMenuRole);
                        }

                        if (_lstAddRolePermission.Count > 0)
                        {
                            _dbContext.RolePermissions.AddRange(_lstAddRolePermission);
                        }

                        if (_lstUpdateRolePermission.Count > 0)
                        {
                            _dbContext.RolePermissions.UpdateRange(_lstUpdateRolePermission);
                        }

                        var _result = _dbContext.SaveChanges(); // Lưu thay đổi vào CSDL

                        // Commit transaction nếu không có lỗi
                        transaction.Commit();

                        return _result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"AdministrationService BatchUpdateRole {ex.ToString()}");
                        transaction.Rollback();
                        return 0;
                    }
                }

            }
            else
            {
                return 0;
            }

        }

        /// <summary>
        /// Lấy danh sách người dùng
        /// </summary>
        /// <param name="posCode"></param>
        /// <param name="departmentCode"></param>
        /// <param name="titleCode"></param>
        /// <param name="fromBirthDay"></param>
        /// <param name="toBirthDay"></param>
        /// <param name="fullName"></param>
        /// <param name="sex"></param>
        /// <param name="userName"></param>
        /// <param name="roleCode"></param>
        /// <returns></returns>
        public List<UserModel> GetUsers(string posCode, string departmentCode, string titleCode, DateTime? fromBirthDay, DateTime? toBirthDay, string fullName, string sex, string userName, string roleCode)
        {
            List<UserModel> _result = new List<UserModel>();
            // var _lstUser = _dbContext.UserViews.ToList();
            var _lstUser = _dbContext.UserViews.Where(w =>
            (string.IsNullOrEmpty(posCode) || posCode == "000100" || w.PosCode == posCode)
            && (string.IsNullOrEmpty(departmentCode) || w.DepartmentCode == departmentCode)
            && (string.IsNullOrEmpty(titleCode) || w.TitleCode == titleCode)
            && (string.IsNullOrEmpty(fullName) || w.FullName.Contains(fullName))
            && (string.IsNullOrEmpty(sex) || w.Sex == sex)
            && (string.IsNullOrEmpty(userName) || w.UserName == userName)
            && (fromBirthDay == null || w.BirthDay >= fromBirthDay)
            && (toBirthDay == null || w.BirthDay <= toBirthDay)
            && (string.IsNullOrEmpty(roleCode) || w.DefaultRole == roleCode)
            ).ToList();
            for (int i = 0; i < _lstUser.Count; i++)
            {
                var _userModel = _mapper.Map<UserModel>(_lstUser[i]);
                _userModel.Order = i + 1;
                _result.Add(_userModel);
            }
            return _result;
        }

        // add UpdateUser
        public async Task<string> UpdateUser(UserModel data, string modifiedBy)
        {
            //"1" - "Tên đăng nhập người dùng  [" + $("#Ten_Nd").val() + "] đã tồn tại. Vui lòng kiểm tra lại!"
            //"2" - "Ngày sinh lớn hơn hoặc bằng ngày hiện tại. Vui lòng kiểm tra lại!";                        
            //"3" - Ngày sinh lớn hơn hoặc bằng ngày ngày cấp CMND/Thẻ căn cước. Vui lòng kiểm tra lại!";            
            //"4" - "Số  CMND/Thẻ căn cước  [" + $("#SoCMT").val() + "] đã tồn tại. Vui lòng kiểm tra lại!";            
            //"5" - "Id của người dùng được thêm mới != 0, Bạn hãy kiểm tra lại.
            //"6" - "Có lỗi xảy ra, bạn hãy kiểm tra lại!"

            if (data.UserBirthday == null || data.UserBirthday >= DateTime.Now)
            {
                return "2";
            }

            if (data.UserBirthday >= data.IssuedDate)
            {
                return "3";
            }

            User _currentUser = _dbContext.Users.Where(w => w.UserName == data.UserName).FirstOrDefault();


            if (_currentUser != null)
            {
                if (data.Id == 0) // them moi
                {
                    //"Tên đăng nhập người dùng  [" + $("#Ten_Nd").val() + "] đã tồn tại. Vui lòng kiểm tra lại!"
                    return "1";
                }
                else
                {
                    _currentUser.FullName = data.FullName;
                    _currentUser.Sex = data.Sex;
                    _currentUser.BirthDay = data.UserBirthday;
                    _currentUser.PosCode = data.PosCode;
                    _currentUser.DepartmentCode = data.DepartmentCode;
                    _currentUser.TitleCode = data.TitleCode;
                    _currentUser.DegreeCode = data.DegreeCode;
                    _currentUser.Mobile = data.Mobile;
                    _currentUser.Email = data.Email;
                    _currentUser.DefaultRole = data.DefaultRole;
                    _currentUser.IdCode = data.IdCode;
                    _currentUser.IssuedDate = data.IssuedDate;
                    _currentUser.IssuedPlace = data.IssuedPlace;
                    _currentUser.Status = data.Status;
                    _currentUser.ModifiedBy = modifiedBy;
                    _currentUser.ModifiedDate = DateTime.Now;
                    _dbContext.Users.Update(_currentUser);
                    int _status = _dbContext.SaveChanges();

                    if (_status > 0)
                    {
                        return "0";
                    }
                    else
                    {
                        // Có lỗi xảy ra, bạn hãy kiểm tra lại!
                        return "6";
                    }

                }
            }
            else
            {
                // Them moi
                if (data.Id != 0) // them moi
                {
                    //"Id của người dùng được thêm mới != 0, Bạn hãy kiểm tra lại.
                    return "5";
                }

                int _idCount = _dbContext.Users.Where(w => w.IdCode == data.IdCode).ToList().Count;

                if (_idCount > 0)
                {
                    //"Số  CMND/Thẻ căn cước  [" + $("#SoCMT").val() + "] đã tồn tại. Vui lòng kiểm tra lại!"
                    return "4";
                }

                User _user = _mapper.Map<User>(data);
                _user.BirthDay = data.UserBirthday;
                _user.ModifiedBy = modifiedBy;
                _user.ModifiedDate = DateTime.Now;


                // Tao user trong identity
                var _identityUser = new IdentityUser
                {
                    UserName = _user.UserName,
                    Email = _user.Email,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(_identityUser, $"{data.Password}");

                if (result.Succeeded)
                {
                    _dbContext.Users.Add(_user);
                    int _status = _dbContext.SaveChanges();

                    if (_status > 0)
                    {
                        return "0"; // Thành công
                    }
                    else
                    {
                        // Có lỗi xảy ra, bạn hãy kiểm tra lại!
                        _userManager.DeleteAsync(_identityUser);
                        return "6";
                    }
                }
                else
                {
                    // Có lỗi xảy ra, bạn hãy kiểm tra lại!
                    return "6";
                }
            }
        }

        /// <summary>
        /// Xóa tài khoản người dùng
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>
        /// 0 - Thành công
        /// 1 - User không tồn tại
        /// 2 - Có lỗi
        /// </returns>


        // add DeleteUser
        public async Task<string> DeleteUser(string userName)
        {
            try
            {
                User _user = _dbContext.Users.Where(w => w.UserName == userName).FirstOrDefault();
                if (_user != null)
                {
                    _dbContext.Users.Remove(_user);
                    int _status = _dbContext.SaveChanges();
                    if (_status > 0)
                    {
                        IdentityUser identityUser = await _userManager.FindByNameAsync(userName);
                        if (identityUser != null)
                        {
                            var result = await _userManager.DeleteAsync(identityUser);
                            if (result.Succeeded)
                            {
                                return "0";
                            }
                            else
                            {
                                return "2";
                            }

                        }
                        else
                        {
                            return "1";
                        }

                    }
                    else
                    {
                        return "2";
                    }
                }
                else
                {
                    //User không tồn tại
                    return "1";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa tài khoản người dùng: {ex.Message}");
                return "2";
            }


        }
        // Adđ Resetpassword
        public async Task<string> ResetPassword(string userName)
        {
            try
            {
                User _user = _dbContext.Users.Where(w => w.UserName == userName).FirstOrDefault();
                if (_user != null)
                {

                    IdentityUser identityUser = await _userManager.FindByNameAsync(userName);
                    if (identityUser != null)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                        var result = await _userManager.ResetPasswordAsync(identityUser, token, "Vbsp@123");
                        if (result.Succeeded)
                        {
                            return "0";
                        }
                        else
                        {
                            return "2";
                        }

                    }
                    else
                    {
                        return "1";
                    }

                }
                else
                {
                    //User không tồn tại
                    return "1";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa tài khoản người dùng: {ex.Message}");
                return "2";
            }


        }

        public Role GetGroupOfUser(string userName)
        {
            throw new NotImplementedException();
        }








        //public int UpdateUserGroup(CreateProject entity)
        //{
        //    throw new NotImplementedException();
        //}

        //public int DeleteUserGroup(CreateProject entity)
        //{
        //    throw new NotImplementedException();
        //}

        //public int CreateUserGroup(CreateProject entity)
        //{
        //    throw new NotImplementedException();
        //}

        public UserModel GetUserByUserName(string userName)
        {
            var _userInfo = _dbContext.UserViews.Where(w => w.UserName == userName).FirstOrDefault();
            return _mapper.Map<UserModel>(_userInfo);
        }

        public async Task<string> UpdcateUser(UserModel data, string modifiedBy)
        {
            //"1" - "Tên đăng nhập người dùng  [" + $("#Ten_Nd").val() + "] đã tồn tại. Vui lòng kiểm tra lại!"
            //"2" - "Ngày sinh lớn hơn hoặc bằng ngày hiện tại. Vui lòng kiểm tra lại!";                        
            //"3" - Ngày sinh lớn hơn hoặc bằng ngày ngày cấp CMND/Thẻ căn cước. Vui lòng kiểm tra lại!";            
            //"4" - "Số  CMND/Thẻ căn cước  [" + $("#SoCMT").val() + "] đã tồn tại. Vui lòng kiểm tra lại!";            
            //"5" - "Id của người dùng được thêm mới != 0, Bạn hãy kiểm tra lại.
            //"6" - "Có lỗi xảy ra, bạn hãy kiểm tra lại!"

            if (data.UserBirthday == null || data.UserBirthday >= DateTime.Now)
            {
                return "2";
            }

            if (data.UserBirthday >= data.IssuedDate)
            {
                return "3";
            }

            User _currentUser = _dbContext.Users.Where(w => w.UserName == data.UserName).FirstOrDefault();


            if (_currentUser != null)
            {
                if (data.Id == 0) // them moi
                {
                    //"Tên đăng nhập người dùng  [" + $("#Ten_Nd").val() + "] đã tồn tại. Vui lòng kiểm tra lại!"
                    return "1";
                }
                else
                {
                    _currentUser.FullName = data.FullName;
                    _currentUser.Sex = data.Sex;
                    _currentUser.BirthDay = data.UserBirthday;
                    _currentUser.PosCode = data.PosCode;
                    _currentUser.DepartmentCode = data.DepartmentCode;
                    _currentUser.TitleCode = data.TitleCode;
                    _currentUser.DegreeCode = data.DegreeCode;
                    _currentUser.Mobile = data.Mobile;
                    _currentUser.Email = data.Email;
                    _currentUser.DefaultRole = data.DefaultRole;
                    _currentUser.IdCode = data.IdCode;
                    _currentUser.IssuedDate = data.IssuedDate;
                    _currentUser.IssuedPlace = data.IssuedPlace;
                    _currentUser.Status = data.Status;
                    _currentUser.ModifiedBy = modifiedBy;
                    _currentUser.ModifiedDate = DateTime.Now;
                    _dbContext.Users.Update(_currentUser);
                    int _status = _dbContext.SaveChanges();// lưu vào db

                    if (_status > 0)
                    {
                        return "0";
                    }
                    else
                    {
                        // Có lỗi xảy ra, bạn hãy kiểm tra lại!
                        return "6";
                    }

                }
            }
            else
            {
                // Them moi
                if (data.Id != 0) // them moi
                {
                    //"Id của người dùng được thêm mới != 0, Bạn hãy kiểm tra lại.
                    return "5";
                }

                int _idCount = _dbContext.Users.Where(w => w.IdCode == data.IdCode).ToList().Count;

                if (_idCount > 0)
                {
                    //"Số  CMND/Thẻ căn cước  [" + $("#SoCMT").val() + "] đã tồn tại. Vui lòng kiểm tra lại!"
                    return "4";
                }

                User _user = _mapper.Map<User>(data);
                _user.ModifiedBy = modifiedBy;
                _user.ModifiedDate = DateTime.Now;
                _dbContext.Users.Add(_user);
                int _status = _dbContext.SaveChanges();

                // Tao user trong identity
                var _identityUser = new IdentityUser
                {
                    UserName = _user.UserName,
                    Email = _user.Email
                };
                var result = await _userManager.CreateAsync(_identityUser, $"{data.Password}");

                if (result.Succeeded && _status > 0)
                {
                    return "0";
                }
                else
                {
                    // Có lỗi xảy ra, bạn hãy kiểm tra lại!
                    return "6";
                }
            }
        }

        public List<StaffView> GetStaffs(string posCode)
        {
            if (string.IsNullOrEmpty(posCode))
            {
                return _dbContext.StaffViews.ToList();
            }
            else
            {
                return _dbContext.StaffViews.Where(w => w.PosCode == posCode).ToList();
            }
        }

        public StaffView GetStaffById(int staffId)
        {
            return _dbContext.StaffViews.Where(w => w.Id == staffId).FirstOrDefault();
        }

        public StaffView GetStaffByCode(string staffCode)
        {
            return _dbContext.StaffViews.Where(w => w.StaffCode == staffCode).FirstOrDefault();
        }

        //List<CreateProject> IAdministrationService.GetCreateProjects(int gantType, int grade)
        //{
        //    throw new NotImplementedException();
        //}


        //CreateProjectModel


        // public DbSet<CreateProjectModel> CreateProjectModels { get; set; }





        //  public DbSet<CreateProject> CreateProjects { get; set; }
        /// <summary>
        /// Xóa tài khoản người dùng
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>
        /// 0 - Thành công
        /// 1 - User không tồn tại
        /// 2 - Có lỗi
        /// </returns>

        //public List<UserModel> GetUsers(string posCode, string departmentCode, string titleCode, DateTime fromBirthDay, DateTime toBirthDay, string fullName, string sex, string userName, string roleCode)
        //{
        //    List<UserModel> _result = new List<UserModel>();
        //    // var _lstUser = _dbContext.UserViews.ToList();
        //    var _lstUser = _dbContext.UserViews.Where(w =>
        //    (string.IsNullOrEmpty(posCode) || posCode == "000100" || w.PosCode == posCode)
        //    && (string.IsNullOrEmpty(departmentCode) || w.DepartmentCode == departmentCode)
        //    && (string.IsNullOrEmpty(titleCode) || w.TitleCode == titleCode)
        //    && (string.IsNullOrEmpty(fullName) || w.FullName.Contains(fullName))
        //    && (string.IsNullOrEmpty(sex) || w.Sex == sex)
        //    && (string.IsNullOrEmpty(userName) || w.UserName == userName)
        //    && (fromBirthDay == null || w.BirthDay >= fromBirthDay)
        //    && (toBirthDay == null || w.BirthDay <= toBirthDay)
        //    ).ToList();
        //    for (int i = 0; i < _lstUser.Count; i++)
        //    {
        //        var _userModel = _mapper.Map<UserModel>(_lstUser[i]);
        //        _userModel.Order = i + 1;
        //        _result.Add(_userModel);
        //    }
        //    return _result;
        //}

        /// <summary>
        /// Hàm lấy thông tin phân quyền của nhóm người dùng theo mã MenuId
        /// </summary>
        /// <param name="roleCode"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public RolePermissionModel GetRolePermissionByMenuId(string roleCode, int menuId)
        {
            var _rolePermission = _dbContext.MenuRoleViews.Where(w => w.RoleCode == roleCode && w.Id == menuId).FirstOrDefault();
            if (_rolePermission != null)
            {
                RolePermissionModel rolePermissionModel = new RolePermissionModel();
                rolePermissionModel.RoleCode = _rolePermission.RoleCode;
                rolePermissionModel.MenuId = _rolePermission.Id;
                rolePermissionModel.Controller = _rolePermission.Controller;
                rolePermissionModel.Action = _rolePermission.Action;
                rolePermissionModel.PermissionFlag = _rolePermission.Permission;
                rolePermissionModel.ViewPermissionFlag = _rolePermission.ViewPermissionFlag;
                rolePermissionModel.CreatePermissionFlag = _rolePermission.CreatePermissionFlag;
                rolePermissionModel.EditPermissionFlag = _rolePermission.EditPermissionFlag;
                rolePermissionModel.DeletePermissionFlag = _rolePermission.DeletePermissionFlag;
                rolePermissionModel.AuthorizePermissionFlag = _rolePermission.AuthorizationPermissionFlag;
                rolePermissionModel.ReportPermissionFlag = _rolePermission.ReportPermissionFlag;
                return rolePermissionModel;
            }
            else
            {
                return null;
            }
        }
        public RolePermissionModel GetRolePermissionByMenuId(string roleCode, string controller, string action)
        {
            var _rolePermission = _dbContext.MenuRoleViews.Where(w => w.RoleCode == roleCode && w.Controller == controller && w.Action == action).FirstOrDefault();
            if (_rolePermission != null)
            {
                RolePermissionModel rolePermissionModel = new RolePermissionModel();
                rolePermissionModel.RoleCode = _rolePermission.RoleCode;
                rolePermissionModel.MenuId = _rolePermission.Id;
                rolePermissionModel.Controller = _rolePermission.Controller;
                rolePermissionModel.Action = _rolePermission.Action;
                rolePermissionModel.PermissionFlag = _rolePermission.Permission;
                rolePermissionModel.ViewPermissionFlag = _rolePermission.ViewPermissionFlag;
                rolePermissionModel.CreatePermissionFlag = _rolePermission.CreatePermissionFlag;
                rolePermissionModel.EditPermissionFlag = _rolePermission.EditPermissionFlag;
                rolePermissionModel.DeletePermissionFlag = _rolePermission.DeletePermissionFlag;
                rolePermissionModel.AuthorizePermissionFlag = _rolePermission.AuthorizationPermissionFlag;
                rolePermissionModel.ReportPermissionFlag = _rolePermission.ReportPermissionFlag;
                return rolePermissionModel;
            }
            else
            {
                return null;
            }
        }
    }
}
