using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Com.Models;
using Com.Core;
using System.Text;
using System.IO;

namespace Com.UI.Controllers
{
    public class DeviceController : BaseController
    {
        #region 设备标识

        [HttpPost]
        /// <summary>
        /// 批量添加设备标示
        /// </summary>
        public async Task<bool> DeviceUpdateMarkID(string deviceids, string markid)
        {
            bool result = false;
            var markidd = markid.ToInt32();
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.UpdateDevicebyMarkID(items.ID, markidd);
                }
            });
            return result;
        }
        public async Task<bool> InsertMark(Mark u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                int id = 0;
                result = MarkBO.Instance.InsertMark(u, out id);
            });
            return result;
        }

        public ActionResult MarkList()
        {
            return View();
        }

        [HttpPost, ActionName("MarkList")]
        public async Task<JsonResult> MarkListPost()
        {

            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<Mark> u = new List<Mark>();
            await Task.Run(() =>
            {
                u = MarkBO.Instance.GetListItems();
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //获取标识
        [HttpPost]
        public JsonResult StatisticGetMarkList()
        {
            List<Mark> marklist = MarkBO.Instance.GetListItems();
            List<TreeModel> marks = new List<TreeModel>();
            foreach (var item in marklist)
            {
                TreeModel mode = new TreeModel()
                {
                    id = item.ID,
                    text = item.Name,
                };
                marks.Add(mode);
            }
            return Json(marks, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]

        public async Task<bool> MarkEdit(Mark u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = MarkBO.Instance.UpdateMark(u);
            });
            return result;
        }

        [HttpPost, ActionName("MarkDelete")]
        public async Task<bool> MarkDelete(int id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = MarkBO.Instance.DeleteByID(id);
            });
            return result;
        }

        //根据登入人员返回人员所在工厂ID 勿删
        [HttpPost]
        public int UserSearchFactory()
        {
            return Loginer.CurrentUser.Department.FactoryID;

        }

        //检查传来id是不是本工厂id，勿删,维修检修统计专用
        [HttpPost, ActionName("CheckFactory")]
        public int CheckFactory(int factoryid)
        {
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            if (temp != null)
            {
                return 0;
            }
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            var b = Loginer.CurrentUser.Department.FactoryID;
            if (b == factoryid && temp1 != null)
            { return 0; }//说明是该工厂的工厂管理员
            if (b != factoryid && temp1 != null)
            { return 1; }//说明不是该工厂的工厂管理员
            if (b == factoryid && temp2 != null)
            { return 0; }//说明是该工厂的维修员
            if (b != factoryid && temp2 != null)
            { return 1; }//说明不是该工厂的维修员
            else
            { return 2; }//说明不是工厂管理员
        }

        //检查传来id是不是本工厂id，勿删,查询专用
        [HttpPost, ActionName("CheckSearchFactory")]
        public int CheckSearchFactory(int factoryid)
        {
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            if (temp != null)
            {
                return 0;
            }
            var b = Loginer.CurrentUser.Department.FactoryID;
            if (b == factoryid)
            { return 0; }//说明是该工厂人员
            else
            {
                return 1;
            }
        }

        //检查是不是那几个管理员
        public bool CheckIsOrNotadmin()
        {
            bool result = false;
            if (Com.Core.Loginer.IsAdmin || Com.Core.Loginer.IsFactoryAdmin ||
        Com.Core.Loginer.IsOutCheckAdmin || Com.Core.Loginer.IsDeviceAdmin
        || Com.Core.Loginer.IsITAdmin || Com.Core.Loginer.IsHouQinAdmin)
                result = true;
            return result;
        }

        //检查传来id是不是本工厂或者本部门id，勿删,维修检修统计专用
        [HttpPost, ActionName("CheckDepartment")]
        public int CheckDepartment(int departid)
        {
            try
            {
                departid = Convert.ToInt32(departid);//不知道为何总会报departid为空的错误
                var departidd = DepartmentBO.Instance.DepartIDByID(departid);
                var factoryid = Loginer.CurrentUser.Department.FactoryID;
                var DepartmentID = Loginer.DepartIDByUser(Loginer.CurrentUser);
                var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
                var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
                var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.品保管理员.ToString());
                var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
                var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
                var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
                var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
                if (temp != null)
                {
                    return 0;
                }
                if (temp1 != null || temp2 != null || temp3 != null
                    || temp4 != null || temp5 != null || temp6 != null)
                {
                    //检查id是不是本工厂部门id
                    bool check = DepartmentBO.Instance.Checkdapartid(factoryid, departid);
                    if (check)
                    { return 0; }
                    else
                    { return 1; }
                }
                if (DepartmentID == departidd)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex);
                return 1;

            }

        }

        [HttpPost]

        public async Task<bool> DeviceUpdateTime(string deviceids, int time)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.UpdateDevicebyTheTime(items.ID, time);
                }
            });
            return result;
        }



        #endregion

        #region 设备类型

        [HttpPost]
        /// <summary>
        /// 批量添加设备类型
        /// </summary>
        public async Task<bool> DeviceUpdateTypeID(string deviceids, string typeid)
        {
            bool result = false;
            var typeidd = typeid.ToInt32();
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.UpdateDevicebyTypeID(items.ID, typeidd);
                }
            });
            return result;
        }

        [HttpPost]
        public async Task<bool> InsertDeviceType(DeviceType u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                int id = 0;
                result = DeviceTypeBO.Instance.InsertDeviceType(u, out id);
            });
            return result;
        }

        public ActionResult DeviceTypeList()
        {
            return View();
        }

        [HttpPost, ActionName("DeviceTypeList")]
        public async Task<JsonResult> DeviceTypeListPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<DeviceType> u = new List<DeviceType>();
            await Task.Run(() =>
            {
                u = DeviceTypeBO.Instance.GetListItems();
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> DeviceTypeEdit(DeviceType u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceTypeBO.Instance.UpdateDeviceType(u);
            });
            return result;
        }

        [HttpPost, ActionName("DeviceTypeDelete")]
        public async Task<bool> DeviceTypeDelete(int id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceTypeBO.Instance.DeleteDeviceType(id);
            });
            return result;
        }

        [HttpPost]
        public async Task<JsonResult> DeviceTypeTreeUpdateJson()
        {
            List<TreeModel> list = new List<TreeModel>();
            await Task.Run(() =>
            {
                var listdtypes = DeviceTypeBO.Instance.GetListItems();
                SerachDeviceTypeTreeByParentID(list, listdtypes, null);
            });
            return Json(list);
        }

        private void SerachDeviceTypeTreeByParentID(List<TreeModel> list, List<DeviceType> departs, TreeModel upmodel)
        {
            foreach (var item in departs)
            {
                TreeModel model = new TreeModel()
                {
                    id = item.ID,
                    text = item.Name,
                    type = typeof(DeviceType).Name,
                    parentid = item.ParentID
                };
                if (upmodel == null || upmodel.parentid == item.ParentID) list.Add(model); else upmodel.children.Add(model);
                if (item.Children != null)
                {
                    model.children = new List<TreeModel>();
                    SerachDeviceTypeTreeByParentID(list, item.Children, model);
                }
            }
        }

        #endregion

        #region 设备供应商

        [HttpPost]
        public async Task<bool> InsertSupplier(Supplier u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                int id = 0;
                result = SupplierBO.Instance.InsertSupplier(u, out id);
            });
            return result;
        }

        public ActionResult SupplierList()
        {
            return View();
        }

        [HttpPost, ActionName("SupplierList")]
        public JsonResult SupplierListPost(string name, string person)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<Supplier> u = new List<Supplier>();
            string condition = "";
            if (!string.IsNullOrEmpty(name))
            {
                string ccc = string.Format(" Name  like '%{0}%' ", name);
                if (string.IsNullOrEmpty(condition)) { condition += ccc; }
                else { condition = condition + " and " + ccc; }
            }
            if (!string.IsNullOrEmpty(person))
            {
                string ccc = string.Format(" LinkMan  like '%{0}%' ", person);
                if (string.IsNullOrEmpty(condition)) { condition += ccc; }
                else { condition = condition + " and " + ccc; }
            }
            u = SupplierBO.Instance.GetListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> SupplierEdit(Supplier u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = SupplierBO.Instance.UpdateSupplier(u);
            });
            return result;
        }

        [HttpPost, ActionName("SupplierDelete")]
        public async Task<bool> SupplierDelete(int id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = SupplierBO.Instance.DeleteSupplierByID(id);
            });
            return result;
        }

        #endregion

        #region 设备报修操作
        [HttpPost]
        public JsonResult GetRepairGroupUserList()
        {
            var factid = Loginer.CurrentUser.Department.FactoryID;
            string factoryid = string.Empty;
            if (Loginer.CurrentUser != null)
            {
                factoryid = string.Format(" ={0}", factid);//默认查看本工厂
            }
            return Json(UserBO.Instance.GetRepairGroupUserss(factoryid), JsonRequestBehavior.DenyGet);
        }

        //根据传来的deviceid展示维修组成员
        [HttpPost]
        public JsonResult GetNeedRepairGroupUserList(string deviceid)
        {
            var markname = DeviceBO.Instance.GetDeviceByID(deviceid).Mark.Name;
            var RepairGroup = "设备维修组";
            if (markname == "自购设备" || markname == "自建设备" || markname == "电动工具")
            {
                RepairGroup = "设备维修组";
            }
            if (markname == "计检设备")
            {
                RepairGroup = "计检维修组";
            }
            if (markname == "后勤设施")
            {
                RepairGroup = "后勤维修组";
            }
            if (markname == "IT设备")
            {
                RepairGroup = "IT维修组";
            }
            if (markname == "办公设备")
            {
                RepairGroup = "办公维修组";
            }
            var factid = Loginer.CurrentUser.Department.FactoryID;
            string factoryid = string.Empty;
            if (Loginer.CurrentUser != null)
            {
                factoryid = string.Format(" ={0}", factid);//默认查看本工厂
            }
            return Json(UserBO.Instance.NewGetRepairGroupUsers(factoryid, RepairGroup, deviceid), JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<bool> InsertDeviceRepair(Repair u)
        {
            bool result = false;
            u.UserID = Loginer.CurrentUser.ID;
            await Task.Run(() =>
            {
                u.DisabilityTime = DateTime.Now;
                string id = string.Empty;
                u.BaoDegreeID = (int)Safeguard.一级报障;
                result = RepairBO.Instance.InsertRepair(u, out id);
                u.ID = id;
            });
            return result;
        }

        public ActionResult DeviceRepairList()
        {
            return View();
        }

        [HttpPost, ActionName("DeviceRepairList")]
        public async Task<JsonResult> DeviceRepairListPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<Repair> u = new List<Repair>();
            await Task.Run(() =>
            {
                u = RepairBO.Instance.GetListItems();
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> DeviceRepairEdit(Repair u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = RepairBO.Instance.UpdateRepair(u);
            });
            return result;
        }

        [HttpPost, ActionName("DeviceRepairDelete")]
        public async Task<bool> DeviceRepairDeletePost(string id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = RepairBO.Instance.DeleteRepairByID(id);
            });
            return result;
        }
        #endregion

        #region 故障原因
        [HttpPost, ActionName("FaultTypeCreate")]
        public bool FaultTypeCreate(FaultType f)
        {
            bool result = false;
            bool check = FaultTypeBO.Instance.CheckNameIsExit(f);
            if (!check)
            {
                result = FaultTypeBO.Instance.InsertFaultType(f);
            }
            return result;
        }

        public ActionResult FaultTypeList()
        {
            return View();
        }

        [HttpPost, ActionName("FaultTypeList")]
        public async Task<JsonResult> FaultTypeListPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<FaultType> u = new List<FaultType>();
            await Task.Run(() =>
            {
                u = FaultTypeBO.Instance.GetListItems();
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> FaultTypeEdit(FaultType u)
        {
            bool result = false;
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    bool check = FaultTypeBO.Instance.CheckNameIsExit(u);
                    if (!check)
                    {
                        result = FaultTypeBO.Instance.UpdateFaultType(u);
                    }
                });
            }
            return result;
        }

        [HttpPost]
        public async Task<bool> FaultTypeDelete(int id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = FaultTypeBO.Instance.DeleteFaultType(id);
            });
            return result;
        }

        //根据传来的deviceid展示故障原因
        [HttpPost]
        public JsonResult GetFaultTypeList()
        {
            List<FaultType> faulttypelist = FaultTypeBO.Instance.GetListItems();
            List<TreeModel> faulttypes = new List<TreeModel>();
            foreach (var item in faulttypelist)
            {
                TreeModel mode = new TreeModel()
                {
                    id = item.ID,
                    text = item.Name,
                };
                faulttypes.Add(mode);
            }
            return Json(faulttypes, JsonRequestBehavior.DenyGet);
        }

        //维修获取获取故障原因
        [HttpPost]
        public JsonResult RepairGetFaultTypeList(string DeviceID)
        {
            var markname = DeviceBO.Instance.GetDeviceByID(DeviceID).Mark.Name;
            int code = 0;
            if (string.IsNullOrEmpty(markname)) { code = 5; }//显示所有
            if (markname == "自购设备" || markname == "自建设备" || markname == "电动工具") { code = 0; }
            else if (markname == "计检设备") { code = 1; }
            else if (markname == "后勤设施") { code = 2; }
            else if (markname == "IT设备") { code = 3; }
            else if (markname == "办公设备") { code = 4; }
            List<FaultType> faulttypelist = FaultTypeBO.Instance.NewGetFaultTypes(code);
            List<TreeModel> faulttypes = new List<TreeModel>();
            foreach (var item in faulttypelist)
            {
                TreeModel mode = new TreeModel()
                {
                    id = item.ID,
                    text = item.Name,
                };
                faulttypes.Add(mode);
            }
            return Json(faulttypes, JsonRequestBehavior.DenyGet);
        }

        #endregion

        #region 故障现象

        [HttpPost, ActionName("FailurePhenomenonCreate")]
        public bool FailurePhenomenonCreate(FailurePhenomenon f)
        {
            bool result = false;
            bool check = FailurePhenomenonBO.Instance.CheckNameIsExit(f);
            if (!check)
            {
                result = FailurePhenomenonBO.Instance.InsertFailurePhenomenon(f);
            }
            return result;
        }

        public ActionResult FailurePhenomenonList()
        {
            return View();
        }

        [HttpPost, ActionName("FailurePhenomenonList")]
        public async Task<JsonResult> FailurePhenomenonListPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<FailurePhenomenon> u = new List<FailurePhenomenon>();
            await Task.Run(() =>
            {
                u = FailurePhenomenonBO.Instance.GetListItems();
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> FailurePhenomenonEdit(FailurePhenomenon u)
        {
            bool result = false;
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    bool check = FailurePhenomenonBO.Instance.CheckNameIsExit(u);
                    if (!check)
                    {
                        result = FailurePhenomenonBO.Instance.UpdateFailurePhenomenon(u);
                    }
                });
            }
            return result;
        }

        [HttpPost]
        public async Task<bool> FailurePhenomenonDelete(int id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = FailurePhenomenonBO.Instance.DeleteFailurePhenomenon(id);
            });
            return result;
        }

        //根据传来的deviceid展示故障现象
        [HttpPost]
        public JsonResult GetFailurePhenomenonList()
        {
            List<FailurePhenomenon> failturephonemenonlist = FailurePhenomenonBO.Instance.GetListItems();
            List<TreeModel> failturephonemenons = new List<TreeModel>();
            foreach (var item in failturephonemenonlist)
            {
                TreeModel mode = new TreeModel()
                {
                    id = item.ID,
                    text = item.Name,
                };
                failturephonemenons.Add(mode);
            }
            return Json(failturephonemenons, JsonRequestBehavior.DenyGet);
        }

        //维修获取获取故障现象
        [HttpPost]
        public JsonResult RepairGetFailurePhenomenonList(string DeviceID)
        {
            var markname = DeviceBO.Instance.GetDeviceByID(DeviceID).Mark.Name;
            int code = 0;
            if (string.IsNullOrEmpty(markname)) { code = 5; }//显示所有
            if (markname == "自购设备" || markname == "自建设备" || markname == "电动工具") { code = 0; }
            else if (markname == "计检设备") { code = 1; }
            else if (markname == "后勤设施") { code = 2; }
            else if (markname == "IT设备") { code = 3; }
            else if (markname == "办公设备") { code = 4; }
            List<FailurePhenomenon> failturephonemenonlist = FailurePhenomenonBO.Instance.NewGetFailurePhenomenons(code);
            List<TreeModel> failturephonemenons = new List<TreeModel>();
            foreach (var item in failturephonemenonlist)
            {
                TreeModel mode = new TreeModel()
                {
                    id = item.ID,
                    text = item.Name,
                };
                failturephonemenons.Add(mode);
            }
            return Json(failturephonemenons, JsonRequestBehavior.DenyGet);
        }

        #endregion

        #region 处理方式

        [HttpPost]
        public async Task<bool> InsertRepairGrade(RepairGrade u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                bool check = RepairGradeBO.Instance.CheckNameIsExit(u);
                if (!check)
                {
                    result = RepairGradeBO.Instance.InsertRepairGrade(u);
                }
            });
            return result;
        }

        public ActionResult RepairGradeList()
        {
            return View();
        }

        [HttpPost, ActionName("RepairGradeList")]
        public JsonResult RepairGradeListPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<RepairGrade> u = RepairGradeBO.Instance.GetListItems();
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> RepairGradeEdit(RepairGrade u)
        {
            bool result = false;
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    bool check = RepairGradeBO.Instance.CheckNameIsExit(u);
                    if (!check)
                    {
                        result = RepairGradeBO.Instance.UpdateRepairGradeBO(u);
                    }
                });
            }
            return result;
        }

        [HttpPost, ActionName("RepairGradeDelete")]
        public async Task<bool> RepairGradeDeletePost(int id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = RepairGradeBO.Instance.DeleteRepairGrade(id);
            });
            return result;
        }

        //根据传来的deviceid展示处理方式
        [HttpPost]
        public JsonResult GetRepairGradeList()
        {
            List<RepairGrade> repairgradelist = RepairGradeBO.Instance.GetListItems();
            List<TreeModel> repairgrades = new List<TreeModel>();
            foreach (var item in repairgradelist)
            {
                TreeModel mode = new TreeModel()
                {
                    id = item.ID,
                    text = item.Name,
                };
                repairgrades.Add(mode);
            }
            return Json(repairgrades, JsonRequestBehavior.DenyGet);
        }

        //维修获取处理方式
        [HttpPost]
        public JsonResult RepairGetRepairGradeList(string DeviceID)
        {
            var markname = DeviceBO.Instance.GetDeviceByID(DeviceID).Mark.Name;
            int code = 0;
            if (string.IsNullOrEmpty(markname)) { code = 5; }//显示所有
            if (markname == "自购设备" || markname == "自建设备" || markname == "电动工具") { code = 0; }
            else if (markname == "计检设备") { code = 1; }
            else if (markname == "后勤设施") { code = 2; }
            else if (markname == "IT设备") { code = 3; }
            else if (markname == "办公设备") { code = 4; }
            List<RepairGrade> repairgradelist = RepairGradeBO.Instance.NewRepairGrades(code);
            List<TreeModel> repairgrades = new List<TreeModel>();
            foreach (var item in repairgradelist)
            {
                TreeModel mode = new TreeModel()
                {
                    id = item.ID,
                    text = item.Name,
                };
                repairgrades.Add(mode);
            }
            return Json(repairgrades, JsonRequestBehavior.DenyGet);
        }

        #endregion

        #region 执行维修操作

        [HttpPost]
        public async Task<bool> ExecutionRepairEdit(ExecutionRepair u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                u.ImagePath = HttpUtility.UrlDecode(u.ImagePath);
                u.UserID = (Session[JstConfig.ACCOUNT_SESSTION] as User).ID;
                u.StartTime = DateTime.Now;
                result = ExecutionRepairBO.Instance.Update(u);
            });
            return result;
        }

        [HttpPost, ActionName("ExecutionRepairDelete")]
        public async Task<bool> ExecutionRepairDeletePost(string id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = ExecutionRepairBO.Instance.Delete(id);
            });
            return result;
        }

        [HttpPost]
        public async Task<bool> InsertExecutionRepair(ExecutionRepair u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                u.StartTime = DateTime.Now;
                u.UserID = (Session[JstConfig.ACCOUNT_SESSTION] as User).ID;
                u.ImagePath = HttpUtility.UrlDecode(u.ImagePath + "");
                result = ExecutionRepairBO.Instance.Insert(u);
            });
            return result;
        }
        public ActionResult ExecutionRepairList()
        {
            return View();
        }
        [HttpPost, ActionName("ExecutionRepairList")]
        public async Task<JsonResult> ExecutionRepairListPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<ExecutionRepair> u = new List<ExecutionRepair>();
            await Task.Run(() =>
            {
                u = ExecutionRepairBO.Instance.GetListItems();
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<JsonResult> GetOutListByRepairID(string repairid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<OutStore> u = new List<OutStore>();
            await Task.Run(() =>
            {
                u = OutStoreBO.Instance.GetListByRepairID(repairid);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> CreateOutStoreBySpareDevice(List<SpareDevice> rows, List<string> reparid)
        {
            bool result = false;
            await Task.Run(() =>
            {
                User uu = Session[JstConfig.ACCOUNT_SESSTION] as User;
                //创建备件申请单
                List<int> array = new List<int>();
                foreach (var item in rows)
                {
                    OutStore o = new OutStore
                    {
                        IsDelete = false,
                        OutNumber = 0,
                        OutTime = DateTime.Now,
                        State = "未领取",
                        SpareID = item.ID,
                        UserID = uu.ID,
                        HandlerUserID = uu.ID
                    };
                    int id = 0;
                    bool outadd = OutStoreBO.Instance.Insert(o, out id);
                    if (outadd) array.Add(id);
                }
                result = RepairRefOutStoreBO.Instance.InsertOrDelete(reparid[0], array.ToArray());
            });
            return result;
        }
        [HttpPost, ActionName("ExecutionRepairHis")]
        public async Task<JsonResult> ExecutionRepairHisPost(string deviceid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<ExecutionRepair> u = new List<ExecutionRepair>();
            await Task.Run(() =>
            {
                u = ExecutionRepairBO.Instance.GetExecRepairListByDeviceID(deviceid);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        #endregion

        #region 管理manage
        public ActionResult Manage()
        {
            return View();
        }

        public ActionResult DeviceImageList()
        {
            return View();
        }

        public string GetImageString()
        {
            var imgstring = string.Empty;
            var item = "/UploadFiles/Barcode/PrintBarcode/";
            string filepath = Server.MapPath(@item);
            imgstring = FindFile(filepath);
            return imgstring;
        }

        public string FindFile(string sSourcePath)
        {
            //遍历文件夹
            var imgstring = string.Empty;
            DirectoryInfo theFolder = new DirectoryInfo(sSourcePath);
            FileInfo[] thefileInfo = theFolder.GetFiles("*.png", SearchOption.TopDirectoryOnly);
            foreach (FileInfo NextFile in thefileInfo)  //遍历文件
            {
                var filenamestring = string.Empty;
                var item = NextFile.FullName;
                filenamestring += ";/UploadFiles/Barcode/PrintBarcode/" + NextFile.Name;
                imgstring += filenamestring;
            }
            return imgstring;
        }
        #endregion

        #region 审批流程设置
        public ActionResult ApproveFlow()
        {
            return View();
        }
        #endregion

        #region 检修计划

        [HttpPost]
        public async Task<bool> InsertDeviceRepairPlan(DeviceRepairPlan u)
        {
            bool result = false;
            {
                await Task.Run(() =>
                {
                    u.CycleWay = u.CycleWay == "on" ? "多次循环" : "单次循环";
                    result = DeviceRepairPlanBO.Instance.InsertDeviceRepairPlan(u);
                });
            }
            return result;
        }

        public ActionResult DeviceRepairPlanList()
        {
            return View();
        }

        [HttpPost, ActionName("DeviceRepairPlanList")]
        public async Task<JsonResult> DeviceRepairPlanListPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<DeviceRepairPlan> u = new List<DeviceRepairPlan>();
            await Task.Run(() =>
            {
                u = DeviceRepairPlanBO.Instance.GetListItems();
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }
        [HttpPost]

        public async Task<bool> DeviceRepairPlanEdit(DeviceRepairPlan u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                u.CycleWay = u.CycleWay == "on" ? "多次循环" : "单次循环";
                result = DeviceRepairPlanBO.Instance.UpdateDeviceRepairPlan(u);

            });
            return result;
        }

        [HttpPost, ActionName("DeviceRepairPlanDelete")]
        public async Task<bool> DeviceRepairPlanDelete(int id)
        {
            bool result = false;
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    result = DeviceRepairPlanBO.Instance.DeleteDeviceRepairPlan(id);
                });
            }
            return result;
        }

        #endregion

        #region 执行检修计划

        public async Task<bool> InsertImplementRepairPlan(ImplementRepairPlan I)
        {
            bool result = false;
            {
                await Task.Run(() =>
                {
                    result = ImplementRepairPlanBO.Instance.InsertImplementRepairPlan(I);
                });
            }
            return result;
        }

        [HttpPost]

        public async Task<bool> ImplementRepairPlanEdit(ImplementRepairPlan u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = ImplementRepairPlanBO.Instance.UpdateDeviceRepairPlan(u);

            });
            return result;
        }

        [HttpPost, ActionName("ImplementRepairPlanDelete")]
        public async Task<bool> ImplementRepairPlanDelete(int id)
        {
            bool result = false;
            if (ModelState.IsValid)
            {
                await Task.Run(() =>
                {
                    result = ImplementRepairPlanBO.Instance.DeleteImplementRepairPlan(id);
                });
            }
            return result;
        }

        #endregion

        #region 设备四级保障【新】
        //获取四级报障组人员
        [HttpPost]
        [OutputCache(Duration = 60000)]
        public JsonResult GetSafeguardGroupUserList()
        {
            int factoryid = Loginer.CurrentUser.Department.FactoryID;
            List<User> userlist = UserBO.Instance.GetSafeguardGroupUsers(factoryid);
            List<TreeModel> users = new List<TreeModel>();
            foreach (var item in userlist)
            {
                TreeModel mode = new TreeModel()
                {
                    code = item.UserCode,
                    parentid = item.DepartmentID,
                    text = item.RealName,
                    id = item.ID
                };
                users.Add(mode);
            }
            return Json(users, JsonRequestBehavior.DenyGet);
        }

        public ActionResult SafeguardManage()
        {
            return View();
        }

        [HttpGet]
        public bool SetDeviceSafeguard()
        {
            string[] user1 = Request.QueryString["user1"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int time1 = Request.QueryString["time1"].ToInt32();
            string[] user2 = Request.QueryString["user2"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int time2 = Request.QueryString["time2"].ToInt32();
            string[] user3 = Request.QueryString["user3"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int time3 = Request.QueryString["time3"].ToInt32();
            string[] user4 = Request.QueryString["user4"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int time4 = Request.QueryString["time4"].ToInt32();
            string device = Request.QueryString["deviceid"].ToString();
            string[] devicestr = device.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string deviceids = devicestr.StringArrayToStr(",");
            return DeviceSafeguardBO.Instance.InsertOrUpdateSafeguard(user1, user2, user3, user4, deviceids, time1, time2, time3, time4);
        }

        [HttpPost]
        public JsonResult GetSafeguardByDeviceID(string id)
        {
            return Json(DeviceSafeguardBO.Instance.GetSafeguardByDeviceID(id), JsonRequestBehavior.DenyGet);
        }

        //删除四级报障
        [HttpPost]
        public async Task<bool> DeleteSafeguardManage(string deviceids)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    DeviceBO.Instance.DeleteSafeguardManage(items.ID);//把设备的几个字段干掉
                    DeviceSafeguardBO.Instance.DeleteSafeguardManage(items.ID);//删除四级报障表
                }
                string[] devicearray = deviceids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string condition = "";
                string condition1 = "";
                foreach (var deviceid in devicearray)
                {
                    if (string.IsNullOrEmpty(condition)) { condition = string.Format(" de.DeviceID = '{0}' ", deviceid); }
                    else { condition = condition + string.Format("or de.DeviceID = '{0}' ", deviceid); }
                }
                List<DeviceSafeguard> DSGList = DeviceSafeguardBO.Instance.GetListItems("(" + condition + ")");//检查四级报障表是否删除
                foreach (var deviceidd in devicearray)
                {
                    if (string.IsNullOrEmpty(condition1)) { condition1 = string.Format(" d.ID = '{0}' ", deviceidd); }
                    else { condition1 = condition1 + string.Format("or d.ID = '{0}' ", deviceidd); }
                }
                List<Device> devicelistt = DeviceBO.Instance.GetListItems("(" + condition1 + ")" + " and d.IsSafeguard=1");//检查设备是否四级报障设置
                if (DSGList.Count == 0 && devicelistt.Count == 0)
                {
                    result = true;
                }
            });
            return result;
        }

        public ActionResult SafeguardManageList()
        {
            return View();
        }

        //四级报障短信列表
        [HttpPost, ActionName("SafeguardManageList")]
        public async Task<JsonResult> SafeguardManageListPost(string devicename, string repairid, string deviceid, string departmentid, string factid, string starttime, string endtime)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<RepairRefeguard> u = new List<RepairRefeguard>();
            string condition = " re.ID is not null";
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                {
                    condition += " and de.FactoryID is not null ";
                }
                else if (temp1 != null)
                { condition += " and de.FactoryID = " + factid; }
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                        {
                            condition += " and de.FactoryID=" + factid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                        }
                        else
                            condition += " and de.FactoryID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                    }
                }
            }
            else
            {
                if (temp != null || temp1 != null)
                { condition += " and de.FactoryID = " + factoryid; }
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                        {
                            condition += " and de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                        }
                        else
                            condition += " and de.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                    }
                }
            }
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(devicename))
                {
                    condition += string.Format(" and d.DeviceName like '%{0}%' ", devicename);
                }
                if (!string.IsNullOrEmpty(deviceid))
                {
                    condition += string.Format(" and d.ID like '%{0}%' ", deviceid);
                }
                if (!string.IsNullOrEmpty(repairid))
                {
                    condition += string.Format(" and re.DeviceRepairID like '%{0}%' ", repairid);
                }
                if (!string.IsNullOrEmpty(departmentid))
                {
                    condition += string.Format(" and de.ID ={0}", departmentid);
                }
                if (!string.IsNullOrEmpty(starttime) && !string.IsNullOrEmpty(endtime))
                {
                    DateTime start = Convert.ToDateTime(starttime + " 00:00:00");
                    DateTime end = Convert.ToDateTime(endtime + " 23:59:59");
                    condition += string.Format(" and re.TroubleTime between '{0}' and '{1}'", start, end);
                }
                u = RepairRefeguardBO.Instance.GetListItemss(condition);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }
        #endregion

        #region 设置检修与检修项目

        public async Task<bool> InsertPlanContext(PlanContext u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = PlanContextBO.Instance.InsertPlanContext(u);
            });
            return result;
        }

        public ActionResult PlanContextList()
        {
            return View();
        }

        [HttpPost, ActionName("PlanContextList")]
        public JsonResult PlanContextListPost(string repairplanfilename, int importentpart, string factoryid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<PlanContext> u = new List<PlanContext>();
            string condition = string.Empty;
            if (factoryid.ToInt32() == 0 || string.IsNullOrEmpty(factoryid))
            {
                condition = " 1=1 ";
            }
            else
            {
                condition = string.Format(" f.FactoryID ={0} ", factoryid);
            }
            if (!string.IsNullOrEmpty(repairplanfilename))
            {
                if (string.IsNullOrEmpty(condition)) condition = string.Format(" f.Name like '%{0}%' ", repairplanfilename);
                else condition += string.Format(" and f.Name like '%{0}%' ", repairplanfilename);
            }
            if (importentpart == 1)
            {
                if (string.IsNullOrEmpty(condition)) condition = " p.ImportentPart = 1";
                else condition += " and p.ImportentPart = 1";
            }
            u = PlanContextBO.Instance.GetListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> PlanContextEdit(PlanContext u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = PlanContextBO.Instance.UpdatePlanContext(u);
            });
            return result;
        }

        [HttpPost, ActionName("PlanContextDelete")]
        public async Task<bool> PlanContextDelete(int id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = PlanContextBO.Instance.DeletePlanContextByID(id);
            });
            return result;
        }

        //设置界面删除检修计划
        [HttpPost]
        public async Task<bool> DeleteDevicePlanContext(string deviceids)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    ExecutionPlan Exdmp = ExecutionPlanBO.Instance.GetItemBySql(string.Format(" d.ID='{0}' and dr.State in(1,2)", items.ID));
                    if (Exdmp != null)
                    {
                        //把计划内未做的检修单删除
                        RepairPlanRefPlanContextBO.Instance.DeleteRepairPlanRefPlanContext(Exdmp.ID);
                        ExecutionPlanBO.Instance.DeleteExecutionPlan(Exdmp.ID);
                    }
                    ExecutionPlan deletecq = ExecutionPlanBO.Instance.GetItemBySql(string.Format(" d.ID='{0}' and dr.State in(11,12)", items.ID));
                    if (deletecq != null)
                    {
                        //把超期的检修单删除
                        RepairPlanRefPlanContextBO.Instance.DeleteRepairPlanRefPlanContext(deletecq.ID);
                        ExecutionPlanBO.Instance.DeleteExecutionPlan(deletecq.ID);
                    }
                    DeviceBO.Instance.DeleteJXDeviceID(items.ID);//把设备的几个字段干掉
                    DeviceRefPlanContextBO.Instance.DeleteDeviceRefPlanContext(items.ID);//二维码删除
                }
                string[] devicearray = deviceids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string condition = "";
                string condition1 = "";
                foreach (var deviceid in devicearray)
                {
                    if (string.IsNullOrEmpty(condition)) { condition = string.Format(" de.ID = '{0}' ", deviceid); }
                    else { condition = condition + string.Format("or de.ID = '{0}' ", deviceid); }
                }
                List<ExecutionPlan> EDMPList = ExecutionPlanBO.Instance.GetListItems("(" + condition + ")" + " and e.State in(1,2,11,12)");//检查检修表是否删除
                foreach (var deviceidd in devicearray)
                {
                    if (string.IsNullOrEmpty(condition1)) { condition1 = string.Format(" d.ID = '{0}' ", deviceidd); }
                    else { condition1 = condition1 + string.Format("or d.ID = '{0}' ", deviceidd); }
                }
                List<Device> devicelistt = DeviceBO.Instance.GetListItems("(" + condition1 + ")" + " and d.IsSetPlan=1");//检查设备是否删除检修设置
                if (EDMPList.Count == 0 && devicelistt.Count == 0)
                {
                    result = true;
                }
            });
            return result;
        }

        /// <summary>
        /// 检修单完成确认及评价页面
        /// </summary>
        public ActionResult RepairPlanConfirmList()
        {
            return View();
        }

        /// <summary>
        /// 检修单完成确认及评价
        /// </summary>
        public async Task<bool> ConfirmExecutionPlan(string id, string assess, int start)
        {
            bool result = false;
            var State = 10;
            ExecutionPlan e = ExecutionPlanBO.Instance.GetItemByID(id);
            if (e.State == 5) { State = 9; }
            else if (e.State == 6) { State = 10; }
            var u = Loginer.CurrentUser;
            await Task.Run(() =>
            {
                //result = ExecutionPlanBO.Instance.UpdateState(state, id, assess, u.ID);
                result = ExecutionPlanBO.Instance.UpdateStates(State, id, assess, u.ID, start);
            });
            return result;
        }
        /// <summary>
        /// 设置检修标准与日期页面
        /// </summary>

        public ActionResult SetPlanList()
        {
            return View();
        }

        /// <summary>
        /// 增加与修改检修单日期页面
        /// </summary>
        public ActionResult AddDeviceRepairPlanList()
        {
            return View();
        }

        //检修二维码页面
        public ActionResult DeviceRefPlanContext()
        {
            return View();
        }

        [HttpPost]

        /// <summary>
        /// 检修二维码
        /// </summary>
        public JsonResult DeviceRefPlanContextList(string devicename, string deviceid, int importentpart, string departmentid, string factid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            string condition = " 1=1";
            string condition1 = "";
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            var dapartid = Loginer.DepartIDByUser(Loginer.CurrentUser);
            List<DeviceRefPlanContext> drdr = new List<DeviceRefPlanContext>();
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            //构造查询字符串
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                {
                    condition = " f.ID is not null ";
                }
                else
                {
                    if (temp != null || temp1 != null) { condition = " f.ID=" + factid; }
                    else
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                        {
                            if (devicemark == "('计检设备')")
                            {
                                condition = " f.ID=" + factid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                            }
                            else
                                condition = " f.ID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                        }
                        else
                        {
                            int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                            List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                            string cond = string.Empty;
                            foreach (Department item in listdepart)
                            {
                                cond += item.ID + ",";
                                if (item.Children == null) { continue; }
                                foreach (var child in item.Children)
                                {
                                    cond += child.ID + ",";
                                }
                            }
                            cond = cond.Substring(0, cond.Length - 1);
                            condition = string.Format(" h.ID in ({0})", cond);
                        }
                    }

                }
            }
            if (!string.IsNullOrEmpty(deviceid))
            { condition += string.Format(" and d.ID like '%{0}%' ", deviceid); }
            if (!string.IsNullOrEmpty(departmentid))
            {
                if (temp != null || temp1 != null || temp2 != null || temp3 != null || temp4 != null || temp5 != null)
                {
                    condition += string.Format(" and d.DepartmentID = {0} ", departmentid);
                }
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and d.DeviceName like '%{0}%' ", devicename);
            }
            if (importentpart == 1)
            {
                condition += string.Format(" and p.ImportentPart = {0} ", importentpart);
            }
            if (string.IsNullOrEmpty(factid))
            {
                if (temp != null || temp1 != null)
                {
                    condition1 = " f.ID=" + factoryid;
                }
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                        {
                            condition1 = " f.ID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                        }
                        else
                            condition1 = " f.ID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition1 = string.Format(" h.ID in ({0})", cond);
                    }
                }
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                if (!string.IsNullOrEmpty(condition))
                    condition = condition + " and " + condition1;
                else condition = condition1;
            }
            drdr = DeviceRefPlanContextBO.Instance.GetListItems(condition);
            int total = drdr.Count;
            var rows = drdr.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        /// <summary>
        /// 检修标准文件名页面
        /// </summary>
        public ActionResult RepairPlanFileNameList()
        {
            return View();
        }

        [HttpPost, ActionName("RepairPlanFileNameList")]

        /// <summary>
        /// 检修标准文件名
        /// </summary>
        public JsonResult RepairPlanFileNameListPost(string devicename, string assetnumber)
        {
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            string condition = string.Format(" FactoryID={0} ", factoryid);
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<RepairPlanFileName> u = new List<RepairPlanFileName>();
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and Name like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(assetnumber))
            {
                condition += string.Format(" and Name like '%{0}%' ", assetnumber);
            }
            u = RepairPlanFileNameBO.Instance.GetListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        #endregion

        #region 检修计划执行记录

        /// <summary>
        /// 添加检修工作人员
        /// </summary>
        public async Task<bool> DeviceUpdateHeadID(string deviceids, string userid)
        {
            bool result = false;
            var useridd = userid.ToInt32();
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.UpdateDevicebyUserID(items.ID, useridd);
                    List<ExecutionPlan> elist = ExecutionPlanBO.Instance.GetListItembyDeviceID(items.ID);
                    foreach (var item in elist)
                    {
                        result = ExecutionPlanBO.Instance.UpdateExecutionPlanbyUserID(item.ID, useridd);
                    }
                }
            });
            return result;
        }

        //增加计划内检修单
        [HttpPost]
        public async Task<bool> AddExecutionPlan(string deviceid, string plantime, string state)
        {
            bool result = false;
            string code = Loginer.CurrentUser.Department.Factory.Code;//工厂代码
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceid);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    ExecutionPlan e = ExecutionPlanBO.Instance.ExecutionPlanbyRepairDate(items.ID, plantime);
                    if (0 == items.FileNameID || e != null)
                    {
                        continue;
                    }
                    ExecutionPlan u = new ExecutionPlan();
                    string[] plantimes = plantime.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    u.TimeNum = plantimes[0].ToInt32() * 10000 + plantimes[1].ToInt32() * 100 + plantimes[2].ToInt32();
                    u.UserCost = 0;
                    u.SpareDeviceCost = 0;
                    u.CompleteTime = null;
                    u.StartTime = null;
                    u.Describe = null;
                    u.RepairUserID = (int)items.HeadID;
                    u.PlanTime = plantime;
                    u.PlanContextID = items.FileNameID;
                    u.State = state.ToInt32();
                    u.DeviceID = items.ID;
                    u.ID = ExecutionPlanBO.Instance.CreateDeviceRepairID(u.PlanTime, items.ID, code);
                    if (u.ID == "计划已存在")
                    {
                        result = true;
                    }
                    else
                    {
                        string condition = string.Format("e.State in(1,2) and e.DeviceID = '{0}'", items.ID);
                        List<ExecutionPlan> list = ExecutionPlanBO.Instance.GetListItems(condition);
                        foreach (var item in list)
                        {
                            RepairPlanRefPlanContextBO.Instance.DeleteRepairPlanRefPlanContext(item.ID);
                            ExecutionPlanBO.Instance.DeleteExecutionPlan(item.ID);
                        }
                        result = ExecutionPlanBO.Instance.InsertExecutionPlan(u);
                        List<PlanContext> plancontextlist = PlanContextBO.Instance.PlanContextList(items.FileNameID);
                        RepairPlanRefPlanContext runTemp = new RepairPlanRefPlanContext();
                        foreach (var item in plancontextlist)
                        {
                            runTemp.PlanContextID = item.ID;
                            runTemp.Project = item.Project;
                            runTemp.Standard = item.Standard;
                            runTemp.Content = item.Content;
                            runTemp.ImportentPart = item.ImportentPart;
                            runTemp.Code = item.Code;
                            runTemp.ExecutionPlanID = u.ID;
                            RepairPlanRefPlanContextBO.Instance.Insert(runTemp);
                        }
                    }
                    result = DeviceBO.Instance.UpdateDevicebyID(items.ID, plantime);
                }
            });
            return result;
        }

        //增加计划外检修单
        [HttpPost]
        public async Task<bool> AddExecutionPlanWai(string deviceid, string plantime, string state, int userid)
        {
            bool result = false;
            string code = Loginer.CurrentUser.Department.Factory.Code;//工厂代码
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceid);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    ExecutionPlan e = ExecutionPlanBO.Instance.ExecutionPlanbyRepairDate(items.ID, plantime);
                    if (0 == items.FileNameID || e != null)
                    {
                        continue;
                    }
                    ExecutionPlan u = new ExecutionPlan();
                    string[] plantimes = plantime.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    u.TimeNum = plantimes[0].ToInt32() * 10000 + plantimes[1].ToInt32() * 100 + plantimes[2].ToInt32();
                    u.UserCost = 0;
                    u.SpareDeviceCost = 0;
                    u.CompleteTime = null;
                    u.StartTime = null;
                    u.Describe = null;
                    u.RepairUserID = userid;
                    u.PlanTime = plantime;
                    u.PlanContextID = items.FileNameID;
                    u.State = state.ToInt32();
                    u.DeviceID = items.ID;
                    u.ID = ExecutionPlanBO.Instance.CreateDeviceRepairID(u.PlanTime, items.ID, code);
                    if (u.ID == "计划已存在")
                    {
                        result = true;
                    }
                    else
                    {
                        result = ExecutionPlanBO.Instance.InsertExecutionPlan(u);
                        List<PlanContext> plancontextlist = PlanContextBO.Instance.PlanContextList(items.FileNameID);
                        RepairPlanRefPlanContext runTemp = new RepairPlanRefPlanContext();
                        foreach (var item in plancontextlist)
                        {
                            runTemp.PlanContextID = item.ID;
                            runTemp.Project = item.Project;
                            runTemp.Standard = item.Standard;
                            runTemp.Content = item.Content;
                            runTemp.ImportentPart = item.ImportentPart;
                            runTemp.Code = item.Code;
                            runTemp.ExecutionPlanID = u.ID;
                            RepairPlanRefPlanContextBO.Instance.Insert(runTemp);
                        }
                    }
                    result = DeviceBO.Instance.UpdateDevicebyID(items.ID, plantime);
                }
            });
            return result;
        }

        //修改检修计划单
        [HttpPost]
        public async Task<bool> EditExecutionPlan(ExecutionPlan u)
        {
            bool result = false;
            ExecutionPlan d = ExecutionPlanBO.Instance.ExecutionPlanbyID(u.ID);
            u.PlanTime = u.PlanTime.Substring(0, 10);
            u.PlanContextID = d.PlanContextID;
            u.State = d.State;
            await Task.Run(() =>
            {
                if (1 == u.State)
                {
                    result = DeviceBO.Instance.UpdateDevicebyID(d.DeviceID, u.PlanTime);
                }
                result = ExecutionPlanBO.Instance.UpdateExecutionPlan(u);
            });
            return result;
        }

        //删除检修计划单
        [HttpPost]
        public async Task<bool> DeleteExecutionPlan(string id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                ExecutionPlan dr = ExecutionPlanBO.Instance.ExecutionPlanbyID(id);
                if (dr.State == 1)
                {
                    DeviceBO.Instance.UpdateBYEDeviceID(dr.DeviceID);
                }
                RepairPlanRefPlanContextBO.Instance.DeleteRepairPlanRefPlanContext(id);
                result = ExecutionPlanBO.Instance.DeleteExecutionPlan(id);
            });
            return result;
        }

        //保存计划执行记录
        public async Task<string> UpdateExecutionPlan(ExecutionPlan dr)
        {
            bool result = false;
            string code = Loginer.CurrentUser.Department.Factory.Code;//工厂代码
            string msg = string.Empty;
            List<RepairPlanRefPlanContext> contextlist = new List<RepairPlanRefPlanContext>();
            contextlist = RepairPlanRefPlanContextBO.Instance.GetRepairContext(dr.ID);
            ExecutionPlan plan = ExecutionPlanBO.Instance.GetItemByID(dr.ID);
            dr.Device = plan.Device;
            dr.DeviceID = plan.DeviceID;
            var de = DeviceBO.Instance.GetDeviceByID(plan.DeviceID);
            foreach (var item in contextlist)
            {
                if (item.IsFault == null)
                {
                    msg = "检修项目没有做完！";
                    return msg;
                }
            }
            dr.StartTime = dr.StartTime.Substring(0, 16);
            dr.CompleteTime = dr.CompleteTime.Substring(0, 16);
            if (plan.State == 1 || plan.State == 3 || plan.State == 11) { dr.State = 5; }
            else { dr.State = 6; }
            dr.UserCost = plan.UserCost;
            dr.SpareDeviceCost = plan.SpareDeviceCost;
            dr.Customer = "电脑端";
            await Task.Run(() =>
            {
                result = ExecutionPlanBO.Instance.UpdateInExecutionPlan(dr);
            });
            msg = result ? "True" : "保存失败！";
            if (result)
            {
                HelperExtensions.PushMessage(0, dr.ID);//透传发检修待确认消息
                result = DeviceRefSpareDeviceBO.Instance.AddbyDeviceRepairorRepairPlan(dr.ID, de.ID, 1);
                if (!result) { msg = "保存关联设备失败"; }
            }
            if (result)
            {
                result = ExecutionPlanBO.Instance.NextRepairPlan(de, (int)dr.State, code);
                if (!result) { msg = "设置下次检修失败"; }
            }
            return msg;

        }

        // 检修延期
        public async Task<bool> LaterExecutionPlan(string id, string reason, string time)
        {
            bool result = false;
            ExecutionPlan u = ExecutionPlanBO.Instance.ExecutionPlanbyID(id);
            string[] plantimes = time.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var plantime = plantimes[0].ToInt32() * 10000 + plantimes[1].ToInt32() * 100 + plantimes[2].ToInt32();
            var state = 0;
            if (u.State == 1 || u.State == 11) { state = 3; }
            else { state = 4; }
            await Task.Run(() =>
            {
                result = ExecutionPlanBO.Instance.LaterExecutionPlan(id, reason, state, time, plantime);
            });
            return result;
        }

        // 检修单页面
        public ActionResult ExecutionPlanList()
        {
            return View();
        }

        [HttpPost, ActionName("ExecutionPlanList")]
        // 检修单页面
        public JsonResult ExecutionPlanListPost(string drid, string deviceid, string did, string devicename, string repairstate, string time1, string time2, string userid, string departmentid, string sql, string factid, string type, string mark)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            bool result = false;
            bool result1 = true;
            int factoryid = Loginer.CurrentUser.Department.FactoryID;
            int daparmentid = Loginer.DepartIDByUser(Loginer.CurrentUser);
            List<ExecutionPlan> e = new List<ExecutionPlan>();
            var condition = "";
            int dapartid = 0;
            User u = Loginer.CurrentUser;
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            if (temp != null || temp1 != null)
            {
                condition = string.Format(" dm.FactoryID={0}", factoryid);
                result = true;
            }
            else
            {
                var devicemark = GetMarkByWhatAdmin();
                if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                {
                    if (devicemark == "('计检设备')")
                        condition = " dm.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                    else
                        condition = " dm.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                }
                else if (temp2 != null)//维修员
                {
                    int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                    List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                    string cond = string.Empty;
                    foreach (Department item in listdepart)
                    {
                        cond += item.ID + ",";
                        if (item.Children == null) { continue; }
                        foreach (var child in item.Children)
                        {
                            cond += child.ID + ",";
                        }
                    }
                    cond = cond.Substring(0, cond.Length - 1);
                    var conditions = string.Format(" dm.ID in ({0})", cond);
                    condition = " dm.FactoryID=" + factoryid + string.Format(" and (RepairUserID={0} or {1})", u.ID, conditions);
                }
                else
                {
                    int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                    List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                    string cond = string.Empty;
                    foreach (Department item in listdepart)
                    {
                        cond += item.ID + ",";
                        if (item.Children == null) { continue; }
                        foreach (var child in item.Children)
                        {
                            cond += child.ID + ",";
                        }
                    }
                    cond = cond.Substring(0, cond.Length - 1);
                    condition = string.Format(" dm.ID in ({0})", cond);
                }
                result = true;
            }
            if (!string.IsNullOrEmpty(repairstate) && repairstate[0] == '1')
            {
                dapartid = Loginer.DepartIDByUser(u);
                result1 = false;
            }
            if (result && result1)
            {
                if (!string.IsNullOrEmpty(repairstate))
                {
                    condition += string.Format(" and {0} ", repairstate);
                }
                if (!string.IsNullOrEmpty(drid))
                {
                    condition += string.Format(" and e.ID like '%{0}%' ", drid);
                }
                if (!string.IsNullOrEmpty(sql))
                {
                    if (!string.IsNullOrEmpty(condition))
                        condition += " and " + sql;
                    else
                        condition = sql;
                }
                if (!string.IsNullOrEmpty(deviceid))
                {
                    condition += string.Format(" and de.ID='{0}' ", deviceid);
                }
                if (!string.IsNullOrEmpty(did))
                {
                    condition += string.Format(" and e.DeviceID like '%{0}%' ", did);
                }
                if (!string.IsNullOrEmpty(devicename))
                {
                    condition += string.Format(" and de.DeviceName like '%{0}%' ", devicename);
                }
                if (!string.IsNullOrEmpty(userid))
                {
                    condition += string.Format(" and u.RealName like '%{0}%' ", userid);
                }
                if (!string.IsNullOrEmpty(type))
                {
                    condition += string.Format(" and dt.Name like '%{0}%' ", type);
                }
                if (!string.IsNullOrEmpty(mark))
                {
                    condition += string.Format(" and m.Name like '%{0}%' ", mark);
                }
                if (!string.IsNullOrEmpty(departmentid) && (temp != null || temp1 != null || temp2 != null
                            || temp3 != null || temp4 != null || temp5 != null || temp6 != null))//防止和前面的普通人员已带出的部门搜索冲突
                {
                    condition += string.Format("  and dm.ID ={0} ", departmentid);
                }
                if (!string.IsNullOrEmpty(time1) && !string.IsNullOrEmpty(time2))
                {
                    if (time1.ToInt32() == 0 && time2.ToInt32() == 0)
                    {
                        string nowtime = "2016/04/01";//得到2016/04/01这天的时间
                        string[] time = nowtime.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        int year = time[0].ToInt32();
                        int moth = time[1].ToInt32();
                        int day = time[2].ToInt32();
                        var a = year * 10000 + moth * 100 + day;
                        string gettime = DateTime.Now.AddDays(+31).ToString("yyyy/MM/dd");//得到当前时间31天后时间
                        string[] thetime = gettime.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        int getyear = thetime[0].ToInt32();
                        int getmoth = thetime[1].ToInt32();
                        int getday = thetime[2].ToInt32();
                        var b = getyear * 10000 + getmoth * 100 + getday;
                        condition += string.Format(" and TimeNum <= {0} and TimeNum >= {1} ", b, a);
                    }
                    else
                    {
                        condition += string.Format(" and TimeNum <= {0} and TimeNum >= {1} ", time2, time1);
                    }
                }
                e = ExecutionPlanBO.Instance.GetListItems(condition);
            }
            else if (!result1)//检修确认专用
            {
                var state = repairstate.Substring(1);
                if (temp != null || temp1 != null)
                {
                    e = ExecutionPlanBO.Instance.GetListItems(string.Format("dm.FactoryID={0} and {1} ", factoryid, state));
                }
                else if (temp2 != null)//维修员
                {
                    int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                    List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                    string cond = string.Empty;
                    foreach (Department item in listdepart)
                    {
                        cond += item.ID + ",";
                        if (item.Children == null) { continue; }
                        foreach (var child in item.Children)
                        {
                            cond += child.ID + ",";
                        }
                    }
                    cond = cond.Substring(0, cond.Length - 1);
                    var conditions = string.Format(" dm.ID in ({0})", cond);
                    var conditionss = " dm.FactoryID=" + factoryid + string.Format(" and (RepairUserID={0} or {1})", u.ID, conditions);
                    e = ExecutionPlanBO.Instance.GetListItems(string.Format("{0} and {1}", conditionss, state));
                }
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        var conditionss = string.Empty;
                        if (devicemark == "('计检设备')")
                            conditionss = " dm.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                        else
                            conditionss = " dm.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                        e = ExecutionPlanBO.Instance.GetListItems(string.Format(" {0} and {1}", conditionss, state));
                    }
                }
            }
            int total = e.Count;
            var rows = e.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        public ActionResult ImplementRepairPlanList()
        {
            return View();
        }

        [HttpPost, ActionName("ImplementRepairPlanList")]
        // 检修记录页面
        public async Task<JsonResult> ImplementRepairPlanListPost(string drid, string deviceid, string did, string devicename, string repairstate, string repairclycle, string time1, string time2, string time3, string time4, string userid, string departmentid, string sql, string factid, string type, string mark)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            int factoryid = Loginer.CurrentUser.Department.FactoryID;
            int daparmentid = Loginer.DepartIDByUser(Loginer.CurrentUser);
            List<ExecutionPlan> e = new List<ExecutionPlan>();
            var condition = "";
            User u = Loginer.CurrentUser;
            int ishidden = 0;//控制评价和评分是否显示，0显示1影藏
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            if (temp2 != null && temp == null && temp1 == null)
            {
                ishidden = 1;
            }
            if (!string.IsNullOrEmpty(factid))
            {
                if (temp != null || temp1 != null || temp2 != null)
                {
                    if (temp != null && factid.ToInt32() == 0)
                        condition = " dm.FactoryID is not null ";
                    else
                        condition = string.Format(" dm.FactoryID={0}", factid);
                }
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition = " dm.FactoryID=" + factid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                        else
                            condition = " dm.FactoryID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition = string.Format(" dm.ID in ({0})", cond);
                    }
                }
            }
            else//当出现空值时factid
            {
                if (temp != null || temp1 != null || temp2 != null)
                {
                    condition = string.Format(" dm.FactoryID={0}", factoryid);
                }
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition = " dm.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                        else
                            condition = " dm.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition = string.Format(" dm.ID in ({0})", cond);
                    }
                }
            }
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(repairstate))
                {
                    condition += string.Format(" and {0} ", repairstate);
                }
                if (!string.IsNullOrEmpty(repairclycle))
                {
                    condition += string.Format(" and de.RepairClycle {0} ", repairclycle);
                }
                if (!string.IsNullOrEmpty(drid))
                {
                    condition += string.Format(" and e.ID like '%{0}%' ", drid);
                }
                if (!string.IsNullOrEmpty(type))
                {
                    condition += string.Format(" and dt.Name like '%{0}%' ", type);
                }
                if (!string.IsNullOrEmpty(mark))
                {
                    condition += string.Format(" and m.Name like '%{0}%' ", mark);
                }
                if (!string.IsNullOrEmpty(sql))
                {
                    if (!string.IsNullOrEmpty(condition))
                        condition = condition + " and " + sql;
                    else
                        condition = sql;
                }
                if (!string.IsNullOrEmpty(deviceid))
                {
                    condition += string.Format(" and de.ID='{0}' ", deviceid);
                }
                if (!string.IsNullOrEmpty(did))
                {
                    condition += string.Format(" and e.DeviceID like '%{0}%' ", did);
                }
                if (!string.IsNullOrEmpty(devicename))
                {
                    condition += string.Format(" and de.DeviceName like '%{0}%' ", devicename);
                }
                if (!string.IsNullOrEmpty(userid))
                {
                    condition += string.Format(" and u.RealName like '%{0}%' ", userid);
                }
                if (!string.IsNullOrEmpty(departmentid) && (temp != null || temp1 != null || temp2 != null
                    || temp3 != null || temp4 != null || temp5 != null || temp6 != null))//防止和前面的普通人员已带出的部门搜索冲突
                {
                    condition += string.Format("  and dm.ID ={0} ", departmentid);
                }
                if (!string.IsNullOrEmpty(time3) && !string.IsNullOrEmpty(time4))
                {
                    DateTime bdt = Convert.ToDateTime(time3 + " 00:00:00");
                    DateTime edt = Convert.ToDateTime(time4 + " 23:59:59");
                    condition += string.Format(" and  CONVERT(datetime,e.CompleteTime) between '{0}' and '{1}'", bdt, edt);
                }
                else if (!string.IsNullOrEmpty(time1) && !string.IsNullOrEmpty(time2))
                {
                    if (time1.ToInt32() == 0 && time2.ToInt32() == 0)
                    {
                        string nowtime = DateTime.Now.ToString("yyyy/MM/dd");//得到今天的时间
                        string[] time = nowtime.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        int year = time[0].ToInt32();
                        int moth = time[1].ToInt32();
                        int day = time[2].ToInt32();
                        var a = year * 10000 + moth * 100 + day;
                        string gettime = DateTime.Now.AddDays(-30).ToString("yyyy/MM/dd");//得到30天前时间
                        string[] thetime = gettime.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        int getyear = thetime[0].ToInt32();
                        int getmoth = thetime[1].ToInt32();
                        int getday = thetime[2].ToInt32();
                        var b = getyear * 10000 + getmoth * 100 + getday;
                        condition += string.Format(" and TimeNum >= {0} and TimeNum <= {1} ", b, a);
                    }
                    else
                        condition += string.Format(" and TimeNum >= {1} and TimeNum <= {0}", time1, time2);
                }
                e = ExecutionPlanBO.Instance.GetListItem(ishidden, condition);
            });
            int total = e.Count;
            var rows = e.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //设备界面查询设备检修记录
        public ActionResult OneImplementRepairPlanList()
        {
            return View();
        }

        [HttpPost, ActionName("OneImplementRepairPlanList")]
        public async Task<JsonResult> OneImplementRepairPlanListPost(string deviceid, string btime, string etime, string jxstate)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<ExecutionPlan> e = new List<ExecutionPlan>();
            var condition = string.Empty;
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(jxstate))
                {
                    if (jxstate == "0") jxstate = " is not null";
                    else if (jxstate == "1") jxstate = " in(11,12)";
                    else if (jxstate == "2") jxstate = " in(1,2,3,4)";
                    else if (jxstate == "3") jxstate = " in(5,6)";
                    else if (jxstate == "4") jxstate = " in(9,10)";
                    condition = string.Format(" e.State {0} ", jxstate);
                }
                if (!string.IsNullOrEmpty(deviceid))
                {
                    string ccc = string.Format(" de.ID='{0}' ", deviceid);
                    if (string.IsNullOrEmpty(condition)) condition += ccc;
                    else condition += " and " + ccc;
                }
                if (!string.IsNullOrEmpty(btime) && !string.IsNullOrEmpty(etime))
                {
                    string ccc = string.Format(" e.CompleteTime between '{0}' and '{1}' ", btime, etime);
                    if (string.IsNullOrEmpty(condition)) condition += ccc;
                    else condition += " and " + ccc;
                }
                e = ExecutionPlanBO.Instance.GetListOneItem(condition, deviceid);
            });
            int total = e.Count;
            var rows = e.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //获取检修标准列表
        [HttpPost]
        public async Task<JsonResult> GetRepairContext(string id)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<RepairPlanRefPlanContext> u = new List<RepairPlanRefPlanContext>();
            await Task.Run(() =>
            {
                u = RepairPlanRefPlanContextBO.Instance.GetRepairContext(id);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        //增加检修人员列表
        public async Task<bool> AddRepairPlanUsers(List<User> userlist, string drid)
        {
            bool result = false;
            await Task.Run(() =>
            {
                List<RepairPlanUserNode> runlist = RepairPlanUserNodeBO.Instance.GetRepairUsers(drid); //如果已经增加过结点，先获取出来
                foreach (var item in runlist)                                                               //把相同的剔除
                {
                    for (int n = 0; n < userlist.Count; n++)
                    {
                        if (item.UserID == userlist[n].ID)
                            userlist.Remove(userlist[n]);
                    }
                }
                result = RepairPlanUserNodeBO.Instance.Inserts(drid, userlist);
            });
            return result;
        }
        //获取检修人员列表
        public async Task<JsonResult> GetRepairPlanUsersForDelete(string drid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<RepairPlanUserNode> u = new List<RepairPlanUserNode>();
            await Task.Run(() =>
            {
                u = ExecutionPlanBO.Instance.GetRepairUsers(drid);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }
        //删除检修人员列表
        public async Task<bool> DeleteRepairPlanUsers(RepairPlanUserNode delrunode)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = RepairPlanUserNodeBO.Instance.DeleteRepairPlanUserNode(delrunode.ID);                   //删除该删除的结点
            });
            return result;
        }

        //添加检修所用备件
        [HttpPost]
        public async Task<bool> AddRepairPlanSpareDevices(List<SpareDevice> sdlist, string drid)
        {
            bool result = false;
            int uuu = Loginer.CurrentUser.ID;
            await Task.Run(() =>
            {
                List<RepairPlanOutStoreNode> rosnlist = RepairPlanOutStoreNodeBO.Instance.GetRepairSpareDevices(drid);
                foreach (var item in rosnlist)
                {
                    for (int n = 0; n < sdlist.Count; n++)
                    {
                        if (item.OutStore.SpareID == sdlist[n].ID)
                            sdlist.Remove(sdlist[n]);
                    }
                }
                List<OutStore> oslist = new List<OutStore>();
                foreach (var item in sdlist)
                {
                    OutStore osTemp = new OutStore();
                    osTemp.OutNumber = 0;
                    osTemp.HandlerUserID = osTemp.UserID = uuu;
                    osTemp.SpareID = item.ID;
                    osTemp.SpareDevice = item;
                    osTemp.IsDelete = false;
                    osTemp.OutTime = DateTime.Now;
                    osTemp.State = "已领取";
                    osTemp.Content = string.Format("检修单号：{0}", drid);
                    int id = 0;
                    OutStoreBO.Instance.Insert(osTemp, out id);
                    osTemp.ID = id;
                    oslist.Add(osTemp);
                }
                result = RepairPlanOutStoreNodeBO.Instance.Insert(drid, oslist);
            });
            return result;
        }

        //获取维修所用备件结点列表（有操作的页面加载这个）              
        [HttpPost]
        public async Task<JsonResult> GetRepairPlanSpareDevicesForDelete(string drid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<RepairPlanOutStoreNode> u = new List<RepairPlanOutStoreNode>();
            await Task.Run(() =>
            {
                u = RepairPlanOutStoreNodeBO.Instance.GetRepairSpareDevices(drid);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //删除维修所用备件
        [HttpPost]
        public async Task<bool> DeleteRepairPlanSpareDevices(RepairPlanOutStoreNode delosnode, string drid)
        {
            bool result = false;
            await Task.Run(() =>
            {
                SpareDeviceBO.Instance.UpdateSpareDeviceStoreNumber(delosnode.OutStore.SpareID, delosnode.OutStore.SpareDevice.StoreNumber + delosnode.OutStore.OutNumber);
                RepairPlanOutStoreNodeBO.Instance.Delete(delosnode.ID);
                result = OutStoreBO.Instance.RealDelete(delosnode.OutStoreID);
            });
            return result;
        }

        //更新维修所用备件数量
        [HttpPost]
        public async Task<bool> UpdateRepairPlanSpareDevices(RepairPlanOutStoreNode sdchange, double oldsdnumber)
        {
            var change = sdchange.OutStore.SpareDevice.StoreNumber;
            var old = oldsdnumber;
            var Number = sdchange.Number;
            bool result = false;
            await Task.Run(() =>
            {
                RepairPlanOutStoreNodeBO.Instance.UpdateNumber(sdchange.ID, sdchange.Number);
                OutStoreBO.Instance.UpdateNumber(sdchange.OutStoreID, sdchange.Number);
                SpareDeviceBO.Instance.UpdateSpareDeviceStoreNumber(sdchange.OutStore.SpareID, sdchange.OutStore.SpareDevice.StoreNumber + oldsdnumber - sdchange.Number);
            });
            return result;
        }

        //更新维修工作人员的用时
        [HttpPost]
        public async Task<bool> UpdateRepairPlanUsers(RepairPlanUserNode uuchange)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = RepairPlanUserNodeBO.Instance.UpdateNumber(uuchange.ID, uuchange.UseTime);
            });
            return result;
        }
        //保存检修标准
        public async Task<bool> UpdatePlanContent(RepairPlanRefPlanContext uuchange)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = RepairPlanRefPlanContextBO.Instance.UpdateNumber(uuchange);
            });
            return result;
        }
        //保存检修花费
        public async Task<bool> UpdateRepairPlanCost(string drid, double uucost, double sdcost)
        {
            bool result = false;
            uucost = Math.Round(uucost, 1);
            sdcost = Math.Round(sdcost, 1);
            await Task.Run(() =>
            {
                result = ExecutionPlanBO.Instance.UpdateCost(drid, uucost, sdcost);
            });
            return result;
        }
        public double GetExecutionPlanMaxTime(string epid)
        {
            var maxtime = RepairPlanUserNodeBO.Instance.GetMaxTimeByEPID(epid);
            return maxtime * 60;
        }

        #endregion

        #region 设备维修（...）


        #region CallRepair(报修)

        public ActionResult CallRepair()
        {
            return View();
        }

        //报修页面加载的表集合，加载(0：已报修 1：处理中  4：暂不处理 )
        [HttpPost]
        public JsonResult CallRepairList()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            var userid = Loginer.CurrentUser.ID;
            List<DeviceRepair> drdr = new List<DeviceRepair>();
            string condition = "dr.State in (0,1,4,7)";
            string condition1 = string.Empty;
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            if (temp != null || temp1 != null)
            {
                condition1 = string.Format("  de.FactoryID={0}", factoryid);
            }
            else
            {
                var devicemark = GetMarkByWhatAdmin();
                if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                {
                    if (devicemark == "('计检设备')")
                    {
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                    }
                    else
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                }
                else if (temp2 != null)
                {
                    condition1 = string.Format(" (dr.TroubleUserID={0} or dr.RepairUserID={1})", userid, userid);
                }
                else
                {
                    condition1 = string.Format(" dr.TroubleUserID={0}", userid);
                }
            }
            if (!string.IsNullOrEmpty(condition1))
            { condition = condition + " and " + condition1; }
            drdr = DeviceRepairBO.Instance.GetListItems(condition);
            if (drdr.Count > 0)
            {
                string deviceids = "";
                foreach (var Item in drdr)
                {
                    deviceids += Item.DeviceID + ",";
                }
                deviceids = deviceids.Substring(0, deviceids.Length - 1);
                string condition5 = string.Format("SafeguardID = 1 and de.DeviceID in ({0})", deviceids);
                List<DeviceSafeguard> d = DeviceSafeguardBO.Instance.GetListItems(condition5);
                foreach (var a in drdr)
                {
                    if (a.State == 0 || a.State == 1)
                    {
                        DeviceSafeguard f = d.FirstOrDefault(x => x.DeviceID == a.DeviceID);
                        if (f != null)
                        {
                            a.TheTime = a.TroubleTime.AddMinutes(f.Time);
                        }
                        else
                        {
                            if (a.Device.TheTime != 0)
                                a.TheTime = a.TroubleTime.AddMinutes(a.Device.TheTime);
                        }
                    }
                }
            }
            int total = drdr.Count;
            var rows = drdr.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //新增报修单
        [HttpPost]
        public async Task<bool> InsertCallRepair(DeviceRepair dr)
        {
            bool result = false;
            string code = Loginer.CurrentUser.Department.Factory.Code;//工厂代码
            dr.TroubleUserID = Loginer.CurrentUser.ID;
            //报修时将设备状态标记为故障
            Device d = DeviceBO.Instance.GetDeviceByID(dr.DeviceID);
            d.UseState = dr.DeviceState;
            DeviceBO.Instance.UpdateDevice(d);
            await Task.Run(() =>
            {
                dr.TroubleTime = DateTime.Now;
                dr.UserCost = 0;
                dr.SpareDeviceCost = 0;
                dr.OffTime = 0;
                string id;
                result = DeviceRepairBO.Instance.Insert(dr, out id, code);
                if (result == true)
                {
                    HelperExtensions.PushMessage(dr.RepairUserID, id);//透传发报修消息
                }
            });
            return result;
        }
        [HttpPost]
        public async Task<bool> CallRepairEdit(DeviceRepair dr)
        {
            dr.TroubleUserID = Loginer.CurrentUser.ID;
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceRepairBO.Instance.UpdateCallRepair(dr);
            });
            return result;
        }

        //删除报修单
        [HttpPost]
        public async Task<bool> CallRepairDelete(DeviceRepair dr)
        {
            bool result = false;
            await Task.Run(() =>
            {
                string condition1 = string.Format(" du.DeviceRepairID={0} ", dr.ID);
                List<DeviceRepairRefUser> list1 = DeviceRepairRefUserBO.Instance.GetItemList(condition1);
                if (list1.Count > 0)
                {
                    DeviceRepairRefUserBO.Instance.DeleteUser(dr.ID);
                }
                string condition2 = string.Format(" du.DeviceRepairID={0} ", dr.ID);
                List<DeviceRepairRefOutStore> list2 = DeviceRepairRefOutStoreBO.Instance.GetItems(condition2);
                if (list2.Count > 0)
                {
                    DeviceRepairRefOutStoreBO.Instance.DeleteSpareDevice(dr.ID);
                }
                List<RepairRefeguard> list3 = RepairRefeguardBO.Instance.GetItems(dr.ID);
                if (list3.Count > 0)
                {
                    RepairRefeguardBO.Instance.DeleteRepairRefeguard(dr.ID);
                }
                result = DeviceRepairBO.Instance.Delete(dr.ID);
            });
            return result;
        }

        //删除正在做的报修单
        [HttpPost]
        public async Task<bool> RepairINGDelete(string id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                string condition1 = string.Format(" du.DeviceRepairID={0} ", id);
                List<DeviceRepairRefUser> list1 = DeviceRepairRefUserBO.Instance.GetItemList(condition1);
                if (list1.Count > 0)
                {
                    DeviceRepairRefUserBO.Instance.DeleteUser(id);
                }
                string condition2 = string.Format(" du.DeviceRepairID={0} ", id);
                List<DeviceRepairRefOutStore> list2 = DeviceRepairRefOutStoreBO.Instance.GetItems(condition2);
                if (list2.Count > 0)
                {
                    DeviceRepairRefOutStoreBO.Instance.DeleteSpareDevice(id);
                }
                List<RepairRefeguard> list3 = RepairRefeguardBO.Instance.GetItems(id);
                if (list3.Count > 0)
                {
                    RepairRefeguardBO.Instance.DeleteRepairRefeguard(id);
                }
                result = DeviceRepairBO.Instance.Delete(id);
            });
            return result;
        }
        #endregion


        #region GetCallRepair（收到报修单）

        public ActionResult GetCallRepair()
        {
            return View();
        }
        //收到报修单列表
        [HttpPost]
        public JsonResult GetCallRepairList()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            var userid = Loginer.CurrentUser.ID;
            List<DeviceRepair> drdr = new List<DeviceRepair>();
            string condition = " dr.State =0";
            string condition1 = string.Empty;
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            if (temp != null || temp1 != null)
            {
                condition1 = string.Format("  de.FactoryID={0}", factoryid);
            }
            else
            {
                var devicemark = GetMarkByWhatAdmin();
                if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                {
                    if (devicemark == "('计检设备')")
                    {
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                    }
                    else
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                }
                else if (temp2 != null)
                {
                    condition1 = string.Format("  (dr.TroubleUserID={0} or dr.RepairUserID={1})", userid, userid);
                }
                else
                {

                    condition1 = string.Format("  dr.TroubleUserID={0}", userid);
                }
            }
            if (!string.IsNullOrEmpty(condition1))
            { condition += " and " + condition1; }
            drdr = DeviceRepairBO.Instance.GetListItems(condition);
            if (drdr.Count > 0)
            {
                string deviceids = "";
                foreach (var Item in drdr)
                {
                    deviceids += Item.DeviceID + ",";
                }
                deviceids = deviceids.Substring(0, deviceids.Length - 1);
                string condition5 = string.Format("SafeguardID = 1 and de.DeviceID in ({0})", deviceids);
                List<DeviceSafeguard> d = DeviceSafeguardBO.Instance.GetListItems(condition5);
                foreach (var a in drdr)
                {
                    if (a.State == 0 || a.State == 1)
                    {
                        DeviceSafeguard f = d.FirstOrDefault(x => x.DeviceID == a.DeviceID);
                        if (f != null)
                        {
                            a.TheTime = a.TroubleTime.AddMinutes(f.Time);
                        }
                        else
                        {
                            if (a.Device.TheTime != 0)
                                a.TheTime = a.TroubleTime.AddMinutes(a.Device.TheTime);
                        }
                    }
                }
            }
            int total = drdr.Count;
            var rows = drdr.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //已报修->处理中
        [HttpPost]
        public async Task<bool> CallRepairToDoRepair(DeviceRepair dr)
        {
            bool result = false;
            dr.State = 1;
            dr.StartTime = DateTime.Now;
            await Task.Run(() =>
            {
                result = DeviceRepairBO.Instance.UpdateState(dr);
            });
            return result;
        }

        //已报修->暂不处理
        [HttpPost]
        public async Task<bool> CallRepairToNoRepair(DeviceRepair dr)
        {
            bool result = false;
            dr.State = 4;
            await Task.Run(() =>
            {
                result = DeviceRepairBO.Instance.UpdateWaitState(dr);
            });
            return result;
        }

        #endregion


        #region WaitToRepair（暂不处理）

        public ActionResult WaitToRepair()
        {
            return View();
        }
        [HttpPost, ActionName("WaitToRepair")]
        public JsonResult WaitToRepairPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            var userid = Loginer.CurrentUser.ID;
            List<DeviceRepair> u = new List<DeviceRepair>();
            string condition = " dr.State=4";
            string condition1 = string.Empty;
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            if (temp != null || temp1 != null)
            {
                condition1 = string.Format(" de.FactoryID={0}", factoryid);
            }
            else
            {
                var devicemark = GetMarkByWhatAdmin();
                if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                {
                    if (devicemark == "('计检设备')")
                    {
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                    }
                    else
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                }
                else if (temp2 != null)
                {
                    condition1 = string.Format("  (dr.TroubleUserID={0} or dr.RepairUserID={1})", userid, userid);
                }
                else
                {
                    condition1 = string.Format("  dr.TroubleUserID={0}", userid);
                }
            }
            if (!string.IsNullOrEmpty(condition1))
            { condition = condition + " and " + condition1; }
            u = DeviceRepairBO.Instance.GetListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }
        #endregion


        #region RepairING（处理中）

        public ActionResult RepairING()
        {
            return View();
        }

        //处理中加载的维修单集合，加载(1：处理中 2：处理完成  5：已驳回)
        [HttpPost, ActionName("RepairING")]
        public JsonResult RepairINGPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            var userid = Loginer.CurrentUser.ID;
            List<DeviceRepair> drdr = new List<DeviceRepair>();
            string condition = " dr.State in (1,2,5)";
            string condition1 = string.Empty;
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            if (temp != null || temp1 != null)
            {
                condition1 = string.Format("  de.FactoryID={0}", factoryid);
            }
            else
            {
                var devicemark = GetMarkByWhatAdmin();
                if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                {
                    if (devicemark == "('计检设备')")
                    {
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                    }
                    else
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                }
                else if (temp2 != null)
                {
                    condition1 = string.Format("  (dr.TroubleUserID={0} or dr.RepairUserID={1})", userid, userid);
                }
                else
                {
                    condition1 = string.Format("  dr.TroubleUserID={0}", userid);
                }
            }
            if (!string.IsNullOrEmpty(condition1))
            { condition = condition + " and " + condition1; }
            drdr = DeviceRepairBO.Instance.GetListItemsDetail(condition);
            int total = drdr.Count;
            var rows = drdr.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //添加维修工作人员
        [HttpPost]
        public async Task<bool> AddRepairUsers(List<User> userlist, string drid)
        {
            bool result = false;
            await Task.Run(() =>
            {
                List<DeviceRepairRefUser> runlist = DeviceRepairRefUserBO.Instance.GetListByDeviceRepairIDForInsert(drid); //如果已经增加过，先获取出来
                foreach (var item in runlist)                                                               //把相同的剔除
                {
                    for (int n = 0; n < userlist.Count; n++)
                    {
                        if (item.UserID == userlist[n].ID)
                            userlist.Remove(userlist[n]);
                    }
                }
                result = DeviceRepairRefUserBO.Instance.Inserts(drid, userlist);
            });
            return result;
        }

        //删除维修人员
        [HttpPost]
        public async Task<bool> DeleteRepairUsers(DeviceRepairRefUser delrunode, string drid)
        {
            bool result = true;
            await Task.Run(() =>
            {
                DeviceRepair dr = DeviceRepairBO.Instance.GetItemByID(drid);
                DeviceRepairBO.Instance.UpdateCost(drid, dr.UserCost - delrunode.UseTime * delrunode.HourWage, dr.SpareDeviceCost, dr.OutCost);
                DeviceRepairRefUserBO.Instance.Delete(delrunode.ID);
            });
            return result;
        }


        //添加维修所用备件
        [HttpPost]
        public async Task<bool> AddRepairSpareDevices(List<SpareDevice> sdlist, string drid)
        {
            bool result = false;
            int unid = Loginer.CurrentUser.ID;
            await Task.Run(() =>
            {
                List<DeviceRepairRefOutStore> runlist = DeviceRepairRefOutStoreBO.Instance.GetListByDeviceRepairID(drid); //如果已经增加过，先获取出来
                foreach (var item in runlist)                                                               //把相同的剔除
                {
                    for (int n = 0; n < sdlist.Count; n++)
                    {
                        if (item.OutStore.SpareID == sdlist[n].ID)
                            sdlist.Remove(sdlist[n]);
                    }
                }
                List<OutStore> oslist = new List<OutStore>();
                foreach (var item in sdlist)            //创建出库单
                {
                    OutStore osTemp = new OutStore();
                    osTemp.OutNumber = 0;
                    osTemp.HandlerUserID = osTemp.UserID = unid;
                    osTemp.SpareID = item.ID;
                    osTemp.SpareDevice = item;
                    osTemp.IsDelete = false;
                    osTemp.OutTime = DateTime.Now;
                    osTemp.State = "已领取";
                    osTemp.Content = string.Format("维修单号：{0}", drid);
                    int id = 0;
                    OutStoreBO.Instance.Insert(osTemp, out id);
                    osTemp.ID = id;
                    oslist.Add(osTemp);
                }
                result = DeviceRepairRefOutStoreBO.Instance.Inserts(drid, oslist);
            });

            return result;
        }

        //删除维修所用备件
        [HttpPost]
        public async Task<bool> DeleteRepairSpareDevices(DeviceRepairRefOutStore delosnode, string drid)
        {
            bool result = true;
            await Task.Run(() =>
            {
                DeviceRepair dr = DeviceRepairBO.Instance.GetItemByID(drid);
                DeviceRepairBO.Instance.UpdateCost(drid, dr.UserCost, dr.SpareDeviceCost - delosnode.Number * delosnode.Price, dr.OutCost);
                SpareDeviceBO.Instance.UpdateSpareDeviceStoreNumber(delosnode.OutStore.SpareID, delosnode.OutStore.SpareDevice.StoreNumber + delosnode.OutStore.OutNumber);
                DeviceRepairRefOutStoreBO.Instance.Delete(delosnode.ID);
                OutStoreBO.Instance.RealDelete(delosnode.OutStoreID);
            });
            return result;
        }

        //获取维修工作人员结点列表
        [HttpPost]
        public async Task<JsonResult> GetRepairUsers(string drid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<DeviceRepairRefUser> u = new List<DeviceRepairRefUser>();
            await Task.Run(() =>
            {
                u = DeviceRepairRefUserBO.Instance.GetListByDeviceRepairID(drid);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //获取维修所用备件
        [HttpPost]
        public async Task<JsonResult> GetRepairSpareDevices(string drid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<DeviceRepairRefOutStore> u = new List<DeviceRepairRefOutStore>();
            await Task.Run(() =>
            {
                u = DeviceRepairRefOutStoreBO.Instance.GetListByDeviceRepairID(drid);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //处理完成（维修内容）
        [HttpPost]
        public async Task<string> CallRepairToDoneRepair(DeviceRepair dr)
        {
            int userid = Loginer.CurrentUser.ID;
            dr.RepairUserID = userid;
            bool result = false;
            dr.State = 2;
            string msg = string.Empty;
            //检测
            double minute = ((DateTime)dr.CompleteTime - (DateTime)dr.StartTime).TotalMinutes;
            if (minute < 1)
            {
                msg = "开始时间不能大于结束时间！";
                return msg;
            }
            //检测员工工作时间
            var u = DeviceRepairRefUserBO.Instance.GetListByDeviceRepairID(dr.ID);
            if (u != null)
            {
                foreach (var item in u)
                {
                    bool check = minute + 30 - item.UseTime * 60 >= 0;
                    if (check) { continue; }
                    return string.Format("员工【{0}】耗时大于整个维修时间", item.User.RealName);
                }
            }
            DeviceRepair r = DeviceRepairBO.Instance.GetItemByID(dr.ID);
            if (r.DeviceState.Equals("停机待修") || r.DeviceState.Equals("停机") || r.DeviceState.Equals("故障停机"))
            {
                if (dr.OffTime == 0)
                {
                    DateTime EndTime = (DateTime)dr.CompleteTime;
                    DateTime StartTime = (DateTime)r.TroubleTime;
                    TimeSpan tsStart = new TimeSpan(StartTime.Ticks);
                    TimeSpan tsEnd = new TimeSpan(EndTime.Ticks);
                    TimeSpan ts = tsEnd.Subtract(tsStart).Duration();
                    double dateDiffHours = ts.Days * 24 + ts.Hours;
                    if (ts.Minutes > 30) dateDiffHours += 1;
                    else if (ts.Minutes < 30 && ts.Minutes > 0 || ts.Minutes == 30) dateDiffHours += 0.5;
                    dr.OffTime = dateDiffHours;
                }
            }
            await Task.Run(() =>
            {
                result = DeviceRepairBO.Instance.UpdateRepair(dr);
            });
            msg = result ? "True" : "保存失败！";
            if (result)
            {
                HelperExtensions.PushMessage(r.TroubleUserID, dr.ID);//透传发维修待确认消息
                DeviceRepair e = DeviceRepairBO.Instance.GetItemByID(dr.ID);
                result = DeviceRefSpareDeviceBO.Instance.AddbyDeviceRepairorRepairPlan(e.ID, e.DeviceID, 0);
                if (!result) { msg = "保存关联设备失败"; }
            }
            return msg;
        }

        //更新维修所用备件数量
        [HttpPost]
        public async Task<bool> UpdateRepairSpareDevices(DeviceRepairRefOutStore sdchange, double oldsdnumber)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceRepairRefOutStoreBO.Instance.UpdateNumber(sdchange.ID, sdchange.Number);
                OutStoreBO.Instance.UpdateNumber(sdchange.OutStoreID, sdchange.Number);
                SpareDeviceBO.Instance.UpdateSpareDeviceStoreNumber(sdchange.OutStore.SpareID, sdchange.OutStore.SpareDevice.StoreNumber + oldsdnumber - sdchange.Number);
            });
            return result;
        }

        //更新维修工作人员的用时
        [HttpPost]
        public async Task<bool> UpdateRepairUsers(DeviceRepairRefUser uuchange)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceRepairRefUserBO.Instance.UpdateUseTime(uuchange.ID, uuchange.UseTime);
            });
            return result;
        }

        public async Task<bool> UpdateCost(string drid, double uucost, double sdcost, double outcost)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceRepairBO.Instance.UpdateCost(drid, uucost, sdcost, outcost);
            });
            return result;
        }

        #endregion


        #region DeviceRepairRecord（维修记录）

        public ActionResult DeviceRepairRecord()
        {
            return View();
        }

        //维修记录列表（3：已确认）
        [HttpPost, ActionName("DeviceRepairRecord")]
        public JsonResult DeviceRepairRecordPost(string drid, string repairuserid, string deviceid, string did, string devicename, string btime, string etime, string departmentid, string factid, string type, string mark, string endtime1, string endtime2)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<DeviceRepair> u = new List<DeviceRepair>();
            string condition = " dr.State=3";
            string condition1 = "";
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            int factoryid = Loginer.CurrentUser.Department.FactoryID;
            var departid = Loginer.DepartIDByUser(Loginer.CurrentUser);
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                    condition += " and de.FactoryID is not null ";
                else
                    if (temp != null || temp1 != null || temp2 != null)
                        condition += string.Format(" and de.FactoryID=" + factid);
                    else
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                        {
                            if (devicemark == "('计检设备')")
                                condition += " and de.FactoryID=" + factid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                            else
                                condition += " and de.FactoryID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                        }
                        else
                        {
                            int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                            List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                            string cond = string.Empty;
                            foreach (Department item in listdepart)
                            {
                                cond += item.ID + ",";
                                if (item.Children == null) { continue; }
                                foreach (var child in item.Children)
                                {
                                    cond += child.ID + ",";
                                }
                            }
                            cond = cond.Substring(0, cond.Length - 1);
                            condition += string.Format(" and de.ID in ({0})", cond);
                        }
                    }
            }
            else //若没有查询条件则走这里
            {
                if (temp != null || temp1 != null || temp2 != null)
                    condition1 = string.Format("  f.ID={0}", factoryid);
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                        else
                            condition1 = " de.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition1 = string.Format(" de.ID in ({0})", cond);
                    }
                }
            }
            if (!string.IsNullOrEmpty(drid))
            {
                condition += string.Format(" and dr.ID like '%{0}%' ", drid);
            }
            if (!string.IsNullOrEmpty(type))
            {
                condition += string.Format(" and dt.Name like '%{0}%' ", type);
            }
            if (!string.IsNullOrEmpty(mark))
            {
                condition += string.Format(" and m.Name like '%{0}%' ", mark);
            }
            if (!string.IsNullOrEmpty(departmentid) && (temp != null || temp1 != null || temp2 != null
                || temp3 != null || temp4 != null || temp5 != null || temp6 != null))
            {
                condition += string.Format(" and de.ID=" + departmentid);
            }
            if (!string.IsNullOrEmpty(repairuserid))
            {
                condition += string.Format(" and dr.RepairUserID={0} ", repairuserid);
            }
            if (!string.IsNullOrEmpty(deviceid))
            {
                condition += string.Format(" and d.ID='{0}' ", deviceid);
            }
            if (!string.IsNullOrEmpty(did))
            {
                condition += string.Format(" and dr.DeviceID  like '%{0}%' ", did);
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and d.DeviceName like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(btime) && !string.IsNullOrEmpty(etime))
            {
                DateTime bdt = Convert.ToDateTime(btime + " 00:00:00");
                DateTime edt = Convert.ToDateTime(etime + " 23:59:59");
                condition += string.Format(" and dr.CompleteTime between '{0}' and '{1}'", bdt, edt);
            }
            else if (!string.IsNullOrEmpty(endtime1) && !string.IsNullOrEmpty(endtime2))
            {
                DateTime bdt = Convert.ToDateTime(endtime1 + " 00:00:00");
                DateTime edt = Convert.ToDateTime(endtime2 + " 23:59:59");
                condition += string.Format(" and dr.TroubleTime between '{0}' and '{1}'", bdt, edt);
            }
            else //没有时间查询
            {
                btime = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                etime = DateTime.Now.ToString("yyyy-MM-dd");
                DateTime bdt = Convert.ToDateTime(btime + " 00:00:00");
                DateTime edt = Convert.ToDateTime(etime + " 23:59:59");
                condition += string.Format(" and dr.CompleteTime between '{0}' and '{1}'", bdt, edt);
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                condition = condition + " and " + condition1;
            }
            u = DeviceRepairBO.Instance.GetListItemsDetailForEnd(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        public ActionResult OneDeviceRepairRecord()
        {
            return View();
        }

        //报修经验库专用维修记录列表
        [HttpPost, ActionName("OneDeviceRepairRecord")]
        public JsonResult OneDeviceRepairRecordPost(string troubleuser, string repairuser, string deviceid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<DeviceRepair> u = new List<DeviceRepair>();
            string condition = "dr.State=3";
            if (!string.IsNullOrEmpty(deviceid))
            {
                condition += string.Format(" and dr.DeviceID='{0}'", deviceid);
            }
            if (!string.IsNullOrEmpty(troubleuser))
            {
                condition += string.Format(" and tu.RealName like '%{0}%' ", troubleuser);
            }
            if (!string.IsNullOrEmpty(repairuser))
            {
                condition += string.Format(" and ru.RealName like '%{0}%' ", repairuser);
            }
            u = DeviceRepairBO.Instance.GetListItemsDetailForEnd(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        public ActionResult OneDeviceRepairRecordList()
        {
            return View();
        }

        //设备界面查看维修记录
        [HttpPost, ActionName("OneDeviceRepairRecordList")]
        public JsonResult OneDeviceRepairRecordListPost(string deviceid, string btime, string etime, string wxstate)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<DeviceRepair> u = new List<DeviceRepair>();
            string condition = string.Empty;
            //0：已报修 1：处理中 2：处理完成 3：确认完成 4：暂不处理 5：电脑端已驳回6：处理的废单子7：手机端已驳回
            if (!string.IsNullOrEmpty(wxstate))
            {
                if (wxstate == "-1") wxstate = " in(0,1,2,3,4,5,7)";
                else if (wxstate == "0") wxstate = " =0";
                else if (wxstate == "1") wxstate = " =1";
                else if (wxstate == "2") wxstate = " =2";
                else if (wxstate == "3") wxstate = " =4";
                else if (wxstate == "4") wxstate = " in(5,7)";
                else if (wxstate == "5") wxstate = " =3";
                condition = string.Format(" e.State {0} ", wxstate);
            }
            if (!string.IsNullOrEmpty(deviceid))
            {
                condition += string.Format(" and de.ID='{0}'", deviceid);
            }
            if (!string.IsNullOrEmpty(btime) && !string.IsNullOrEmpty(etime))
            {
                condition += string.Format(" e.CompleteTime between '{0}' and '{1}' ", btime, etime);
            }
            u = DeviceRepairBO.Instance.GetOneRecordList(condition, deviceid);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> ClearRepairWordPath(string repairid)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceRepairBO.Instance.UpdateWordPath(repairid, "NULL");
            });
            return result;
        }

        [HttpPost]
        public void DropRepairWord(string repairid)
        {
            string wordpath = DeviceRepairBO.Instance.GetWordPathByID(repairid);
            var listpath = wordpath.Split(';');
            for (int i = 0; i < listpath.Length; i++)
            {
                if (listpath[i].Length == 0)
                {
                    continue;
                }
                var item = listpath[i].Substring(0, listpath[i].Length - 1);
                string deletepath = Server.MapPath(@item);
                List<string> tsArrayy = new List<string> { "+", "?", "#", "&", "=", " " };
                List<string> tdArrayy = new List<string> { "%2B", "%3F", "%23", "%26", "%3D", "%20" };
                for (int j = 0; j < tsArrayy.Count; j++)
                {
                    deletepath = deletepath.Replace(tdArrayy[j], tsArrayy[j]);//特殊符号替换路径
                }
                if (System.IO.File.Exists(deletepath))
                {
                    System.IO.File.Delete(deletepath);
                }
            }
        }

        #endregion


        #region ConfirmToDone（报修人确认完成）

        public ActionResult ConfirmToDone()
        {
            return View();
        }

        //确认完成操作
        [HttpPost, ActionName("ConfirmToDone")]
        public JsonResult ConfirmToDonePost(string repairid, string remork, int start)
        {
            try
            {
                //报修时将设备状态标记为完好
                DeviceRepair dd = DeviceRepairBO.Instance.GetItemByID(repairid);
                Device d = DeviceBO.Instance.GetDeviceByID(dd.DeviceID);
                d.UseState = "完好";
                DeviceBO.Instance.UpdateDevice(d);
                bool result = false;
                DeviceRepair dr = new DeviceRepair { ID = repairid, Appraisal = remork, Score = start };
                dr.State = (int)RepairState.确认完成;
                result = DeviceRepairBO.Instance.UpdateForConfirm(dr);
                string msg = result ? "确认成功！" : "确认失败！";
                return Json(new JsonData { Msg = msg, Result = result });
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex);
                return Json(new JsonData { Msg = ex.Message, Result = false });
            }
        }

        public JsonResult RepairDoneList()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            var departid = Loginer.DepartIDByUser(Loginer.CurrentUser);
            var userid = Loginer.CurrentUser.ID;
            List<DeviceRepair> u = new List<DeviceRepair>();
            string condition = "dr.State in (2,5)";
            string condition1 = string.Empty;
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            if (temp != null || temp1 != null)
            {
                condition1 = string.Format("  de.FactoryID={0}", factoryid);
            }
            else
            {
                var devicemark = GetMarkByWhatAdmin();
                if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                {
                    if (devicemark == "('计检设备')")
                    {
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                    }
                    else
                        condition1 = " de.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                }
                else
                {

                    condition1 = string.Format("  (dr.TroubleUserID={0} or de.ID={1}) ", userid, departid);
                }
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                condition = condition + " and " + condition1;
            }
            u = DeviceRepairBO.Instance.GetListItemsDetailForEnd(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //驳回操作
        [HttpPost]
        public async Task<bool> DoneRepairToReject(DeviceRepair dr)
        {
            bool result = false;
            dr.State = 5;
            await Task.Run(() =>
            {
                result = DeviceRepairBO.Instance.UpdateForConfirm(dr);
            });
            return result;
        }

        #endregion

        #endregion

        #region 设置保养与保养项目

        public async Task<bool> InsertMaintenancePlan(MaintenancePlan u)
        {
            bool result = false;
            //每个保养文件的每条内容
            await Task.Run(() =>
            {
                result = MaintenancePlanBO.Instance.InsertMaintenancePlan(u);
            });
            return result;
        }

        //更新每条内容
        [HttpPost]
        public async Task<bool> UpdateMaintenancePlan(MaintenancePlanRefPlanContext uuchange)
        {
            var byid = uuchange.ExecutionDeviceMaintenancePlanID;
            ExecutionDeviceMaintenancePlan ex = ExecutionDeviceMaintenancePlanBO.Instance.GetItemByID(byid);
            if (string.IsNullOrEmpty(ex.StartTime.ToString()))
            {
                var nowtime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                ExecutionDeviceMaintenancePlanBO.Instance.UpdateSartTime(ex, nowtime);//更新开始时间
            }
            bool result = false;//新增           
            await Task.Run(() =>
            {
                result = MaintenancePlanRefPlanContextBO.Instance.UpdateNumber(uuchange);
            });
            return result;
        }

        //查看详细时不修改内容
        [HttpPost]
        public async Task<bool> UpdateMaintenancePlanRecord(MaintenancePlanRefPlanContext uuchange)
        {
            bool result = false;//新增
            await Task.Run(() =>
            {
                result = MaintenancePlanRefPlanContextBO.Instance.NoUpdateNumber(uuchange);
            });
            return result;
        }

        public ActionResult MaintenancePlanList()
        {
            return View();
        }

        //获得MaintenancePlan列表
        [HttpPost, ActionName("MaintenancePlanList")]
        public JsonResult MaintenancePlanListPost(string maintenanceplanfilename, int importentpart, string factoryid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<MaintenancePlan> u = new List<MaintenancePlan>();
            string condition = string.Empty;
            if (factoryid.ToInt32() == 0 || string.IsNullOrEmpty(factoryid))
            {
                condition = " 1=1 ";
            }
            else
            {
                condition = string.Format(" f.FactoryID ={0} ", factoryid);
            }
            if (!string.IsNullOrEmpty(maintenanceplanfilename))
            {
                condition += string.Format(" and f.Name like '%{0}%' ", maintenanceplanfilename);
            }
            if (importentpart == 1)
            {
                condition += " and p.ImportentPart = 1";
            }
            u = MaintenancePlanBO.Instance.GetListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> MaintenancePlanEdit(MaintenancePlan u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = MaintenancePlanBO.Instance.UpdateMaintenancePlan(u);
            });
            return result;
        }

        [HttpPost, ActionName("MaintenancePlanDelete")]
        public async Task<bool> MaintenancePlanDelete(int id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = MaintenancePlanBO.Instance.DeleteMaintenancePlanByID(id);
            });
            return result;
        }

        //设置设备保养计划
        public ActionResult SetMaintenancePlanList()
        {
            return View();
        }
        // //加载保养excel文件列表
        public ActionResult MaintenancePlanFileNameList()
        {
            return View();
        }

        [HttpPost, ActionName("MaintenancePlanFileNameList")]

        public JsonResult MaintenancePlanFileNameListPost(string devicename, string assetnumber)
        {
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            string condition = string.Format(" FactoryID={0} ", factoryid);
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<MaintenancePlanFileName> u = new List<MaintenancePlanFileName>();
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and Name like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(assetnumber))
            {
                condition += string.Format(" and Name like '%{0}%' ", assetnumber);
            }
            u = MaintenancePlanFileNameBO.Instance.GetListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        public ActionResult DeviceRefMaintenancePlan()
        {
            return View();
        }

        [HttpPost]
        public JsonResult DeviceRefMaintenancePlanList(string devicename, string deviceid, int importentpart, string departmentid, string factid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            string condition = " 1=1 ";
            string condition1 = "";
            var factoryid = Loginer.CurrentUser.Department.FactoryID;
            var dapartid = Loginer.DepartIDByUser(Loginer.CurrentUser);
            List<DeviceRefMaintenancePlan> drdr = new List<DeviceRefMaintenancePlan>();
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                    condition = " f.ID is not null ";
                else
                {
                    if (temp != null || temp1 != null) { condition = " f.ID=" + factid; }
                    else
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                        {
                            if (devicemark == "('计检设备')")
                                condition = " f.ID=" + factid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                            else
                                condition = " f.ID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                        }
                        else
                        {
                            int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                            List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                            string cond = string.Empty;
                            foreach (Department item in listdepart)
                            {
                                cond += item.ID + ",";
                                if (item.Children == null) { continue; }
                                foreach (var child in item.Children)
                                {
                                    cond += child.ID + ",";
                                }
                            }
                            cond = cond.Substring(0, cond.Length - 1);
                            condition = string.Format(" h.ID  in ({0})", cond);
                        }
                    }
                }
            }
            else
            {
                if (temp != null || temp1 != null)
                    condition1 = " f.ID=" + factoryid;
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition1 = " f.ID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                        else
                            condition1 = " f.ID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition1 = string.Format(" h.ID in ({0})", cond);
                    }
                }
            }
            if (!string.IsNullOrEmpty(deviceid))
            { condition += string.Format(" and d.ID like '%{0}%' ", deviceid); }
            if (!string.IsNullOrEmpty(departmentid))
            {
                if (temp != null || temp1 != null || temp2 != null || temp3 != null || temp4 != null || temp5 != null)
                {
                    condition += string.Format(" and h.ID = {0} ", departmentid);
                }
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and d.DeviceName like '%{0}%' ", devicename);
            }
            if (importentpart == 1)
            {
                condition += string.Format(" and p.ImportentPart = {0} ", importentpart);
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                if (!string.IsNullOrEmpty(condition))
                    condition = condition + " and " + condition1;
                else condition = condition1;
            }
            drdr = DeviceRefMaintenancePlanBO.Instance.GetListItems(condition);
            int total = drdr.Count;
            var rows = drdr.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        public async Task<bool> SetBYWeekTime(string BYWeekTime, string deviceids)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.UpdateBYWeekTime(items.ID, BYWeekTime);
                }
            });
            return result;
        }

        public async Task<bool> SetBYDayTime(double BYDayMinTime, double BYDayMaxTime, string deviceids)
        {
            //当设置区段的时候还需要把BYDaYIsOut字段变成1
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.UpdateBYDayTime(items.ID, BYDayMinTime, BYDayMaxTime);
                }
            });
            return result;
        }

        public async Task<bool> CloseBYDayTime(string deviceids)
        {
            //当关闭区段的时候需要把BYDayIsOpen字段变成0
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.CloseBYDayTime(items.ID);
                }
            });
            return result;
        }

        //超级管理员可以关闭所有保养有效区段
        public async Task<bool> CloseAllBYDayTime()
        {
            //当关闭区段的时候需要把BYDayIsOpen字段变成0
            bool result = false;
            var factoryid = Loginer.CurrentUser.Department.Factory.ID;
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            if (temp != null || temp1 != null)
            {
                await Task.Run(() =>
                {
                    result = DeviceBO.Instance.CloseAllBYDayTime(factoryid);
                });
            }
            return result;
        }

        #endregion

        #region 保养计划执行
        //增加计划执行记录，当设置完成后，
        [HttpPost]
        public async Task<bool> DeviceUpdateFileName1ID(string deviceids, int filenameid)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            var filename = filenameid;
            List<MaintenancePlan> maintenanceplanlist = MaintenancePlanBO.Instance.MaintenancePlanList(filename);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    MaintenancePlanFileName r = MaintenancePlanFileNameBO.Instance.MaintenancePlanFileNamebyID(filename);
                    if (items.FileName1ID == 0)//当初次设置保养计划时
                    {
                        result = DeviceBO.Instance.UpdateBYDeviceID(r.Name, r.ID, items.ID);
                        StringBuilder builder = new StringBuilder();
                        foreach (var item in maintenanceplanlist)//插入新的二维码
                        {
                            builder.Append(string.Format("insert into DeviceRefMaintenancePlan([DeviceID],[MaintenancePlanID]) values('{0}','{1}');", items.ID, item.ID));
                        }
                        DeviceRefPlanContextBO.Instance.InsertBySql(builder.ToString());
                    }
                    else//当修改保养计划时
                    {
                        List<MaintenancePlan> oldmaintenanceplanlist = MaintenancePlanBO.Instance.MaintenancePlanList(items.FileName1ID);
                        DeviceRefMaintenancePlanBO.Instance.Delete(items.ID, oldmaintenanceplanlist);//删除旧的二维码
                        StringBuilder builder = new StringBuilder();
                        foreach (var item in maintenanceplanlist)//插入新的二维码
                        {
                            builder.Append(string.Format("insert into DeviceRefMaintenancePlan([DeviceID],[MaintenancePlanID]) values('{0}','{1}');", items.ID, item.ID));
                        }
                        DeviceRefPlanContextBO.Instance.InsertBySql(builder.ToString());
                        result = DeviceBO.Instance.UpdateBYDeviceID(r.Name, r.ID, items.ID);
                    }
                }
            });
            return result;
        }

        public async Task<bool> AddExecutionDeviceMaintenancePlan(string deviceids)
        {
            var day = DateTime.Now.Day;//几号
            var week = DateTime.Now.DayOfWeek.ToString();//Friday代表周五  
            //var factorycode = Loginer.CurrentUser.Department.Factory.Code;
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            var factorycode = string.Empty;//工厂代码
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    if (items.FileName1ID == 0)
                    {
                        return;
                    }
                    factorycode = DepartmentBO.Instance.GetFactoryByDepartmentID(items.Department.ID).Code;
                    string condition = string.Format(" DeviceID ='{0}' and ID like '{1}%'", items.ID, factorycode.Substring(0,1));
                    string condition1 = string.Format(" DeviceID ='{0}' ", items.ID);
                    string ExeID = string.Empty;
                    string maxid = ExecutionDeviceMaintenancePlanBO.Instance.GetMaxIDByDeviceID(condition);
                    string maxid1 = ExecutionDeviceMaintenancePlanBO.Instance.GetMaxIDByDeviceID(condition1);                
                    var ymday = System.DateTime.Now.ToString("yy-MM-dd").Split('-');
                    if (!string.IsNullOrEmpty(maxid1) && maxid1.Substring(2, 6) == (ymday[0] + ymday[1] + ymday[2]) && maxid1.Substring(0, 1) != factorycode.Substring(0, 1))
                    {
                        MaintenancePlanRefPlanContextBO.Instance.DeleteMaintenancePlanRefPlanContext(maxid1);//把今天做了的或者未做的剔除
                        ExecutionDeviceMaintenancePlanBO.Instance.DeleteExecutionMaintenance(maxid1);
                    }
                    //如果单子是最新的则清除改变状态
                    if (!string.IsNullOrEmpty(maxid) && maxid.Substring(2, 6) == (ymday[0] + ymday[1] + ymday[2]))//有当日单子存在
                    {
                        List<ExecutionDeviceMaintenancePlan> list = ExecutionDeviceMaintenancePlanBO.Instance.GetListItems(string.Format("d.DeviceID='{0}' and d.state=0", items.ID));
                        foreach (var item in list)
                        {
                            if (item.ID.Substring(0, 1) == factorycode.Substring(0, 1))
                            {
                                ExeID = item.ID;
                                break;
                            }
                        }
                        list.ForEach(a =>
                        {
                            MaintenancePlanRefPlanContextBO.Instance.DeleteMaintenancePlanRefPlanContext(a.ID);//把今天做了的或者未做的剔除
                            ExecutionDeviceMaintenancePlanBO.Instance.DeleteExecutionMaintenance(a.ID);
                        });
                    }
                    else
                    {
                        ExeID = ExecutionDeviceMaintenancePlanBO.Instance.CreateDeviceMaintenanceID(factorycode);
                    }
                    ExecutionDeviceMaintenancePlan u = new ExecutionDeviceMaintenancePlan();
                    u.StartTime = null;
                    u.EndTime = null;
                    u.Describe = null;
                    u.State = 0;//默认未执行
                    u.ErrorAmount = null;
                    u.MaintenancePlanID = items.FileName1ID;
                    u.DeviceID = items.ID;
                    u.ID = factorycode.Substring(0, 1) + ExeID.Substring(1, 10);
                    result = ExecutionDeviceMaintenancePlanBO.Instance.InsertExecutionDeviceMaintenancePlan(u);
                    List<MaintenancePlan> maintenanceplanList = ExecutionDeviceMaintenancePlanBO.Instance.MaintenancePlanList(items.FileName1ID);
                    MaintenancePlanRefPlanContext runTemp = new MaintenancePlanRefPlanContext();
                    foreach (var item in maintenanceplanList)
                    {
                        runTemp.MaintenancePlanID = item.ID;
                        runTemp.Project = item.Project;
                        runTemp.Standard = item.Standard;
                        runTemp.TheContent = item.TheContent;
                        runTemp.ImportentPart = item.ImportentPart;
                        runTemp.Code = item.Code;
                        runTemp.Standard = item.Standard;
                        runTemp.DealWay = item.DealWay;
                        runTemp.Cycle = item.Cycle;
                        runTemp.ExecutionDeviceMaintenancePlanID = factorycode.Substring(0, 1) + ExeID.Substring(1, 10);
                        runTemp.IsHide = 0;
                        if ((item.Cycle == "每周一次" && week == "Friday") || (item.Cycle == "每月一次" && day == 15)
                        || (item.Cycle == "早" || item.Cycle == "中" || item.Cycle == "晚" || item.Cycle == "每日一次"))
                        {
                            runTemp.IsHide = 1;
                        }
                        MaintenancePlanRefPlanContextBO.Instance.FirstInsert(runTemp);
                    }
                    List<string> stringsql = new List<string>();
                    string updatesql1 = "";
                    var deviceid = items.ID;
                    var BYID = u.ID;
                    string sql0 = string.Format("  ExecutionDeviceMaintenancePlanID='{0}'", BYID);
                    string sql1 = string.Format("  FileName1ID =(select FileName1ID from Device where ID='{0}') and Cycle ='每周一次'", deviceid);
                    string sql2 = string.Format("  FileName1ID =(select FileName1ID from Device where ID='{0}') and Cycle ='每月一次'", deviceid);
                    string sql3 = string.Format("  FileName1ID =(select FileName1ID from Device where ID='{0}') and Cycle in('每周一次','每月一次')", deviceid);
                    List<MaintenancePlanRefPlanContext> b = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sql0);//得到保养项目ID
                    List<MaintenancePlan> c = MaintenancePlanBO.Instance.ChangeSpecial(sql1);
                    List<MaintenancePlan> d = MaintenancePlanBO.Instance.ChangeSpecial(sql2);
                    List<MaintenancePlan> e = MaintenancePlanBO.Instance.ChangeSpecial(sql3);
                    var countb = b.Count;//所有列表
                    var countc = c.Count;//周五列表
                    var countd = d.Count;//15号列表
                    var counte = e.Count;//周五和15号列表
                    //处理特殊全是周检月检的单子
                    if (countb == counte)
                    {
                        if (countb == countc && week != "Friday")//说明全是周检项，今天不是周五
                        {
                            u.State = (int)ExcuteBYState.计划已执行;
                            u.Describe = "本日无需保养，系统自动将其状态改变";
                            u.StartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                            u.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                            ExecutionDeviceMaintenancePlanBO.Instance.UpdateState(u);
                        }
                        else if (countb == countc && week == "Friday")//说明全是周检，今天是周五
                        {
                            updatesql1 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0} and  Cycle = '每周一次'", u.ID);
                            stringsql.Add(updatesql1);
                        }
                        else if (countb == countd && day != 15)//说明全是月检，但是今天不是15
                        {
                            u.State = (int)ExcuteBYState.计划已执行;
                            u.Describe = "本日无需保养，系统自动将其状态改变";
                            u.StartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                            u.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                            ExecutionDeviceMaintenancePlanBO.Instance.UpdateState(u);
                        }
                        else if (countb == countd && day == 15)//说明全是月检，今天是15
                        {
                            updatesql1 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0} and  Cycle = '每月一次'", u.ID);
                            stringsql.Add(updatesql1);
                        }
                        else if (countc != counte && countd != counte && day != 15 && week != "Friday")//说明全是周检,月检(2者都有)不是周五，15号
                        {
                            u.State = (int)ExcuteBYState.计划已执行;
                            u.Describe = "本日无需保养，系统自动将其状态改变";
                            u.StartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                            u.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                            ExecutionDeviceMaintenancePlanBO.Instance.UpdateState(u);
                        }
                        else if (countc != counte && countd != counte && day == 15 && week != "Friday")//说明全是周检,月检(2者都有)，15号
                        {
                            updatesql1 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0} and  Cycle in('每月一次')", u.ID);
                            stringsql.Add(updatesql1);
                        }
                        else if (countc != counte && countd != counte && day != 15 && week == "Friday")//说明全是周检,月检(2者都有)，是周五
                        {
                            updatesql1 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0} and  Cycle in( '每周一次')", u.ID);
                            stringsql.Add(updatesql1);
                        }
                        else if (countc != counte && countd != counte && day == 15 && week == "Friday")//说明全是周检,月检(2者都有)，是周五，15号
                        {
                            updatesql1 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0} and  Cycle in( '每周一次','每月一次')", u.ID);
                            stringsql.Add(updatesql1);
                        }
                    }
                }
            });
            return result;
        }

        //删除保养设置
        [HttpPost]
        public async Task<bool> DeleteDeviceMaintenancePlan(string deviceids)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            string ExMPID = Loginer.CurrentUser.Department.Factory.Code.Substring(0, 1) + 2 + DateTime.Now.ToString("yyMMdd");
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    ExecutionDeviceMaintenancePlan Exdmp = ExecutionDeviceMaintenancePlanBO.Instance.GetItemBySql(string.Format(" d.ID='{0}' and dr.ID like '{1}%'", items.ID, ExMPID));
                    if (Exdmp != null)
                    {
                        MaintenancePlanRefPlanContextBO.Instance.DeleteMaintenancePlanRefPlanContext(Exdmp.ID);//把今天做了的或者未做的剔除
                        ExecutionDeviceMaintenancePlanBO.Instance.DeleteExecutionMaintenance(Exdmp.ID);
                    }
                    DeviceBO.Instance.DeleteBYDeviceID(items.ID);//把设备的几个字段干掉
                    DeviceRefMaintenancePlanBO.Instance.DeleteDeviceRefMaintenancePlan(items.ID);//二维码删除
                }
                string[] devicearray = deviceids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string condition = "";
                string condition1 = "";
                string conditionn = "";
                foreach (var deviceid in devicearray)
                {
                    if (string.IsNullOrEmpty(condition)) condition = string.Format(" de.ID = '{0}' ", deviceid);
                    else condition = condition + string.Format("or de.ID = '{0}' ", deviceid);
                }
                conditionn = "(" + condition + ")" + string.Format(" and d.ID like '{0}%'", ExMPID);
                List<ExecutionDeviceMaintenancePlan> EDMPList = ExecutionDeviceMaintenancePlanBO.Instance.GetListItems(conditionn);//检查保养表是否删除
                foreach (var deviceidd in devicearray)
                {
                    if (string.IsNullOrEmpty(condition1)) condition1 = string.Format(" d.ID = '{0}' ", deviceidd);
                    else condition1 = condition1 + string.Format("or d.ID = '{0}' ", deviceidd);
                }
                List<Device> devicelistt = DeviceBO.Instance.GetListItems("(" + condition1 + ")" + " and d.IsSetPlan1=1");//检查设备是否删除保养设置
                if (EDMPList.Count == 0 && devicelistt.Count == 0)
                {
                    result = true;
                }
            });
            return result;
        }

        //得到当前没有执行的保养单子列表
        public ActionResult ExecutionDeviceMaintenancePlanList()
        {
            return View();
        }

        [HttpPost, ActionName("ExecutionDeviceMaintenancePlanList")]
        //已将异步改为同步，此步改了方法
        public JsonResult ExecutionDeviceMaintenancePlanListPost(string departmentid, string devicename, string type, string assetnumber, string state, string factid, string mark, string btime, string etime)
        {
            int factoryid = Loginer.CurrentUser.Department.FactoryID;
            string condition = " 1=1";
            string condition1 = "";
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<ExecutionDeviceMaintenancePlan> u = new List<ExecutionDeviceMaintenancePlan>();
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.品保管理员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                {
                    condition += " and dm.FactoryID is not null ";
                }
                else
                    if (temp != null || temp1 != null || temp2 != null)
                        condition += string.Format(" and dm.FactoryID=" + factid);
                    else
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                        {
                            if (devicemark == "('计检设备')")
                                condition = condition + " and dm.FactoryID=" + factid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                            else
                                condition = condition + " and dm.FactoryID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                        }
                        else
                        {
                            int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                            List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                            string cond = string.Empty;
                            foreach (Department item in listdepart)
                            {
                                cond += item.ID + ",";
                                if (item.Children == null) { continue; }
                                foreach (var child in item.Children)
                                {
                                    cond += child.ID + ",";
                                }
                            }
                            cond = cond.Substring(0, cond.Length - 1);
                            condition += string.Format(" and dm.ID in ({0})", cond);
                        }
                    }
            }
            else //若没有factid则走这里
            {
                if (temp != null || temp1 != null || temp2 != null)
                    condition1 = string.Format(" dm.FactoryID=" + factoryid);
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition1 = "  dm.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                        else
                            condition1 = "  dm.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition1 = string.Format(" dm.ID in ({0})", cond);
                    }
                }
            }
            if (!string.IsNullOrEmpty(departmentid))
            {
                if (temp != null || temp1 != null || temp2 != null
                || temp3 != null || temp4 != null || temp5 != null || temp6 != null)
                {
                    condition += string.Format(" and dm.ID=" + departmentid);
                }
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and de.DeviceName like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(type))
            {
                condition += string.Format(" and dt.Name like '%{0}%' ", type);
            }
            if (!string.IsNullOrEmpty(mark))
            {
                condition += string.Format(" and m.Name like '%{0}%' ", mark);
            }
            if (!string.IsNullOrEmpty(assetnumber))
            {
                condition += string.Format(" and de.ID like '%{0}%' ", assetnumber);
            }
            if (!string.IsNullOrEmpty(state))
            {
                condition += string.Format(" and d.State in {0} ", state);
            }
            if (!string.IsNullOrEmpty(btime) && !string.IsNullOrEmpty(etime))
            {
                var start = btime.Split('-');
                var startid = 2 + start[0].Substring(2, 2) + start[1] + start[2];
                var end = etime.Split('-');
                var endid = 2 + end[0].Substring(2, 2) + end[1] + end[2];
                condition += string.Format(" and SUBSTRING(d.ID,2,7) between '{0}' and '{1}' ", startid, endid);
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                condition = condition + " and " + condition1;
            }
            u = ExecutionDeviceMaintenancePlanBO.Instance.GetListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //执行保养页面获取保养标准列表
        [HttpPost]
        public async Task<JsonResult> GetMaintenancePlan(string id)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<MaintenancePlanRefPlanContext> u = new List<MaintenancePlanRefPlanContext>();
            await Task.Run(() =>
            {
                u = MaintenancePlanRefPlanContextBO.Instance.GetMaintenancePlan(id);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //保养记录页面获取保养标准列表
        [HttpPost]
        public async Task<JsonResult> GetMaintenancePlanRecord(string id)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<MaintenancePlanRefPlanContext> u = new List<MaintenancePlanRefPlanContext>();
            await Task.Run(() =>
            {
                u = MaintenancePlanRefPlanContextBO.Instance.GetMaintenancePlanRecord(id);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        //保存未执行->计划已完成
        public async Task<int> UpdateExecutionMaintenancePlan(ExecutionDeviceMaintenancePlan dr)
        {
            //1,正常，2，项目未勾选，3，时间不对
            int code = 3;
            bool result = false;
            var gettime = DateTime.Now.ToString("yyMMdd");
            if (dr.ID.Substring(2, 6) == gettime)
            {
                dr.UserID = Loginer.CurrentUser.ID;
                dr.State = 1;
                dr.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                bool check = MaintenancePlanRefPlanContextBO.Instance.CkeckMaintenancePlanIsDone(dr.ID);
                if (!check) { code = 2; }//还有没做完的
                if (code != 2)
                {
                    await Task.Run(() =>
                    {
                        result = ExecutionDeviceMaintenancePlanBO.Instance.UpdateInExecutionMaintenancePlan(dr);
                        if (result) { code = 1; }
                    });
                }
            }
            return code;
        }

        //保存未执行->本日休息
        [HttpPost]
        public async Task<bool> MaintenancePlanStateChange2(ExecutionDeviceMaintenancePlan dr)
        {
            bool result = false;
            dr.UserID = Loginer.CurrentUser.ID;
            dr.State = 2;
            dr.EndTime = DateTime.Now;
            dr.Describe = "本日休息无需保养";
            await Task.Run(() =>
            {
                var condition = string.Format(" ExecutionDeviceMaintenancePlanID={0} and IsHide=1", dr.ID);//找出需要做的列表IsHide变为2
                List<MaintenancePlanRefPlanContext> List = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(condition);
                foreach (var item in List)
                {
                    MaintenancePlanRefPlanContextBO.Instance.RestAndNowork(item.ID);
                }
                result = ExecutionDeviceMaintenancePlanBO.Instance.UpdateState(dr);
            });
            return result;
        }

        //保存未执行->设备未生产
        [HttpPost]
        public async Task<bool> MaintenancePlanStateChange4(ExecutionDeviceMaintenancePlan dr)
        {
            bool result = false;
            dr.UserID = Loginer.CurrentUser.ID;
            dr.State = 4;
            dr.EndTime = DateTime.Now;
            dr.Describe = "本日设备未生产无需保养";
            await Task.Run(() =>
            {
                var condition = string.Format(" ExecutionDeviceMaintenancePlanID={0} and IsHide=1", dr.ID);//找出需要做的列表IsHide变为2
                List<MaintenancePlanRefPlanContext> List = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(condition);
                foreach (var item in List)
                {
                    MaintenancePlanRefPlanContextBO.Instance.RestAndNowork(item.ID);
                }
                result = ExecutionDeviceMaintenancePlanBO.Instance.UpdateState(dr);
            });
            return result;
        }

        #endregion

        #region 保养记录
        //得到已经执行或者已经进行本日休息操作的保养单子列表
        public ActionResult DeviceMaintenanceRecordList()
        {
            return View();
        }

        [HttpPost, ActionName("DeviceMaintenanceRecordList")]

        public JsonResult DeviceMaintenanceRecordListpost(string drid, string deviceid, string btime, string etime, string userid, string departmentid, int state, string factid, string type, string mark, string did, string devicename)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<ExecutionDeviceMaintenancePlan> u = new List<ExecutionDeviceMaintenancePlan>();
            string condition = "";
            string condition1 = "";
            int factoryid = Loginer.CurrentUser.Department.FactoryID;
            var departid = Loginer.DepartIDByUser(Loginer.CurrentUser);
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.品保管理员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            if (state == 0 || state == -1) { condition = " e.state in (1,2,4,5)"; }
            else if (state == 1) { condition = " e.state =1"; }
            else if (state == 2) { condition = " e.state =2"; }
            else if (state == 3) { condition = " e.state =4"; }
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                    condition += " and d.FactoryID is not null ";
                else
                    if ((temp != null || temp1 != null || temp2 != null) && factid.ToInt32() != 0)
                        condition += string.Format(" and d.FactoryID=" + factid);
                    else
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                        {
                            if (devicemark == "('计检设备')")
                                condition += " and d.FactoryID=" + factid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                            else
                                condition += " and d.FactoryID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                        }
                        else
                        {
                            int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                            List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                            string cond = string.Empty;
                            foreach (Department item in listdepart)
                            {
                                cond += item.ID + ",";
                                if (item.Children == null) { continue; }
                                foreach (var child in item.Children)
                                {
                                    cond += child.ID + ",";
                                }
                            }
                            cond = cond.Substring(0, cond.Length - 1);
                            condition = condition + " and " + string.Format(" d.ID in ({0})", cond);
                        }
                    }
            }
            else //若没有查询条件则走这里,显示本部门的，预加载时会使用到
            {
                if (temp != null || temp1 != null || temp2 != null)
                    condition1 = string.Format(" d.FactoryID=" + factoryid);
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition1 = "  d.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                        else
                            condition1 = "  d.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition1 = string.Format(" d.ID in ({0})", cond);
                    }
                }
            }
            if (!string.IsNullOrEmpty(drid))
            {
                condition += string.Format(" and e.ID like '%{0}%' ", drid);
            }
            if (!string.IsNullOrEmpty(type))
            {
                condition += string.Format(" and dt.Name like '%{0}%' ", type);
            }
            if (!string.IsNullOrEmpty(mark))
            {
                condition += string.Format(" and m.Name like '%{0}%' ", mark);
            }
            if (!string.IsNullOrEmpty(deviceid))
            {
                condition += string.Format(" and de.ID='{0}' ", deviceid);
            }
            if (!string.IsNullOrEmpty(did))
            {
                condition += string.Format(" and e.DeviceID like '%{0}%' ", did);
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and de.DeviceName like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(btime) && !string.IsNullOrEmpty(etime))
            {
                var start = btime.Split('-');
                var startid = 2 + start[0].Substring(2, 2) + start[1] + start[2];
                var end = etime.Split('-');
                var endid = 2 + end[0].Substring(2, 2) + end[1] + end[2];
                condition += string.Format(" and SUBSTRING(e.ID,2,7) between '{0}' and '{1}' ", startid, endid);
            }
            else
            {
                btime = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                etime = DateTime.Now.ToString("yyyy-MM-dd");
                var start = btime.Split('-');
                var startid = 2 + start[0].Substring(2, 2) + start[1] + start[2];
                var end = etime.Split('-');
                var endid = 2 + end[0].Substring(2, 2) + end[1] + end[2];
                condition += string.Format(" and SUBSTRING(e.ID,2,7) between '{0}' and '{1}' ", startid, endid);
            }
            if (!string.IsNullOrEmpty(userid))
            {
                condition += string.Format(" and u.RealName like '%{0}%' ", userid);
            }
            if (!string.IsNullOrEmpty(departmentid) && (temp != null || temp1 != null || temp2 != null
                || temp3 != null || temp4 != null || temp5 != null || temp6 != null))
            {
                condition += string.Format(" and d.ID=" + departmentid);
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                condition += " and " + condition1;
            }
            u = ExecutionDeviceMaintenancePlanBO.Instance.GetRecordListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        public ActionResult OneDeviceMaintenanceRecordList()
        {
            return View();
        }

        [HttpPost, ActionName("OneDeviceMaintenanceRecordList")]

        public JsonResult OneDeviceMaintenanceRecordListpost(string deviceid, string btime, string etime, string bystate)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<ExecutionDeviceMaintenancePlan> u = new List<ExecutionDeviceMaintenancePlan>();
            string condition = "";
            if (!string.IsNullOrEmpty(bystate))//设备界面查看保养记录
            {
                int ss = Convert.ToInt32(bystate);
                if (ss == -1) condition = " e.state in (0,1,2,3,4,5)";
                else if (ss == 0) condition = " e.state =0";
                else if (ss == 1) condition = " e.state =1";
                else if (ss == 2) condition = " e.state =2";
                else if (ss == 3) condition = " e.state =3";
                else if (ss == 4) condition = " e.state =4";
            }
            if (!string.IsNullOrEmpty(deviceid))
            {
                condition += string.Format(" and de.ID='{0}' ", deviceid);
            }
            if (!string.IsNullOrEmpty(btime) && !string.IsNullOrEmpty(etime))
            {
                var start = btime.Split('-');
                var startid = 2 + start[0].Substring(2, 2) + start[1] + start[2];
                var end = etime.Split('-');
                var endid = 2 + end[0].Substring(2, 2) + end[1] + end[2];
                condition += string.Format(" and SUBSTRING(e.ID,2,7) between '{0}' and '{1}' ", startid, endid);
            }
            u = ExecutionDeviceMaintenancePlanBO.Instance.GetOneItemList(condition, deviceid);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }
        #endregion

        #region 车间环境监测
        public ActionResult CRHRecordList()
        {
            return View();
        }

        [HttpPost, ActionName("CRHRecordList")]
        public JsonResult CRHRecordListtpost(string devicename, string byid, string deviceid, string factid, string starttime, string endtime, string username, string departmentid, string type, string mark)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<Tbl_CRHRecord> u = new List<Tbl_CRHRecord>();
            string condition = " 1=1";
            string condition1 = "";
            int factoryid = Loginer.CurrentUser.Department.FactoryID;
            var departid = Loginer.DepartIDByUser(Loginer.CurrentUser);
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.品保管理员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                    condition += " and dm.FactoryID is not null ";
                else
                    if ((temp != null || temp1 != null || temp2 != null) && factid.ToInt32() != 0)
                        condition += string.Format(" and dm.FactoryID=" + factid);
                    else
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                        {
                            if (devicemark == "('计检设备')")
                                condition += " and dm.FactoryID=" + factid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                            else
                                condition += " and dm.FactoryID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                        }
                        else
                        {
                            int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                            List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                            string cond = string.Empty;
                            foreach (Department item in listdepart)
                            {
                                cond += item.ID + ",";
                                if (item.Children == null) { continue; }
                                foreach (var child in item.Children)
                                {
                                    cond += child.ID + ",";
                                }
                            }
                            cond = cond.Substring(0, cond.Length - 1);
                            condition += string.Format(" and dm.ID in ({0})", cond);
                        }
                    }
            }
            else //若没有查询条件则走这里,显示本部门的，预加载时会使用到
            {
                if (temp != null || temp1 != null || temp2 != null)
                    condition1 = string.Format(" dm.FactoryID=" + factoryid);
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition1 = "  dm.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or de.IsOut=1)", devicemark);
                        else
                            condition1 = "  dm.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition1 = string.Format(" dm.ID in ({0})", cond);
                    }
                }
            }
            if (!string.IsNullOrEmpty(byid))
            {
                condition += string.Format(" and cr.ByID like '%{0}%' ", byid);
            }
            if (!string.IsNullOrEmpty(type))
            {
                condition += string.Format(" and dt.ID ={0} ", type);
            }
            if (!string.IsNullOrEmpty(mark))
            {
                condition += string.Format(" and m.ID in ({0}) ", mark);
            }
            if (!string.IsNullOrEmpty(deviceid))
            {
                condition += string.Format(" and de.ID like '%{0}%' ", deviceid);
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and de.DeviceName like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(starttime) && !string.IsNullOrEmpty(endtime))
            {
                condition += string.Format(" and cr.TodayTime between '{0}' and '{1}' ", starttime, endtime);
            }
            if (!string.IsNullOrEmpty(username))
            {
                List<User> listuser = UserBO.Instance.GetListByUserName(username);
                string cond = string.Empty;
                foreach (User item in listuser)
                {
                    cond += item.ID + ",";
                }
                if (!string.IsNullOrEmpty(cond))
                {
                    cond = cond.Substring(0, cond.Length - 1);
                    condition += string.Format(" and (cr.UserID8 in ({0}) or cr.UserID14 in ({0})) ", cond);
                }
            }
            if (!string.IsNullOrEmpty(departmentid))
            {
                if (temp != null || temp1 != null || temp2 != null || temp3 != null || temp4 != null || temp5 != null || temp6 != null)
                {
                    condition += string.Format(" and dm.ID=" + departmentid);
                }
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                condition = condition + " and " + condition1;
            }
            u = CRHRecordBO.Instance.GetListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }
        #endregion

        #region 设备操作
        public string GetMarkByWhatAdmin()
        {
            string DeviceMark = string.Empty;
            var temps = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            var temph = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var tempj = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var tempi = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            if (temps != null) { DeviceMark = "('自购设备','自建设备','电动工具')"; }
            else if (temph != null) { DeviceMark = "('后勤设施')"; }
            else if (tempj != null) { DeviceMark = "('计检设备')"; }
            else if (tempi != null) { DeviceMark = "('IT设备','办公设备')"; }
            return DeviceMark;
        }

        [HttpPost]
        public async Task<bool> DeviceCreate(Device u)
        {
            bool result = false;
            var factorycode = FactoryBO.Instance.GetFactoryCodeByDepartID(u.DepartmentID);
            await Task.Run(() =>
            {
                u.ID = factorycode + u.ID;
                u.IsOut = 0;//默认不是委外设备
                result = DeviceBO.Instance.InsertDevice(u);
            });
            return result;
        }

        public ActionResult DeviceList()
        {
            return View();
        }

        [HttpPost, ActionName("DeviceList")]
        public JsonResult DeviceListPost(string usestate, string outset, string typeid, string devicename, string model, string repairtime, string departmentid, string repairdate, string assetnumber, string id, string mark, string factid, string IsSafeguard, string jxdate, string byset, string markid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<Device> u = new List<Device>();
            //构造查询字符串
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.品保管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp7 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            string condition = "d.ID is not null";
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                    condition += " and de.FactoryID is not null ";
                else
                    if (temp != null || temp1 != null || temp2 != null || temp3 != null)
                        condition += " and de.FactoryID=" + factid;
                    else
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                        {
                            if (devicemark == "('计检设备')")
                                condition = condition + " and de.FactoryID=" + factid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                            else
                                condition = condition + " and de.FactoryID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                        }
                        else
                        {
                            int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                            List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                            string cond = string.Empty;
                            foreach (Department item in listdepart)
                            {
                                cond += item.ID + ",";
                                if (item.Children == null) { continue; }
                                foreach (var child in item.Children)
                                {
                                    cond += child.ID + ",";
                                }
                            }
                            cond = cond.Substring(0, cond.Length - 1);
                            condition += string.Format(" and de.ID in ({0})", cond);
                        }
                    }
            }
            else if (string.IsNullOrEmpty(factid))//若没有查询条件则走这里
            {
                int factoryid = Loginer.CurrentUser.Department.FactoryID;
                if (temp != null || temp1 != null || temp2 != null || temp3 != null)
                    condition += string.Format(" and de.FactoryID=" + factoryid);
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition += " and de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                        else
                            condition += " and de.FactoryID=" + factoryid + string.Format(" and m.Name in {0}", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition += string.Format(" and de.ID in ({0})", cond);
                    }
                }
            }
            if (!string.IsNullOrEmpty(typeid)) { condition += " and  d.TypeID=" + typeid; }
            if (!string.IsNullOrEmpty(usestate)) { condition += " and  d.UseState " + usestate; }
            if (string.IsNullOrEmpty(usestate)) { condition += " and  d.UseState " + "!='报废'"; }
            if (!string.IsNullOrEmpty(outset)) { condition += " and  d.IsOut " + outset; }
            if (!string.IsNullOrEmpty(mark))
            {
                condition += string.Format(" and m.Name like '%{0}%' ", mark);
            }
            if (!string.IsNullOrEmpty(markid))
            {
                condition += string.Format(" and m.ID ={0}", markid);
            }
            if (!string.IsNullOrEmpty(IsSafeguard))//四级报障是否设置查询
            {
                condition += string.Format(" and d.IsSafeguard {0}", IsSafeguard);
            }
            if (!string.IsNullOrEmpty(jxdate))//检修是否设置查询
            {
                condition += string.Format(" and d.RepairDate {0}", jxdate);
            }
            if (!string.IsNullOrEmpty(byset))//保养是否设置查询
            {
                condition += string.Format(" and d.MaintenanceFileName {0}", byset);
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and d.DeviceName like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(repairdate))
            {
                string[] repairdates = repairdate.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string dd = "";
                foreach (var date in repairdates)
                {
                    string ccc = string.Format(" d.RepairDate  like '%{0}%' ", date);
                    if (string.IsNullOrEmpty(dd)) { dd += ccc; }
                    else { dd += " or " + ccc; }
                }
                if (string.IsNullOrEmpty(condition)) condition += dd;
                else condition += " and " + dd;
            }
            if (!string.IsNullOrEmpty(model))
            {
                condition += string.Format(" and d.Model like '%{0}%' ", model);
            }
            if (!string.IsNullOrEmpty(assetnumber))
            {
                condition += string.Format(" and d.ID like '%{0}%' ", assetnumber);
            }
            if (!string.IsNullOrEmpty(repairtime))
            {
                condition += string.Format(" and d.RepairTime like '%{0}%' ", repairtime);
            }
            if (!string.IsNullOrEmpty(departmentid))
            {
                if (temp != null || temp1 != null || temp2 != null || temp3 != null
                    || temp4 != null || temp5 != null || temp6 != null || temp7 != null)
                {
                    condition += " and de.ID= " + departmentid;
                }
            }
            u = DeviceBO.Instance.GetListItems(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }
        //设备资格操作
        [HttpPost]
        public JsonResult DeviceLists(string usestate, string outset, string typeid, string devicename, string model, string repairtime, string departmentid, string repairdate, string assetnumber, string id, string mark, string factid, string IsSafeguard, string jxdate, string byset)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<Device> u = new List<Device>();
            //构造查询字符串
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.品保管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp7 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            string condition = "d.ID is not null";
            string condition1 = "";
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                    condition += " and de.FactoryID is not null ";
                else
                    if (temp != null || temp1 != null || temp2 != null || temp3 != null)
                        condition += " and de.FactoryID=" + factid;
                    else
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                        {
                            if (devicemark == "('计检设备')")
                                condition += " and de.FactoryID=" + factid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                            else
                                condition += " and de.FactoryID=" + factid + string.Format(" and (m.Name in {0})", devicemark);
                        }
                        else
                        {
                            int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                            List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                            string cond = string.Empty;
                            foreach (Department item in listdepart)
                            {
                                cond += item.ID + ",";
                                if (item.Children == null) { continue; }
                                foreach (var child in item.Children)
                                {
                                    cond += child.ID + ",";
                                }
                            }
                            cond = cond.Substring(0, cond.Length - 1);
                            condition += string.Format(" and de.ID in ({0})", cond);
                        }
                    }
            }
            else if (string.IsNullOrEmpty(factid))//若没有查询条件则走这里
            {
                int factoryid = Loginer.CurrentUser.Department.FactoryID;
                if (temp != null || temp1 != null || temp2 != null || temp3 != null)
                    condition1 = string.Format(" de.FactoryID=" + factoryid);
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                        else
                            condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0})", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition1 = string.Format(" de.ID in ({0})", cond);
                    }
                }
            }
            if (!string.IsNullOrEmpty(typeid)) { condition += " and  d.TypeID=" + typeid; }
            if (string.IsNullOrEmpty(usestate)) { condition += " and  d.UseState " + "!='报废'"; }
            if (!string.IsNullOrEmpty(mark))
            {
                condition += string.Format(" and m.Name like '%{0}%' ", mark);
            }
            if (!string.IsNullOrEmpty(IsSafeguard))//四级报障是否设置查询
            {
                condition += string.Format(" and d.IsSafeguard {0}", IsSafeguard);
            }
            if (!string.IsNullOrEmpty(jxdate))//检修是否设置查询
            {
                condition += string.Format(" and d.RepairDate {0}", jxdate);
            }
            if (!string.IsNullOrEmpty(byset))//保养是否设置查询
            {
                condition += string.Format(" and d.MaintenanceFileName {0}", byset);
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and d.DeviceName like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(model))
            {
                condition += string.Format(" and d.Model like '%{0}%' ", model);
            }
            if (!string.IsNullOrEmpty(assetnumber))
            {
                condition += string.Format(" and d.ID like '%{0}%' ", assetnumber);
            }
            if (!string.IsNullOrEmpty(departmentid))
            {
                if (temp != null || temp1 != null || temp2 != null || temp3 != null
                    || temp4 != null || temp5 != null || temp6 != null || temp7 != null)
                {
                    condition += " and de.ID= " + departmentid;
                }
                else
                {
                    int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                    List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                    string cond = string.Empty;
                    foreach (Department item in listdepart)
                    {
                        cond += item.ID + ",";
                        if (item.Children == null) { continue; }
                        foreach (var child in item.Children)
                        {
                            cond += child.ID + ",";
                        }
                    }
                    cond = cond.Substring(0, cond.Length - 1);
                    condition += string.Format(" and de.ID in ({0})", cond);
                }
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                condition += " and " + condition;
            }
            u = DeviceBO.Instance.GetListItems(condition);
            if (u != null && u.Count > 0)
            {
                DeviceRefQualificationsBO.Instance.GetQualificationsbyListDevices(u);
            }
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> DeviceEdit(Device u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceBO.Instance.UpdateDevice(u);
            });
            return result;
        }
        [HttpPost, ActionName("DeviceDelete")]
        public async Task<bool> DeviceDelete(string id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceBO.Instance.DeleteDevice(id);
            });
            return result;
        }

        [HttpPost]
        public async Task<bool> ClearImagePath(string deviceid)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceBO.Instance.UpdateImagePath(deviceid, "NULL");
            });
            return result;
        }

        //删除图片
        [HttpPost]
        public void DropImageWord(string deviceid)
        {
            string imagepath = DeviceBO.Instance.GetImagePathByID(deviceid);
            var listpath = imagepath.Split(';');
            for (int i = 0; i < listpath.Length; i++)
            {
                if (listpath[i].Length == 0)
                {
                    continue;
                }
                var item = listpath[i].Substring(1, listpath[i].Length - 2);
                string deletepath = Server.MapPath(@item);
                List<string> tsArrayy = new List<string> { "+", "?", "#", "&", "=", " " };
                List<string> tdArrayy = new List<string> { "%2B", "%3F", "%23", "%26", "%3D", "%20" };
                for (int j = 0; j < tsArrayy.Count; j++)
                {
                    deletepath = deletepath.Replace(tdArrayy[j], tsArrayy[j]);//特殊符号替换路径
                }
                if (System.IO.File.Exists(deletepath))
                {
                    System.IO.File.Delete(deletepath);
                }
            }
        }

        [HttpPost]
        public async Task<bool> ClearDeviceWord(string deviceid)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceBO.Instance.UpdateWordPath(deviceid, "NULL");
            });
            return result;
        }

        //删除文档
        [HttpPost]
        public void DropDeviceWord(string deviceid)
        {
            string wordpath = DeviceBO.Instance.GetWordPathByID(deviceid);
            var listpath = wordpath.Split(';');
            for (int i = 0; i < listpath.Length; i++)
            {
                if (listpath[i].Length == 0)
                {
                    continue;
                }
                var item = listpath[i].Substring(0, listpath[i].Length - 1);
                string deletepath = Server.MapPath(@item);
                List<string> tsArrayy = new List<string> { "+", "?", "#", "&", "=", " " };
                List<string> tdArrayy = new List<string> { "%2B", "%3F", "%23", "%26", "%3D", "%20" };
                for (int j = 0; j < tsArrayy.Count; j++)
                {
                    deletepath = deletepath.Replace(tdArrayy[j], tsArrayy[j]);//特殊符号替换路径
                }
                if (System.IO.File.Exists(deletepath))
                {
                    System.IO.File.Delete(deletepath);
                }
            }
        }

        //财务模块
        public ActionResult DeviceFinanceList()
        {
            return View();
        }

        [HttpPost, ActionName("DeviceFinanceList")]
        public JsonResult DeviceFinanceListPost(string usestate, string typeid, string devicename, string model, string departmentid, string assetnumber, string factid, string mark)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<Device> u = new List<Device>();
            //构造查询字符串
            string condition = " d.ID is not null";
            string condition1 = "";
            if (!string.IsNullOrEmpty(factid))
            {
                var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
                if (factid.ToInt32() == 0 && temp != null)
                    condition += " and de.FactoryID is not null ";
                else
                    condition += " and de.FactoryID=" + factid;
            }
            else if (string.IsNullOrEmpty(factid))
            {
                var factoryid = Loginer.CurrentUser.Department.FactoryID;
                condition += " and de.FactoryID=" + factoryid;
            }

            if (!string.IsNullOrEmpty(typeid))
            {
                condition += string.Format(" and d.TypeID = {0} ", typeid);
            }
            if (!string.IsNullOrEmpty(usestate)) { condition += " and  d.UseState " + usestate; }
            if (string.IsNullOrEmpty(usestate)) { condition += " and  d.UseState !='报废'"; }
            if (!string.IsNullOrEmpty(mark))
            {
                condition += string.Format(" and m.ID = {0} ", mark);
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and d.DeviceName like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(model))
            {
                condition += string.Format(" and d.Model like '%{0}%' ", model);
            }
            if (!string.IsNullOrEmpty(assetnumber))
            {
                condition += string.Format(" and d.ID like '%{0}%' ", assetnumber);
            }
            if (!string.IsNullOrEmpty(departmentid))
            {
                condition += " and de.ID= " + departmentid;
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                condition = condition1 + " and " + condition;
            }
            u = DeviceBO.Instance.GetListItemss(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<string> DeviceFinanceEdit(Device u)
        {
            bool result = false;
            string msg = "";
            u.Yclprice = u.Yclprice == null ? 0 : u.Yclprice;
            u.NowPrice = u.NowPrice == null ? 0 : u.NowPrice;
            u.DepreciationPrice = u.DepreciationPrice == null ? 0 : u.DepreciationPrice;
            if (u.NowPrice != (u.Yclprice - u.DepreciationPrice))
            {
                msg = "账面价值加累积折旧不等于购置单价！";
            }
            else
            {
                await Task.Run(() =>
                {
                    result = DeviceBO.Instance.UpdateDeviceFinance(u);
                });
                msg = result ? "True" : "False";
            }
            return msg;
        }

        //委外送检模块
        public ActionResult DeviceOutCheckList()
        {
            return View();
        }

        [HttpPost, ActionName("DeviceOutCheckList")]
        public JsonResult DeviceOutCheckListPost(string usestate, string typeid, string devicename, string model, string departmentid, string assetnumber, string factid, string state, string mark, string checkway)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<Device> u = new List<Device>();
            //构造查询字符串
            string condition = " d.ID is not null";
            string condition1 = "";
            var nowdate = DateTime.Now.ToString("yyyy-MM-dd");
            var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
            var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
            var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
            var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
            var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
            var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
            var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
            if (!string.IsNullOrEmpty(factid))
            {
                if (factid.ToInt32() == 0 && temp != null)
                    condition += " and de.FactoryID is not null ";
                else
                    if (temp != null || temp1 != null || temp2 != null)
                        condition += " and de.FactoryID=" + factid;
                    else
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                        {
                            if (devicemark == "('计检设备')")
                                condition += " and de.FactoryID=" + factid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                            else
                                condition += " and de.FactoryID=" + factid + string.Format(" and m.Name in {0}", devicemark);
                        }
                        else
                        {
                            int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                            List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                            string cond = string.Empty;
                            foreach (Department item in listdepart)
                            {
                                cond += item.ID + ",";
                                if (item.Children == null) { continue; }
                                foreach (var child in item.Children)
                                {
                                    cond += child.ID + ",";
                                }
                            }
                            cond = cond.Substring(0, cond.Length - 1);
                            condition += string.Format(" and de.ID in ({0})", cond);
                        }
                    }
            }
            else if (string.IsNullOrEmpty(factid))//若没有查询条件则走这里
            {
                int factoryid = Loginer.CurrentUser.Department.FactoryID;
                if (temp != null || temp1 != null || temp2 != null)
                    condition1 = string.Format(" de.FactoryID=" + factoryid);
                else
                {
                    var devicemark = GetMarkByWhatAdmin();
                    if (!string.IsNullOrEmpty(devicemark))//说明是4种设备标识管理员
                    {
                        if (devicemark == "('计检设备')")
                            condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or d.IsOut=1)", devicemark);
                        else
                            condition1 = " de.FactoryID=" + factoryid + string.Format(" and (m.Name in {0})", devicemark);
                    }
                    else
                    {
                        int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                        List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                        string cond = string.Empty;
                        foreach (Department item in listdepart)
                        {
                            cond += item.ID + ",";
                            if (item.Children == null) { continue; }
                            foreach (var child in item.Children)
                            {
                                cond += child.ID + ",";
                            }
                        }
                        cond = cond.Substring(0, cond.Length - 1);
                        condition1 = string.Format(" de.ID in ({0})", cond);
                    }
                }
            }
            if (!string.IsNullOrEmpty(usestate)) { condition += " and  d.UseState " + usestate; }
            if (string.IsNullOrEmpty(usestate)) { condition += " and  d.UseState !='报废'"; }
            if (!string.IsNullOrEmpty(typeid))
            {
                condition += string.Format(" and d.TypeID = {0} ", typeid);
            }
            if (!string.IsNullOrEmpty(state))
            {
                if (state.ToInt32() == 0)
                {
                    condition += " and (d.NextCheckTime is not null or d.IsOut=1 or m.Name='计检设备')";
                }
                else if (state.ToInt32() == 1)
                {
                    condition += string.Format(" and d.NextCheckTime<='{0}'", nowdate);
                }
                else if (state.ToInt32() == 2)
                {
                    condition += string.Format(" and (d.NextCheckTime>'{0}' or (d.IsOut=1 and d.NextCheckTime>'{1}') or (m.Name='计检设备' and d.NextCheckTime>'{2}')) ", nowdate, nowdate, nowdate);
                }
            }
            if (string.IsNullOrEmpty(state))
            {
                condition += " and (d.NextCheckTime is not null or d.IsOut=1 or m.Name='计检设备')";
            }
            if (!string.IsNullOrEmpty(devicename))
            {
                condition += string.Format(" and d.DeviceName like '%{0}%' ", devicename);
            }
            if (!string.IsNullOrEmpty(model))
            {
                condition += string.Format(" and d.Model like '%{0}%' ", model);
            }
            if (!string.IsNullOrEmpty(mark))
            {
                condition += string.Format(" and m.ID = {0} ", mark);
            }
            if (!string.IsNullOrEmpty(checkway))
            {
                condition += string.Format(" and d.CheckWay like '%{0}%' ", checkway);
            }
            if (!string.IsNullOrEmpty(assetnumber))
            {
                condition += string.Format(" and d.ID like '%{0}%' ", assetnumber);
            }
            if (!string.IsNullOrEmpty(departmentid))
            {
                if (temp != null || temp1 != null || temp2 != null || temp3 != null
                    || temp4 != null || temp5 != null || temp6 != null)
                {
                    condition += " and de.ID= " + departmentid;
                }
                else
                {
                    int departmentidd = Loginer.DepartIDByUser(Loginer.CurrentUser);
                    List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentidd);
                    string cond = string.Empty;
                    foreach (Department item in listdepart)
                    {
                        cond += item.ID + ",";
                        if (item.Children == null) { continue; }
                        foreach (var child in item.Children)
                        {
                            cond += child.ID + ",";
                        }
                    }
                    cond = cond.Substring(0, cond.Length - 1);
                    condition += string.Format(" and de.ID in ({0})", cond);
                }
            }
            if (!string.IsNullOrEmpty(condition1))
            {
                condition = condition1 + " and " + condition;
            }
            u = DeviceBO.Instance.GetListItemsss(condition);
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        /// <summary>
        /// 批量设置委外设备
        /// </summary>
        public async Task<bool> DeviceUpdateIsOut(string deviceids)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.UpdateOutDevice(items.ID);
                }
            });
            return result;
        }

        [HttpPost]
        /// <summary>
        /// 批量解除委外设备
        /// </summary>
        public async Task<bool> DeviceDeleteIsOut(string deviceids)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.DeleteOutDevice(items.ID);
                }
            });
            return result;
        }

        [HttpPost]
        public async Task<bool> DeviceOutCheckEdit(Device u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceBO.Instance.UpdateDeviceOutCheck(u);
            });
            return result;
        }

        [HttpPost]
        public async Task<bool> SetPlanDate(Device u)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceBO.Instance.UpdateDevice(u);
            });
            return result;
        }

        public async Task<bool> DeviceUpdateFileNameID(string deviceids, string filenameid)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            var filename = filenameid.ToInt32();
            List<PlanContext> plancontextlist = PlanContextBO.Instance.PlanContextList(filename);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    RepairPlanFileName r = RepairPlanFileNameBO.Instance.RepairPlanFileNamebyID(filename);
                    items.RepairPlanFileName = r.Name;
                    if (items.FileNameID == 0)
                    {
                        items.FileNameID = r.ID;
                        result = DeviceBO.Instance.UpdateFileNameID(items);
                        StringBuilder builder = new StringBuilder();
                        foreach (var item in plancontextlist)
                        {
                            builder.Append(string.Format("insert into DeviceRefPlanContext([DeviceID],[PlanContextID]) values('{0}','{1}');", items.ID, item.ID));
                        }
                        DeviceRefPlanContextBO.Instance.InsertBySql(builder.ToString());
                    }
                    else
                    {
                        List<PlanContext> oldplancontextlist = PlanContextBO.Instance.PlanContextList(items.FileNameID);
                        DeviceRefPlanContextBO.Instance.Delete(items.ID, oldplancontextlist);
                        StringBuilder builder = new StringBuilder();
                        foreach (var item in plancontextlist)
                        {
                            builder.Append(string.Format("insert into DeviceRefPlanContext([DeviceID],[PlanContextID]) values('{0}','{1}');", items.ID, item.ID));
                        }
                        DeviceRefPlanContextBO.Instance.InsertBySql(builder.ToString());
                        List<ExecutionPlan> elist = ExecutionPlanBO.Instance.GetListItemss(items.FileNameID, items.ID);
                        foreach (var item in elist)
                        {
                            RepairPlanRefPlanContextBO.Instance.DeleteRepairPlanRefPlanContext(item.ID);//把相同的剔除
                            ExecutionPlanBO.Instance.InsertExecutionPlanFileID(item.ID, filename);
                            RepairPlanRefPlanContext runTemp = new RepairPlanRefPlanContext();
                            foreach (var itemss in plancontextlist)
                            {
                                runTemp.PlanContextID = itemss.ID;
                                runTemp.Project = itemss.Project;
                                runTemp.Standard = itemss.Standard;
                                runTemp.Content = itemss.Content;
                                runTemp.Code = itemss.Code;
                                runTemp.ImportentPart = itemss.ImportentPart;
                                runTemp.ExecutionPlanID = item.ID;
                                result = RepairPlanRefPlanContextBO.Instance.Insert(runTemp);
                            }
                        }
                        items.FileNameID = r.ID;
                        result = DeviceBO.Instance.UpdateFileNameID(items);
                    }
                }
            });
            return result;
        }

        public async Task<bool> DeviceUpdateRepairDate(string deviceids, string repairdate)
        {
            bool result = false;

            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);

            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    items.RepairDate = repairdate;
                    result = DeviceBO.Instance.UpdateRepairDate(items);
                }
            });
            return result;
        }

        public async Task<bool> DeviceCylceList(string id)
        {
            await Task.Run(() =>
            {
                DeviceBO.Instance.DeviceCylceEdit(id);
            });
            return true;
        }

        [HttpPost]
        public async Task<JsonResult> GetDeviceRefSpareDevice(string drid)
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            List<DeviceRefSpareDevice> u = new List<DeviceRefSpareDevice>();
            await Task.Run(() =>
            {
                u = DeviceRefSpareDeviceBO.Instance.GetSpareDeviceByDevice(drid);
            });
            int total = u.Count;
            var rows = u.Skip((page - 1) * row).Take(row).ToList();
            return Json(new { total = total, rows = rows });
        }

        [HttpPost]
        public async Task<bool> AddDeviceRefSpareDevices(List<SpareDevice> sdlist, string drid)
        {
            bool result = false;
            await Task.Run(() =>
            {
                List<DeviceRefSpareDevice> rosnlist = DeviceRefSpareDeviceBO.Instance.GetSpareDeviceByDevice(drid);
                foreach (var item in rosnlist)
                {
                    for (int n = 0; n < sdlist.Count; n++)
                    {
                        if (item.SpareDeviceID == sdlist[n].ID)
                            sdlist.Remove(sdlist[n]);
                    }
                }
                result = DeviceRefSpareDeviceBO.Instance.InsertDeviceRefSpareDevice(drid, sdlist);
            });
            return result;
        }

        //删除设备关联备件
        [HttpPost]
        public async Task<bool> DeviceRefSpareDeviceDelete(int id)
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = DeviceRefSpareDeviceBO.Instance.Delete(id);

            });
            return result;
        }

        #endregion

        #region 创建检修单子
        //这个可能没有用，暂时没发现有用到的地方
        [HttpPost]
        public async Task<bool> CreatDeviceE()
        {
            bool result = false;
            string code = Loginer.CurrentUser.Department.Factory.Code;//工厂代码
            string condition = "d.ID is not null and d.RepairDate is not null and d.HeadID is not null and d.FileNameID is not null";
            List<Device> devicelist = new List<Device>();
            devicelist = DeviceBO.Instance.GetListItems(condition);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    ExecutionPlan e = ExecutionPlanBO.Instance.ExecutionPlanbyRepairDate(items.ID, items.RepairDate);
                    if (0 == items.FileNameID || e != null)
                    {
                        continue;
                    }
                    ExecutionPlan u = new ExecutionPlan();
                    string[] plantimes = items.RepairDate.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    u.TimeNum = plantimes[0].ToInt32() * 10000 + plantimes[1].ToInt32() * 100 + plantimes[2].ToInt32();
                    u.UserCost = 0;
                    u.SpareDeviceCost = 0;
                    u.CompleteTime = null;
                    u.StartTime = null;
                    u.Describe = null;
                    u.RepairUserID = (int)items.HeadID;
                    u.PlanTime = items.RepairDate;
                    u.PlanContextID = items.FileNameID;
                    u.State = 1;
                    u.DeviceID = items.ID;
                    u.ID = ExecutionPlanBO.Instance.CreateDeviceRepairID(u.PlanTime, items.ID, code);
                    if (u.ID == "计划已存在")
                    {
                        result = true;
                    }
                    else
                    {
                        result = ExecutionPlanBO.Instance.InsertExecutionPlan(u);
                        List<PlanContext> plancontextlist = PlanContextBO.Instance.PlanContextList(items.FileNameID);
                        RepairPlanRefPlanContext runTemp = new RepairPlanRefPlanContext();
                        foreach (var item in plancontextlist)
                        {
                            runTemp.PlanContextID = item.ID;
                            runTemp.Project = item.Project;
                            runTemp.Standard = item.Standard;
                            runTemp.Content = item.Content;
                            runTemp.ImportentPart = item.ImportentPart;
                            runTemp.Code = item.Code;
                            runTemp.ExecutionPlanID = u.ID;
                            RepairPlanRefPlanContextBO.Instance.Insert(runTemp);
                        }
                    }
                }
            });
            return result;
        }

        #endregion

        #region 滚动显示检修单和委外送检
        //检查未做完的检修单子
        [HttpPost]
        public JsonResult GetMsgByCurrenUser()
        {
            try
            {
                //1.得到没有做完的检修单（未确认和该做未做）
                var condition = "";
                var condition1 = "";
                User u = Loginer.CurrentUser;
                var factoryid = u.Department.FactoryID;
                var nowdate = DateTime.Now.ToString("yyyy-MM-dd");
                int departmentid = Loginer.DepartIDByUser(Loginer.CurrentUser);
                List<Department> listdepart = DepartmentBO.Instance.GetListByParentID(departmentid);
                string cond = string.Empty;
                foreach (Department item in listdepart)
                {
                    cond += item.ID + ",";
                    if (item.Children == null) { continue; }
                    foreach (var child in item.Children)
                    {
                        cond += child.ID + ",";
                    }
                }
                cond = cond.Substring(0, cond.Length - 1);
                var temp = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.超级管理员.ToString());
                var temp1 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.工厂管理员.ToString());
                var temp2 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.计检管理员.ToString());
                var temp3 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.维修员.ToString());
                var temp4 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.后勤管理员.ToString());
                var temp5 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.IT管理员.ToString());
                var temp6 = Loginer.CurrentUser.UserRoles.FirstOrDefault(a => a.Name == RoleEnum.设备管理员.ToString());
                if (temp != null || temp1 != null || temp2 != null)
                {
                    condition1 = string.Format(" de.FactoryID={0} and d.NextCheckTime<='{1}'  and  d.UseState !='报废'", factoryid, nowdate);
                    if (temp != null || temp1 != null)//超级和工厂管理员
                    {
                        condition = string.Format(" dm.FactoryID={0}  and  de.UseState !='报废'", factoryid);
                    }
                    else if (temp2 != null)//计检管理员
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        condition = "  dm.FactoryID=" + factoryid + string.Format(" and (m.Name in {0} or de.IsOut=1)  and  de.UseState !='报废'", devicemark);
                    }
                }
                else//维修员和普通人员,3个管理员
                {
                    condition1 = string.Format(" de.ID in ({0}) and d.NextCheckTime<='{1}'  and  d.UseState !='报废'", cond, nowdate);
                    if (temp3 != null)//维修员
                    {
                        condition = string.Format(" (e.RepairUserID={0} or dm.ID in ({1}))  and  de.UseState !='报废'", u.ID, cond);
                    }
                    else// 普通人员,3个管理员
                    {
                        var devicemark = GetMarkByWhatAdmin();
                        if (!string.IsNullOrEmpty(devicemark))//说明是3种设备标识管理员
                        {
                            condition = "  dm.FactoryID=" + factoryid + string.Format(" and m.Name in {0}  and  de.UseState !='报废'", devicemark);
                        }
                        else
                            condition = string.Format(" dm.ID in ({0})  and  de.UseState !='报废' ", cond);
                    }
                }
                var conditionn = string.Empty;
                var conditionnn = string.Empty;
                var timezhou = DateTime.Now.AddDays(+7).ToString("yyyy-MM-dd");
                var timeyue = DateTime.Now.AddDays(+30).ToString("yyyy-MM-dd");
                conditionn = condition + string.Format(" and  '{0}'>=e.PlanTime  and e.state in (1,2,3,4,5,6,11,12)", timeyue);//本月未完成单子
                conditionnn = condition + string.Format(" and  '{0}'>=e.PlanTime  and e.state in (1,2,3,4,5,6,11,12)", timezhou);//本周未完成单子
                List<ExecutionPlan> uplist = ExecutionPlanBO.Instance.GetListItems(conditionn);
                List<ExecutionPlan> uplistt = ExecutionPlanBO.Instance.GetListItems(conditionnn);
                List<Device> uplisttt = DeviceBO.Instance.GetListItemsss(condition1);
                int total = uplist.Count;
                int totall = uplistt.Count;
                int totalll = uplisttt.Count;
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("一个月内未完工检修单:【{0}】个 ,一周内未完工检修单:【{1}】个 ,委外送检设备:【{2}】台 ", total, totall, totalll);
                return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex);
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region 将某个工厂的超期保养单做完
        //处理超期保养
        public async Task<bool> ExecutionCQ(DateTime time1, DateTime time2, string factorycode)
        {
            bool result = true;
            var start = time1.ToString().Split('/', ' ');
            var startid = factorycode.Substring(0, 1) + 2 + start[0].Substring(2, 2) + start[1] + start[2];
            var end = time2.ToString().Split('/', ' ');
            var endid = factorycode.Substring(0, 1) + 2 + end[0].Substring(2, 2) + end[1] + end[2];
            await Task.Run(() =>
            {
                List<ExecutionDeviceMaintenancePlan> List = new List<ExecutionDeviceMaintenancePlan>();
                var condition = string.Format(" SUBSTRING(d.ID,1,8) between '{0}' and '{1}' and f.Code={2} and d.state=3", startid, endid, factorycode);
                List = ExecutionDeviceMaintenancePlanBO.Instance.GetListItems(condition);
                foreach (var item in List)
                {
                    MaintenancePlanRefPlanContextBO.Instance.UpdateCQ(item.ID);//解决 MaintenancePlanRefPlanContext
                    Device de = DeviceBO.Instance.GetDeviceByID(item.DeviceID);
                    if (de.DepartmentID == 276) { item.UserID = 2441; }
                    else if (de.DepartmentID == 270) { item.UserID = 2386; }
                    else if (de.DepartmentID == 422) { item.UserID = 2346; }
                    else if (de.DepartmentID == 259) { item.UserID = 2278; }
                    else if (de.DepartmentID == 421) { item.UserID = 2535; }
                    else if (de.DepartmentID == 420) { item.UserID = 2371; }
                    else if (de.DepartmentID == 417) { item.UserID = 2484; }
                    else if (de.DepartmentID == 431) { item.UserID = 2419; }
                    else if (de.DepartmentID == 255) { item.UserID = 2261; }
                    else if (de.DepartmentID == 231) { item.UserID = 2133; }
                    else if (de.DepartmentID == 305) { item.UserID = 2543; }
                    else if (de.DepartmentID == 256) { item.UserID = 2267; }
                    else
                    {
                        var usercondition = string.Format(" d.ID={0}", de.DepartmentID);
                        List<User> ulist = UserBO.Instance.GetListItems(usercondition);//userid  
                        if (ulist.Count > 0)
                            item.UserID = ulist[0].ID;
                        else
                            item.UserID = 3571;
                    }
                    DateTime gettime = DateTime.Now;
                    if (item.EndTime == null)
                    {
                        var month = item.ID.Substring(4, 2);
                        var day = item.ID.Substring(6, 2);
                        string timestring = string.Format("2016-{0}-{1} 07:01:00.000", month, day);
                        gettime = Convert.ToDateTime(timestring);
                    }
                    else
                    {
                        gettime = Convert.ToDateTime(item.EndTime).AddDays(-1);
                    }
                    DateTime starttime = gettime.AddHours(-0.5);//开始时间
                    DateTime endtime = gettime.AddHours(0.5);//结束时间                                 
                    item.StartTime = starttime;
                    item.EndTime = endtime;
                    item.ErrorAmount = 0;
                    item.State = (int)ExcuteBYState.计划已执行;
                    item.Describe = "故障数量:0处";
                    ExecutionDeviceMaintenancePlanBO.Instance.UpdateState(item);
                }
            });
            return result;
        }
        #endregion

        #region 累积维修待机时间
        public async Task<bool> SetTime()
        {
            bool result = false;
            List<DeviceRepair> List = new List<DeviceRepair>();
            string condition = " dr.State=3";
            await Task.Run(() =>
            {
                List = DeviceRepairBO.Instance.GetListItems(condition);
                foreach (var Item in List)
                {
                    if (Item.DeviceState.Equals("停机待修") || Item.DeviceState.Equals("停机"))
                    {
                        if (Item.OffTime == 0)
                        {
                            DateTime EndTime = (DateTime)Item.CompleteTime;
                            DateTime StartTime = (DateTime)Item.StartTime;
                            TimeSpan tsStart = new TimeSpan(StartTime.Ticks);
                            TimeSpan tsEnd = new TimeSpan(EndTime.Ticks);
                            TimeSpan ts = tsEnd.Subtract(tsStart).Duration();
                            double dateDiffHours = ts.Days * 24 + ts.Hours;
                            if (ts.Minutes > 30) dateDiffHours += 1;
                            else if (ts.Minutes < 30 && ts.Minutes > 0 || ts.Minutes == 30) dateDiffHours += 0.5;
                            result = DeviceRepairBO.Instance.UpdateOffTime(Item.ID, dateDiffHours);
                        }
                        if (!result) break;
                    }
                }
            });
            return result;
        }
        #endregion

        #region 处理历史保养记录的isdide字段
        public async Task<bool> DealWithIsHide()
        {
            //后台创建 = 0,执行显示 = 1,记录显示 = 2
            bool result = false;
            var thetime = DateTime.Now.ToString("yyMMdd");
            var id1 = 12 + thetime;
            var id2 = 22 + thetime;
            string conditions = string.Format(" and (ID like '%{0}%' or ID like '%{1}%')", id1, id2);
            List<ExecutionDeviceMaintenancePlan> List1245 = ExecutionDeviceMaintenancePlanBO.Instance.BYIsHideGetListItem(conditions);//休假，未生产，车辆外出
            List<ExecutionDeviceMaintenancePlan> List03 = ExecutionDeviceMaintenancePlanBO.Instance.BYIsHideGetListItems(conditions);//未执行和超期        
            await Task.Run(() =>
            {
                foreach (var items in List1245)
                {
                    string ID = 20 + items.ID.Substring(2);
                    //从保养单号得到保养单的计划时间,通过这个时间解决hide字段
                    int y = (ID.Substring(0, 4)).ToInt32();  //  年  2016  
                    int m = (ID.Substring(4, 2)).ToInt32();  //  月  5
                    int d = (ID.Substring(6, 2)).ToInt32();  //  日  29
                    int ss = (ID.Substring(0, 8)).ToInt32();//20160529
                    if (m == 1) m = 13;
                    if (m == 2) m = 14;
                    int week = (d + 2 * m + 3 * (m + 1) / 5 + y + y / 4 - y / 100 + y / 400) % 7 + 1;
                    string weekstr = "";
                    switch (week)
                    {
                        case 1: weekstr = "星期一"; break;
                        case 2: weekstr = "星期二"; break;
                        case 3: weekstr = "星期三"; break;
                        case 4: weekstr = "星期四"; break;
                        case 5: weekstr = "星期五"; break;
                        case 6: weekstr = "星期六"; break;
                        case 7: weekstr = "星期日"; break;
                    }
                    List<string> stringsql = new List<string>();
                    //先将早中晚每日一次的展示出来
                    string sql1 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=2 where ExecutionDeviceMaintenancePlanID={0} and Cycle in ('早','中','晚','每日一次')", items.ID);
                    stringsql.Add(sql1);
                    MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(stringsql);//执行sql语句 
                    var condition = string.Format(" ExecutionDeviceMaintenancePlanID={0} and cycle ='每周一次'", items.ID);//将每周一次的找出来
                    List<MaintenancePlanRefPlanContext> WeekList = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(condition);
                    var condition1 = string.Format(" ExecutionDeviceMaintenancePlanID={0} and cycle ='每月一次'", items.ID);//将每月一次的找出来
                    List<MaintenancePlanRefPlanContext> MonthList = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(condition1);
                    if (WeekList.Count > 0 || MonthList.Count > 0)
                    {
                        if (WeekList.Count > 0)//当Count大于0执行下面操作
                        {
                            List<string> updatesql = new List<string>();
                            if (weekstr == "星期五")
                            {
                                var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=2 where ExecutionDeviceMaintenancePlanID={0} and  Cycle = '每周一次'", items.ID);
                                updatesql.Add(sql3);
                            }
                            else
                            {
                                var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=0 where ExecutionDeviceMaintenancePlanID={0} and  Cycle ='每周一次'", items.ID);
                                updatesql.Add(sql3);
                            }
                            MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(updatesql);//执行sql语句 ,每周一次处理掉
                        }
                        if (MonthList.Count > 0)//当Count大于0执行下面操作
                        {
                            List<string> updatesql = new List<string>();
                            if (d == 15)
                            {
                                var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=2 where ExecutionDeviceMaintenancePlanID={0} and  Cycle = '每月一次'", items.ID);
                                updatesql.Add(sql3);
                            }
                            else
                            {
                                var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=0 where ExecutionDeviceMaintenancePlanID={0} and  Cycle ='每月一次'", items.ID);
                                updatesql.Add(sql3);
                            }
                            MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(updatesql);//执行sql语句 ,每月一次处理掉
                        }
                    }
                }
                foreach (var items in List03)
                {
                    string ID = 20 + items.ID.Substring(2);
                    //从保养单号得到保养单的计划时间,通过这个时间解决hide字段
                    int y = (ID.Substring(0, 4)).ToInt32();  //  年  2016  
                    int m = (ID.Substring(4, 2)).ToInt32();  //  月  5
                    int d = (ID.Substring(6, 2)).ToInt32();  //  日  29
                    int ss = (ID.Substring(0, 8)).ToInt32();//20160529
                    if (m == 1) m = 13;
                    if (m == 2) m = 14;
                    int week = (d + 2 * m + 3 * (m + 1) / 5 + y + y / 4 - y / 100 + y / 400) % 7 + 1;
                    string weekstr = "";
                    switch (week)
                    {
                        case 1: weekstr = "星期一"; break;
                        case 2: weekstr = "星期二"; break;
                        case 3: weekstr = "星期三"; break;
                        case 4: weekstr = "星期四"; break;
                        case 5: weekstr = "星期五"; break;
                        case 6: weekstr = "星期六"; break;
                        case 7: weekstr = "星期日"; break;
                    }
                    List<string> stringsql = new List<string>();
                    string sql1 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0} and Cycle in ('早','中','晚','每日一次')", items.ID);
                    stringsql.Add(sql1);
                    MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(stringsql);//执行sql语句 ,先将早中晚，每日一次的处理掉
                    var condition = string.Format(" ExecutionDeviceMaintenancePlanID={0} and cycle ='每周一次'", items.ID);//将每周一次的找出来
                    List<MaintenancePlanRefPlanContext> WeekList = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(condition);
                    var condition1 = string.Format(" ExecutionDeviceMaintenancePlanID={0} and cycle ='每月一次'", items.ID);//将每月一次的找出来
                    List<MaintenancePlanRefPlanContext> MonthList = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(condition1);
                    if (WeekList.Count > 0 || MonthList.Count > 0)
                    {
                        if (WeekList.Count > 0)//当Count大于0执行下面操作
                        {
                            List<string> updatesql = new List<string>();
                            if (weekstr == "星期五")
                            {
                                var sql2 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0}  and  Cycle = '每周一次'", items.ID);
                                updatesql.Add(sql2);
                            }
                            else
                            {
                                var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=0 where ExecutionDeviceMaintenancePlanID={0} and  Cycle ='每周一次'", items.ID);
                                updatesql.Add(sql3);
                            }
                            MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(updatesql);//执行sql语句 ,每周一次处理掉
                        }
                        if (MonthList.Count > 0)//当Count大于0执行下面操作
                        {
                            List<string> updatesql = new List<string>();
                            if (d == 15)
                            {
                                var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0}  and  Cycle = '每月一次'", items.ID);
                                updatesql.Add(sql3);
                            }
                            else
                            {
                                var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=0 where ExecutionDeviceMaintenancePlanID={0} and  Cycle ='每月一次'", items.ID);
                                updatesql.Add(sql3);
                            }
                            MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(updatesql);//执行sql语句 ,每月一次处理掉
                        }
                    }
                }
            });
            return result;
        }

        #endregion

        #region 处理休息
        public async Task<bool> DealWithSleep()
        {
            bool result = false;
            var gettime = DateTime.Now.ToString("yyyy-MM-dd");
            var thetime = DateTime.Now.ToString("yyMMdd");
            string condition = string.Format("  DATEDIFF(day,d.Time,'{0}')=0 ", gettime);//得到今天的休假申请
            List<DeviceRefSleep> drslist = DeviceRefSleepBO.Instance.GetListItems(condition);
            await Task.Run(() =>
            {
                if (drslist.Count > 0)
                {
                    foreach (var item in drslist)
                    {
                        User u = UserBO.Instance.GetUserByID(item.UserID);//提出休假的人员
                        var theday = DateTime.Now.Day;
                        var theweek = DateTime.Now.DayOfWeek.ToString();//Friday代表周五  
                        List<string> updatesql = new List<string>();
                        string sqlupdate = "";
                        string condition1 = string.Format(" d.ID='{0}' and dr.ID like '%{1}%' and dr.State=0", item.DeviceID, thetime);
                        ExecutionDeviceMaintenancePlan dr = ExecutionDeviceMaintenancePlanBO.Instance.GetItemBySql(condition1);
                        if (dr != null)
                        {
                            if (string.IsNullOrEmpty(item.Describe))//说明是全天休息
                            {
                                //处理全是周检月检的
                                var sqls0 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%' and Cycle in('早','中','晚','每日一次')", dr.ID);
                                List<MaintenancePlanRefPlanContext> b0 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls0);//早，中，晚，每日一次项目列表
                                var sqls1 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%' and Cycle in('每周一次')", dr.ID);
                                List<MaintenancePlanRefPlanContext> b1 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls1);//每周一次保养项目列表ID
                                var sqls2 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%' and Cycle in('每月一次')", dr.ID);
                                List<MaintenancePlanRefPlanContext> b2 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls2);//每月一次保养项目列表ID
                                var sqls3 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%' and Cycle in('每周一次','每月一次')", dr.ID);
                                List<MaintenancePlanRefPlanContext> b3 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls3);//每周，每月一次保养项目列表ID                     
                                var sqls4 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%'", dr.ID);
                                List<MaintenancePlanRefPlanContext> b4 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls4);//所有保养项目列表ID                                         
                                if (b0.Count > 0
                                    || (b1.Count == b4.Count && theweek == "Friday")
                                    || (b2.Count == b4.Count && theday == 15)
                                    || (b3.Count == b4.Count && b1.Count != b4.Count && b2.Count != b4.Count && theweek == "Friday" && theday == 15))
                                //说明这些单子每天都有项目可做
                                {
                                    dr.UserID = item.UserID;
                                    dr.State = (int)ExcuteBYState.本日休息;
                                    dr.Describe = "本日休息无需保养";
                                    dr.StartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 06:45"));
                                    dr.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 06:47"));
                                    sqlupdate = string.Format("update  MaintenancePlanRefPlanContext set IsHide=2 where ExecutionDeviceMaintenancePlanID={0} and  IsHide=1 ", dr.ID);
                                    updatesql.Add(sqlupdate);
                                    MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(updatesql);
                                    ExecutionDeviceMaintenancePlanBO.Instance.UpdateState(dr);
                                }
                                else
                                    continue;
                            }
                            else if (item.Describe == "中班休息" || item.Describe == "晚班休息")//说明只是中班或者晚班休息
                            {
                                if (item.Describe == "中班休息")
                                {
                                    string condition3 = string.Format(" e.DeviceID='{0}' and d.ExecutionDeviceMaintenancePlanID like '%{1}%' and d.Cycle='中' and IsHide=1", item.DeviceID, dr.ID);
                                    List<MaintenancePlanRefPlanContext> m = MaintenancePlanRefPlanContextBO.Instance.GetOneItemBySql(condition3);
                                    if (m.Count > 0)//说明有这条数据
                                    {
                                        foreach (var itemm in m)
                                        {
                                            var describe = string.Format("中班休息，操作人员:{0}", u.RealName);
                                            MaintenancePlanRefPlanContextBO.Instance.ChangeIsHide(itemm.ID, describe);
                                        }
                                    }
                                }
                                else if (item.Describe == "晚班休息")
                                {
                                    string condition3 = string.Format(" e.DeviceID='{0}' and d.ExecutionDeviceMaintenancePlanID like '%{1}%' and d.Cycle='晚' and IsHide=1", item.DeviceID, thetime);
                                    List<MaintenancePlanRefPlanContext> m = MaintenancePlanRefPlanContextBO.Instance.GetOneItemBySql(condition3);
                                    if (m.Count > 0)//说明有这条数据
                                    {
                                        foreach (var itemm in m)
                                        {
                                            var describe = string.Format("晚班休息，操作人员:{0}", u.RealName);
                                            MaintenancePlanRefPlanContextBO.Instance.ChangeIsHide(itemm.ID, describe);
                                        }
                                    }
                                }
                            }
                            else if (item.Describe == "设备未生产")//说明是设备未生产
                            {
                                //处理全是周检月检的
                                var sqls0 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%' and Cycle in('早','中','晚','每日一次')", dr.ID);
                                List<MaintenancePlanRefPlanContext> b0 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls0);//早，中，晚，每日一次项目列表
                                var sqls1 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%' and Cycle in('每周一次')", dr.ID);
                                List<MaintenancePlanRefPlanContext> b1 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls1);//每周一次保养项目列表ID
                                var sqls2 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%' and Cycle in('每月一次')", dr.ID);
                                List<MaintenancePlanRefPlanContext> b2 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls2);//每月一次保养项目列表ID
                                var sqls3 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%' and Cycle in('每周一次','每月一次')", dr.ID);
                                List<MaintenancePlanRefPlanContext> b3 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls3);//每周，每月一次保养项目列表ID                     
                                var sqls4 = string.Format(" ExecutionDeviceMaintenancePlanID like '%{0}%'", dr.ID);
                                List<MaintenancePlanRefPlanContext> b4 = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(sqls4);//所有保养项目列表ID                                         
                                if (b0.Count > 0
                                    || (b1.Count == b4.Count && theweek == "Friday")
                                    || (b2.Count == b4.Count && theday == 15)
                                    || (b3.Count == b4.Count && b1.Count != b4.Count && b2.Count != b4.Count && theweek == "Friday" && theday == 15))
                                //说明这些单子每天都有项目可做
                                {
                                    dr.UserID = item.UserID;
                                    dr.State = (int)ExcuteBYState.设备未生产;
                                    dr.Describe = "本日设备未生产无需保养";
                                    dr.StartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                                    dr.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                                    sqlupdate = string.Format("update  MaintenancePlanRefPlanContext set IsHide=2 where ExecutionDeviceMaintenancePlanID={0} and  IsHide=1 ", dr.ID);
                                    updatesql.Add(sqlupdate);
                                    MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(updatesql);
                                    ExecutionDeviceMaintenancePlanBO.Instance.UpdateState(dr);
                                }
                                else
                                    continue;
                            }
                        }
                    }
                    result = true;
                }
            });
            return result;
        }

        #endregion

        #region 处理空保养单子
        //处理超期保养
        public async Task<bool> DealWithNullBY()
        {
            bool result = true;
            await Task.Run(() =>
            {
                List<ExecutionDeviceMaintenancePlan> List = new List<ExecutionDeviceMaintenancePlan>();
                var condition = " d.State in (0,3)";
                List = ExecutionDeviceMaintenancePlanBO.Instance.GetListItems(condition);
                List.ForEach(a =>
                {
                    List<MaintenancePlanRefPlanContext> onelist = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(string.Format("IsHide in (1,2) and ExecutionDeviceMaintenancePlanID={0}", a.ID));
                    if (onelist.Count == 0)
                    {
                        a.State = (int)ExcuteBYState.计划已执行;
                        a.Describe = "本日无需保养，系统自动将其状态改变";
                        var starttime = string.Format("2016/{0}/{1} 06:45", a.ID.Substring(4, 2), a.ID.Substring(6, 2));
                        var endtime = string.Format("2016/{0}/{1} 06:50", a.ID.Substring(4, 2), a.ID.Substring(6, 2));
                        a.StartTime = Convert.ToDateTime(starttime);
                        a.EndTime = Convert.ToDateTime(endtime);
                        ExecutionDeviceMaintenancePlanBO.Instance.UpdateState(a);
                    }
                });
            });
            return result;
        }
        #endregion

        #region 设备封存

        public async Task<bool> CloseDevice(string deviceids)
        {
            //保养步骤，将今天的单子，关联表删除
            //检修步骤，有超期将其删除，将正常的state变个值，如13,14，后台检测是否超期方法当时间到时继续删除超期，建正常单子
            //最后将设备状态变为封存，成功true
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.UpdateDeviceState(items.ID, "封存");
                    var condition = string.Format(" dr.ID=(select Max(ID) from ExecutionDeviceMaintenancePlan where DeviceID='{0}')", items.ID);
                    ExecutionDeviceMaintenancePlan d = ExecutionDeviceMaintenancePlanBO.Instance.GetItemBySql(condition);
                    if (d != null)
                    {
                        List<string> deletemsql = new List<string>();
                        List<string> deleteesql = new List<string>();
                        string sql1 = string.Format(" delete from MaintenancePlanRefPlanContext where ExecutionDeviceMaintenancePlanID ='{0}' ", d.ID);
                        deletemsql.Add(sql1);
                        MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(deletemsql);//删除关联表
                        string sql2 = string.Format(" delete from ExecutionDeviceMaintenancePlan where ID ='{0}' ", d.ID);
                        deleteesql.Add(sql2);
                        ExecutionDeviceMaintenancePlanBO.Instance.ExecuteSql(deleteesql);//删除保养表                      
                    }
                    var condition1 = string.Format("e.State in (1, 2, 3, 4, 11, 12) and e.DeviceID = '{0}'", items.ID);
                    List<ExecutionPlan> executionlist = ExecutionPlanBO.Instance.GetListItems(condition1);
                    if (executionlist.Count != 0)
                    {
                        foreach (var item in executionlist)
                        {
                            if (item.State == 11 || item.State == 12)
                            {
                                RepairPlanRefPlanContextBO.Instance.DeleteRepairPlanRefPlanContext(item.ID);
                                ExecutionPlanBO.Instance.DeleteExecutionPlan(item.ID);
                            }
                            else
                            {
                                if (item.State % 2 == 1)
                                {
                                    ExecutionPlanBO.Instance.UpdateExecutionState(item.ID, 13);
                                }
                                else
                                {
                                    ExecutionPlanBO.Instance.UpdateExecutionState(item.ID, 14);
                                }
                            }
                        }
                    }
                }
            });
            return result;
        }

        public async Task<bool> OpenDevice(string deviceids)
        {
            //保养步骤，复制最近那天的单子
            //检修步骤，将state的值改变
            //最后将设备状态变为完好
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var items in devicelist)
                {
                    result = DeviceBO.Instance.UpdateDeviceState(items.ID, "完好");
                    //检修步骤，将state的值改变
                    var conditionj = string.Format("e.State in (13, 14) and e.DeviceID = '{0}'", items.ID);
                    List<ExecutionPlan> executionlist = ExecutionPlanBO.Instance.GetListItems(conditionj);
                    if (executionlist.Count != 0)
                    {
                        foreach (var item in executionlist)
                        {
                            if (item.State % 2 == 1)
                            {
                                ExecutionPlanBO.Instance.UpdateExecutionState(item.ID, 1);
                            }
                            else
                            {
                                ExecutionPlanBO.Instance.UpdateExecutionState(item.ID, 2);
                            }
                        }
                    }
                    //保养步骤，复制最近那天的单子
                    var condition = string.Format(" dr.ID=(select Max(ID) from ExecutionDeviceMaintenancePlan where DeviceID='{0}')", items.ID);
                    ExecutionDeviceMaintenancePlan f = ExecutionDeviceMaintenancePlanBO.Instance.GetItemBySql(condition);
                    string id = "";
                    if (f != null)
                    {
                        id = f.ID.Substring(0, 2) + (DateTime.Now.ToString("yyyyMMdd")).Substring(2, 6) + f.ID.Substring(f.ID.Length - 3);
                    }
                    if (f != null && id.CompareTo(f.ID) != 0)
                    {
                        ExecutionDeviceMaintenancePlan a = new ExecutionDeviceMaintenancePlan();
                        //修改单号:62+当前日期截取前2位 状态state全为0
                        a.ID = f.ID.Substring(0, 2) + (DateTime.Now.ToString("yyyyMMdd")).Substring(2, 6) + f.ID.Substring(f.ID.Length - 3);
                        a.State = (int)ExcuteBYState.计划未执行;
                        a.StartTime = null;
                        a.EndTime = null;
                        a.Describe = "";
                        a.UserID = null;
                        a.DeviceID = items.ID;
                        a.ErrorAmount = null;
                        a.MaintenancePlanID = items.FileName1ID;

                        ExecutionDeviceMaintenancePlanBO.Instance.InsertExecutionDeviceMaintenancePlan(a);
                        //根据设备找到关联保养表ID
                        var listplan = MaintenancePlanBO.Instance.GetListItems(string.Format(" p.FileName1ID={0}", items.FileName1ID));
                        if (listplan != null)
                        {
                            listplan.ForEach(p =>
                            {
                                //插入语句
                                MaintenancePlanRefPlanContext planref = new MaintenancePlanRefPlanContext()
                                {
                                    Cycle = p.Cycle,
                                    DealWay = p.DealWay,
                                    ExecutionDeviceMaintenancePlanID = f.ID,
                                    ImportentPart = p.ImportentPart,
                                    MaintenancePlanID = p.ID,
                                    Project = p.Project,
                                    TheContent = p.TheContent,
                                    Standard = p.Standard,
                                    Code = p.Code,
                                };
                                MaintenancePlanRefPlanContextBO.Instance.Insert(planref);
                            });
                            string ID = 20 + a.ID.Substring(2);
                            //从保养单号得到保养单的计划时间,通过这个时间解决hide字段
                            int y = (ID.Substring(0, 4)).ToInt32();  //  年  2016  
                            int m = (ID.Substring(4, 2)).ToInt32();  //  月  5
                            int d = (ID.Substring(6, 2)).ToInt32();  //  日  29
                            int ss = (ID.Substring(0, 8)).ToInt32();//20160529
                            if (m == 1) m = 13;
                            if (m == 2) m = 14;
                            int week = (d + 2 * m + 3 * (m + 1) / 5 + y + y / 4 - y / 100 + y / 400) % 7 + 1;
                            string weekstr = "";
                            switch (week)
                            {
                                case 1: weekstr = "星期一"; break;
                                case 2: weekstr = "星期二"; break;
                                case 3: weekstr = "星期三"; break;
                                case 4: weekstr = "星期四"; break;
                                case 5: weekstr = "星期五"; break;
                                case 6: weekstr = "星期六"; break;
                                case 7: weekstr = "星期日"; break;
                            }
                            List<string> stringsql = new List<string>();
                            string sql1 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0} and Cycle in ('早','中','晚','每日一次')", a.ID);
                            stringsql.Add(sql1);
                            MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(stringsql);//执行sql语句 ,先将早中晚，每日一次的处理掉
                            var conditions = string.Format(" ExecutionDeviceMaintenancePlanID={0} and cycle ='每周一次'", a.ID);//将每周一次的找出来
                            List<MaintenancePlanRefPlanContext> WeekList = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(conditions);
                            var condition1 = string.Format(" ExecutionDeviceMaintenancePlanID={0} and cycle ='每月一次'", a.ID);//将每月一次的找出来
                            List<MaintenancePlanRefPlanContext> MonthList = MaintenancePlanRefPlanContextBO.Instance.GetRefListByExID(condition1);
                            if (WeekList.Count > 0 || MonthList.Count > 0)
                            {
                                if (WeekList.Count > 0)//当Count大于0执行下面操作
                                {
                                    List<string> updatesql = new List<string>();
                                    if (weekstr == "星期四" || weekstr == "星期五" || weekstr == "星期六")
                                    {
                                        var sql2 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0}  and  Cycle = '每周一次'", a.ID);
                                        updatesql.Add(sql2);
                                    }
                                    else
                                    {
                                        var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=0 where ExecutionDeviceMaintenancePlanID={0} and  Cycle ='每周一次'", a.ID);
                                        updatesql.Add(sql3);
                                    }
                                    MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(updatesql);//执行sql语句 ,每周一次处理掉
                                }
                                if (MonthList.Count > 0)//当Count大于0执行下面操作
                                {
                                    List<string> updatesql = new List<string>();
                                    if (d == 14 || d == 15 || d == 16)
                                    {
                                        var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=1 where ExecutionDeviceMaintenancePlanID={0}  and  Cycle = '每月一次'", a.ID);
                                        updatesql.Add(sql3);
                                    }
                                    else
                                    {
                                        var sql3 = string.Format("update  MaintenancePlanRefPlanContext set IsHide=0 where ExecutionDeviceMaintenancePlanID={0} and  Cycle ='每月一次'", a.ID);
                                        updatesql.Add(sql3);
                                    }
                                    MaintenancePlanRefPlanContextBO.Instance.ExecuteSql(updatesql);//执行sql语句 ,每月一次处理掉
                                }
                            }
                        }
                    }
                }
            });
            return result;
        }

        #endregion

        #region 资格操作

        public ActionResult DeviceRefQualificationsList()
        {
            return View();
        }

        //设置操作设备资质
        [HttpPost]
        public async Task<bool> SetDeviceRefQualifications(string qualificationsid, string deviceids)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            DeviceRefQualifications drq = new DeviceRefQualifications();
            drq.QualificationsID = qualificationsid.ToInt32();
            await Task.Run(() =>
            {
                foreach (var item in devicelist)
                {
                    drq.DeviceID = item.ID;
                    result = DeviceRefQualificationsBO.Instance.InsertDeviceRefQualifications(drq);
                    if (!result)
                    { break; }
                }
            });
            return result;
        }

        //删除操作设备资质
        [HttpPost]
        public async Task<bool> DeleteDeviceRefQualifications(string deviceids)
        {
            bool result = false;
            List<Device> devicelist = DeviceBO.Instance.GetListItemsbyIDs(deviceids);
            await Task.Run(() =>
            {
                foreach (var item in devicelist)
                {
                    result = DeviceRefQualificationsBO.Instance.DeleteDeviceRefQualifications(item.ID);
                    if (!result)
                    { break; }
                }
            });
            return result;
        }

        #endregion


    }

}